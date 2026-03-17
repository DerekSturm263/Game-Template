// Assets/ProjectOrganizer/Editor/ProjectFolderGenerator.cs
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectOrganizerTool
{
    [Serializable]
    public class FileMoveOperation
    {
        public string Source; // Unity asset path (Assets/..)
        public string Destination; // Unity asset path (Assets/..)

        public FileMoveOperation(string source, string destination)
        {
            Source = source;
            Destination = destination;
        }
    }

    [Serializable]
    public class UndoMoveBatch
    {
        public List<FileMoveOperation> Moves = new();
    }

    public class ProjectOrganizerWindow : EditorWindow
    {
        // -------- Undo log --------
        private const string UndoLogPath = "Library/ProjectOrganizerUndo.json";

        // -------- Templates folder (generator output) --------
        private const string ProfilesFolder = ProjectOrganizerProfileTemplatesGenerator.ProfilesFolder;

        // -------- Theme --------
        private const float BG_OVERLAY_ALPHA = 0.26f;
        private static readonly Color COL_BG = new(0.12f, 0.12f, 0.14f, 1f);
        private static readonly Color COL_PANEL = new(0.16f, 0.17f, 0.20f, 1f);
        private static readonly Color COL_PANEL_2 = new(0.20f, 0.21f, 0.24f, 1f);
        private static readonly Color COL_BORDER = new(0.30f, 0.32f, 0.36f, 1f);
        private static readonly Color COL_TEXT = new(0.94f, 0.94f, 0.97f, 1f);
        private static readonly Color COL_TEXT_SUB = new(0.78f, 0.80f, 0.84f, 1f);
        private static readonly Color COL_ROW_EVEN = new(1f, 1f, 1f, 0.03f);

        private static readonly Color ACCENT_VIOLET = new(0.43f, 0.34f, 0.68f, 1f);
        private static readonly Color ACCENT_BLUE = new(0.36f, 0.62f, 0.97f, 1f);
        private static readonly Color ACCENT_V_HOVER = new(0.54f, 0.45f, 0.80f, 1f);
        private static readonly Color ACCENT_B_HOVER = new(0.52f, 0.74f, 1.00f, 1f);

        private static readonly Color COL_DANGER = new(0.80f, 0.08f, 0.15f, 1f);
        private static readonly Color COL_WARN = new(1.00f, 0.86f, 0.45f, 1f);
        private static readonly Color DARK_RED = new(0.40f, 0.08f, 0.12f, 1f);

        // -------- Profile / State --------
        private ProjectOrganizerProfile _profile;
        private ProjectOrganizerTemplatePreset _preset;
        private bool _dryRunMode = true;
        private bool _filesScanned;
        private string _fileSearch = "";
        private string _statusMessage = "";

        // Files (Unity asset paths)
        private readonly Dictionary<string, bool> _fileToggles = new();
        private readonly Dictionary<string, string> _fileToTargetFolder = new();

        // Undo
        private UndoMoveBatch _lastUndoBatch;

        // UI
        private Texture2D _bgTex, _texPanel, _texPanel2, _logoTexture;

        private GUIStyle _titleStyle,
            _subtitleStyle,
            _cardHeaderStyle,
            _cardBodyStyle,
            _dirMini,
            _footerMini,
            _centerMini;

        private Vector2 _outerScroll, _filesScroll, _previewScroll;

        // Logs (avoid huge strings)
        private const int MaxLogLines = 220;
        private readonly List<string> _lastLogLines = new();

        // Danger button textures
        private Texture2D _texDanger, _texDangerHover, _texDangerActive;

        [MenuItem("Tools/Project Organizer")]
        public static void ShowWindow()
        {
            var w = GetWindow<ProjectOrganizerWindow>("Project Organizer");
            w.minSize = new Vector2(720, 800);
        }

        // -------- Plan ops (dry-run safe) --------
        private enum OpKind
        {
            CreateFolder,
            MoveAsset
        }

        private struct Op
        {
            public OpKind Kind;
            public string A; // folder path OR source path
            public string B; // destination path (MoveAsset)
            public string Label;

            public static Op Mkdir(string folder) => new Op { Kind = OpKind.CreateFolder, A = folder };

            public static Op Move(string from, string to, string label = null) =>
                new Op { Kind = OpKind.MoveAsset, A = from, B = to, Label = label };
        }

        private List<Op> BuildPlan(List<string> selected)
        {
            var ops = new List<Op>();

            // 1) Template folders (PLAN only)
            if (_profile.TemplateFolders != null)
            {
                foreach (var folder in _profile.TemplateFolders)
                {
                    var unityFolder = NormalizeUnityPath(Path.Combine("Assets", folder).Replace("\\", "/"));
                    if (!AssetDatabase.IsValidFolder(unityFolder))
                        ops.Add(Op.Mkdir(unityFolder));
                }
            }

            // 2) Moves + needed target folders (PLAN only)
            var seenFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var assetPath in selected)
            {
                var fileName = Path.GetFileName(assetPath);

                var targetFolder = _fileToTargetFolder.TryGetValue(assetPath, out var tf)
                    ? tf
                    : GetTargetFolderForAsset(assetPath);

                targetFolder = NormalizeUnityPath(targetFolder);

                if (!targetFolder.StartsWith("Assets/", StringComparison.Ordinal))
                {
                    ops.Add(Op.Move(assetPath, assetPath,
                        $"[ERROR] Invalid target folder for {fileName}: {targetFolder}"));
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(targetFolder) && seenFolders.Add(targetFolder))
                    ops.Add(Op.Mkdir(targetFolder));

                var targetPath = NormalizeUnityPath($"{targetFolder}/{fileName}");

                if (assetPath == targetPath)
                    continue;

                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetPath) != null)
                {
                    ops.Add(Op.Move(assetPath, targetPath, $"[CONFLICT] {fileName} exists at destination (ignored)."));
                    continue;
                }

                ops.Add(Op.Move(assetPath, targetPath));
            }

            // 3) Clean mkdir duplicates (template + targets)
            var outOps = new List<Op>();
            var mkdirSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var op in ops)
            {
                if (op.Kind == OpKind.CreateFolder)
                {
                    if (mkdirSet.Add(op.A)) outOps.Add(op);
                }
                else outOps.Add(op);
            }

            return outOps;
        }

        private void OnEnable()
        {
            _bgTex = Resources.Load<Texture2D>("settings_bg");
            if (_bgTex == null) _bgTex = Resources.Load<Texture2D>("inspector_bg");

            _logoTexture = Resources.Load<Texture2D>("Icon-160x160 - Project Organizer");

            LoadOrPickProfile();
        }

        void LoadOrPickProfile()
        {
            _profile = ProjectOrganizerProfileUtil.GetSelectedProfile();

            if (_profile == null)
            {
                var guid = AssetDatabase.FindAssets("t:ProjectOrganizerProfile").FirstOrDefault();
                if (!string.IsNullOrEmpty(guid))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    _profile = AssetDatabase.LoadAssetAtPath<ProjectOrganizerProfile>(path);
                    ProjectOrganizerProfileUtil.SetSelectedProfile(_profile);
                }
            }

            if (_profile == null)
                _statusMessage = "No profile selected. Create one.";
        }

        // -------- Ensure theme --------
        void EnsureTheme()
        {
            if (Event.current == null) return;

            if (_texPanel == null) _texPanel = MakeTex(2, 2, COL_PANEL);
            if (_texPanel2 == null) _texPanel2 = MakeTex(2, 2, COL_PANEL_2);

            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    { fontSize = 18, alignment = TextAnchor.MiddleLeft, richText = true };
                _titleStyle.normal.textColor = COL_TEXT;
            }

            if (_subtitleStyle == null)
            {
                _subtitleStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
                _subtitleStyle.normal.textColor = COL_TEXT_SUB;
            }

            if (_cardHeaderStyle == null)
            {
                _cardHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                    { fontSize = 12, alignment = TextAnchor.MiddleLeft };
                _cardHeaderStyle.normal.textColor = COL_TEXT;
            }

            if (_cardBodyStyle == null)
            {
                _cardBodyStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(12, 12, 10, 12),
                    margin = new RectOffset(0, 0, 0, 0)
                };
                _cardBodyStyle.normal.background = _texPanel2;
            }

            if (_dirMini == null)
            {
                _dirMini = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10 };
                _dirMini.normal.textColor = new Color(0.76f, 0.88f, 1f, 1f);
                _dirMini.wordWrap = false;
                _dirMini.clipping = TextClipping.Clip;
            }

            if (_footerMini == null)
            {
                _footerMini = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    { alignment = TextAnchor.LowerCenter, fontStyle = FontStyle.Italic };
                _footerMini.normal.textColor = COL_TEXT_SUB;
            }

            if (_centerMini == null)
            {
                _centerMini = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };
                _centerMini.normal.textColor = COL_TEXT;
            }
        }

        // -------- Layout helpers --------
        void DrawBackground()
        {
            var r = new Rect(0, 0, position.width, position.height);
            if (_bgTex != null)
            {
                GUI.DrawTexture(r, _bgTex, ScaleMode.ScaleAndCrop, true);
                EditorGUI.DrawRect(r, new Color(0, 0, 0, BG_OVERLAY_ALPHA));
            }
            else EditorGUI.DrawRect(r, COL_BG);
        }

        void Header()
        {
            GUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (_logoTexture != null) GUILayout.Label(_logoTexture, GUILayout.Width(40), GUILayout.Height(40));
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("Project Organizer", _titleStyle);
                    EditorGUILayout.LabelField("Organize & migrate assets with rules • dry-run • undo", _subtitleStyle);
                }
            }

            var line = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(line, COL_BORDER);
            GUILayout.Space(2);
        }

        void Footer()
        {
            var line = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(line, COL_BORDER);

            var r = GUILayoutUtility.GetRect(1, 32, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint) GUI.DrawTexture(r, _texPanel);
            GUI.Label(r, string.IsNullOrEmpty(_statusMessage) ? "Ready." : _statusMessage, _centerMini);

            GUILayout.Label("© 2026 JulesTools • ProjectOrganizerTool", _footerMini);
            GUILayout.Space(4);
        }

        void Card(string title, Action body)
        {
            var header = GUILayoutUtility.GetRect(1, 24, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint) GUI.DrawTexture(header, _texPanel);
            GUI.Label(new Rect(header.x + 10, header.y, header.width - 20, header.height), title, _cardHeaderStyle);
            EditorGUI.DrawRect(new Rect(header.x, header.yMax - 1, header.width, 1), COL_BORDER);

            EditorGUILayout.BeginVertical(_cardBodyStyle);
            try
            {
                body?.Invoke();
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(10);
        }

        bool PrimaryButton(string label, float height = 42f, float minWidth = 240f, bool expand = false)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.miniButton,
                GUILayout.Height(height),
                GUILayout.MinWidth(minWidth),
                expand ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));

            bool hover = rect.Contains(Event.current.mousePosition);
            var top = new Rect(rect.x, rect.y, rect.width, Mathf.Round(rect.height * 0.5f));
            EditorGUI.DrawRect(top, hover ? ACCENT_B_HOVER : ACCENT_BLUE);
            var bot = new Rect(rect.x, rect.y + top.height, rect.width, rect.height - top.height);
            EditorGUI.DrawRect(bot, hover ? ACCENT_V_HOVER : ACCENT_VIOLET);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), COL_BORDER);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), COL_BORDER);

            var style = new GUIStyle(GUI.skin.button)
                { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.white;
            return GUI.Button(rect, label, style);
        }

        bool SecondaryButton(string label, float height = 28f, float maxWidth = 180f)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.miniButton,
                GUILayout.Height(height), GUILayout.MaxWidth(maxWidth));
            EditorGUI.DrawRect(rect, COL_PANEL_2);
            var style = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = COL_TEXT;
            return GUI.Button(rect, label, style);
        }

        bool MiniButton(string label, float width)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent(label), EditorStyles.miniButton, GUILayout.Width(width));
            EditorGUI.DrawRect(rect, COL_PANEL_2);
            var s = new GUIStyle(EditorStyles.miniButton);
            s.normal.textColor = COL_TEXT;
            return GUI.Button(rect, label, s);
        }

        bool DangerMiniButton(string label = "X", float width = 26f, float height = 22f)
        {
            if (_texDanger == null) _texDanger = MakeTex(2, 2, DARK_RED);
            if (_texDangerHover == null)
                _texDangerHover = MakeTex(2, 2, new Color(
                    Mathf.Min(DARK_RED.r + 0.08f, 1f),
                    Mathf.Min(DARK_RED.g + 0.08f, 1f),
                    Mathf.Min(DARK_RED.b + 0.08f, 1f), 1f));
            if (_texDangerActive == null)
                _texDangerActive = MakeTex(2, 2, new Color(
                    Mathf.Min(DARK_RED.r + 0.16f, 1f),
                    Mathf.Min(DARK_RED.g + 0.16f, 1f),
                    Mathf.Min(DARK_RED.b + 0.16f, 1f), 1f));

            var s = new GUIStyle(EditorStyles.miniButton)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };

            s.normal.background = _texDanger;
            s.hover.background = _texDangerHover;
            s.active.background = _texDangerActive;
            s.onNormal.background = _texDanger;
            s.onHover.background = _texDangerHover;
            s.onActive.background = _texDangerActive;

            s.normal.textColor = Color.white;
            s.hover.textColor = Color.white;
            s.active.textColor = Color.white;

            return GUILayout.Button(label, s, GUILayout.Width(width), GUILayout.Height(height));
        }

        // -------- OnGUI --------
        private void OnGUI()
        {
            EnsureTheme();
            DrawBackground();
            Header();

            DrawProfileBar();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var tStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
                tStyle.normal.textColor = COL_TEXT_SUB;
                GUILayout.Label("Mode:", tStyle, GUILayout.Width(40));

                _dryRunMode = GUILayout.Toggle(
                    _dryRunMode,
                    _dryRunMode ? "Dry Run" : "Apply Moves",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(92)
                );

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_profile == null))
                {
                    if (GUILayout.Button("Scan assets", EditorStyles.toolbarButton, GUILayout.Width(110)))
                    {
                        ScanAssets();
                        _filesScanned = true;
                        _fileSearch = "";
                    }

                    if (GUILayout.Button(_dryRunMode ? "Simulate" : "Organize Now",
                            EditorStyles.toolbarButton, GUILayout.Width(150)))
                    {
                        Organize(!_dryRunMode);
                    }
                }
            }

            _outerScroll = EditorGUILayout.BeginScrollView(_outerScroll);

            Card("Rules & Scope", DrawProfileConfigUI);
            Card("Scan & Select Assets", DrawScanSelect);
            Card("Preview", DrawPreview);
            Card("Actions", DrawActions);

            EditorGUILayout.EndScrollView();
            Footer();
        }

        void DrawProfileBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var newProfile = (ProjectOrganizerProfile)EditorGUILayout.ObjectField(
                    _profile, typeof(ProjectOrganizerProfile), false, GUILayout.Width(340));

                if (newProfile != _profile)
                {
                    _profile = newProfile;
                    ProjectOrganizerProfileUtil.SetSelectedProfile(_profile);
                    ResetScanState();
                    _statusMessage = _profile ? "Profile selected." : "No profile selected.";
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create Profile", EditorStyles.toolbarButton, GUILayout.Width(110)))
                {
                    var created = ProjectOrganizerProfileUtil.CreateProfileAsset();
                    if (created != null)
                    {
                        _profile = created;
                        ResetScanState();
                        _statusMessage = "Profile created.";
                    }
                }

                if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    if (_profile) EditorGUIUtility.PingObject(_profile);
                }
            }
        }

        void ResetScanState()
        {
            _filesScanned = false;
            _fileToggles.Clear();
            _fileToTargetFolder.Clear();
            _lastLogLines.Clear();
        }

        // -------- Sections --------
        void DrawProfileConfigUI()
        {
            if (_profile == null)
            {
                EditorGUILayout.HelpBox("No profile selected. Create or select a profile to edit rules.",
                    MessageType.Info);
                return;
            }

            // === QUICK PROFILE TEMPLATES =====================================
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Quick Profile Templates", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.HelpBox(
                    "Generate ready-to-use profiles and auto-select them here (no manual setup).",
                    MessageType.None);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Basic", GUILayout.Height(24)))
                        GenerateAndSelectProfile("PO_Profile_Basic_Project");

                    if (GUILayout.Button("2D Game", GUILayout.Height(24)))
                        GenerateAndSelectProfile("PO_Profile_2D_Game");

                    if (GUILayout.Button("3D Game", GUILayout.Height(24)))
                        GenerateAndSelectProfile("PO_Profile_3D_Game");

                    if (GUILayout.Button("Jam", GUILayout.Height(24)))
                        GenerateAndSelectProfile("PO_Profile_Jam_Setup");
                }

                EditorGUILayout.Space(4);

                if (GUILayout.Button("Generate ALL templates", GUILayout.Height(24)))
                {
                    ProjectOrganizerProfileTemplatesGenerator.GenerateAll();
                    GenerateAndSelectProfile("PO_Profile_Basic_Project", onlySelect: true);
                }
            }

            EditorGUILayout.Space(10);

            // SerializedObject = stable UI + Undo
            var so = new SerializedObject(_profile);
            so.Update();

            // === PRESETS ======================================================
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Organization Presets", EditorStyles.boldLabel);

            _preset = (ProjectOrganizerTemplatePreset)EditorGUILayout.ObjectField(
                "Preset", _preset, typeof(ProjectOrganizerTemplatePreset), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_preset == null))
                {
                    if (GUILayout.Button("Apply (replace)", GUILayout.Height(22)))
                    {
                        ApplyPresetReplace(_preset, _profile);
                        so = new SerializedObject(_profile);
                        so.Update();
                    }

                    if (GUILayout.Button("Merge (add missing)", GUILayout.Height(22)))
                    {
                        ApplyPresetMerge(_preset, _profile);
                        so = new SerializedObject(_profile);
                        so.Update();
                    }
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(10);

            // === SCOPE ========================================================
            EditorGUILayout.LabelField("Rules & Scope", EditorStyles.boldLabel);

            var includeFoldersProp = so.FindProperty("IncludeFolders");
            var excludeFoldersProp = so.FindProperty("ExcludeFolders");
            var extensionsToScanProp = so.FindProperty("ExtensionsToScan");

            if (includeFoldersProp != null)
                DrawStringListSerialized(includeFoldersProp, "Include Folders", "Add include folder",
                    allowEmpty: false);

            if (excludeFoldersProp != null)
                DrawStringListSerialized(excludeFoldersProp, "Exclude Folders", "Add exclude folder", allowEmpty: true);

            if (extensionsToScanProp != null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(extensionsToScanProp, new GUIContent("Extensions To Scan (optional)"));
                EditorGUILayout.HelpBox("Example: .prefab,.png,.fbx", MessageType.None);
            }

            EditorGUILayout.Space(12);

            // === TEMPLATE FOLDERS ============================================
            var templateFoldersProp = so.FindProperty("TemplateFolders");
            if (templateFoldersProp != null)
            {
                EditorGUILayout.LabelField("Template Folders (relative to Assets/)", EditorStyles.boldLabel);
                DrawStringListSerialized(templateFoldersProp, null, "Add template folder", allowEmpty: false);
                EditorGUILayout.HelpBox("Example: Art, Audio, Prefabs. DO NOT write 'Assets/Art'.", MessageType.None);
                EditorGUILayout.Space(12);
            }

            // === EXTENSION MAPPINGS ==========================================
            var mappingsProp = so.FindProperty("ExtensionMappings");
            if (mappingsProp != null)
            {
                EditorGUILayout.LabelField("Extension Mappings", EditorStyles.boldLabel);

                using (new EditorGUILayout.VerticalScope("box"))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Extension", GUILayout.Width(120));
                        EditorGUILayout.LabelField("Target Folder");
                        GUILayout.Space(22);
                    }

                    for (int i = 0; i < mappingsProp.arraySize; i++)
                    {
                        var element = mappingsProp.GetArrayElementAtIndex(i);
                        var extProp = element.FindPropertyRelative("Extension");
                        var folderProp = element.FindPropertyRelative("TargetFolder");

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (extProp != null)
                            {
                                EditorGUI.BeginChangeCheck();
                                string ext = EditorGUILayout.TextField(extProp.stringValue, GUILayout.Width(120));
                                if (EditorGUI.EndChangeCheck())
                                    extProp.stringValue = NormalizeExtension(ext);
                            }

                            if (folderProp != null)
                                folderProp.stringValue =
                                    NormalizeUnityPath(EditorGUILayout.TextField(folderProp.stringValue));

                            if (GUILayout.Button("X", GUILayout.Width(22)))
                            {
                                mappingsProp.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                    }

                    EditorGUILayout.Space(6);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Add mapping", GUILayout.Width(120), GUILayout.Height(22)))
                        {
                            int idx = mappingsProp.arraySize;
                            mappingsProp.InsertArrayElementAtIndex(idx);
                            var el = mappingsProp.GetArrayElementAtIndex(idx);

                            var extProp = el.FindPropertyRelative("Extension");
                            var folderProp = el.FindPropertyRelative("TargetFolder");
                            if (extProp != null) extProp.stringValue = ".ext";
                            if (folderProp != null) folderProp.stringValue = "Assets/_Organized";
                        }
                    }
                }

                EditorGUILayout.HelpBox(
                    "Target folder must be under Assets/ (ex: Assets/Art). Extensions like .png, .fbx.",
                    MessageType.None);
            }

            if (so.ApplyModifiedProperties())
                EditorUtility.SetDirty(_profile);
        }

        // ---- templates: generate + auto select ----
        private void GenerateAndSelectProfile(string assetName, bool onlySelect = false)
        {
            if (!onlySelect)
            {
                switch (assetName)
                {
                    case "PO_Profile_Basic_Project": ProjectOrganizerProfileTemplatesGenerator.GenerateBasic(); break;
                    case "PO_Profile_2D_Game": ProjectOrganizerProfileTemplatesGenerator.Generate2D(); break;
                    case "PO_Profile_3D_Game": ProjectOrganizerProfileTemplatesGenerator.Generate3D(); break;
                    case "PO_Profile_Jam_Setup": ProjectOrganizerProfileTemplatesGenerator.GenerateJam(); break;
                    default:
                        _statusMessage = $"Unknown template: {assetName}";
                        return;
                }
            }

            var path = $"{ProfilesFolder}/{assetName}.asset";
            var created = AssetDatabase.LoadAssetAtPath<ProjectOrganizerProfile>(path);
            if (created == null)
            {
                _statusMessage = $"Template not found: {path}";
                return;
            }

            _profile = created;
            ProjectOrganizerProfileUtil.SetSelectedProfile(_profile);
            ResetScanState();

            _statusMessage = $"Template selected: {assetName}";
            GUI.FocusControl(null);
            Repaint();
        }

        // ---- Serialized list drawer (with allowEmpty) ----
        private void DrawStringListSerialized(SerializedProperty listProp, string header, string addLabel,
            bool allowEmpty)
        {
            if (listProp == null || !listProp.isArray) return;

            if (!string.IsNullOrEmpty(header))
                EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

            int toRemove = -1;

            for (int i = 0; i < listProp.arraySize; i++)
            {
                var el = listProp.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.HorizontalScope())
                {
                    var v = EditorGUILayout.TextField(el.stringValue);
                    if (!allowEmpty && string.IsNullOrWhiteSpace(v))
                        v = el.stringValue;

                    el.stringValue = v;

                    if (DangerMiniButton("X"))
                        toRemove = i;
                }
            }

            if (toRemove >= 0)
                listProp.DeleteArrayElementAtIndex(toRemove);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (SecondaryButton(addLabel))
                {
                    listProp.InsertArrayElementAtIndex(listProp.arraySize);
                    listProp.GetArrayElementAtIndex(listProp.arraySize - 1).stringValue = "";
                }

                GUILayout.FlexibleSpace();
            }
        }

        void DrawScanSelect()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _fileSearch = EditorGUILayout.TextField(new GUIContent("🔎 Search"), _fileSearch);
                if (MiniButton("✔ All", 56))
                    foreach (var k in _fileToggles.Keys.ToList())
                        _fileToggles[k] = true;
                if (MiniButton("✖ None", 64))
                    foreach (var k in _fileToggles.Keys.ToList())
                        _fileToggles[k] = false;
            }

            if (_profile == null)
            {
                GUILayout.Space(6);
                EditorGUILayout.HelpBox("Select or create a profile first.", MessageType.Info);
                return;
            }

            if (!_filesScanned)
            {
                GUILayout.Space(6);
                EditorGUILayout.HelpBox("Click “Scan assets” in the toolbar to populate the list.", MessageType.Info);
                return;
            }

            var filtered = string.IsNullOrWhiteSpace(_fileSearch)
                ? _fileToggles.Keys.ToList()
                : _fileToggles.Keys.Where(k =>
                    Path.GetFileName(k).IndexOf(_fileSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            _filesScroll = EditorGUILayout.BeginScrollView(_filesScroll, GUILayout.Height(320));
            for (int i = 0; i < filtered.Count; i++)
            {
                string assetPath = filtered[i];
                var row = GUILayoutUtility.GetRect(1, 22, GUILayout.ExpandWidth(true));
                if (i % 2 == 0) EditorGUI.DrawRect(row, COL_ROW_EVEN);

                var tRect = new Rect(row.x + 6, row.y + 2, 18, 18);
                _fileToggles[assetPath] = EditorGUI.Toggle(tRect, _fileToggles[assetPath]);

                var nameRect = new Rect(tRect.xMax + 6, row.y + 1, 280, 18);
                var nameStyle = new GUIStyle(EditorStyles.label);
                nameStyle.normal.textColor = COL_TEXT;
                GUI.Label(nameRect, Path.GetFileName(assetPath), nameStyle);

                var dirRect = new Rect(nameRect.xMax + 8, row.y + 1, row.width - nameRect.width - 250, 18);
                GUI.Label(dirRect, $"[{Path.GetDirectoryName(assetPath)}]", _dirMini);

                var target = _fileToTargetFolder.TryGetValue(assetPath, out var tf) ? tf : "Assets/Misc";
                var targetRect = new Rect(row.xMax - 240, row.y + 1, 230, 18);
                var mini = new GUIStyle(EditorStyles.miniLabel);
                mini.normal.textColor = COL_TEXT_SUB;
                GUI.Label(targetRect, $"→ {target}", mini);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawPreview()
        {
            _dryRunMode =
                EditorGUILayout.ToggleLeft(new GUIContent("Dry run (preview only, no asset moves)"), _dryRunMode);
            GUILayout.Space(6);

            if (_profile == null)
            {
                EditorGUILayout.HelpBox("Select a profile first.", MessageType.None);
                return;
            }

            if (_fileToggles.Count == 0 || !_fileToggles.Any(kv => kv.Value))
            {
                EditorGUILayout.HelpBox("Nothing to preview. Scan assets and select some entries.", MessageType.None);
                return;
            }

            _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, GUILayout.Height(220));
            foreach (var kv in _fileToggles.Where(kv => kv.Value))
            {
                var assetPath = kv.Key;
                var fileName = Path.GetFileName(assetPath);
                var fromDir = Path.GetDirectoryName(assetPath);
                var targetFolder = _fileToTargetFolder.TryGetValue(assetPath, out var tf) ? tf : "Assets/Misc";

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("→", GUILayout.Width(16));
                    GUILayout.Label(fileName, GUILayout.Width(220));
                    GUILayout.Label("From:", GUILayout.Width(36));
                    GUILayout.Label(fromDir, EditorStyles.miniLabel, GUILayout.MinWidth(120));
                    GUILayout.Label("To:", GUILayout.Width(20));
                    GUILayout.Label(targetFolder, EditorStyles.boldLabel, GUILayout.MinWidth(120));
                }
            }

            if (_lastLogLines.Count > 0)
            {
                GUILayout.Space(6);
                EditorGUILayout.LabelField("Last log (capped)", EditorStyles.boldLabel);
                foreach (var l in _lastLogLines) EditorGUILayout.LabelField(l, EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_profile == null))
                {
                    if (PrimaryButton(_dryRunMode ? "SIMULATE" : "ORGANIZE NOW", 42f, 240f, expand: false))
                        Organize(!_dryRunMode);
                }

                GUILayout.FlexibleSpace();
            }

            var undoBatch = _lastUndoBatch ?? LoadUndoBatchFromFile();
            if (undoBatch != null && undoBatch.Moves != null && undoBatch.Moves.Count > 0)
            {
                GUILayout.Space(6);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (SecondaryButton("⏪ UNDO last organize", 30f, 220f))
                    {
                        _lastUndoBatch = undoBatch;
                        UndoLastMoveBatch();
                        Repaint();
                    }

                    GUILayout.FlexibleSpace();
                }
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                var col = _statusMessage.Contains("UNDO")
                    ? COL_WARN
                    : (_dryRunMode
                        ? new Color(0.6f, 0.8f, 1f, 1f)
                        : (_statusMessage.Contains("Moved") ? ACCENT_BLUE : COL_DANGER));

                var style = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 13 };
                style.normal.textColor = col;
                GUILayout.Space(6);
                EditorGUILayout.LabelField(_statusMessage, style);
            }
        }

        static string NormalizeExtension(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) return "";
            ext = ext.Trim().ToLowerInvariant();
            if (!ext.StartsWith(".")) ext = "." + ext;
            return ext;
        }

        // -------- Core logic --------
        void ScanAssets()
        {
            if (_profile == null)
            {
                _statusMessage = "No profile selected.";
                return;
            }

            _fileToggles.Clear();
            _fileToTargetFolder.Clear();
            _lastLogLines.Clear();

            var includeFolders = NormalizeUnityFolders(_profile.IncludeFolders);
            if (includeFolders.Count == 0) includeFolders.Add("Assets");

            var excludeFolders = NormalizeUnityFolders(_profile.ExcludeFolders);

            var exts = ParseExtensions(_profile.ExtensionsToScan);
            if (exts.Count == 0)
            {
                _statusMessage = "No extensions configured in profile.";
                return;
            }

            try
            {
                var guids = AssetDatabase.FindAssets("", includeFolders.ToArray());

                int added = 0;
                for (int i = 0; i < guids.Length; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Project Organizer", "Scanning assets...",
                            (float)i / guids.Length))
                        break;

                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (string.IsNullOrEmpty(path)) continue;
                    if (!path.StartsWith("Assets/", StringComparison.Ordinal)) continue;

                    if (IsUnderExcluded(path, excludeFolders)) continue;
                    if (AssetDatabase.IsValidFolder(path)) continue;

                    var ext = Path.GetExtension(path).ToLowerInvariant();
                    if (!exts.Contains(ext)) continue;

                    if (_fileToggles.ContainsKey(path)) continue;

                    _fileToggles.Add(path, true);
                    _fileToTargetFolder.Add(path, GetTargetFolderForAsset(path));
                    added++;
                }

                _filesScanned = true;
                _statusMessage = $"Scan done: {added} assets found.";
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        string GetTargetFolderForAsset(string assetPath)
        {
            var ext = Path.GetExtension(assetPath).ToLowerInvariant();

            var mapping = _profile.ExtensionMappings.FirstOrDefault(m =>
                !string.IsNullOrWhiteSpace(m.Extension) &&
                m.Extension.Trim().ToLowerInvariant() == ext);

            var folder = mapping != null ? (mapping.TargetFolder ?? "Assets/Misc") : "Assets/Misc";
            folder = NormalizeUnityPath(folder);

            if (!folder.StartsWith("Assets/", StringComparison.Ordinal))
                return "Assets/Misc";

            return folder;
        }

        void Organize(bool applyMoves)
        {
            if (_profile == null)
            {
                _statusMessage = "No profile selected.";
                return;
            }

            _lastLogLines.Clear();

            var selected = _fileToggles.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            if (selected.Count == 0)
            {
                _statusMessage = "Nothing to organize! Please scan and check assets first.";
                return;
            }

            int moved = 0, skipped = 0, conflict = 0, failed = 0, mkdirs = 0;

            var undoBatch = new UndoMoveBatch();
            var ops = BuildPlan(selected);

            try
            {
                if (applyMoves) AssetDatabase.StartAssetEditing();

                for (int i = 0; i < ops.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Project Organizer",
                            applyMoves ? "Applying..." : "Simulating...",
                            (float)i / ops.Count))
                    {
                        AddLog("[CANCEL] Operation cancelled by user.");
                        break;
                    }

                    var op = ops[i];

                    if (op.Kind == OpKind.CreateFolder)
                    {
                        mkdirs++;
                        AddLog($"{(applyMoves ? "[MKDIR]" : "[SIMULATE]")} {op.A}");

                        if (applyMoves)
                            EnsureUnityFolder(op.A);

                        continue;
                    }

                    // MoveAsset ops
                    var from = op.A;
                    var to = op.B;

                    // special labeled ops for errors/conflicts (we used Move(...) as carrier)
                    if (!string.IsNullOrEmpty(op.Label))
                    {
                        AddLog(op.Label);
                        if (op.Label.StartsWith("[CONFLICT]")) conflict++;
                        else if (op.Label.StartsWith("[ERROR]")) failed++;
                        continue;
                    }

                    var fileName = Path.GetFileName(from);

                    if (from == to)
                    {
                        skipped++;
                        AddLog($"[SKIP] {fileName} already in correct folder.");
                        continue;
                    }

                    if (!applyMoves)
                    {
                        AddLog($"[SIMULATE] {fileName} → {Path.GetDirectoryName(to)}");
                        continue;
                    }

                    var moveResult = AssetDatabase.MoveAsset(from, to);
                    if (string.IsNullOrEmpty(moveResult))
                    {
                        moved++;
                        AddLog($"[MOVE] {fileName} → {Path.GetDirectoryName(to)}");
                        undoBatch.Moves.Add(new FileMoveOperation(to, from));
                    }
                    else
                    {
                        failed++;
                        AddLog($"[ERROR] {fileName}: {moveResult}");
                    }
                }
            }
            finally
            {
                if (applyMoves) AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();

                // ✅ Refresh uniquement si apply (dry-run = 0 side-effects)
                if (applyMoves) AssetDatabase.Refresh();
            }

            if (applyMoves)
            {
                _lastUndoBatch = undoBatch;
                SaveUndoBatchToFile(undoBatch);
                _statusMessage =
                    $"Moved: {moved} | Skipped: {skipped} | Conflicts: {conflict} | Failed: {failed} | Folders: {mkdirs}";
            }
            else
            {
                _statusMessage =
                    $"SIMULATE OK (no changes). Planned: Moves={ops.Count(o => o.Kind == OpKind.MoveAsset)} | Folders={mkdirs}";
            }

            Repaint();
        }

        // -------- UNDO --------
        void SaveUndoBatchToFile(UndoMoveBatch batch)
        {
            try
            {
                File.WriteAllText(UndoLogPath, JsonUtility.ToJson(batch, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not save undo log: " + e);
            }
        }

        UndoMoveBatch LoadUndoBatchFromFile()
        {
            if (!File.Exists(UndoLogPath)) return null;
            try
            {
                return JsonUtility.FromJson<UndoMoveBatch>(File.ReadAllText(UndoLogPath));
            }
            catch
            {
                return null;
            }
        }

        void UndoLastMoveBatch()
        {
            if (_lastUndoBatch == null || _lastUndoBatch.Moves == null || _lastUndoBatch.Moves.Count == 0)
            {
                _statusMessage = "Nothing to undo.";
                return;
            }

            _lastLogLines.Clear();

            int undone = 0, failed = 0, conflicts = 0, missing = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < _lastUndoBatch.Moves.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Project Organizer", "Undoing...",
                            (float)i / _lastUndoBatch.Moves.Count))
                    {
                        AddLog("[CANCEL] Undo cancelled by user.");
                        break;
                    }

                    var move = _lastUndoBatch.Moves[i];

                    if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(move.Source) == null)
                    {
                        AddLog($"[MISSING] {move.Source}");
                        missing++;
                        continue;
                    }

                    if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(move.Destination) != null)
                    {
                        AddLog($"[CONFLICT] {move.Destination} exists.");
                        conflicts++;
                        continue;
                    }

                    EnsureUnityFolder(NormalizeUnityPath(Path.GetDirectoryName(move.Destination) ?? "Assets"));

                    var result = AssetDatabase.MoveAsset(move.Source, move.Destination);
                    if (string.IsNullOrEmpty(result))
                    {
                        AddLog($"[UNDO] {Path.GetFileName(move.Source)}");
                        undone++;
                    }
                    else
                    {
                        AddLog($"[ERROR] {move.Source}: {result}");
                        failed++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

            try
            {
                File.Delete(UndoLogPath);
            }
            catch
            {
                /* ignore */
            }

            _lastUndoBatch = null;

            _statusMessage = $"UNDO: {undone} restored • {conflicts} conflicts • {missing} missing • {failed} errors.";
            Repaint();
        }

        // -------- Helpers --------
        static HashSet<string> ParseExtensions(string s)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(s)) return set;

            var parts = s.Split(',', ';');
            foreach (var p in parts)
            {
                var ext = (p ?? "").Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(ext)) continue;
                if (!ext.StartsWith(".")) ext = "." + ext; // <-- important
                set.Add(ext);
            }

            return set;
        }

        static string NormalizeUnityPath(string p)
        {
            if (string.IsNullOrEmpty(p)) return "";
            p = p.Replace("\\", "/").Trim();
            if (p.Length > 1 && p.EndsWith("/")) p = p.TrimEnd('/');
            return p;
        }

        static List<string> NormalizeUnityFolders(List<string> folders)
        {
            var list = new List<string>();
            if (folders == null) return list;

            foreach (var f in folders)
            {
                var p = NormalizeUnityPath(f);
                if (string.IsNullOrEmpty(p)) continue;

                if (p == "Assets")
                {
                    list.Add("Assets");
                    continue;
                }

                if (p.StartsWith("Assets/", StringComparison.Ordinal)) list.Add(p);
            }

            return list.Distinct(StringComparer.Ordinal).ToList();
        }

        static bool IsUnderExcluded(string assetPath, List<string> excluded)
        {
            if (excluded == null || excluded.Count == 0) return false;

            foreach (var ex in excluded)
            {
                if (string.IsNullOrEmpty(ex)) continue;
                if (assetPath == ex) return true;
                if (assetPath.StartsWith(ex + "/", StringComparison.Ordinal)) return true;
            }

            return false;
        }

        static void EnsureUnityFolder(string unityFolder)
        {
            unityFolder = NormalizeUnityPath(unityFolder);
            if (string.IsNullOrEmpty(unityFolder)) return;

            if (unityFolder == "Assets") return;
            if (!unityFolder.StartsWith("Assets/", StringComparison.Ordinal)) return;

            if (AssetDatabase.IsValidFolder(unityFolder)) return;

            var parts = unityFolder.Substring("Assets/".Length).Split('/');
            var parent = "Assets";

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                var testPath = $"{parent}/{part}";
                if (!AssetDatabase.IsValidFolder(testPath))
                    AssetDatabase.CreateFolder(parent, part);
                parent = testPath;
            }
        }

        void AddLog(string line)
        {
            if (_lastLogLines.Count >= MaxLogLines)
                _lastLogLines.RemoveAt(0);
            _lastLogLines.Add(line);
        }

        // -------- Utils --------
        private static Texture2D MakeTex(int w, int h, Color c)
        {
            var tex = new Texture2D(w, h);
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = c;
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        static void ApplyPresetReplace(ProjectOrganizerTemplatePreset preset, ProjectOrganizerProfile profile)
        {
            if (preset == null || profile == null) return;

            profile.TemplateFolders = new List<string>(preset.TemplateFolders);
            profile.ExtensionMappings = preset.ExtensionMappings
                .Select(m => new ExtensionMapping(m.Extension, m.TargetFolder)).ToList();

            profile.IncludeFolders = new List<string>(preset.IncludeFolders);
            profile.ExcludeFolders = new List<string>(preset.ExcludeFolders);
            profile.ExtensionsToScan = preset.ExtensionsToScan;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        static void ApplyPresetMerge(ProjectOrganizerTemplatePreset preset, ProjectOrganizerProfile profile)
        {
            if (preset == null || profile == null) return;

            foreach (var f in preset.TemplateFolders)
                if (!profile.TemplateFolders.Contains(f))
                    profile.TemplateFolders.Add(f);

            foreach (var m in preset.ExtensionMappings)
            {
                var ext = (m.Extension ?? "").Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(ext)) continue;

                bool exists = profile.ExtensionMappings.Any(x =>
                    (x.Extension ?? "").Trim().ToLowerInvariant() == ext);

                if (!exists)
                    profile.ExtensionMappings.Add(new ExtensionMapping(m.Extension, m.TargetFolder));
            }

            foreach (var f in preset.IncludeFolders)
                if (!profile.IncludeFolders.Contains(f))
                    profile.IncludeFolders.Add(f);

            foreach (var f in preset.ExcludeFolders)
                if (!profile.ExcludeFolders.Contains(f))
                    profile.ExcludeFolders.Add(f);

            if (string.IsNullOrWhiteSpace(profile.ExtensionsToScan))
                profile.ExtensionsToScan = preset.ExtensionsToScan;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }
    }
}
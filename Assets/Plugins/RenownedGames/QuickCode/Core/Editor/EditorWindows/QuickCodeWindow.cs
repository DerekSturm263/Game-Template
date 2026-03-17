/* ================================================================
   ---------------------------------------------------
   Project   :    Quick Code
   Publisher :    Renowned Games
   Author    :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using RenownedGames.ExLibEditor.Windows;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RenownedGames.QuickCodeEditor
{
    public sealed class QuickCodeWindow : EditorWindow, IHasCustomMenu
    {
        public enum Mode
        {
            Code,
            Settings,
            Template
        }

        [SerializeField]
        private QuickFunc quickFunc;

        // Stored required properties.
        private Mode mode;
        private string templateName;
        private string code;
        private int index;
        private Vector2 scrollPosition;

        // Stored serialized properties.
        private SerializedObject serializedObject;
        private SerializedProperty serializedQuickFunc;
        private ReorderableList assemblyList;
        private ReorderableList namespaceList;

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            quickFunc = QuickFunc.CreateFromDefaultTemplate();
            SetupSerializedProperties();
        }

        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        private void OnGUI()
        {
            switch (mode)
            {
                case Mode.Code:
                    OnCodeModeGUI();
                    OnFooterGUI();
                    break;
                case Mode.Settings:
                    OnSettingsModeGUI();
                    OnFooterGUI();
                    break;
                case Mode.Template:
                    OnTemplateModeGUI();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called when selected code mode.
        /// </summary>
        private void OnCodeModeGUI()
        {
            float width = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 55;
            templateName = EditorGUILayout.TextField("Name", templateName);

            if (string.IsNullOrEmpty(templateName))
            {
                const string PLACEHOLDER = "Write name of your template...";

                GUIStyle placeholderStyle = new GUIStyle(GUI.skin.label);
                placeholderStyle.fontStyle = FontStyle.Italic;
                placeholderStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                placeholderStyle.hover.textColor = placeholderStyle.normal.textColor;

                Rect namePlaceholderPosition = GUILayoutUtility.GetLastRect();
                namePlaceholderPosition.x += EditorGUIUtility.labelWidth + 4;
                GUI.Label(namePlaceholderPosition, PLACEHOLDER, placeholderStyle);
            }
            EditorGUIUtility.labelWidth = width;

            code = EditorGUILayout.TextArea(code, GUILayout.ExpandHeight(true));
        }

        /// <summary>
        /// Called when selected settings mode.
        /// </summary>
        private void OnSettingsModeGUI()
        {
            serializedObject.Update();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            SerializedProperty hotCompile = serializedQuickFunc.FindPropertyRelative("hotCompile");
            hotCompile.boolValue = EditorGUILayout.ToggleLeft("Hot Compile", hotCompile.boolValue);

            SerializedProperty assemblies = serializedQuickFunc.FindPropertyRelative("assemblies");
            assemblies.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(assemblies.isExpanded, "Assemblies");
            if (assemblies.isExpanded)
            {
                assemblyList.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            SerializedProperty namespaces = serializedQuickFunc.FindPropertyRelative("namespaces");
            namespaces.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(namespaces.isExpanded, "Namespaces");
            if (namespaces.isExpanded)
            {
                namespaceList.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Called when selected template mode.
        /// </summary>
        private void OnTemplateModeGUI()
        {
            QuickFuncTemplates templates = QuickFuncTemplates.Current;

            if (templates.GetTemplateCount() > 0)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                for (int i = 0; i < templates.GetTemplateCount(); i++)
                {
                    Template template = templates.GetTemplate(i);

                    Rect templatePosition = GUILayoutUtility.GetRect(0, 22);
                    GUI.Box(templatePosition, GUIContent.none, EditorStyles.toolbar);

                    Rect removeButtonPosition = new Rect(templatePosition.xMin, templatePosition.y, 25, 25);
                    if (GUI.Button(removeButtonPosition, EditorGUIUtility.IconContent("winbtn_win_close@2x"), EditorStyles.toolbarButton))
                    {
                        templates.RemoveTemplate(i);
                        EditorUtility.SetDirty(QuickFuncTemplates.Current);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        break;
                    }

                    Rect namePosition = new Rect(removeButtonPosition.xMax + 3, templatePosition.y - 2, templatePosition.width, 25);
                    GUI.Label(namePosition, template.GetName());

                    Rect openPosition = new Rect(templatePosition.xMax - 75, templatePosition.y, 40, 25);
                    if (GUI.Button(openPosition, "Open", EditorStyles.toolbarButton))
                    {
                        templateName = template.GetName();
                        code = template.GetCode();
                        quickFunc = new QuickFunc(template.GetQuickFunc());
                        mode = Mode.Code;
                        GUI.FocusControl(string.Empty);
                        Repaint();
                    }

                    Rect runPosition = new Rect(openPosition.xMax, templatePosition.y, 35, openPosition.height);
                    if (GUI.Button(runPosition, "Run", EditorStyles.toolbarButton))
                    {
                        template.Run();
                    }
                }
                GUILayout.EndScrollView();
            }
            else
            {
                Rect labelPosition = new Rect(0, (position.height - 20) / 2, position.width, 20);
                GUI.Label(labelPosition, "No saved templates...", EditorStyles.centeredGreyMiniLabel);
            }
        }

        /// <summary>
        /// Called when selected code or settings modes.
        /// </summary>
        private void OnFooterGUI()
        {
            Rect footerPosition = GUILayoutUtility.GetRect(0, 27);

            GUI.Box(footerPosition, GUIContent.none);

            footerPosition.x += 3;
            footerPosition.width -= 6;
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(code));
            Rect compileButtonPosition = new Rect(footerPosition.xMax - 120, position.height - 24, 60, 20);
            if (GUI.Button(compileButtonPosition, "Compile", "ButtonLeft"))
            {
                quickFunc.Compile(code);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!quickFunc.IsCompiled());
            Rect executeButtonPosition = new Rect(compileButtonPosition.xMax, compileButtonPosition.y, compileButtonPosition.width, compileButtonPosition.height);
            if (GUI.Button(executeButtonPosition, "Execute", "ButtonRight"))
            {
                quickFunc.Execute();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(templateName));
            Rect saveButtonPosition = new Rect(footerPosition.x, compileButtonPosition.y, 30, compileButtonPosition.height);
            if (GUI.Button(saveButtonPosition, EditorGUIUtility.IconContent("SaveAs"), "ButtonLeft"))
            {
                QuickFuncTemplates.Current.AddTemplate(new Template(templateName, code, quickFunc));
                EditorUtility.SetDirty(QuickFuncTemplates.Current);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUI.EndDisabledGroup();

            if (mode == Mode.Code && index != -1)
            {
                index = -1;
            }

            Rect settingsButtonPosition = new Rect(saveButtonPosition.xMax, compileButtonPosition.y, 27, compileButtonPosition.height);
            EditorGUI.BeginChangeCheck();
            int prevIndex = index;
            index = GUI.Toolbar(settingsButtonPosition, index, new[] { EditorGUIUtility.IconContent("Settings") }, "ButtonRight");
            if (EditorGUI.EndChangeCheck())
            {
                if (index == prevIndex)
                {
                    index = -1;
                    mode = Mode.Code;
                }
                else
                {
                    SetupSerializedProperties();
                    mode = Mode.Settings;
                }
            }
        }

        /// <summary>
        /// Setup serialized properties of current QuickFunc instance.
        /// </summary>
        public void SetupSerializedProperties()
        {
            serializedObject = new SerializedObject(this);
            serializedQuickFunc = serializedObject.FindProperty("quickFunc");

            SerializedProperty assemblies = serializedQuickFunc.FindPropertyRelative("assemblies");
            assemblyList = new ReorderableList(serializedObject, assemblies, true, false, true, true);
            assemblyList.elementHeight = 20;
            assemblyList.drawElementCallback += (position, index, isActive, isFocused) =>
            {
                SerializedProperty assembly = assemblies.GetArrayElementAtIndex(index);

                position.y += 2;
                Rect fieldPosition = new Rect(position.x, position.y, position.width - 20, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(fieldPosition, assembly, new GUIContent("Assembly"));

                Rect buttonPosition = new Rect(fieldPosition.xMax + 5, position.y, 20, 20);
                if (GUI.Button(buttonPosition, EditorGUIUtility.IconContent("Assembly Icon"), "IconButton"))
                {
                    string filePath = EditorUtility.OpenFilePanel("Select Assembly", EditorApplication.applicationPath, "*.dll");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        assembly.stringValue = filePath;
                        serializedObject.ApplyModifiedProperties();
                        GUI.SetNextControlName(null);
                    }
                }
            };

            SerializedProperty namespaces = serializedQuickFunc.FindPropertyRelative("namespaces");
            namespaceList = new ReorderableList(serializedObject, namespaces, true, false, true, true);
            namespaceList.elementHeight = 20;
            namespaceList.drawElementCallback += (position, index, isActive, isFocused) =>
            {
                SerializedProperty namespaceValue = namespaces.GetArrayElementAtIndex(index);
                position.y += 2;
                Rect fieldPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(fieldPosition, namespaceValue, new GUIContent("Namespace"));
            };
        }

        #region [IHasCustomMenu Implementation]
        /// <summary>
        /// Adds your custom menu items to an Editor Window.
        /// </summary>
        public void AddItemsToMenu(GenericMenu menu)
        {
            string modeLabel = mode == Mode.Template ? "Switch Code Mode" : "Switch Template Mode";
            menu.AddItem(new GUIContent(modeLabel), false, () =>
            {
                mode = mode == Mode.Template ? Mode.Code : Mode.Template;
                GUI.FocusControl(string.Empty);
                Repaint();
            });
        }
        #endregion

        #region [Static Methods]
        [MenuItem("Renowned Games/Quick Code/Quick Code Window", false, 70)]
        public static void Open()
        {
            QuickCodeWindow window = GetWindow<QuickCodeWindow>("Quick Code");
            window.minSize = new Vector2(300, 250);
            window.MoveToCenter();
            window.Show();
        }
        #endregion
    }
}
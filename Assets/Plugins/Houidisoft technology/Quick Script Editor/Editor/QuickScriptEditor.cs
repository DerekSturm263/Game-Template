
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace QuickScriptEditor{
public class QuickScriptEditor : EditorWindow
{
    private const string PrefsPathKey = "UITKScriptEditor_Path";
    private const string PrefsContentKey = "UITKScriptEditor_Content";
    private const string PrefsFontSizeKey = "UITKScriptEditor_FontSize";

    private TextField codeArea;
    private ScrollView codeScrollView;
    private ScrollView lineNumberScrollView;
    private VisualElement editorContainer;
    private VisualElement dropZone;
    private VisualElement lineNumberContainer;
    private Label fileNameLabel;

    private string currentPath;
    private string backupContent;
    private double lastUpdateTime;
    private int fontSize;

    private Stack<string> undoStack = new Stack<string>();
    private Stack<string> redoStack = new Stack<string>();
    private double lastEditTime;
    private const double UndoDelay = 0.3;

    [MenuItem("Window/Quick Script Editor")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<QuickScriptEditor>("Quick Script Editor");
        wnd.minSize = new Vector2(450, 350);
    }

    [MenuItem("Assets/Quick Edit this Script", false, 1)]
    public static void QuickEditSelectedScript()
    {
        var script = Selection.activeObject as MonoScript;
        if (script != null)
        {
            string path = AssetDatabase.GetAssetPath(script);
            var wnd = GetWindow<QuickScriptEditor>("Quick Script Editor");
            wnd.LoadTextFile(path);
        }
    }

    private void OnEnable()
    {
        string savedPath = EditorPrefs.GetString(PrefsPathKey, "");
        string savedContent = EditorPrefs.GetString(PrefsContentKey, "");
        fontSize = EditorPrefs.GetInt(PrefsFontSizeKey, 14);

        var root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Column;
        root.Clear();

        var toolbar = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                paddingLeft = 4,
                paddingTop = 4,
                paddingBottom = 4,
                paddingRight = 4,
                backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f)
            }
        };
        toolbar.Add(new Button(OpenFile) { text = "Open" });
        toolbar.Add(new Button(SaveFile) { text = "Save" });
        toolbar.Add(new Button(Restore) { text = "Restore" });
        toolbar.Add(new Button(CloseFile) { text = "Close" });
        toolbar.Add(new Button(() => ChangeFontSize(2)) { text = "A+" });
        toolbar.Add(new Button(() => ChangeFontSize(-2)) { text = "A-" });
        toolbar.Add(new Button(Undo) { text = "Undo" });
        toolbar.Add(new Button(Redo) { text = "Redo" });

        fileNameLabel = new Label("No file loaded")
        {
            style = {
                unityTextAlign = TextAnchor.MiddleLeft,
                marginLeft = 10,
                unityFontStyleAndWeight = FontStyle.Bold,
                color = Color.white
            }
        };
        toolbar.Add(fileNameLabel);
        root.Add(toolbar);

        dropZone = new Label("No file loaded. Drag & drop a .cs, .txt, .shader or .hlsl file here.")
        {
            style =
            {
                flexGrow = 1,
                unityTextAlign = TextAnchor.MiddleCenter,
                unityFontStyleAndWeight = FontStyle.Italic,
                color = new Color(0.7f, 0.7f, 0.7f),
                paddingTop = 20,
                paddingBottom = 20,
                paddingLeft = 10,
                paddingRight = 10
            }
        };
        dropZone.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
        dropZone.RegisterCallback<DragPerformEvent>(OnDragPerform);
        root.Add(dropZone);

        editorContainer = new VisualElement
        {
            style = { flexGrow = 1, flexDirection = FlexDirection.Row, display = DisplayStyle.None }
        };
        root.Add(editorContainer);

        lineNumberContainer = new VisualElement { style = { paddingTop = 4 } };
        lineNumberScrollView = new ScrollView(ScrollViewMode.Vertical)
        {
            style = { width = 40, overflow = Overflow.Hidden },
            verticalScrollerVisibility = ScrollerVisibility.Hidden,
            horizontalScrollerVisibility = ScrollerVisibility.Hidden
        };
        lineNumberScrollView.Add(lineNumberContainer);
        editorContainer.Add(lineNumberScrollView);

        codeArea = new TextField
        {
            multiline = true,
            style = { flexGrow = 1, paddingTop = 4, paddingLeft = 4, fontSize = fontSize }
        };
        codeArea.RegisterValueChangedCallback(OnCodeValueChanged);
        codeArea.RegisterCallback<KeyDownEvent>(OnKeyDown);

        codeScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
        {
            style = { flexGrow = 1, overflow = Overflow.Hidden },
            verticalScrollerVisibility = ScrollerVisibility.Hidden,
            horizontalScrollerVisibility = ScrollerVisibility.Hidden
        };
        codeScrollView.Add(codeArea);
        editorContainer.Add(codeScrollView);

        codeScrollView.schedule.Execute(() =>
        {
            var offset = codeScrollView.scrollOffset;
            lineNumberScrollView.scrollOffset = new Vector2(0, offset.y);
        }).Every(30);

        if (!string.IsNullOrEmpty(savedPath))
            LoadTextFile(savedPath, savedContent);
        else
            ShowEditor(false);
    }

    private void OnDisable()
    {
        if (!string.IsNullOrEmpty(currentPath))
        {
            EditorPrefs.SetString(PrefsPathKey, currentPath);
            EditorPrefs.SetString(PrefsContentKey, codeArea.value);
        }
        EditorPrefs.SetInt(PrefsFontSizeKey, fontSize);
    }

    private void OpenFile()
    {
        string path = EditorUtility.OpenFilePanel("Open File", "Assets", "cs,txt,shader,hlsl");
        if (!string.IsNullOrEmpty(path))
            LoadTextFile(path);
    }

    private void LoadTextFile(string path, string presetContent = null)
    {
        try
        {
            currentPath = path;
            backupContent = presetContent ?? File.ReadAllText(path);
            codeArea.value = backupContent;
            fileNameLabel.text = Path.GetFileName(currentPath);
            UpdateLineNumbers();
            ShowEditor(true);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load file: " + ex.Message);
        }
    }

    private void SaveFile()
    {
        if (string.IsNullOrEmpty(currentPath)) return;
        try
        {
            File.WriteAllText(currentPath, codeArea.value);
            if (currentPath.StartsWith(Application.dataPath))
                AssetDatabase.ImportAsset(GetRelativeAssetPath(currentPath));
            Debug.Log("File saved: " + currentPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save file: " + ex.Message);
        }
    }

    private void Restore()
    {
        if (!string.IsNullOrEmpty(backupContent))
        {
            codeArea.value = backupContent;
            UpdateLineNumbers();
        }
    }

    private void CloseFile()
    {
        currentPath = "";
        backupContent = "";
        codeArea.value = "";
        fileNameLabel.text = "No file loaded";
        lineNumberContainer.Clear();
        ShowEditor(false);
    }

    private void ChangeFontSize(int delta)
    {
        fontSize = Mathf.Clamp(fontSize + delta, 8, 48);
        codeArea.style.fontSize = fontSize;
        UpdateLineNumbers();
    }

    private void OnCodeValueChanged(ChangeEvent<string> evt)
    {
        double now = EditorApplication.timeSinceStartup;
        if (now - lastEditTime > UndoDelay)
        {
            undoStack.Push(evt.previousValue);
            redoStack.Clear();
        }
        lastEditTime = now;
        UpdateLineNumbers();
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.ctrlKey)
        {
            if (evt.keyCode == KeyCode.Z)
            {
                Undo();
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.Y)
            {
                Redo();
                evt.StopImmediatePropagation();
            }
        }
    }

    private void Undo()
    {
        if (undoStack.Count == 0) return;
        redoStack.Push(codeArea.value);
        codeArea.SetValueWithoutNotify(undoStack.Pop());
        UpdateLineNumbers();
    }

    private void Redo()
    {
        if (redoStack.Count == 0) return;
        undoStack.Push(codeArea.value);
        codeArea.SetValueWithoutNotify(redoStack.Pop());
        UpdateLineNumbers();
    }

    private void UpdateLineNumbers()
    {
        lineNumberContainer.Clear();
        var lines = codeArea.value.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var lbl = new Label((i + 1).ToString())
            {
                style = {
                    unityTextAlign = TextAnchor.MiddleRight,
                    height = fontSize + 4,
                    paddingRight = 4,
                    fontSize = fontSize
                }
            };
            lineNumberContainer.Add(lbl);
        }
    }

    private void ShowEditor(bool visible)
    {
        dropZone.style.display = visible ? DisplayStyle.None : DisplayStyle.Flex;
        editorContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static string GetRelativeAssetPath(string fullPath)
    {
        var dataPath = Application.dataPath;
        return fullPath.StartsWith(dataPath)
            ? "Assets" + fullPath.Substring(dataPath.Length)
            : fullPath;
    }

    private void OnDragUpdated(DragUpdatedEvent evt)
    {
        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
        {
            string ext = Path.GetExtension(DragAndDrop.paths[0]).ToLowerInvariant();
            if (ext == ".cs" || ext == ".txt" || ext == ".shader" || ext == ".hlsl")
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.StopPropagation();
            }
        }
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
        {
            string path = DragAndDrop.paths[0];
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".cs" || ext == ".txt" || ext == ".shader" || ext == ".hlsl")
            {
                LoadTextFile(path);
                DragAndDrop.AcceptDrag();
                evt.StopPropagation();
            }
        }
    }
}
}
using System.Collections.Generic;
using System.Linq;
#if UNITY_NETCODE_PRESENT
using Unity.Netcode;
#endif
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A custom editor window for finding NetworkObjects by their prefab ID hash.
/// </summary>
public class NetworkObjectFinderWindow : EditorWindow
{
    private const string NetcodeDefine = "UNITY_NETCODE_PRESENT";
    // Input field for the user to enter the PrefabIdHash to search for.
    private TextField m_GlobalObjectIdHashToFind;
    // ScrollView to display the list of found NetworkObjects
    private ScrollView m_ObjectListScrollView;
    // Container to hold the list of found NetworkObjects
    private VisualElement m_ObjectListContainer;
    private ObjectField gameObjectField;

    /// <summary>
    /// Adds a menu item to open the NetworkObject Finder window.
    /// </summary>
    [MenuItem("Window/Project Browser Pro/Tools/Find Network Objects")]
    public static void ShowNetworkObjectFinderWindow()
    {
        // Add define symbol if Unity.Netcode is present
        EnsureNetcodeDefine();
        // Create and display the NetworkObject Finder window.
        var editorWindow = EditorWindow.GetWindow<NetworkObjectFinderWindow>();
        editorWindow.titleContent = new GUIContent("NetworkObject Finder");
    }
#if UNITY_NETCODE_PRESENT

    /// <summary>
    /// Initializes the UI elements for the editor window.
    /// </summary>
    private void CreateGUI()
    {
        // Get the root visual element for this window.
        var root = rootVisualElement;

        // Create and set up the label and input field for the hash.
        var findLabel = new Label("NetworkObject-Hash:");
        m_GlobalObjectIdHashToFind = new TextField();

        // Create the button that triggers the search for a specific NetworkObject by hash.
        var findButton = new Button(OnButtonClick)
        {
            text = "Start Search",
            style =
            {
                flexDirection = FlexDirection.Row,
                marginBottom = 50 // Add space between buttons (adjust the value as needed)
            }
        };

        // Create the button that searches for all NetworkObjects in the scene.
        var findAllButton = new Button(OnFindAllButtonClick)
        {
            text = "Find All NetworkObjects in Project",

        };

        // Create a ScrollView for the list of found NetworkObjects.
        m_ObjectListScrollView = new ScrollView
        {
            style =
            {
                flexGrow = 1, // Allow the ScrollView to expand
                height = 400 // Set a fixed height for the ScrollView
            }
        };

        // Create a container for the list of found NetworkObjects.
        m_ObjectListContainer = new VisualElement();
        m_ObjectListScrollView.Add(m_ObjectListContainer); // Add the container to the ScrollView


        // Create an ObjectField for the NetworkPrefabsList slot
        gameObjectField = new ObjectField("DefaultPrefabList to add Gameobjects to")
        {
            objectType = typeof(NetworkPrefabsList)
        };

        // Assign the selected GameObject to the DefaultPrefabList field
        gameObjectField.RegisterValueChangedCallback(evt =>
        {
            DefaultPrefabList = (NetworkPrefabsList)evt.newValue;
        });

        // Add the ObjectField to the root visual element
        rootVisualElement.Add(gameObjectField);

        // Find the first NetworkPrefabsList in the project
        string[] guids = AssetDatabase.FindAssets("t:NetworkPrefabsList");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            NetworkPrefabsList networkPrefabsList = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(path);
            if (networkPrefabsList != null)
            {
                gameObjectField.value = networkPrefabsList;
            }
            else
            {
                Debug.LogError("Failed to load NetworkPrefabsList asset.");
            }
        }
        else
        {
            Debug.LogError("No NetworkPrefabsList found in the project.");
        }


        // Add the clear button to the root visual element

        // Add UI elements to the root.
        root.Add(findLabel);
        root.Add(m_GlobalObjectIdHashToFind);
        root.Add(findButton);
        root.Add(findAllButton);
        root.Add(m_ObjectListScrollView); // Add the ScrollView to the root
        root.Add(gameObjectField); // Add the GameObject slot to the GUI

    }

    public NetworkPrefabsList DefaultPrefabList;

    private void OnAddToNetworkPrefabListClick()
    {
        Debug.Log("OnAddToNetworkPrefabListClick called.");

        if (DefaultPrefabList == null)
        {
            Debug.LogWarning("No NetworkPrefabsList asset assigned.");
            return;
        }

        Debug.Log("NetworkPrefabsList is assigned.");

        foreach (var networkObject in networkObjectsFound)
        {
            Debug.Log($"Processing network object: {networkObject.name}");

            if (!DefaultPrefabList.PrefabList.Any(prefab => prefab.SourcePrefabGlobalObjectIdHash == networkObject.PrefabIdHash))
            {
                if (networkObject.gameObject.scene.isLoaded == false)
                {
                    DefaultPrefabList.Add(new NetworkPrefab { Prefab = networkObject.gameObject });
                    Debug.Log($"{networkObject.name} added to Network Prefab List.");
                }
            }
            else
            {
                Debug.LogWarning($"{networkObject.name} is null or already in the Network Prefab List.");
            }
        }
    }
    /// <summary>
    /// Callback method called when the specific find button is clicked.
    /// Searches for the NetworkObject by the specified hash.
    /// </summary>
    private void OnButtonClick()
    {
        // Clear the object list before searching
        m_ObjectListContainer.Clear();
        if (FindNetworkObjects(true) == null)
        {
            // List of possible messages to display when nothing is found
            string[] messages = {
                "Nothing was found :(",
                "Nope",
                "Better luck next time",
                "Try again",
                "Oops! The treasure chest was empty.",
                "Not today, friend.",
                "Still loading… Just kidding, there's nothing here.",
                "Your search results took a vacation.",
                "Welp, that was a wild goose chase.",
                "Error 404: Object Not Found.",
                "The network object ghosted you.",
                "A tumbleweed rolls by…",
                "Invisible mode activated? Try again!",
                "Hmm… maybe it’s hiding under the couch.",
                "Nope! But hey, nice try!",
                "The results ran off to join the circus.",
                "Nada, zilch, zip. Try one more time?",
                "Who needs it anyway?",
                "It was here a minute ago… or was it?"
            };
            // Randomly select one message from the list
            int randomIndex = Random.Range(0, messages.Length);
            string randomMessage = messages[randomIndex];

            // Create and set up the label with the selected random message
            var findLabel = new Label(randomMessage);
            m_ObjectListContainer.Add(findLabel); // Assuming you're adding the label to a container
            return;
        }
        else
        {
            CreateButtonForObject(networkObjectsFound[0]);
            // Button to add selected GameObject to the Network Prefab List
            m_ObjectListContainer.Add(new Button(OnAddToNetworkPrefabListClick)
            {
                text = "Add to Network Prefab List"
            });
        }


    }

    /// <summary>
    /// Callback method called when the list all button is clicked.
    /// Lists all instantiated NetworkObjects in the hierarchy.
    /// </summary>
    private void OnFindAllButtonClick()
    {
        // Clear the previous list before searching
        m_ObjectListContainer.Clear();

        // Find all NetworkObjects in the scene
        var allNetworkObjects = FindNetworkObjects(false); // Includes inactive objects

        // Check if any NetworkObjects are found
        if (allNetworkObjects.Count == 0)
        {
            m_ObjectListContainer.Add(new Label("No NetworkObjects found in the scene."));
            return;
        }

        // Create buttons for each found NetworkObject
        foreach (var networkObject in allNetworkObjects)
        {
            CreateButtonForObject(networkObject);
        }

        m_ObjectListContainer.Add(new Button(OnAddToNetworkPrefabListClick)
        {
            text = "Add all to Network Prefab List"
        });

    }

    private void CreateButtonForObject(NetworkObject networkObject)
    {
        // Create a button for each NetworkObject
        var button = new Button(() => SelectNetworkObject(networkObject))
        {
            style = { flexDirection = FlexDirection.Row }, // Align elements in a row

        };

        // Create a layout for the button to include the thumbnail and name
        var layout = new VisualElement();
        layout.style.flexDirection = FlexDirection.Row; // Align elements in a row

        // Display the mini thumbnail of the object
        Texture2D texture = AssetPreview.GetMiniThumbnail(networkObject.gameObject);
        if (texture != null)
        {
            var thumbnail = new Image
            {
                style =
                    {
                        width = 20, // Set width of the thumbnail
                        height = 20  // Set height of the thumbnail
                    }
            };
            thumbnail.image = texture; // Set the image property to the texture
            layout.Add(thumbnail); // Add thumbnail to the layout
        }

        // Create a label for the NetworkObject's name
        var label = new Label(networkObject.gameObject.name + " [" + networkObject.PrefabIdHash + "] ")
        {
            style =
                {
                    flexGrow = 1, // Allow the label to take up available space
                }
        };

        layout.Add(label); // Add label to the layout
        button.Add(layout); // Add the layout to the button

        // Add the button to the container
        m_ObjectListContainer.Add(button);
    }

    /// <summary>
    /// Selects the given NetworkObject in the Unity editor.
    /// </summary>
    /// <param name="networkObject">The NetworkObject to select.</param>
    private void SelectNetworkObject(NetworkObject networkObject)
    {
        Selection.activeObject = networkObject.gameObject; // Select the GameObject in the editor
        EditorGUIUtility.PingObject(networkObject.gameObject); // Ping the object in the project view
    }

    private List<NetworkObject> networkObjectsFound;
    private List<NetworkObject> FindNetworkObjects(bool singleSearch)
    {
        List<NetworkObject> networkObjects = new List<NetworkObject>();
        // Search through all prefabs in the Assets folder
        var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

        // Check each prefab in the Asset folder
        foreach (var prefabGUID in prefabGUIDs)
        {
            // Get the path from the GUID and load the prefab
            var path = AssetDatabase.GUIDToAssetPath(prefabGUID);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // Get the NetworkObject component from the prefab
            var networkObject = prefab.GetComponent<NetworkObject>();

            // If the prefab has a NetworkObject, check its hash
            if (networkObject != null)
            {
                if (networkObject.PrefabIdHash.ToString() == m_GlobalObjectIdHashToFind.text && singleSearch)
                {
                    // Select the prefab in the editor and log the result
                    Selection.activeObject = prefab;
                    networkObjectsFound = new List<NetworkObject>() { networkObject };
                    return networkObjectsFound; // Exit after finding the instantiated object
                }
                else
                {
                    networkObjects.Add(networkObject);
                }
            }
        }

        // Check all instantiated GameObjects in the hierarchy
        var allGameObjects = GameObject.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None); // Includes inactive objects

        foreach (var networkObject in allGameObjects)
        {
            // Compare the hash of each instantiated NetworkObject
            if (networkObject.PrefabIdHash.ToString() == m_GlobalObjectIdHashToFind.text && singleSearch)
            {
                // Select the instantiated object in the editor and log the result
                Selection.activeObject = networkObject.gameObject;
                networkObjectsFound = new List<NetworkObject>() { networkObject };
                return networkObjectsFound; // Exit after finding the instantiated object
            }
            else
            {
                networkObjects.Add(networkObject);
            }
        }
        networkObjectsFound = networkObjects;
        if (singleSearch) return null;
        return networkObjects;
    }
#else
    /// <summary>
    /// Displays a simple message when Unity Netcode is not installed.
    /// </summary>
    private void CreateGUI()
    {
        var root = rootVisualElement;
        var warningLabel = new Label("NGO (Netcode for GameObjects) is not installed.")
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                color = Color.red,
                marginTop = 10,
                marginLeft = 10
            }
        };

        root.Add(warningLabel);
    }
#endif
    /// <summary>
    /// Ensures the UNITY_NETCODE_PRESENT define symbol is added if Unity.Netcode is available.
    /// </summary>
    private static void EnsureNetcodeDefine()
    {
        // Check if Unity.Netcode is present
        bool netcodeInstalled = CompilationPipeline.GetAssemblies()
            .Any(assembly => assembly.name == "Unity.Netcode.Runtime");

        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            // Skip obsolete and unsupported BuildTargetGroup values
            if (group == BuildTargetGroup.Unknown || IsObsolete(group))
                continue;

            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

            if (netcodeInstalled && !defines.Contains(NetcodeDefine))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, $"{defines};{NetcodeDefine}");
            }
            else if (!netcodeInstalled && defines.Contains(NetcodeDefine))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines.Replace($";{NetcodeDefine}", "").Replace(NetcodeDefine, ""));
            }
        }
    }

    /// <summary>
    /// Checks if a BuildTargetGroup is obsolete.
    /// </summary>
    /// <param name="group">The BuildTargetGroup to check.</param>
    /// <returns>True if the BuildTargetGroup is obsolete; otherwise, false.</returns>
    private static bool IsObsolete(BuildTargetGroup group)
    {
        var attributes = typeof(BuildTargetGroup).GetField(group.ToString())
            ?.GetCustomAttributes(typeof(System.ObsoleteAttribute), false);
        return attributes != null && attributes.Length > 0;
    }
}

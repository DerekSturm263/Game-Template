using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOrganizerTool
{
    [Serializable]
    public class ExtensionMapping
    {
        public string Extension;
        public string TargetFolder;

        public ExtensionMapping(string ext, string folder)
        {
            Extension = ext;
            TargetFolder = folder;
        }
    }

    [CreateAssetMenu(menuName = "Project Organizer/Profile", fileName = "ProjectOrganizerProfile")]
    public class ProjectOrganizerProfile : ScriptableObject
    {
        [Header("Mappings")]
        public List<ExtensionMapping> ExtensionMappings = new()
        {
            new ExtensionMapping(".cs", "Assets/Scripts/Core"),
            new ExtensionMapping(".shader", "Assets/Shaders"),
            new ExtensionMapping(".mat", "Assets/Materials/Environment"),
            new ExtensionMapping(".prefab", "Assets/Prefabs/Props"),
            new ExtensionMapping(".unity", "Assets/Scenes/Levels"),
            new ExtensionMapping(".png", "Assets/Art/Textures"),
            new ExtensionMapping(".jpg", "Assets/Art/Textures"),
            new ExtensionMapping(".jpeg", "Assets/Art/Textures"),
            new ExtensionMapping(".tga", "Assets/Art/Textures"),
            new ExtensionMapping(".psd", "Assets/Art/Textures"),
            new ExtensionMapping(".gif", "Assets/Art/Textures"),
            new ExtensionMapping(".mp3", "Assets/Audio/SFX"),
            new ExtensionMapping(".wav", "Assets/Audio/SFX"),
            new ExtensionMapping(".ogg", "Assets/Audio/SFX"),
            new ExtensionMapping(".aiff", "Assets/Audio/SFX"),
            new ExtensionMapping(".flac", "Assets/Audio/SFX"),
            new ExtensionMapping(".ttf", "Assets/UI/Fonts"),
            new ExtensionMapping(".otf", "Assets/UI/Fonts"),
            new ExtensionMapping(".mp4", "Assets/Video"),
            new ExtensionMapping(".avi", "Assets/Video"),
            new ExtensionMapping(".mov", "Assets/Video"),
            new ExtensionMapping(".asset", "Assets/Resources"),
        };

        [Header("Scope")]
        public List<string> IncludeFolders = new() { "Assets" };

        public List<string> ExcludeFolders = new()
        {
            "Assets/ProjectSettings",
            "Assets/Packages",
            "Assets/Library",
            "Assets/Logs",
            "Assets/obj",
            "Assets/.git",
            "Assets/.vs",
            "Assets/Temp",
            "Assets/UserSettings",
            "Assets/TextMesh Pro",
            "Assets/TMP",
            "Assets/ExternalAssets",
            "Assets/Plugins",
            "Assets/StreamingAssets",
            "Assets/Gizmos",
            "Assets/Editor Default Resources",
            "Assets/Standard Assets"
        };

        [Header("Scan")]
        public string ExtensionsToScan =
            ".cs,.shader,.prefab,.unity,.mat,.png,.jpg,.jpeg,.tga,.psd,.gif,.mp3,.wav,.ogg,.aiff,.flac,.ttf,.otf,.mp4,.avi,.mov,.asset";

        [Header("Template Folders (created before organize)")]
        public List<string> TemplateFolders = new()
        {
            "Art/Sprites", "Art/Textures", "Art/Animations", "Art/Characters", "Art/Environment", "Art/UI",
            "Audio/Music", "Audio/SFX", "Audio/Voices",
            "Materials/Characters", "Materials/Environment",
            "Prefabs/Characters", "Prefabs/Enemies", "Prefabs/Environment", "Prefabs/Props", "Prefabs/UI",
            "Scenes/Main", "Scenes/Levels", "Scenes/UI",
            "Scripts/Core", "Scripts/Managers", "Scripts/Gameplay", "Scripts/Player", "Scripts/Enemies", "Scripts/UI",
            "Scripts/Utils",
            "Shaders", "UI/Images", "UI/Fonts", "UI/Prefabs", "Plugins", "Resources", "Editor", "StreamingAssets",
            "Video", "Misc"
        };
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOrganizerTool
{
    [Serializable]
    public class ProjectOrganizerConfigMapping
    {
        public string Extension;
        public string TargetFolder;

        public ProjectOrganizerConfigMapping(string ext, string folder)
        {
            Extension = ext;
            TargetFolder = folder;
        }
    }

    // NOTE: ce ScriptableObject n'est pas utilisé par la Window actuelle.
    // Il reste ici seulement si tu t'en sers ailleurs. Sinon: supprime carrément ce fichier.
    [CreateAssetMenu(menuName = "Project Organizer/Config", fileName = "ProjectOrganizerConfig")]
    public class ProjectOrganizerConfig : ScriptableObject
    {
        public List<ProjectOrganizerConfigMapping> ExtensionMappings = new()
        {
            new ProjectOrganizerConfigMapping(".cs", "Assets/Scripts/Core"),
            new ProjectOrganizerConfigMapping(".shader", "Assets/Shaders"),
            new ProjectOrganizerConfigMapping(".mat", "Assets/Materials/Environment"),
            new ProjectOrganizerConfigMapping(".prefab", "Assets/Prefabs/Props"),
            new ProjectOrganizerConfigMapping(".unity", "Assets/Scenes/Levels"),
            new ProjectOrganizerConfigMapping(".png", "Assets/Art/Textures"),
            new ProjectOrganizerConfigMapping(".jpg", "Assets/Art/Textures"),
            new ProjectOrganizerConfigMapping(".jpeg", "Assets/Art/Textures"),
            new ProjectOrganizerConfigMapping(".tga", "Assets/Art/Textures"),
            new ProjectOrganizerConfigMapping(".psd", "Assets/Art/Textures"),
            new ProjectOrganizerConfigMapping(".gif", "Assets/Art/Textures"),
            new ProjectOrganizerConfigMapping(".mp3", "Assets/Audio/SFX"),
            new ProjectOrganizerConfigMapping(".wav", "Assets/Audio/SFX"),
            new ProjectOrganizerConfigMapping(".ogg", "Assets/Audio/SFX"),
            new ProjectOrganizerConfigMapping(".aiff", "Assets/Audio/SFX"),
            new ProjectOrganizerConfigMapping(".flac", "Assets/Audio/SFX"),
            new ProjectOrganizerConfigMapping(".ttf", "Assets/UI/Fonts"),
            new ProjectOrganizerConfigMapping(".otf", "Assets/UI/Fonts"),
            new ProjectOrganizerConfigMapping(".mp4", "Assets/Video"),
            new ProjectOrganizerConfigMapping(".avi", "Assets/Video"),
            new ProjectOrganizerConfigMapping(".mov", "Assets/Video"),
            new ProjectOrganizerConfigMapping(".asset", "Assets/Resources"),
        };

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

        public string ExtensionsToScan =
            ".cs,.shader,.prefab,.unity,.mat,.png,.jpg,.jpeg,.tga,.psd,.gif,.mp3,.wav,.ogg,.aiff,.flac,.ttf,.otf,.mp4,.avi,.mov,.asset";

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

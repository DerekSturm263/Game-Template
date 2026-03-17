using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectOrganizerTool
{
    public static class ProjectOrganizerProfileTemplatesGenerator
    {
        public const string ProfilesFolder = "Assets/ProjectOrganizer/Profiles";

        public static void GenerateAll()
        {
            EnsureFolderExists(ProfilesFolder);
            GenerateBasic_Internal();
            Generate2D_Internal();
            Generate3D_Internal();
            GenerateJam_Internal();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateBasic()
        {
            EnsureFolderExists(ProfilesFolder);
            GenerateBasic_Internal();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void Generate2D()
        {
            EnsureFolderExists(ProfilesFolder);
            Generate2D_Internal();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void Generate3D()
        {
            EnsureFolderExists(ProfilesFolder);
            Generate3D_Internal();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateJam()
        {
            EnsureFolderExists(ProfilesFolder);
            GenerateJam_Internal();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ---------------- INTERNAL ----------------

        private static void GenerateBasic_Internal()
        {
            CreateOrReplaceProfile(
                "PO_Profile_Basic_Project",
                includeFolders: new List<string> { "Assets" },
                excludeFolders: new List<string> { "Assets/Plugins", "Assets/ThirdParty" },
                extensionsToScan:
                    ".unity,.prefab,.cs,.asset,.png,.jpg,.jpeg,.tga,.psd,.fbx,.wav,.mp3,.ogg,.mat,.shader,.shadergraph,.controller,.anim,.overridecontroller,.mp4,.txt,.json,.bytes",
                templateFolders: new List<string>
                {
                    "Art",
                    "Art/Textures",
                    "Art/Models",
                    "Art/Materials",
                    "Art/Shaders",
                    "Audio",
                    "Audio/SFX",
                    "Audio/Music",
                    "Prefabs",
                    "Scenes",
                    "Scripts",
                    "Resources",
                    "VFX",
                    "Animations",
                    "Data"
                },
                mappings: new (string ext, string folder)[]
                {
                    (".unity", "Assets/Scenes"),
                    (".prefab", "Assets/Prefabs"),
                    (".cs", "Assets/Scripts"),
                    (".asset", "Assets/Data"),

                    (".png", "Assets/Art/Textures"),
                    (".jpg", "Assets/Art/Textures"),
                    (".jpeg", "Assets/Art/Textures"),
                    (".tga", "Assets/Art/Textures"),
                    (".psd", "Assets/Art/Textures"),

                    (".fbx", "Assets/Art/Models"),
                    (".mat", "Assets/Art/Materials"),
                    (".shader", "Assets/Art/Shaders"),
                    (".shadergraph", "Assets/Art/Shaders"),

                    (".anim", "Assets/Animations"),
                    (".controller", "Assets/Animations"),
                    (".overridecontroller", "Assets/Animations"),

                    (".wav", "Assets/Audio/SFX"),
                    (".mp3", "Assets/Audio/Music"),
                    (".ogg", "Assets/Audio/Music"),

                    (".mp4", "Assets/Art"),
                    (".json", "Assets/Data"),
                    (".txt", "Assets/Data"),
                    (".bytes", "Assets/Data"),
                }
            );
        }

        private static void Generate2D_Internal()
        {
            CreateOrReplaceProfile(
                "PO_Profile_2D_Game",
                includeFolders: new List<string> { "Assets" },
                excludeFolders: new List<string> { "Assets/Plugins", "Assets/ThirdParty" },
                extensionsToScan:
                    ".unity,.prefab,.cs,.asset,.png,.psd,.jpg,.jpeg,.anim,.controller,.overridecontroller,.mat,.shader,.shadergraph,.wav,.mp3,.ogg,.ttf,.otf",
                templateFolders: new List<string>
                {
                    "Scenes",
                    "Prefabs",
                    "Scripts",
                    "Art",
                    "Art/Sprites",
                    "Art/Tilesets",
                    "Art/Atlases",
                    "Art/Materials",
                    "Art/Shaders",
                    "Animations",
                    "Audio",
                    "Audio/SFX",
                    "Audio/Music",
                    "UI",
                    "UI/Fonts",
                    "Data",
                    "Resources"
                },
                mappings: new (string ext, string folder)[]
                {
                    (".unity", "Assets/Scenes"),
                    (".prefab", "Assets/Prefabs"),
                    (".cs", "Assets/Scripts"),
                    (".asset", "Assets/Data"),

                    (".png", "Assets/Art/Sprites"),
                    (".jpg", "Assets/Art/Sprites"),
                    (".jpeg", "Assets/Art/Sprites"),
                    (".psd", "Assets/Art/Sprites"),

                    (".mat", "Assets/Art/Materials"),
                    (".shader", "Assets/Art/Shaders"),
                    (".shadergraph", "Assets/Art/Shaders"),

                    (".anim", "Assets/Animations"),
                    (".controller", "Assets/Animations"),
                    (".overridecontroller", "Assets/Animations"),

                    (".wav", "Assets/Audio/SFX"),
                    (".mp3", "Assets/Audio/Music"),
                    (".ogg", "Assets/Audio/Music"),

                    (".ttf", "Assets/UI/Fonts"),
                    (".otf", "Assets/UI/Fonts"),
                }
            );
        }

        private static void Generate3D_Internal()
        {
            CreateOrReplaceProfile(
                "PO_Profile_3D_Game",
                includeFolders: new List<string> { "Assets" },
                excludeFolders: new List<string> { "Assets/Plugins", "Assets/ThirdParty" },
                extensionsToScan:
                    ".unity,.prefab,.cs,.asset,.fbx,.blend,.png,.jpg,.jpeg,.tga,.exr,.hdr,.mat,.shader,.shadergraph,.anim,.controller,.overridecontroller,.wav,.mp3,.ogg,.rendertexture,.vfx",
                templateFolders: new List<string>
                {
                    "Scenes",
                    "Prefabs",
                    "Scripts",
                    "Art",
                    "Art/Models",
                    "Art/Textures",
                    "Art/Materials",
                    "Art/Shaders",
                    "Art/Lighting",
                    "Animations",
                    "VFX",
                    "Audio",
                    "Audio/SFX",
                    "Audio/Music",
                    "UI",
                    "Data",
                    "Resources"
                },
                mappings: new (string ext, string folder)[]
                {
                    (".unity", "Assets/Scenes"),
                    (".prefab", "Assets/Prefabs"),
                    (".cs", "Assets/Scripts"),
                    (".asset", "Assets/Data"),

                    (".fbx", "Assets/Art/Models"),
                    (".blend", "Assets/Art/Models"),

                    (".png", "Assets/Art/Textures"),
                    (".jpg", "Assets/Art/Textures"),
                    (".jpeg", "Assets/Art/Textures"),
                    (".tga", "Assets/Art/Textures"),
                    (".exr", "Assets/Art/Textures"),
                    (".hdr", "Assets/Art/Textures"),

                    (".mat", "Assets/Art/Materials"),
                    (".shader", "Assets/Art/Shaders"),
                    (".shadergraph", "Assets/Art/Shaders"),

                    (".anim", "Assets/Animations"),
                    (".controller", "Assets/Animations"),
                    (".overridecontroller", "Assets/Animations"),

                    (".vfx", "Assets/VFX"),
                    (".rendertexture", "Assets/Art/Lighting"),

                    (".wav", "Assets/Audio/SFX"),
                    (".mp3", "Assets/Audio/Music"),
                    (".ogg", "Assets/Audio/Music"),
                }
            );
        }

        private static void GenerateJam_Internal()
        {
            CreateOrReplaceProfile(
                "PO_Profile_Jam_Setup",
                includeFolders: new List<string> { "Assets" },
                excludeFolders: new List<string>(),
                extensionsToScan: ".unity,.prefab,.cs,.png,.jpg,.jpeg,.fbx,.wav,.mp3,.ogg,.mat,.asset",
                templateFolders: new List<string>
                {
                    "Scenes",
                    "Scripts",
                    "Art",
                    "Audio",
                    "Prefabs",
                    "Data"
                },
                mappings: new (string ext, string folder)[]
                {
                    (".unity", "Assets/Scenes"),
                    (".cs", "Assets/Scripts"),
                    (".prefab", "Assets/Prefabs"),
                    (".png", "Assets/Art"),
                    (".jpg", "Assets/Art"),
                    (".jpeg", "Assets/Art"),
                    (".fbx", "Assets/Art"),
                    (".mat", "Assets/Art"),
                    (".wav", "Assets/Audio"),
                    (".mp3", "Assets/Audio"),
                    (".ogg", "Assets/Audio"),
                    (".asset", "Assets/Data"),
                }
            );
        }

        private static void CreateOrReplaceProfile(
            string assetName,
            List<string> includeFolders,
            List<string> excludeFolders,
            string extensionsToScan,
            List<string> templateFolders,
            IEnumerable<(string ext, string folder)> mappings)
        {
            var path = $"{ProfilesFolder}/{assetName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ProjectOrganizerProfile>(path);

            ProjectOrganizerProfile profile;
            if (existing == null)
            {
                profile = ScriptableObject.CreateInstance<ProjectOrganizerProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            else
            {
                profile = existing;
            }

            Undo.RecordObject(profile, "Generate Project Organizer Profile");

            profile.IncludeFolders = includeFolders ?? new List<string> { "Assets" };
            profile.ExcludeFolders = excludeFolders ?? new List<string>();
            profile.ExtensionsToScan = extensionsToScan ?? "";
            profile.TemplateFolders = templateFolders ?? new List<string>();

            profile.ExtensionMappings = (mappings ?? Enumerable.Empty<(string ext, string folder)>())
                .Where(m => !string.IsNullOrWhiteSpace(m.ext) && !string.IsNullOrWhiteSpace(m.folder))
                .Select(m => new ExtensionMapping(NormalizeExt(m.ext), NormalizeUnityFolder(m.folder)))
                .ToList();

            EditorUtility.SetDirty(profile);
        }

        private static string NormalizeExt(string ext)
        {
            ext = (ext ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) return "";
            if (!ext.StartsWith(".")) ext = "." + ext;
            return ext;
        }

        private static string NormalizeUnityFolder(string p)
        {
            p = (p ?? "").Replace("\\", "/").Trim();
            if (p.EndsWith("/")) p = p.TrimEnd('/');

            if (string.IsNullOrEmpty(p)) return "Assets/Misc";
            if (p == "Assets") return "Assets/Misc";
            if (!p.StartsWith("Assets/")) return "Assets/Misc";

            return p;
        }

        private static void EnsureFolderExists(string unityFolder)
        {
            unityFolder = (unityFolder ?? "").Replace("\\", "/").Trim().TrimEnd('/');
            if (string.IsNullOrEmpty(unityFolder)) return;
            if (AssetDatabase.IsValidFolder(unityFolder)) return;

            var parts = unityFolder.Split('/');
            var current = parts[0]; // "Assets"

            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}

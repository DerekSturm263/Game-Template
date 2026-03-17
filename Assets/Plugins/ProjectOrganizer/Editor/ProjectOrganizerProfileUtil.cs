// ReSharper disable CheckNamespace
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectOrganizerTool
{
    static class ProjectOrganizerProfileUtil
    {
        public static ProjectOrganizerProfile GetSelectedProfile()
        {
            var guid = ProjectOrganizerPrefs.SelectedGuid;
            if (string.IsNullOrEmpty(guid)) return null;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            return AssetDatabase.LoadAssetAtPath<ProjectOrganizerProfile>(path);
        }

        public static void SetSelectedProfile(ProjectOrganizerProfile profile)
        {
            if (profile == null)
            {
                ProjectOrganizerPrefs.SelectedGuid = string.Empty;
                return;
            }

            var path = AssetDatabase.GetAssetPath(profile);
            ProjectOrganizerPrefs.SelectedGuid = string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
        }

        public static ProjectOrganizerProfile CreateProfileAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Project Organizer Profile",
                "ProjectOrganizerProfile",
                "asset",
                "Choose location for the profile asset");

            if (string.IsNullOrEmpty(path)) return null;

            var asset = ScriptableObject.CreateInstance<ProjectOrganizerProfile>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);

            SetSelectedProfile(asset);
            return asset;
        }
    }
}
#endif
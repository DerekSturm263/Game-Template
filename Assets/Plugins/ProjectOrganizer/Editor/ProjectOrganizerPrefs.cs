// ReSharper disable CheckNamespace
#if UNITY_EDITOR
using UnityEditor;

namespace ProjectOrganizerTool
{
    static class ProjectOrganizerPrefs
    {
        const string Key = "ProjectOrganizerTool.SelectedProfileGuid";

        public static string SelectedGuid
        {
            get => EditorPrefs.GetString(Key, string.Empty);
            set => EditorPrefs.SetString(Key, value ?? string.Empty);
        }
    }
}
#endif
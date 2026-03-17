/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Tamerlan Shakirov
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

using UnityEditor;
using UnityEngine;

namespace RenownedGames.ExLibEditor
{
    public class SettingsSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        /// <summary>
        /// Current settings asset in project.
        /// </summary>
        public static T GetCurrent()
        {
            string GUID = $"{typeof(T).Name} Singleton Asset";
            if (!EditorBuildSettings.TryGetConfigObject<T>(GUID, out T settings))
            {
                T[] allSettings = ProjectDatabase.LoadAll<T>();
                if(allSettings.Length > 0)
                {
                    settings = allSettings[0];
                    if(allSettings.Length > 1) 
                    {
                        Debug.LogWarning($"There is more than one <b>{GUID}</b> found in the project. The first <b>{GUID}</b> found will be used. Delete the extra <b>{GUID}s</b>.");
                    }
                }
                else
                {
                    settings = CreateInstance<T>();
                    settings.name = typeof(T).Name;
                    string path = AssetDatabase.GenerateUniqueAssetPath($"Assets/{settings.name}.asset");
                    AssetDatabase.CreateAsset(settings, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                EditorBuildSettings.AddConfigObject(GUID, settings, true);
            }
            return settings;
        }
    }
}
/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Tamerlan Shakirov
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RenownedGames.ExLibEditor
{
    public static class ProjectDatabase
    {
        /// <summary>
        /// Load all assets of type T.
        /// </summary>
        public static T[] LoadAll<T>() where T : Object
        {
            List<T> assets = new List<T>();

            string[] paths = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                T asset = AssetDatabase.LoadAssetAtPath<T>(GetRelativePath(path));
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets.ToArray();
        }

        /// <summary>
        /// Enumerate all assets of type T.
        /// </summary>
        public static IEnumerable<T> EnumerateAssets<T>() where T : Object
        {
            foreach(string path in Directory.EnumerateFiles(Application.dataPath, "*", SearchOption.AllDirectories))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(GetRelativePath(path));
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }

        /// <summary>
        /// Get relative path from absolute path.
        /// </summary>
        /// <param name="path">Absolute path to asset.</param>
        public static string GetRelativePath(string path)
        {
            return path.Remove(0, path.IndexOf("Assets"));
        }
    }
}
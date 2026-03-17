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
    public static class EditorResources
    {
        /// <summary>
        /// Returns the first asset object of type type at given path assetPath.
        /// </summary>
        /// <param name="assetPath">Relative path of the asset to load.</param>
        /// <param name="type">Data type of the asset.</param>
        /// <returns>The asset matching the parameters.</returns>
        public static Object Load(string assetPath, System.Type type)
        {
            string[] paths = GetAllPaths();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                path = Path.Combine(path, assetPath);
                if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Object asset = AssetDatabase.LoadAssetAtPath(path, type);
                    if (asset != null)
                    {
                        return asset;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the first asset object of type type at given path assetPath.
        /// </summary>
        /// <param name="assetPath">Relative path of the asset to load.</param>
        /// <returns>The asset matching the parameters.</returns>
        public static T Load<T>(string assetPath) where T : Object
        {
            string[] paths = GetAllPaths();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                path = Path.Combine(path, assetPath);
                if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (asset != null)
                    {
                        return asset;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the array of the assets at given path assetPath.
        /// </summary>
        /// <param name="assetsPath">Relative path of the assets to load.</param>
        /// <returns>The asset matching the parameters.</returns>
        public static T[] LoadAll<T>(string assetsPath, SearchOption searchOption) where T : Object
        {
            List<T> assets = new List<T>();
            string[] paths = GetAllPaths();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                path = Path.Combine(path, assetsPath);
                if (Directory.Exists(path))
                {
                    string[] filePaths = Directory.GetFiles(path, "*", searchOption);
                    for (int j = 0; j < filePaths.Length; j++)
                    {
                        T asset = AssetDatabase.LoadAssetAtPath<T>(filePaths[j]);
                        if (asset != null)
                        {
                            assets.Add(asset);
                        }
                    }
                }
            }
            return assets.ToArray();
        }

        /// <summary>
        /// Get all paths until editor resources, relative project.
        /// </summary>
        public static string[] GetAllPaths()
        {
            string[] paths = Directory.GetDirectories(Application.dataPath, "EditorResources", SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = ProjectDatabase.GetRelativePath(paths[i]);
            }
            return paths;
        }
    }
}
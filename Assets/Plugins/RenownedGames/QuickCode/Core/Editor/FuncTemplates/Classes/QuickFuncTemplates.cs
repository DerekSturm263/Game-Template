/* ================================================================
   ---------------------------------------------------
   Project   :    Quick Code
   Publisher :    Renowned Games
   Author    :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using RenownedGames.ExLibEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RenownedGames.QuickCodeEditor
{
    public sealed class QuickFuncTemplates : ScriptableObject
    {
        [SerializeField]
        private List<Template> templates = new List<Template>();

        public void AddTemplate(Template template)
        {
            for (int i = 0; i < templates.Count; i++)
            {
                Template _template = templates[i];
                if(_template.GetName() == template.GetName())
                {
                    if(EditorUtility.DisplayDialog("Quick Code", "A template with this name is already in the list, do you want to overwrite this template?", "Yes", "No"))
                    {
                        templates[i] = template;
                    }
                    return;
                }
            }
            templates.Add(template);
        }

        public void RemoveTemplate(Template template)
        {
            templates.Remove(template);
        }

        public void RemoveTemplate(int index)
        {
            templates.RemoveAt(index);
        }

        public Template GetTemplate(int index)
        {
            return templates[index];
        }

        public int GetTemplateCount()
        {
            return templates.Count;
        }

        public IEnumerable<Template> Templates
        {
            get
            {
                return templates;
            }
        }

        #region [Static Methods]
        /// <summary>
        /// Get current apex settings asset.
        /// </summary>
        public static QuickFuncTemplates Current
        {
            get
            {
                const string EDITOR_BUILD_SETTINGS_GUID = "QuickFunc Templates Asset";
                if (!EditorBuildSettings.TryGetConfigObject<QuickFuncTemplates>(EDITOR_BUILD_SETTINGS_GUID, out QuickFuncTemplates settings))
                {
                    settings = ProjectDatabase
                        .LoadAll<QuickFuncTemplates>()
                        .Where(t => t is QuickFuncTemplates)
                        .FirstOrDefault();

                    if (settings == null)
                    {
                        settings = CreateInstance<QuickFuncTemplates>();
                        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/QuickFuncTemplates.asset");
                        AssetDatabase.CreateAsset(settings, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    EditorBuildSettings.AddConfigObject(EDITOR_BUILD_SETTINGS_GUID, settings, true);
                }
                return settings;
            }
        }
        #endregion
    }
}

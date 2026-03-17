using System.Collections.Generic;
using UnityEngine;

namespace ProjectOrganizerTool
{
    [CreateAssetMenu(menuName = "Project Organizer/Template Preset", fileName = "ProjectOrganizerTemplatePreset")]
    public class ProjectOrganizerTemplatePreset : ScriptableObject
    {
        public string PresetName = "New Preset";

        [Header("Folders created before organize")]
        public List<string> TemplateFolders = new();

        [Header("Extension → Target Folder")]
        public List<ExtensionMapping> ExtensionMappings = new();

        [Header("Scope")]
        public List<string> IncludeFolders = new() { "Assets" };
        public List<string> ExcludeFolders = new();
        public string ExtensionsToScan = "";
    }
}
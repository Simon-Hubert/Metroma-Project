using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Metroma.CameraTool.Editor
{
    public static class FocusCamMigrationTool
    {
        // TARGET GUIDS (NEW ONES)
        private const string NEW_CLIP_GUID = "f03002f66b96a264398af1f5776b3e7a";
        private const string NEW_TRACK_GUID = "2c5babc2f4880d24bace57156ddc1380";

        // MAPPING: OLD GUID -> NEW GUID
        private static readonly Dictionary<string, string> GUID_MAPPING = new Dictionary<string, string>
        {
            // Old Clips
            { "2958cf08d9048bc4a88dc2a74e9566db", NEW_CLIP_GUID },
            { "37c1e8e2b308b69459a7cea92efd65e5", NEW_CLIP_GUID },
            
            // Old Tracks
            { "278a5430cbefddd48a37dceff7b9ea03", NEW_TRACK_GUID },
            { "64112e4f07a760c40a59972304856012", NEW_TRACK_GUID }
        };

        [MenuItem("Metroma/Camera/🧪 Migrate FocusCam Timelines (Repair)", false, 100)]
        public static void MigrateTimelines()
        {
            string[] guids = AssetDatabase.FindAssets("t:TimelineAsset");
            Debug.Log($"<color=cyan>[Migration]</color> Found {guids.Length} TimelineAssets. Starting Surgical Repair...");

            int totalFixedFiles = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".playable")) continue;

                string content = File.ReadAllText(path);
                bool changed = false;

                // 1. Surgical GUID Replacement
                foreach (var entry in GUID_MAPPING)
                {
                    if (content.Contains(entry.Key))
                    {
                        content = content.Replace(entry.Key, entry.Value);
                        changed = true;
                    }
                }

                // 2. Assembly Redirect (Namespace repair)
                string[] oldIdentifiers = { 
                    "Metroma.FocusCam::Metroma.FocusCam.FocusCamClip",
                    "Metroma.FocusCam::Metroma.FocusCam.FocusCamTrack"
                };
                
                foreach (var oldId in oldIdentifiers)
                {
                    if (content.Contains(oldId))
                    {
                        string newId = oldId.Replace("Metroma.FocusCam::", "Metroma.CameraTool::");
                        content = content.Replace(oldId, newId);
                        changed = true;
                    }
                }
                
                // Fallback catch-all for any missed assembly headers
                if (content.Contains("Metroma.FocusCam::"))
                {
                    content = content.Replace("Metroma.FocusCam::", "Metroma.CameraTool::");
                    changed = true;
                }

                if (changed)
                {
                    File.WriteAllText(path, content);
                    totalFixedFiles++;
                    Debug.Log($"<color=green>[Migration]</color> Surgical repair successful: {path}");
                }
            }

            if (totalFixedFiles > 0)
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("FocusCam Migration", 
                    $"Surgical repair complete.\nFixed {totalFixedFiles} playable files.\n\nYour timelines should be fully restored now.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("FocusCam Migration", "No legacy references found. Everything seems to be up to date.", "OK");
            }
        }
    }
}

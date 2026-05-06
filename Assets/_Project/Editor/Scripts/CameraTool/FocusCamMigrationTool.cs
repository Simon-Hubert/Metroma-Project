using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;
using Metroma.FocusCam;

namespace Metroma.CameraTool.Editor
{
    public static class FocusCamMigrationTool
    {
        // GUIDs of the scripts to relink if lost
        private const string CLIP_SCRIPT_GUID = "f03002f66b96a264398af1f5776b3e7a";
        private const string TRACK_SCRIPT_GUID = "2c5babc2f4880d24bace57156ddc1380";

        [MenuItem("Metroma/Camera/🧪 Migrate FocusCam Timelines (Repair)", false, 100)]
        public static void MigrateTimelines()
        {
            string[] guids = AssetDatabase.FindAssets("t:TimelineAsset");
            int totalFixed = 0;

            MonoScript clipScript = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(CLIP_SCRIPT_GUID));
            MonoScript trackScript = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(TRACK_SCRIPT_GUID));

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
                if (timeline == null) continue;

                bool timelineDirty = false;

                // Unity stores sub-assets (tracks/clips) inside the TimelineAsset
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object asset in allAssets)
                {
                    if (asset == null) continue;

                    // Use SerializedObject to check for missing scripts
                    SerializedObject so = new SerializedObject(asset);
                    SerializedProperty scriptProp = so.FindProperty("m_Script");

                    if (scriptProp != null && scriptProp.objectReferenceValue == null)
                    {
                        // Potential missing script. Let's check the type name if possible.
                        // However, if the script is missing, the type is lost. 
                        // We rely on the fact that the object might still have its fields.
                        
                        string assetName = asset.name;
                        if (assetName.Contains("FocusCamTrack") || asset is TrackAsset) // Rough check
                        {
                            if (trackScript != null)
                            {
                                scriptProp.objectReferenceValue = trackScript;
                                so.ApplyModifiedPropertiesWithoutUndo();
                                timelineDirty = true;
                                totalFixed++;
                            }
                        }
                        else if (assetName.Contains("FocusCamClip") || asset is PlayableAsset)
                        {
                            if (clipScript != null)
                            {
                                scriptProp.objectReferenceValue = clipScript;
                                so.ApplyModifiedPropertiesWithoutUndo();
                                timelineDirty = true;
                                totalFixed++;
                            }
                        }
                    }
                }

                if (timelineDirty)
                {
                    EditorUtility.SetDirty(timeline);
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("FocusCam Migration", 
                $"Scan complete. Fixed {totalFixed} potential broken references.\n\nNote: If some clips are still broken, they might need manual re-assignment or the tracks might be fully corrupted.", "OK");
        }
    }
}

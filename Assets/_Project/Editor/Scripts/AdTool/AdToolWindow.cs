using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Metroma.Editor
{
    public class AdToolWindow : EditorWindow
    {
        private AdBase _ad;
        private SerializedObject _adObj;
        private SerializedProperty _blocks;
        private ReorderableList _reorderableList;
        
        [MenuItem("Tools/AdTool")]
        private static void ShowWindow()
        {
            GetWindow<AdToolWindow>("AdTool");
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject.TryGetComponent<AdBase>(out AdBase ad))
            {
                if (_ad == null || ad != _ad)
                {
                    _ad = ad;
                    Init();
                    Repaint();
                }
            }
            else
            {
                Debug.LogWarning("No ad component selected");
            }
        }
        
        private void Init()
        {
            _adObj = new SerializedObject(_ad);
            _blocks = _adObj.FindProperty("_blocks");
            _reorderableList = new ReorderableList(_adObj, _blocks);

            _reorderableList.drawElementCallback = DrawElementOfList;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            if (_ad == null)
            {
                EditorGUILayout.HelpBox("Sélectionnez un GameObject contenant un composant AdBase dans la scène.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Cible Actuelle : {_ad.gameObject.name}", EditorStyles.boldLabel);
            
            GUILayout.Space(10);

            if (GUILayout.Button("Auto-Fetch Child Blocks", GUILayout.Height(30)))
            {
                Undo.RecordObject(_ad, "Fetch Blocks");
                _ad.AutoFetchChildBlocks();
                EditorUtility.SetDirty(_ad);
                Init();
            }

            GUILayout.Space(10);

            if (_adObj != null)
            {
                _adObj.Update();
                _reorderableList.DoLayoutList();
                _adObj.ApplyModifiedProperties();
            }
        }

        private void DrawElementOfList(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _blocks.GetArrayElementAtIndex(index);
            rect.y += 2;

            float buttonWidth = 60f;
            Rect objRect = new Rect(rect.x, rect.y, Application.isPlaying ? rect.width - buttonWidth - 5 : rect.width, EditorGUIUtility.singleLineHeight);
                
            EditorGUI.PropertyField(objRect, element, GUIContent.none);

            if (Application.isPlaying)
            {
                Rect btnRect = new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth,
                    EditorGUIUtility.singleLineHeight);
                GUI.backgroundColor = new Color(164, 255, 164, 255);
                if (GUI.Button(btnRect, "▶ Play"))
                {
                    _ad.PlayBlock(index);
                }

                GUI.backgroundColor = Color.white;
            }
        }
    }
}

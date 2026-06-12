using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metroma.Editor
{
    public class AdToolWindow : EditorWindow
    {/*
        private AdBase             _ad;
        private SerializedObject   _adObj;
        private SerializedProperty _blocks;
        private ReorderableList    _reorderableList;
        
        [SerializeField] private string _adPath = "";
        
        private bool _orderChanged = false;
        
        private static readonly Color ActiveBg    = new Color(0.25f, 0.75f, 0.25f, 0.22f);
        private static readonly Color CursorColor = new Color(0.3f, 1f, 0.3f, 1f);

        [MenuItem("Tools/AdTool")]
        private static void ShowWindow() => GetWindow<AdToolWindow>("AdTool ▶");

        private void OnEnable()
        {
            TryRestoreTarget();
        }
        
        private void TryRestoreTarget()
        {
            if (_ad != null) return;
            if (string.IsNullOrEmpty(_adPath)) return;

            var go = GameObject.Find(_adPath);
            if (go == null) return;

            if (go.TryGetComponent<AdBase>(out var ad))
            {
                _ad = ad;
                _orderChanged = false;
                Init();
                Repaint();
            }
            else
            {
                _adPath = "";
            }
        }
        
        private void OnInspectorUpdate()
        {
            if (Application.isPlaying) Repaint();
        }
        
        private void OnSelectionChange()
        {
            if (Selection.activeGameObject == null) return;
            
            if (Selection.activeGameObject.TryGetComponent<AdBase>(out var ad))
            {
                if (_ad == null || ad != _ad)
                {
                    _ad = ad;
                    _orderChanged = false;
                    Init();
                    Repaint();
                }
            }
        }
        
        private void Init()
        {
            _adObj  = new SerializedObject(_ad);
            _blocks = _adObj.FindProperty("_blocks");
            
            _adPath = GetGameObjectPath(_ad.gameObject);

            _reorderableList = new ReorderableList(_adObj, _blocks, true, true, true, true);
            _reorderableList.drawHeaderCallback  = DrawHeader;
            _reorderableList.drawElementCallback = DrawElement;
            
            _reorderableList.onReorderCallback = _ =>
            {
                _adObj.ApplyModifiedProperties();
                _orderChanged = true;
                Repaint();
            };
        }
        
        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
        
        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Blocs", EditorStyles.boldLabel);
        }
        
        private void OnGUI()
        {
            TryRestoreTarget();

            GUILayout.Space(10);
            if (_ad == null)
            {
                EditorGUILayout.HelpBox(
                    "Sélectionnez un GameObject contenant un composant AdBase dans la scène.",
                    MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Cible : {_ad.gameObject.name}", EditorStyles.boldLabel);

            if (!Application.isPlaying && AdBase.HasRuntimeSandbox)
            {
                EditorGUILayout.HelpBox(
                    "UN ORDRE TEMPORAIRE A ÉTÉ TESTÉ EN PLAY MODE.\n" +
                    "Souhaitez-vous l'appliquer définitivement à la scène ?", 
                    MessageType.Info);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Sauvegarder sur la Scène", GUILayout.Height(30)))
                        PersistSandboxToDisk();
                    
                    if (GUILayout.Button("Ignorer", GUILayout.Height(30), GUILayout.Width(80)))
                        AdBase.ClearRuntimeSandbox();
                }
                GUILayout.Space(10);
            }

            if (_adObj != null)
            {
                _adObj.Update();
                _reorderableList.DoLayoutList();
                _adObj.ApplyModifiedProperties();
            }

            if (_orderChanged)
            {
                GUILayout.Space(6);
                EditorGUILayout.HelpBox(
                    "L'ordre a changé. Rechargez la scène pour appliquer le nouvel ordre.",
                    MessageType.Warning);

                if (GUILayout.Button("Rafraîchir (Bloc 0)", GUILayout.Height(26)))
                {
                    _orderChanged = false;
                    ReloadSceneAtIndex(0);
                }
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Auto-Fetch Child Blocks", GUILayout.Height(28)))
            {
                Undo.RecordObject(_ad, "Fetch Blocks");
                _ad.AutoFetchChildBlocks();
                EditorUtility.SetDirty(_ad);
                Init();
            }

            GUILayout.Space(8);
        }
        
        private void PersistSandboxToDisk()
        {
            if (_ad == null || !AdBase.HasRuntimeSandbox) return;

            Undo.RecordObject(_ad, "Persist Sandbox Order");
            
            var sandbox = _ad.GetSandboxBlocks();
            if (sandbox == null) return;

            _blocks.arraySize = sandbox.Length;
            for (int i = 0; i < sandbox.Length; i++)
            {
                _blocks.GetArrayElementAtIndex(i).objectReferenceValue = sandbox[i];
            }
            
            _adObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(_ad);
            
            if (PrefabUtility.IsPartOfPrefabInstance(_ad))
                PrefabUtility.RecordPrefabInstancePropertyModifications(_ad);
            
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AdBase.ClearRuntimeSandbox();
            _orderChanged = false;
            
            Debug.Log("[AdTool] Ordre sandbox persisté sur le disque.");
        }
        
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_blocks == null || index >= _blocks.arraySize) return;

            var element = _blocks.GetArrayElementAtIndex(index);
            rect.y += 2;

            bool isCurrent = Application.isPlaying
                          && _ad != null
                          && index == _ad.CurrentBlockIndex;
            
            if (isCurrent)
            {
                EditorGUI.DrawRect(
                    new Rect(rect.x, rect.y - 2, rect.width, EditorGUIUtility.singleLineHeight + 4),
                    ActiveBg);
            }
            
            const float cursorW = 20f;
            Rect cursorRect = new Rect(rect.x, rect.y, cursorW, EditorGUIUtility.singleLineHeight);

            if (isCurrent)
            {
                var saved = GUI.color;
                GUI.color = CursorColor;
                GUI.Label(cursorRect, "►");
                GUI.color = saved;
            }
            
            Rect fieldRect = new Rect(
                rect.x + cursorW, rect.y,
                rect.width - cursorW, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(fieldRect, element, GUIContent.none);
            
            if (Application.isPlaying)
            {
                Event e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    Rect fullRow = new Rect(rect.x, rect.y - 2, rect.width, EditorGUIUtility.singleLineHeight + 4);
                    if (fullRow.Contains(e.mousePosition))
                    {
                        ReloadSceneAtIndex(index);
                        e.Use();
                    }
                }
            }
        }
        
        private void ReloadSceneAtIndex(int index)
        {
            if (_ad == null) return;

            if (Application.isPlaying)
            {
                _adObj?.ApplyModifiedProperties();
                Block[] sandboxOrder = new Block[_blocks.arraySize];
                for (int i = 0; i < _blocks.arraySize; i++)
                {
                    sandboxOrder[i] = _blocks.GetArrayElementAtIndex(i).objectReferenceValue as Block;
                }
                
                AdBase.SetRuntimeSandbox(index, sandboxOrder);
                EditorApplication.delayCall += () =>
                {
                    EditorSceneManager.LoadSceneInPlayMode(
                        SceneManager.GetActiveScene().path,
                        new LoadSceneParameters(LoadSceneMode.Single));
                };
                
                Debug.Log($"[AdTool] Runtime Sandbox → Reload bloc {index}");
            }
            else
            {
                _adObj?.ApplyModifiedProperties();
                EditorUtility.SetDirty(_ad);
                
                if (PrefabUtility.IsPartOfPrefabInstance(_ad))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(_ad);

                Undo.FlushUndoRecordObjects();
                AdBase.SetPendingBlockIndex(index);
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                EditorSceneManager.LoadSceneInPlayMode(
                    SceneManager.GetActiveScene().path,
                    new LoadSceneParameters(LoadSceneMode.Single));
                
                Debug.Log($"[AdTool] Edit Mode → Sauvegarde Disque + Reload bloc {index}");
            }
        }*/ 
    }
}

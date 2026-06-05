using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Metroma.Editor
{
public class SequenceSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public Sequence Sequence;
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>();

            Assembly assembly = typeof(ASequencable).Assembly;
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Sequencables")));

            foreach (Type type in assembly.GetTypes()) {
                if (typeof(ASequencable).IsAssignableFrom(type) && !type.IsAbstract) {
                    SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(type.Name));
                    entry.level = 1;
                    entry.userData = type;
                    tree.Add(entry);
                }
            }

            return tree;
        }
        
        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
            AddSequencable(SearchTreeEntry.userData as Type);
            return true;
        }

        public void AddSequencable(Type type) {
            GameObject child = new GameObject(type.Name);
            child.transform.SetParent(Sequence.transform);
            child.transform.localPosition = Vector3.zero;

            child.AddComponent(type);
            Undo.RegisterCreatedObjectUndo(child, $"Add {type.Name}");
            Selection.activeObject = child;
        }
    }
    
    [CustomEditor(typeof(Sequence))]
    public class SequenceCustomInspector : UnityEditor.Editor
    {
        private SequenceSearchProvider _searchProvider;
        
        public override VisualElement CreateInspectorGUI() {
            
            VisualElement root = new VisualElement();
            _searchProvider = CreateInstance<SequenceSearchProvider>();
            _searchProvider.Sequence = target as Sequence;
            
            Button button = new Button(() => OnAddItemClicked(
                GUIUtility.GUIToScreenPoint(root.worldBound.center + 30f* Vector2.up))
            );
            button.text = "Add Item";
            root.Add(button);
            return root;
        }

        private void OnAddItemClicked(Vector2 pos) {
            SearchWindow.Open(new SearchWindowContext(pos), _searchProvider);
        }
    }
}

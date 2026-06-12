using UnityEditor;
using UnityEngine;

namespace Metroma.Editor
{
    [InitializeOnLoad]
    public static class HierarchyHighlighter
    {
        static HierarchyHighlighter() {
            EditorApplication.hierarchyWindowItemOnGUI += _OnHierarchyGUI;
        }
    
        private static void _OnHierarchyGUI(int instanceID, Rect selectionRect) {
            Object instanceObject = EditorUtility.EntityIdToObject(instanceID);
            if(null == instanceObject) return;
            GameObject instanceGameObject = instanceObject as GameObject;
            if(null == instanceGameObject) return;
            ASequencable sequencable = instanceGameObject.GetComponent<ASequencable>();
            if (null == sequencable) return;
            
            Texture texture = EditorGUIUtility.whiteTexture;
            Color save = GUI.color;
            InspectorColor color = sequencable.GetInspectorColor();
            GUI.color = color.Background;
            GUI.DrawTexture(selectionRect, texture, ScaleMode.StretchToFill, false);
            GUI.color = save;

            if (!sequencable.RequirementsValidated()) {
                float offset = selectionRect.height;
                Rect rect = selectionRect;
                rect.width = offset;
                selectionRect.x += offset;
                selectionRect.width -= offset;
                GUI.DrawTexture(rect,EditorGUIUtility.FindTexture("console.warnicon.sml") );
            }

            GUI.color = color.Text;
            GUI.Label(selectionRect, sequencable.name);
            GUI.color = save;

        }
    }
}

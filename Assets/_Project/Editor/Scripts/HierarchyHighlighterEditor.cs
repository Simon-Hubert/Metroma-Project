using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyHighlighterEditor
{
    static HierarchyHighlighterEditor() {
        EditorApplication.hierarchyWindowItemOnGUI += _OnHierarchyGUI;
    }
    
    private static void _OnHierarchyGUI(int instanceID, Rect selectionRect) {
        Object instanceObject = EditorUtility.EntityIdToObject(instanceID);
        if(null == instanceObject) return;
        GameObject instanceGameObject = instanceObject as GameObject;
        if(null == instanceGameObject) return;
    }
}

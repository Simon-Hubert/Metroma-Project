using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace Metroma.Editor
{
    [CustomPropertyDrawer(typeof(ConditionAttribute))]
    public class ConditionPropertyDrawer : PropertyDrawer
    
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConditionAttribute conditionAttribute = attribute as ConditionAttribute;
            Type[] types = conditionAttribute.GetTypes();
            
            property.managedReferenceValue ??= FormatterServices.GetUninitializedObject(types[0]);
            
            GUIContent[] options = new GUIContent[types.Length];
            
            int selectedIndex = 0;
            for (int i = 0; i < types.Length; i++) {
                Type type = types[i];
                string typeAndAssemblyName = $"{type.Assembly.GetName().Name} {type.FullName}";
                options[i] = new GUIContent(type.Name);
                if (property.managedReferenceFullTypename == typeAndAssemblyName) {
                    selectedIndex = i;
                }
            }
            
            int newSelectedIndex = EditorGUI.Popup(
                new Rect(position.x + position.width - 130, position.y, 130, EditorGUIUtility.singleLineHeight),
                selectedIndex, options);
        
            if (selectedIndex != newSelectedIndex)
            {
                Undo.RegisterCompleteObjectUndo(property.serializedObject.targetObject, "selected type change");
                Type selectedType = types[newSelectedIndex];
                property.managedReferenceValue = FormatterServices.GetUninitializedObject(selectedType);
            }
        
            EditorGUI.PropertyField(
                new Rect(
                    position.x,
                    position.y,
                    position.width,
                    position.height),
                property,
                true);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}

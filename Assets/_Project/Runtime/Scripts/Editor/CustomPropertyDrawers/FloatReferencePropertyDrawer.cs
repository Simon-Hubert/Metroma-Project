using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Metroma
{
    [CustomPropertyDrawer(typeof(FloatReference))]
    public class FloatReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty targetProp = property.FindPropertyRelative("_value");
            SerializedProperty varNameProp = property.FindPropertyRelative("_variableName");

            float halfWidth = position.width / 2f;
            Rect targetRect = new Rect(position.x, position.y, halfWidth - 2, position.height);
            Rect popupRect = new Rect(position.x + halfWidth + 2, position.y, halfWidth - 2, position.height);
            
            EditorGUI.PropertyField(targetRect, targetProp, GUIContent.none);

            UnityEngine.Object currentObj = targetProp.objectReferenceValue;

            if (currentObj != null)
            {
                GameObject go = currentObj as GameObject;
                if (go == null && currentObj is Component c) go = c.gameObject;

                if (go != null)
                {
                    var options = new List<(Component comp, string varName, string displayName)>();
                    
                    foreach (Component comp in go.GetComponents<Component>())
                    {
                        if (comp == null) continue;

                        var validMembers = comp.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Where(m => Attribute.IsDefined(m, typeof(ConditionParamAttribute)))
                            .Where(m => (m is FieldInfo f && f.FieldType == typeof(float)) ||
                                        (m is PropertyInfo p && p.PropertyType == typeof(float)));

                        foreach (var member in validMembers)
                        {
                            options.Add((comp, member.Name, $"{comp.GetType().Name}/{member.Name}"));
                        }
                    }

                    if (options.Count > 0)
                    {
                        int currentIndex = 0;
                        for (int i = 0; i < options.Count; i++)
                        {
                            if (options[i].comp == currentObj && options[i].varName == varNameProp.stringValue)
                            {
                                currentIndex = i;
                                break;
                            }
                        }

                        string[] displayNames = options.Select(o => o.displayName).ToArray();
                        int newIndex = EditorGUI.Popup(popupRect, currentIndex, displayNames);

                        targetProp.objectReferenceValue = options[newIndex].comp;
                        varNameProp.stringValue = options[newIndex].varName;
                    }
                    else
                    {
                        GUI.enabled = false;
                        EditorGUI.TextField(popupRect, "Aucun [ConditionParam] Float");
                        GUI.enabled = true;
                        varNameProp.stringValue = "";
                    }
                }
                else 
                {
                    Type type = currentObj.GetType();
                    var validMembers = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m => Attribute.IsDefined(m, typeof(ConditionParamAttribute)))
                        .Where(m => (m is FieldInfo f && f.FieldType == typeof(float)) ||
                                    (m is PropertyInfo p && p.PropertyType == typeof(float)))
                        .Select(m => m.Name).ToArray();

                    if (validMembers.Length > 0)
                    {
                        int currentIndex = Mathf.Max(0, Array.IndexOf(validMembers, varNameProp.stringValue));
                        int newIndex = EditorGUI.Popup(popupRect, currentIndex, validMembers);
                        varNameProp.stringValue = validMembers[newIndex];
                    }
                    else
                    {
                        GUI.enabled = false;
                        EditorGUI.TextField(popupRect, "Aucun [ConditionParam] Float");
                        GUI.enabled = true;
                        varNameProp.stringValue = "";
                    }
                }
            }
            else
            {
                GUI.enabled = false;
                EditorGUI.TextField(popupRect, "<- Assignez un objet");
                GUI.enabled = true;
            }

            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}

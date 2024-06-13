// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Drawer for EnableIfTrueAttribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(EnableIfTrueAttribute))]
    public class EnableIfTrueDrawer : PropertyDrawer
    {

        private bool EnableThis(SerializedProperty property)
        {
            var enableIfTrue = attribute as EnableIfTrueAttribute;
            var otherProperty = property.serializedObject.FindProperty(enableIfTrue.OtherPropertyName);
            if (otherProperty == null)
            {
                Debug.LogError($"[EnableIfTrue(\"{enableIfTrue.OtherPropertyName}\")] can't find property named \"{enableIfTrue.OtherPropertyName}\".");
                return true;
            }
            else
            {
                return otherProperty.boolValue;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(!EnableThis(property));
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }

    }
}

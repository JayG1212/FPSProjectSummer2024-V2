// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Utility methods to add input definitions to Unity's built-in input manager.
    /// </summary>
    public static class InputManagerUtility
    {

        public static bool IsAxisDefined(string axisName)
        {
            try
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
                if (assets == null || assets.Length == 0) return true; // Gracefully skip if can't load InputManager.
                SerializedObject serializedObject = new SerializedObject(assets[0]);
                SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");
                for (int i = 0; i < axesProperty.arraySize; i++)
                {
                    var axis = axesProperty.GetArrayElementAtIndex(i);
                    if (axis.displayName == axisName) return true;
                }
                return false;
            }
            catch (System.InvalidOperationException)
            {
                return false;
            }
        }

        public static void AddButton(string axisName, string defaultPositive, string defaultAltPositive,
            string defaultNegative, string defaultAltNegative)
        {
            Debug.Log($"Adding {axisName} (positive={defaultPositive}/{defaultAltPositive}, negative={defaultNegative}/{defaultAltNegative}) but you may need to check its values (Edit > Project Settings > Input).");
            AddAxis(new InputAxis()
            {
                name = axisName,
                type = AxisType.KeyOrMouseButton,
                positiveButton = defaultPositive,
                altPositiveButton = defaultAltPositive,
                negativeButton = defaultNegative,
                altNegativeButton = defaultAltNegative,
                gravity = 1000,
                dead = 0.0001f,
                sensitivity = 1000
            });
        }

        public static void AddAxis(string axisName, int axisNumber, bool invertAxis = false)
        {
            Debug.Log($"Adding {axisName} (axis #{axisNumber}) but you may need to check its values (Edit > Project Settings > Input).");
            AddAxis(new InputAxis()
            {
                name = axisName,
                type = AxisType.JoystickAxis,
                dead = 0.2f,
                sensitivity = 1f,
                axis = axisNumber,
                joyNum = 0,
                invert = invertAxis
            });
        }

        public static void AddMouseAxis(string axisName, bool xAxis)
        {
            Debug.Log($"Adding Mouse Movement {axisName} but you may need to check its values (Edit > Project Settings > Input).");
            AddAxis(new InputAxis()
            {
                name = axisName,
                type = AxisType.MouseMovement,
                dead = 0f,
                sensitivity = 0.1f,
                axis = (xAxis ? 1 : 2),
                joyNum = 0,
            });
        }

        private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
        {
            SerializedProperty child = parent.Copy();
            child.Next(true);
            do
            {
                if (child.name == name) return child;
            }
            while (child.Next(false));
            return null;
        }

        public enum AxisType
        {
            KeyOrMouseButton = 0,
            MouseMovement = 1,
            JoystickAxis = 2
        };

        public class InputAxis
        {
            public string name;
            public string descriptiveName;
            public string descriptiveNegativeName;
            public string negativeButton;
            public string positiveButton;
            public string altNegativeButton;
            public string altPositiveButton;

            public float gravity;
            public float dead;
            public float sensitivity;

            public bool snap = false;
            public bool invert = false;

            public AxisType type;

            public int axis;
            public int joyNum;
        }

        private static void AddAxis(InputAxis axis)
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            axesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);

            GetChildProperty(axisProperty, "m_Name").stringValue = axis.name;
            GetChildProperty(axisProperty, "descriptiveName").stringValue = axis.descriptiveName;
            GetChildProperty(axisProperty, "descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            GetChildProperty(axisProperty, "negativeButton").stringValue = axis.negativeButton;
            GetChildProperty(axisProperty, "positiveButton").stringValue = axis.positiveButton;
            GetChildProperty(axisProperty, "altNegativeButton").stringValue = axis.altNegativeButton;
            GetChildProperty(axisProperty, "altPositiveButton").stringValue = axis.altPositiveButton;
            GetChildProperty(axisProperty, "gravity").floatValue = axis.gravity;
            GetChildProperty(axisProperty, "dead").floatValue = axis.dead;
            GetChildProperty(axisProperty, "sensitivity").floatValue = axis.sensitivity;
            GetChildProperty(axisProperty, "snap").boolValue = axis.snap;
            GetChildProperty(axisProperty, "invert").boolValue = axis.invert;
            GetChildProperty(axisProperty, "type").intValue = (int)axis.type;
            GetChildProperty(axisProperty, "axis").intValue = axis.axis - 1;
            GetChildProperty(axisProperty, "joyNum").intValue = axis.joyNum;

            serializedObject.ApplyModifiedProperties();
        }

    }
}
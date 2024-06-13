// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Custom editor for GridControllerInput that provides a button to add
    /// default input definitions.
    /// </summary>
    [CustomEditor(typeof(GridControllerInput), true)]
    public class GridControllerInputEditor : Editor
    {

        private static GUIContent buttonLabel = new GUIContent(
            "Check Input Settings",
            "Add any inputs listed above to Input Manager if not already present.");
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button(buttonLabel))
            {
                CheckInputSettings();
            }
        }

        protected virtual void CheckInputSettings()
        {
            if (!(target is GridControllerInput)) return;
            var input = target as GridControllerInput;
            if (!InputManagerUtility.IsAxisDefined(input.HorizontalMoveAxis))
            {
                InputManagerUtility.AddButton(input.HorizontalMoveAxis, "d", "right", "a", "left");
                InputManagerUtility.AddAxis(input.HorizontalMoveAxis, 2);
            }
            if (!InputManagerUtility.IsAxisDefined(input.VerticalMoveAxis))
            {
                InputManagerUtility.AddButton(input.VerticalMoveAxis, "w", "up", "s", "down");
                InputManagerUtility.AddAxis(input.VerticalMoveAxis, 1);
            }
            if (!InputManagerUtility.IsAxisDefined(input.StrafeLeftInput))
            {
                InputManagerUtility.AddButton(input.StrafeLeftInput, "q", "joystick button 4", "", "");
            }
            if (!InputManagerUtility.IsAxisDefined(input.StrafeRightInput))
            {
                InputManagerUtility.AddButton(input.StrafeRightInput, "e", "joystick button 5", "", "");
            }
            if (!InputManagerUtility.IsAxisDefined(input.HorizontalLookAxis))
            {
                InputManagerUtility.AddMouseAxis(input.HorizontalLookAxis, true); // Mouse
                InputManagerUtility.AddAxis(input.HorizontalLookAxis, 4); // Joystick
            }
            if (!InputManagerUtility.IsAxisDefined(input.VerticalLookAxis))
            {
                InputManagerUtility.AddMouseAxis(input.VerticalLookAxis, false); // Mouse
                InputManagerUtility.AddAxis(input.VerticalLookAxis, 5, true); // Joystick
            }
            if (!InputManagerUtility.IsAxisDefined(input.CrouchInput))
            {
                InputManagerUtility.AddButton(input.CrouchInput, "c", "joystick button 8", "", "");
            }
            if (!string.IsNullOrEmpty(input.JoystickHorizontalAxis) && !InputManagerUtility.IsAxisDefined(input.JoystickHorizontalAxis))
            {
                InputManagerUtility.AddAxis(input.JoystickHorizontalAxis, 2);
            }
            if (!string.IsNullOrEmpty(input.JoystickVerticalAxis) && !InputManagerUtility.IsAxisDefined(input.JoystickVerticalAxis))
            {
                InputManagerUtility.AddAxis(input.JoystickVerticalAxis, 1);
            }
            if (!string.IsNullOrEmpty(input.RecenterInput) && !InputManagerUtility.IsAxisDefined(input.RecenterInput))
            {
                InputManagerUtility.AddButton(input.RecenterInput, "joystick button 9", "", "", "");
            }
            if (!string.IsNullOrEmpty(input.InteractInput) && !InputManagerUtility.IsAxisDefined(input.InteractInput))
            {
                InputManagerUtility.AddButton(input.InteractInput, "space", "joystick button 0", "", "");
            }
            Debug.Log("Input Manager has all inputs listed in GridControllerInput.", target);
        }

        /// <summary>
        /// Adds default inputs used by an uncustomized GridControllerInput component.
        /// </summary>
        public static void CheckDefaultInputSettings()
        {
            if (!InputManagerUtility.IsAxisDefined("Horizontal"))
            {
                InputManagerUtility.AddButton("Horizontal", "d", "right", "a", "left");
                InputManagerUtility.AddAxis("Horizontal", 2);
            }
            if (!InputManagerUtility.IsAxisDefined("Vertical"))
            {
                InputManagerUtility.AddButton("Vertical", "w", "up", "s", "down");
                InputManagerUtility.AddAxis("Vertical", 1);
            }
            if (!InputManagerUtility.IsAxisDefined("StrafeLeft"))
            {
                InputManagerUtility.AddButton("StrafeLeft", "q", "joystick button 4", "", "");
            }
            if (!InputManagerUtility.IsAxisDefined("StrafeRight"))
            {
                InputManagerUtility.AddButton("StrafeRight", "e", "joystick button 5", "", "");
            }
            if (!InputManagerUtility.IsAxisDefined("HorizontalLook"))
            {
                InputManagerUtility.AddMouseAxis("HorizontalLook", true); // Mouse
                InputManagerUtility.AddAxis("HorizontalLook", 4); // Joystick
            }
            if (!InputManagerUtility.IsAxisDefined("VerticalLook"))
            {
                InputManagerUtility.AddMouseAxis("VerticalLook", false); // Mouse
                InputManagerUtility.AddAxis("VerticalLook", 5, true); // Joystick
                Debug.Log("Note: HorizontalLook & VerticalLook are set up for Xbox controllers. You may need to change axis numbers for other controller types, or switch to Rewired or Input System package.");
            }
            if (!InputManagerUtility.IsAxisDefined("Crouch"))
            {
                InputManagerUtility.AddButton("Crouch", "c", "joystick button 8", "", "");
            }
            if (!InputManagerUtility.IsAxisDefined("JoystickHorizontal"))
            {
                InputManagerUtility.AddAxis("JoystickHorizontal", 2);
            }
            if (!InputManagerUtility.IsAxisDefined("JoystickVertical"))
            {
                InputManagerUtility.AddAxis("JoystickVertical", 1);
            }
            if (!InputManagerUtility.IsAxisDefined("Recenter"))
            {
                InputManagerUtility.AddButton("Recenter", "joystick button 9", "", "", "");
            }
            if (!InputManagerUtility.IsAxisDefined("Interact"))
            {
                InputManagerUtility.AddButton("Interact", "space", "joystick button 0", "", "");
            }
            Debug.Log("Input Manager now has all default inputs used by GridControllerInput component.");
        }

    }
}
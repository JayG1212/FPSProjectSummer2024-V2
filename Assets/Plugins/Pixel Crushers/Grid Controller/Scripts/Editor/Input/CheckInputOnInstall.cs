// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.GridController
{

    public static class CheckInputOnInstall
    {

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            var editorSettings = GridControllerEditorSettings.Load();
            if (editorSettings == null) return;
            if (editorSettings.CheckedInputManager) return;
            editorSettings.CheckedInputManager = true;
            if (!InputManagerUtility.IsAxisDefined("VerticalLook")) // If defined, inputs are probably already added.
            {
                if (EditorUtility.DisplayDialog("Add Input Settings?", "Grid Controller is currently configured to use Unity's built-in Input Manager. Would you like to add the Input Manager settings required to control Grid Controller? If you plan to use Rewired or the Input System package, you can click Skip.", "Add", "Skip"))
                {
                    GridControllerInputEditor.CheckDefaultInputSettings();
                }
            }
            EditorUtility.SetDirty(editorSettings);
        }

    }
}


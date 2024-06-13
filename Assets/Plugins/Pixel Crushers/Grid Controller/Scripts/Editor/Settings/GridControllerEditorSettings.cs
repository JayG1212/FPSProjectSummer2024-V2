// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.GridController
{

    //--- Need only the one that's already created: [CreateAssetMenu]
    public class GridControllerEditorSettings : ScriptableObject
    {

        [SerializeField] private bool checkedInputManager = false;

        public bool CheckedInputManager { get => checkedInputManager; set => checkedInputManager = value; }

        private static GridControllerEditorSettings editorSettings = null;

        public static GridControllerEditorSettings Load()
        {
            if (editorSettings == null)
            {
                var guids = AssetDatabase.FindAssets("t:GridControllerEditorSettings");
                if (guids.Length > 0)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    editorSettings = AssetDatabase.LoadAssetAtPath<GridControllerEditorSettings>(assetPath);
                }
            }
            return editorSettings;
        }

    }
}


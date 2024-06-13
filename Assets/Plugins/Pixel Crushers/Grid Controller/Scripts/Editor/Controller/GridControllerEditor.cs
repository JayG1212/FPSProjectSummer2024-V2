// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Custom editor for GridController that checks for input, audio, free look, and interactor components.
    /// </summary>
    [CustomEditor(typeof(GridController), true)]
    public class GridControllerEditor : Editor
    {

        private GridController controller;
        private SerializedProperty checkForAuxiliaryComponentsProperty;

        private static GUIContent ignoreMissingComponentsButtonLabel = new GUIContent(
            "Ignore Missing Components",
            "Stop checking for the missing components above.");

        protected virtual void OnEnable()
        {
            controller = target as GridController;
            serializedObject.Update();
            checkForAuxiliaryComponentsProperty = serializedObject.FindProperty("checkForAuxiliaryComponents");
            if (checkForAuxiliaryComponentsProperty != null && checkForAuxiliaryComponentsProperty.boolValue)
            {
                var hasCamera = controller.GetComponentInChildren<Camera>() != null;
                var hasInput = controller.GetComponent<IGridControllerInput>() != null;
                var hasAudio = controller.GetComponent<IGridControllerAudio>() != null;
                var hasFreeLook = controller.GetComponent<FreeLook>() != null;
                var hasInteractor = controller.GetComponent<Interactor>() != null;
                if (hasCamera && hasInput && hasAudio && hasFreeLook && hasInteractor)
                {
                    checkForAuxiliaryComponentsProperty.boolValue = false;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (checkForAuxiliaryComponentsProperty != null && checkForAuxiliaryComponentsProperty.boolValue)
            {
                var hasCamera = controller.GetComponentInChildren<Camera>() != null;
                var hasInput = controller.GetComponent<IGridControllerInput>() != null;
                var hasAudio = controller.GetComponent<IGridControllerAudio>() != null;
                var hasFreeLook = controller.GetComponent<FreeLook>() != null;
                var hasInteractor = controller.GetComponent<Interactor>() != null;
                if (hasInput && hasAudio && hasFreeLook && hasInteractor)
                {
                    checkForAuxiliaryComponentsProperty.boolValue = false;
                    serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    if (!hasCamera)
                    {
                        EditorGUILayout.HelpBox("GridController requires a Camera on a child GameObject. Click below to add a Camera.", MessageType.Warning);
                        if (GUILayout.Button("Add Camera"))
                        {
                            AddCamera();
                        }
                    }
                    if (!hasInput)
                    {
                        EditorGUILayout.HelpBox("GridController requires an input component. Click below to add GridControllerInput which uses Unity's built-in input manager. Or see the manual for other input components such as the Input System package or Rewired.", MessageType.Warning);
                        if (GUILayout.Button("Add GridControllerInput"))
                        {
                            AddMissingComponent<GridControllerInput>();
                        }
                    }
                    if (!hasAudio)
                    {
                        EditorGUILayout.HelpBox("Audio component is optional. Click below to add GridControllerAudio which uses Unity's built-in audio manager to play sounds such as footsteps. Or see the manual for other audio systems.", MessageType.Info);
                        if (GUILayout.Button("Add GridControllerAudio"))
                        {
                            AddMissingComponent<GridControllerAudio>();
                        }
                    }
                    if (!hasFreeLook)
                    {
                        EditorGUILayout.HelpBox("FreeLook component is optional. Click below to add FreeLook to enable free look.", MessageType.Info);
                        if (GUILayout.Button("Add FreeLook"))
                        {
                            AddMissingComponent<FreeLook>();
                        }
                    }
                    if (!hasInteractor)
                    {
                        EditorGUILayout.HelpBox("Interactor component is optional. Click below to add Interactor to enable interaction with interactable objects.", MessageType.Info);
                        if (GUILayout.Button("Add Interactor"))
                        {
                            AddMissingComponent<GridControllerInput>();
                        }
                    }
                    if (GUILayout.Button(ignoreMissingComponentsButtonLabel))
                    {
                        checkForAuxiliaryComponentsProperty.boolValue = false;
                        serializedObject.ApplyModifiedProperties();
                    }
                }

            }
            base.OnInspectorGUI();
        }

        protected virtual void AddMissingComponent<T>() where T : MonoBehaviour
        {
            controller.gameObject.AddComponent<T>();
        }

        protected virtual void AddCamera()
        {
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.tag = Tags.MainCamera;
            camera.transform.SetParent(controller.transform);
            camera.transform.localPosition = new Vector3(0, 1, 0);
        }

    }
}
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Manages an onscreen arrow pad UI. Sends input to GridController, or to
    /// Interactor if interact button is clicked.
    /// Can be used to move with mouse clicks or touch control.
    /// </summary>
    [AddComponentMenu("Grid Controller/Arrow Pad")]
    public class ArrowPad : MonoBehaviour
    {

        [SerializeField] private Button forward;
        [SerializeField] private Button backward;
        [SerializeField] private Button turnLeft;
        [SerializeField] private Button turnRight;
        [SerializeField] private Button strafeLeft;
        [SerializeField] private Button strafeRight;
        [SerializeField] private Button crouch;
        [SerializeField] private Button interact;

        private Canvas canvas;

        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        protected virtual void Start()
        {
            canvas = GetComponent<Canvas>();
#if UNITY_2023_1_OR_NEWER
            var gridController = GridController.Instance ?? FindFirstObjectByType<GridController>();
#else
            var gridController = GridController.Instance ?? FindObjectOfType<GridController>();
#endif
            if (forward != null) forward.onClick.AddListener(() => { gridController.EnqueueMovementCommand(MovementCommands.Forward); DeselectUI(); });
            if (backward != null) backward.onClick.AddListener(() => { gridController.EnqueueMovementCommand(MovementCommands.Backward); DeselectUI(); });
            if (turnLeft != null) turnLeft.onClick.AddListener(() => { gridController.EnqueueMovementCommand(MovementCommands.TurnLeft); DeselectUI(); });
            if (turnRight != null) turnRight.onClick.AddListener(() => { gridController.EnqueueMovementCommand(MovementCommands.TurnRight); DeselectUI(); });
            if (strafeLeft != null) strafeLeft.onClick.AddListener(() => { gridController.EnqueueMovementCommand(MovementCommands.StrafeLeft); DeselectUI(); });
            if (strafeRight != null) strafeRight.onClick.AddListener(() => { gridController.EnqueueMovementCommand(MovementCommands.StrafeRight); DeselectUI(); });
            if (crouch != null) crouch.onClick.AddListener(() => { gridController.ToggleCrouching(); DeselectUI(); });
            var interactor = gridController.GetComponent<Interactor>();
            if (interact != null && interactor != null)
            {
                interact.onClick.AddListener(() => { interactor.TryInteractForward(); DeselectUI(); });
            }
        }

        /// <summary>
        /// Sets the visibility of the arrow pad's canvas.
        /// </summary>
        public virtual void SetVisibility(bool value)
        {
            if (canvas != null) canvas.enabled = value;
        }

        protected virtual void DeselectUI()
        {
            // DeselectUI the UI so movement/interaction input (e.g., Space to interact) doesn't instead click UI button.
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

    }
}
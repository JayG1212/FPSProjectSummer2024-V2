// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Allows interaction with Interactable components.
    /// </summary>
    [AddComponentMenu("Grid Controller/Interaction/Interactor")]
    [RequireComponent(typeof(GridController))]
    public class Interactor : MonoBehaviour
    {

        [Tooltip("Detect interactables on these layers.")]
        [SerializeField] private LayerMask interactableLayerMask = ~0;

        private GridController gridController;

        protected virtual void Awake()
        {
            gridController = GetComponent<GridController>();
        }

        protected virtual void Update()
        {
            if (!gridController.IsMoving)
            {
                CheckInput();
            }
        }

        protected virtual void CheckInput()
        {
            if (gridController.Input.Interact)
            {
                TryInteractForward();
            }
            else if (gridController.Input.MouseInteract && !IsPointerOverUI())
            {
                TryInteractAtScreenPosition(gridController.Input.MousePosition);
            }
        }

        /// <summary>
        /// Look for an interactable in the square directly ahead and activate it.
        /// </summary>
        public virtual void TryInteractForward()
        {
            TryInteractAtScreenPosition(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        /// <summary>
        /// Look for an interactable at the specified screen position (typically mouse position) and activate it.
        /// </summary>
        public virtual void TryInteractAtScreenPosition(Vector3 screenPos)
        {
            var ray = Camera.main.ScreenPointToRay(screenPos);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, gridController.SquareSize, interactableLayerMask))
            {
                var interactable = hitInfo.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable is MonoBehaviour && (interactable as MonoBehaviour).enabled)
                {
                    interactable.Interact(gridController.transform.position, gridController.CompassDirection);
                }
            }
        }

        protected virtual bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

    }
}


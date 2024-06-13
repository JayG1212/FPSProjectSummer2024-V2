// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Basic interactable component. Manually call Interact() to activate it.
    /// </summary>
    [AddComponentMenu("Grid Controller/Interaction/Interactable")]
    public class Interactable : MonoBehaviour, IInteractable
    {

        [SerializeField] private UnityEvent onInteract = new UnityEvent();
        [SerializeField] private bool disableAfterUse = false;

        public event System.Action<Vector3> Interacted = null;

        public bool DisableAfterUse => disableAfterUse;

        /// <summary>
        /// Activate the interactable, given the grid controller's position and compass direction.
        /// </summary>
        public virtual void Interact(Vector3 playerPosition, CompassDirection playerCompassDirection)
        {
            Interacted?.Invoke(playerPosition);
            onInteract.Invoke();
            if (disableAfterUse) enabled = false;
        }

    }
}

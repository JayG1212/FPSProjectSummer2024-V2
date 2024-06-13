// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// General purpose trigger that invokes events when the grid controller 
    /// enters and/or exits its trigger collider.
    /// </summary>
    [AddComponentMenu("Grid Controller/Interaction/Entry Exit Trigger")]
    public class EntryExitTrigger : MonoBehaviour
    {

        [SerializeField] private UnityEvent onGridControllerEnter = new UnityEvent();
        [SerializeField] private UnityEvent onGridControllerExit = new UnityEvent();

        public event System.Action Entered = null;
        public event System.Action Exited = null;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tags.Player))
            {
                HandleGridControllerEnter(other.GetComponent<GridController>());
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tags.Player))
            {
                HandleGridControllerExit(other.GetComponent<GridController>());
            }
        }

        protected virtual void HandleGridControllerEnter(GridController gridController)
        {
            Entered?.Invoke();
            onGridControllerEnter.Invoke();
        }

        protected virtual void HandleGridControllerExit(GridController gridController)
        {
            Exited?.Invoke();
            onGridControllerExit.Invoke();
        }

    }
}

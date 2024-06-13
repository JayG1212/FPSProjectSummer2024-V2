// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// When the grid controller enters the trigger collider, automatically
    /// moves to a destination point, such as a Scene Portal at the end of stairs.
    /// </summary>
    [AddComponentMenu("Grid Controller/Auto Mover")]
    public class AutoMover : EntryExitTrigger
    {

        [Tooltip("When entering trigger collider, move grid controller to this position.")]
        [SerializeField] private Transform destination;

        [Tooltip("Duration over which to move one square's distance.")]
        [SerializeField] private float movementDuration = 0.2f;

        private const float MinMovementDuration = 0.05f;

        protected override void HandleGridControllerEnter(GridController gridController)
        {
            base.HandleGridControllerEnter(gridController);
            if (destination == null)
            {
                Debug.LogWarning("Auto Mover destination is not set.", this);
            }
            else if (GridController.Instance == null)
            {
                Debug.LogWarning("No Grid Controller in scene.", this);
            }
            else
            {
                StartCoroutine(MoveToDestination());
            }
        }

        protected virtual IEnumerator MoveToDestination()
        {
            GridController.Instance.StopAllCoroutines();
            GridController.Instance.CanMove = false;
            var gridControllerTransform = GridController.Instance.transform;
            var startPosition = gridControllerTransform.position;
            var startRotation = gridControllerTransform.rotation;
            var distance = Vector3.Distance(startPosition, destination.position);
            var duration = Mathf.Max(MinMovementDuration, movementDuration * (distance / GridController.Instance.SquareSize));
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                var position = Vector3.Lerp(startPosition, destination.position, t);
                var rotation = Quaternion.Lerp(startRotation, destination.rotation, t);
                gridControllerTransform.SetPositionAndRotation(position, rotation);
                yield return null;
                elapsed += Time.deltaTime;
            }
            GridController.Instance.Teleport(destination.position, CompassDirectionUtility.AngleToCompassDirection(destination.transform.rotation.eulerAngles.y));
            GridController.Instance.CanMove = true;
        }

    }
}

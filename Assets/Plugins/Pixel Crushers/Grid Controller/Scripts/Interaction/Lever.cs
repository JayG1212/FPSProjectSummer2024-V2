// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Interactable that also manages a lever handle GameObject that rotates when activated.
    /// Can be paired with another lever to keep their positions in sync.
    /// </summary>
    [AddComponentMenu("Grid Controller/Interaction/Lever")]
    public class Lever : Interactable
    {

        [SerializeField] private Transform handle;
        [SerializeField] private float rotationAngle = 120f;
        [SerializeField] private float rotationDuration = 0.2f;

        [Tooltip("If assigned, when this lever is pulled the Paired Lever also moves.")]
        [SerializeField] private Lever pairedLever;

        private bool flipDirection = false;

        /// <summary>
        /// Rotate the lever. When finished, invoke the interaction.
        /// </summary>
        public override void Interact(Vector3 playerPosition, CompassDirection playerCompassDirection)
        {
            StartCoroutine(PullLever(playerPosition, playerCompassDirection));
            if (pairedLever != null) pairedLever.MoveAsPairedLever();
        }

        public void MoveAsPairedLever()
        {
            StartCoroutine(AnimateLever());
        }

        protected virtual IEnumerator PullLever(Vector3 playerPosition, CompassDirection playerCompassDirection)
        {
            yield return AnimateLever();
            base.Interact(playerPosition, playerCompassDirection);
        }

        protected virtual IEnumerator AnimateLever()
        {
            if (DisableAfterUse) enabled = false; // Disable now since animation will take time.
            var startRotation = handle.localRotation;
            var startEuler = startRotation.eulerAngles;
            var finalEulerX = startEuler.x + (rotationAngle * (flipDirection ? -1 : 1));
            var destinationRotation = Quaternion.Euler(finalEulerX, startEuler.y, startEuler.z);
            if (rotationDuration > 0)
            {
                float elapsed = 0;
                while (elapsed < rotationDuration)
                {
                    var t = elapsed / rotationDuration;
                    handle.localRotation = Quaternion.Lerp(startRotation, destinationRotation, t);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
            }
            handle.localRotation = destinationRotation;
            flipDirection = !flipDirection;
        }
    }
}

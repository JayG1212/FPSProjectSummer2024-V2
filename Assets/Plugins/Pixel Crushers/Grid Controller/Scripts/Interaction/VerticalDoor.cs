// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Controls a vertically-rising door, gate, portcullis, etc.
    /// </summary>
    [AddComponentMenu("Grid Controller/Vertical Door")]
    public class VerticalDoor : MonoBehaviour
    {

        [Tooltip("Transform of door part that moves up/down and blocks controller.")]
        [SerializeField] private Transform door;
        [Tooltip("Animate open/close over this duration.")]
        [SerializeField] private float animationDuration = 0.2f;
        [Tooltip("Set door open at start.")]
        [SerializeField] private bool startOpen = false;

        [Header("Audio")]
        public AudioInfo openSound;
        public AudioInfo closeSound;

        private Vector3 closedPosition;
        private Vector3 openPosition;
        private Coroutine lerpCoroutine = null;

        public virtual bool IsOpen { get; protected set; } = false;

        protected virtual void Start()
        {
            closedPosition = (door != null) ? door.position : transform.position;
            openPosition = closedPosition + Vector3.up * GetDoorHeight();
            if (startOpen) SetStateInstantly(true);
        }

        protected virtual float GetDoorHeight()
        {
            if (GridController.Instance != null) return GridController.Instance.SquareSize;
            var boxCollider = GetComponentInChildren<BoxCollider>();
            if (boxCollider != null) return boxCollider.size.y;
            return 1;
        }

        /// <summary>
        /// Set the door open (raised) or closed instantly without animating its motion or playing audio.
        /// </summary>
        /// <param name="open"></param>
        public virtual void SetStateInstantly(bool open)
        {
            IsOpen = open;
            if (door != null)
            {
                door.position = open ? openPosition : closedPosition;
            }
        }

        /// <summary>
        /// Play the door open audio and animate the door to its open (raised) position.
        /// </summary>
        public virtual void Open()
        {
            if (!IsOpen)
            {
                PlayAudio(true);
                Lerp(true);
            }
        }

        /// <summary>
        /// Play the door close audio and animate the door to its closed (lowered) position.
        /// </summary>
        public virtual void Close()
        {
            if (IsOpen)
            {
                PlayAudio(false);
                Lerp(false);
            }
        }

        /// <summary>
        /// Open the door if it's closed; close the door if it's open.
        /// </summary>
        public virtual void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        protected virtual void PlayAudio(bool open)
        {
            GridController.Instance?.Audio.PlayDoor(open ? openSound : closeSound, transform.position);
        }

        protected virtual void Lerp(bool open)
        {
            IsOpen = open;
            var destinationPosition = open ? openPosition : closedPosition;
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(LerpCoroutine(destinationPosition));
        }

        protected virtual IEnumerator LerpCoroutine(Vector3 destinationPosition)
        {
            if (door != null)
            {
                if (animationDuration > 0)
                {
                    var startPosition = door.position;
                    float elapsed = 0;
                    while (elapsed < animationDuration)
                    {
                        var t = elapsed / animationDuration;
                        door.position = Vector3.Lerp(startPosition, destinationPosition, t);
                        yield return null;
                        elapsed += Time.deltaTime;
                    }
                }
                door.position = destinationPosition;
            }
        }

    }
}

// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Manages free look rotation. If the player rotates beyond 80 degrees,
    /// free look will rotate the player if Auto Turn is enabled.
    /// </summary>
    [AddComponentMenu("Grid Controller/Free Look")]
    [RequireComponent(typeof(GridController))]
    public class FreeLook : MonoBehaviour
    {

        [Tooltip("Allow free look only when Grid Controller's CanMove property is true.")]
        [SerializeField] private bool allowOnlyWhenCanMove = true;
        [Tooltip("Free look rotation sensitivity. Higher values rotate faster.")]
        [SerializeField] private float speed = 100;
        [Tooltip("Duration in seconds to snap back to original orientation when exiting free look.")]
        [SerializeField] private float snapbackDuration = 0.2f;
        [Tooltip("Automatically rotate controller 90 degrees if looking more than 80 degrees to side.")]
        [SerializeField] private bool autoTurn = true;
        [Tooltip("Restrict left/right rotation to Max Horizontal Rotation.")]
        [SerializeField] private bool limitHorizontalRotation = false;
        [EnableIfTrue("limitHorizontalRotation")]
        [Tooltip("Max left/right rotation. Only valid if Limit Horizontal Rotation is ticked; otherwise rotation is unlimited. Limited to 79 degrees because past 80 degrees controller actually turns 90 degrees.")]
        [Range(0, 79)]
        [SerializeField] private float maxHorizontalRotation = 79f;
        [Tooltip("Max up/down rotation.")]
        [Range(0, 90)]
        [SerializeField] private float maxVerticalRotation = 55f;

        private Transform cameraTransform;
        private GridController gridController;
        private Vector2 rotation = Vector2.zero;
        private Coroutine snapbackCoroutine = null;
        private bool isTurningDueToFreeLook = false;
        private bool previousEnableHeadBob;

        public bool IsRotated { get; protected set; } = false;
        public Vector2 Rotation => rotation;

        public bool AllowOnlyWhenCanMove { get => allowOnlyWhenCanMove; set => allowOnlyWhenCanMove = value; }
        public float Speed { get => speed; set => speed = value; }
        public float SnapbackDuration { get => snapbackDuration; set => snapbackDuration = value; }
        public bool AutoTurn { get => autoTurn; set => autoTurn = value; }
        public bool LimitHorizontalRotation { get => limitHorizontalRotation; set => limitHorizontalRotation = value; }
        public float MaxHorizontalRotation { get => maxHorizontalRotation; set => maxHorizontalRotation = value; }
        public float MaxVerticalRotation { get => maxVerticalRotation; set => maxVerticalRotation = value; }

        protected virtual void Awake()
        {
            gridController = GetComponent<GridController>();
            cameraTransform = GetComponentInChildren<Camera>().transform;
            if (gridController == null || cameraTransform == null) enabled = false;
        }

        protected virtual void OnEnable()
        {
            gridController.ExitingPosition += ExitRotation;
            gridController.Turning += ExitRotation;
        }

        protected virtual void OnDisable()
        {
            if (gridController == null) return;
            gridController.ExitingPosition -= ExitRotation;
            gridController.Turning -= ExitRotation;
        }

        protected virtual void Update()
        {
            if (allowOnlyWhenCanMove && !gridController.CanMove) return;
            if (gridController.Input == null) return;
            if (gridController.Input.Recenter && IsRotated)
            {
                ExitRotation();
            }
            Cursor.lockState = gridController.Input.FreeLook ? CursorLockMode.Locked : CursorLockMode.None;
            if (gridController.Input.FreeLook && snapbackCoroutine == null)
            {
                IsRotated = true;

                // Free look rotate based on LookX/LookY input:
                rotation.y += gridController.Input.LookX * speed * Time.deltaTime;
                rotation.x += -gridController.Input.LookY * speed * Time.deltaTime;
                rotation.x = Mathf.Clamp(rotation.x, -maxVerticalRotation, maxVerticalRotation);
                if (limitHorizontalRotation)
                {
                    rotation.y = Mathf.Clamp(rotation.y, -maxHorizontalRotation, maxHorizontalRotation);
                }
                cameraTransform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0);

                // If free look rotating past 80 degrees, rotate the Grid Controller's actual transform 90 degrees:
                if (autoTurn && gridController.CanTurn && Mathf.Abs(rotation.y) > 80)
                {
                    var turningLeft = rotation.y < 0;
                    isTurningDueToFreeLook = true;
                    gridController.SetCompassDirection(CompassDirectionUtility.RotateCompassDirection(gridController.CompassDirection, turningLeft));
                    rotation.y -= Mathf.Sign(rotation.y) * 90;
                    cameraTransform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0);
                }
            }
        }

        protected virtual void ExitRotation(Vector3 position)
        {
            if (isTurningDueToFreeLook)
            {
                isTurningDueToFreeLook = false;
            }
            else
            {
                ExitRotation();
            }
        }

        protected virtual void ExitRotation()
        { 
            if (IsRotated)
            {
                if (snapbackCoroutine != null)
                {
                    StopCoroutine(snapbackCoroutine);
                    gridController.EnableHeadBob = previousEnableHeadBob;
                }
                snapbackCoroutine = StartCoroutine(Snapback());
            }
        }

        protected virtual IEnumerator Snapback()
        {
            if (snapbackDuration > 0)
            {
                // Turn off head bob while we lerp back to unrotated position since
                // head bob would conflict by also controlling camera transform.
                previousEnableHeadBob = gridController.EnableHeadBob;
                gridController.EnableHeadBob = false;
                var currentRotation = cameraTransform.localRotation;
                float elapsed = 0;
                while (elapsed < snapbackDuration)
                {
                    var t = elapsed / snapbackDuration;
                    cameraTransform.localRotation = Quaternion.Lerp(currentRotation, Quaternion.identity, t);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
                gridController.EnableHeadBob = previousEnableHeadBob;
            }
            cameraTransform.localRotation = Quaternion.identity;
            rotation = Vector2.zero;
            IsRotated = false;
            snapbackCoroutine = null;
        }

    }
}

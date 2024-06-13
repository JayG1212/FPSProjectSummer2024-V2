// Copyright (c) Pixel Crushers. All rights reserved.

#if REWIRED

using UnityEngine;
using Rewired;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Implements IGridControllerInput using Guavaman's Rewired.
    /// </summary>
    [RequireComponent(typeof(GridController))]
    public class GridControllerRewiredInput : MonoBehaviour, IGridControllerInput
    {

        [Header("Rewired Settings")]
        [SerializeField] private int rewiredPlayerID = 0;

        [Header("Haptic Feedback")]
        [SerializeField] private int bounceMotorIndex = 0;
        [SerializeField] private float bounceMotorLevel = 0.5f;
        [SerializeField] private float bounceMotorDuration = 0.1f;

        [SerializeField] private string horizontalMoveAxis = "Horizontal";
        [SerializeField] private string verticalMoveAxis = "Vertical";
        [SerializeField] private string horizontalLookAxis = "HorizontalLook";
        [SerializeField] private string verticalLookAxis = "VerticalLook";
        [SerializeField] private string strafeLeftInput = "StrafeLeft";
        [SerializeField] private string strafeRightInput = "StrafeRight";
        [Tooltip("Toggles crouching.")]
        [SerializeField] private string crouchInput = "Crouch";
        [Tooltip("Cancels free look and returns camera to grid-oriented position.")]
        [SerializeField] private string recenterInput = "Recenter";
        [Tooltip("Activates interactable objects.")]
        [SerializeField] private string interactInput = "Interact";
        [Tooltip("When an input axis is held down, register new press at this frequency in seconds.")]
        [SerializeField] private float inputRepeatSpeed = 0.5f;
        [Tooltip("Don't register input if axis is within this dead zone.")]
        [SerializeField] private float inputDeadZone = 0.1f;

        [Header("Mouse")]
        [Tooltip("Mouse action to activate interactable objects when clicked.")]
        [SerializeField] private string interactMouse = "InteractMouse";
        [Tooltip("Mouse action to use free look when held.")]
        [SerializeField] private string freeLookMouse = "FreeLookMouse";
#pragma warning disable 0414 // Only used on iOS, so suppress warning.
        [Tooltip("Prevent Rewired from reading mouse on iOS. Ignored in non-iOS builds.")]
        [SerializeField] private bool disableMouseOnIOS = true;
#pragma warning restore 0414

        public bool ForwardPressed { get; protected set; }
        public bool BackwardPressed { get; protected set; }
        public bool TurnLeftPressed { get; protected set; }
        public bool TurnRightPressed { get; protected set; }
        public bool StrafeLeftPressed { get; protected set; }
        public bool StrafeRightPressed { get; protected set; }
        public bool CrouchPressed { get; protected set; }
        public bool InputRepeatReleased { get; protected set; }

        public bool FreeLook { get; protected set; }
        public float LookX { get; protected set; }
        public float LookY { get; protected set; }
        public bool Recenter { get; protected set; }

        public bool Interact { get; protected set; }
        public bool MouseInteract { get; protected set; }
        public Vector2 MousePosition { get; protected set; }

        public int BounceMotorIndex => bounceMotorIndex;
        public float BounceMotorLevel => bounceMotorLevel;
        public float BounceMotorDuration => bounceMotorDuration;
        public string HorizontalMoveAxis => horizontalMoveAxis;
        public string VerticalMoveAxis => verticalMoveAxis;
        public string HorizontalLookAxis => horizontalLookAxis;
        public string VerticalLookAxis => verticalLookAxis;
        public string StrafeLeftInput => strafeLeftInput;
        public string StrafeRightInput => strafeRightInput;
        public string CrouchInput => crouchInput;
        public string RecenterInput => recenterInput;
        public string InteractInput => interactInput;
        public float InputRepeatSpeed => inputRepeatSpeed;
        public float InputDeadZone => inputDeadZone;
        public string InteractMouse => interactMouse;
        public string FreeLookMouse => freeLookMouse;

        public bool IsUsingJoystick => WasLastControllerJoystick();

        private Rewired.Player rewiredPlayer = null;

        protected float durationForwardHeld;
        protected float durationBackwardHeld;
        protected float durationTurnLeftHeld;
        protected float durationTurnRightHeld;
        protected float durationStrafeLeftHeld;
        protected float durationStrafeRightHeld;
        protected bool isRepeatingForward;
        protected bool isRepeatingBackward;
        protected bool isRepeatingTurnLeft;
        protected bool isRepeatingTurnRight;
        protected bool isRepeatingStrafeLeft;
        protected bool isRepeatingStrafeRight;

        protected virtual void Start()
        {
            rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerID);
#if UNITY_IOS && !UNITY_EDITOR
            if (disableMouseOnIOS)
            {
                Rewired.ReInput.controllers.Mouse.enabled = false;
            }
#endif
        }

        protected virtual void Update()
        {
            CheckInput();
        }

        protected virtual bool WasLastControllerJoystick()
        {
            if (rewiredPlayer == null) return false;
            var lastController = rewiredPlayer.controllers.GetLastActiveController();
            return lastController != null && lastController.type == ControllerType.Joystick;
        }

        protected virtual void CheckInput()
        {
            if (rewiredPlayer == null) return;

            InputRepeatReleased = false;

            var vertical = rewiredPlayer.GetAxisRaw(verticalMoveAxis);
            var horizontal = rewiredPlayer.GetAxisRaw(horizontalMoveAxis);
            ForwardPressed = CheckInputAxis(vertical, true, ref durationForwardHeld, ref isRepeatingForward);
            BackwardPressed = CheckInputAxis(vertical, false, ref durationBackwardHeld, ref isRepeatingBackward);
            TurnLeftPressed = CheckInputAxis(horizontal, false, ref durationTurnLeftHeld, ref isRepeatingTurnLeft);
            TurnRightPressed = CheckInputAxis(horizontal, true, ref durationTurnRightHeld, ref isRepeatingTurnRight);

            StrafeLeftPressed = CheckInputButton(strafeLeftInput, ref durationStrafeLeftHeld, ref isRepeatingStrafeLeft);
            StrafeRightPressed = CheckInputButton(strafeRightInput, ref durationStrafeRightHeld, ref isRepeatingStrafeRight);

            CrouchPressed = string.IsNullOrEmpty(crouchInput) ? false : rewiredPlayer.GetButtonDown(crouchInput);

            if (IsUsingJoystick)
            {
                var lookHorizontal = rewiredPlayer.GetAxis(horizontalLookAxis);
                var lookVertical = rewiredPlayer.GetAxis(verticalLookAxis);
                FreeLook = (Mathf.Abs(lookVertical) > inputDeadZone) || (Mathf.Abs(lookHorizontal) > inputDeadZone);
                LookY = FreeLook ? lookVertical : 0;
                LookX = FreeLook ? lookHorizontal : 0;
            }
            else
            {
                FreeLook = rewiredPlayer.GetButton(freeLookMouse); ;
                LookX = FreeLook ? rewiredPlayer.GetAxis(horizontalLookAxis) : 0;
                LookY = FreeLook ? rewiredPlayer.GetAxis(verticalLookAxis) : 0;
            }

            Recenter = !string.IsNullOrEmpty(recenterInput) ? rewiredPlayer.GetButtonDown(recenterInput) : false;
            if (Recenter) FreeLook = false;

            Interact = !string.IsNullOrEmpty(interactInput) ? rewiredPlayer.GetButtonDown(interactInput) : false;
            MouseInteract = rewiredPlayer.GetButtonDown(interactMouse);
            MousePosition = Input.mousePresent ? Input.mousePosition : new Vector3(Screen.width / 2, Screen.height / 2, 0);
        }

        /// <summary>
        /// Registers "pressed" if pressed this frame or held for the input repeat duration.
        /// </summary>
        protected bool CheckInputAxis(float axis, bool positive, ref float durationHeld, ref bool isRepeating)
        {
            if ((positive && axis > inputDeadZone) || (!positive && axis < -inputDeadZone))
            {
                if (durationHeld == 0)
                {
                    // Just pressed direction this frame, so count as press:
                    durationHeld = Time.deltaTime;
                    isRepeating = false;
                    return true;
                }
                else
                {
                    // Otherwise update duration held, counting as press when it reaches inputRepeatSpeed:
                    durationHeld += Time.deltaTime;
                    if (durationHeld >= inputRepeatSpeed)
                    {
                        durationHeld -= inputRepeatSpeed;
                        isRepeating = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Otherwise record that the direction is not held:
                durationHeld = 0;
                if (isRepeating)
                {
                    InputRepeatReleased = true;
                    isRepeating = false;
                }
                return false;
            }
        }

        protected bool CheckInputButton(string input, ref float durationHeld, ref bool isRepeating)
        {
            if (rewiredPlayer.GetButtonDown(input))
            {
                // If pressed this frame, count as press:
                durationHeld = Time.deltaTime;
                isRepeating = false;
                return true;
            }
            else if (rewiredPlayer.GetButton(input))
            {
                // Otherwise update duration held, counting as press when it reaches inputRepeatSpeed:
                durationHeld += Time.deltaTime;
                if (durationHeld >= inputRepeatSpeed)
                {
                    durationHeld -= inputRepeatSpeed;
                    isRepeating = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Otherwise record that the button is not held:
                durationHeld = 0;
                if (isRepeating)
                {
                    InputRepeatReleased = true;
                    isRepeating = false;
                }
                return false;
            }
        }

        public virtual void BounceFeedback() // No bounce feedback implemented for built-in input manager.
        {
            if (rewiredPlayer == null) return;
            rewiredPlayer.SetVibration(bounceMotorIndex, bounceMotorLevel, bounceMotorDuration);
        }

    }
}

#endif

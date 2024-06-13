// Copyright (c) Pixel Crushers. All rights reserved.

#if ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Implements IGridControllerInput using Unity's Input System package.
    /// </summary>
    [AddComponentMenu("Grid Controller/Grid Controller Input System")]
    [RequireComponent(typeof(GridController))]
    [RequireComponent(typeof(PlayerInput))]
    public class GridControllerInputSystem : MonoBehaviour, IGridControllerInput
    {

        [SerializeField] private InputActionReference horizontalMoveAxis;
        [SerializeField] private InputActionReference verticalMoveAxis;
        [SerializeField] private InputActionReference horizontalLookAxis;
        [SerializeField] private InputActionReference verticalLookAxis;
        [SerializeField] private InputActionReference strafeLeftAction;
        [SerializeField] private InputActionReference strafeRightAction;
        [Tooltip("Enters and exits crouch.")]
        [SerializeField] private InputActionReference crouchAction;
        [Tooltip("Cancels free look and returns camera to grid-oriented position.")]
        [SerializeField] private InputActionReference recenterAction;
        [Tooltip("Activates interactable objects.")]
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("When an input axis is held down, register new press at this frequency in seconds.")]
        [SerializeField] private float inputRepeatSpeed = 0.5f;
        [Tooltip("Don't register input if axis is within this dead zone.")]
        [SerializeField] private float inputDeadZone = 0.1f;

        [Header("Haptic Feedback")]
        [SerializeField] private float bounceHapticLowFrequencySpeed = 0.5f;
        [SerializeField] private float bounceHapticHighFrequencySpeed = 0.5f;
        [SerializeField] private float bounceHapticDuration = 0.2f;

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

        public bool IsUsingJoystick { get { return playerInput != null && playerInput.currentControlScheme.Equals("Gamepad"); } }

        private PlayerInput playerInput;
        private float durationForwardHeld;
        private float durationBackwardHeld;
        private float durationTurnLeftHeld;
        private float durationTurnRightHeld;
        private float durationStrafeLeftHeld;
        private float durationStrafeRightHeld;
        private bool isRepeatingForward;
        private bool isRepeatingBackward;
        private bool isRepeatingTurnLeft;
        private bool isRepeatingTurnRight;
        private bool isRepeatingStrafeLeft;
        private bool isRepeatingStrafeRight;

        protected virtual void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput.actions == null) Debug.LogWarning("You must assign an InputActionsAsset to PlayerInput", this);
        }

        protected virtual void Update()
        {
            CheckInput();
        }

        protected virtual void CheckInput()
        {
            InputRepeatReleased = false;

            var vertical = verticalMoveAxis.action.ReadValue<float>();
            var horizontal = horizontalMoveAxis.action.ReadValue<float>();
            ForwardPressed = CheckInputAxis(vertical, true, ref durationForwardHeld, ref isRepeatingForward);
            BackwardPressed = CheckInputAxis(vertical, false, ref durationBackwardHeld, ref isRepeatingBackward);
            TurnLeftPressed = CheckInputAxis(horizontal, false, ref durationTurnLeftHeld, ref isRepeatingTurnLeft);
            TurnRightPressed = CheckInputAxis(horizontal, true, ref durationTurnRightHeld, ref isRepeatingTurnRight);

            StrafeLeftPressed = CheckInputButton(strafeLeftAction, ref durationStrafeLeftHeld, ref isRepeatingStrafeLeft);
            StrafeRightPressed = CheckInputButton(strafeRightAction, ref durationStrafeRightHeld, ref isRepeatingStrafeRight);

            CrouchPressed = (crouchAction == null) ? false : crouchAction.action.triggered;

            if (IsUsingJoystick)
            {
                var lookHorizontal = horizontalLookAxis.action.ReadValue<float>();
                var lookVertical = verticalLookAxis.action.ReadValue<float>();
                FreeLook = (Mathf.Abs(lookVertical) > inputDeadZone) || (Mathf.Abs(lookHorizontal) > inputDeadZone);
                LookY = FreeLook ? lookVertical : 0;
                LookX = FreeLook ? lookHorizontal : 0;
            }
            else
            {
                FreeLook = Mouse.current.rightButton.isPressed;
                LookX = FreeLook ? horizontalLookAxis.action.ReadValue<float>() : 0;
                LookY = FreeLook ? verticalLookAxis.action.ReadValue<float>() : 0;
            }

            Recenter = (recenterAction == null) ? false : recenterAction.action.WasPressedThisFrame();
            if (Recenter) FreeLook = false;

            Interact = (interactAction == null) ? false : interactAction.action.WasPressedThisFrame();

            MouseInteract = Mouse.current.leftButton.wasPressedThisFrame;
            MousePosition = Mouse.current.position.ReadDefaultValue();
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

        protected bool CheckInputButton(InputActionReference inputAction, ref float durationHeld, ref bool isRepeating)
        {
            if (inputAction.action.WasPressedThisFrame())
            {
                // If pressed this frame, count as press:
                durationHeld = Time.deltaTime;
                isRepeating = false;
                return true;
            }
            else if (inputAction.action.IsPressed())
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
            if (IsUsingJoystick)
            {
                CancelInvoke(nameof(StopBounceFeedback));
                Invoke(nameof(StopBounceFeedback), bounceHapticDuration);
                Gamepad.current.SetMotorSpeeds(bounceHapticLowFrequencySpeed, bounceHapticHighFrequencySpeed);
            }
        }

        protected virtual void StopBounceFeedback()
        {
            Gamepad.current.SetMotorSpeeds(0, 0);
        }

    }
}

#endif

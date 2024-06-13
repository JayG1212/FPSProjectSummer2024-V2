// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Implements IGridControllerInput using Unity's built-in input manager.
    /// Works with keyboard+mouse and joystick.
    /// </summary>
    [AddComponentMenu("Grid Controller/Grid Controller Input")]
    [RequireComponent(typeof(GridController))]
    public class GridControllerInput : MonoBehaviour, IGridControllerInput
    {

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
        [SerializeField] private string interactInput = "Jump";
        [Tooltip("When an input axis is held down, register new press at this frequency in seconds.")]
        [SerializeField] private float inputRepeatSpeed = 0.5f;
        [Tooltip("Don't register input if axis is within this dead zone.")]
        [SerializeField] private float inputDeadZone = 0.1f;

        [Header("Mouse")]
        [Tooltip("Mouse button to activate interactable objects when clicked.")]
        [SerializeField] private int interactMouseButton = 0;
        [Tooltip("Mouse button to use free look when held.")]
        [SerializeField] private int freeLookMouseButton = 1;

        [Header("Joystick")]
        [Tooltip("Switch to joystick mode (read input from joystick) when detecting this input.")]
        [SerializeField] private string joystickHorizontalAxis = "JoystickHorizontal";
        [Tooltip("Switch to joystick mode (read input from joystick) when detecting this input.")]
        [SerializeField] private string joystickVerticalAxis = "JoystickVertical";

        [Header("Optimization")]
        [Tooltip("Assume you've clicked Check Input Settings and inputs defined above are all defined in Input Manager. Eliminates overhead of checking for missing inputs.")]
        [SerializeField] private bool assumeInputsDefined = false;

        public bool IsUsingJoystick { get; protected set; }

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
        public int InteractMouseButton => interactMouseButton;
        public int FreeLookMouseButton => freeLookMouseButton;
        public string JoystickHorizontalAxis => joystickHorizontalAxis;
        public string JoystickVerticalAxis => joystickVerticalAxis;

        protected Vector3 lastMousePosition;
        protected const float MouseMovementThreshold = 32; // Mouse must move 32 pixels to consider in use (in case player bumps mouse while using joytick).
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

        protected bool hasReportedMissingInput = false;

        protected virtual void Start()
        {
            IsUsingJoystick = false;
            lastMousePosition = Input.mousePosition;
        }

        protected virtual void Update()
        {
            CheckInputDevices();
            CheckInput();
        }

        protected virtual void CheckInput()
        {
            InputRepeatReleased = false;

            var vertical = GetAxisRaw(verticalMoveAxis);
            var horizontal = GetAxisRaw(horizontalMoveAxis);
            ForwardPressed = CheckInputAxis(vertical, true, ref durationForwardHeld, ref isRepeatingForward);
            BackwardPressed = CheckInputAxis(vertical, false, ref durationBackwardHeld, ref isRepeatingBackward);
            TurnLeftPressed = CheckInputAxis(horizontal, false, ref durationTurnLeftHeld, ref isRepeatingTurnLeft);
            TurnRightPressed = CheckInputAxis(horizontal, true, ref durationTurnRightHeld, ref isRepeatingTurnRight);

            StrafeLeftPressed = CheckInputButton(strafeLeftInput, ref durationStrafeLeftHeld, ref isRepeatingStrafeLeft);
            StrafeRightPressed = CheckInputButton(strafeRightInput, ref durationStrafeRightHeld, ref isRepeatingStrafeRight);

            CrouchPressed = string.IsNullOrEmpty(crouchInput) ? false : GetButtonDown(crouchInput);

            if (IsUsingJoystick)
            {
                var lookHorizontal = GetAxis(horizontalLookAxis);
                var lookVertical = GetAxis(verticalLookAxis);
                FreeLook = (Mathf.Abs(lookVertical) > inputDeadZone) || (Mathf.Abs(lookHorizontal) > inputDeadZone);
                LookY = FreeLook ? lookVertical : 0;
                LookX = FreeLook ? lookHorizontal : 0;
            }
            else
            {
                FreeLook = Input.mousePresent ? Input.GetMouseButton(freeLookMouseButton) : false;
                LookX = FreeLook ? GetAxis(horizontalLookAxis) : 0;
                LookY = FreeLook ? GetAxis(verticalLookAxis) : 0;
            }

            Recenter = !string.IsNullOrEmpty(recenterInput) ? GetButtonDown(recenterInput) : false;
            if (Recenter) FreeLook = false;

            Interact = !string.IsNullOrEmpty(interactInput) ? GetButtonDown(interactInput) : false;
            MouseInteract = Input.mousePresent && Input.GetMouseButtonDown(interactMouseButton);
            MousePosition = Input.mousePresent ? Input.mousePosition : new Vector3(Screen.width / 2, Screen.height / 2, 0);
        }

        protected void ReportMissingInput()
        {
            if (hasReportedMissingInput) return;
            hasReportedMissingInput = true;
            Debug.LogError($"{GetType().Name}: Some inputs are missing from Unity's input manager. Click the Check Input Settings button to add them.", this);
            var prefab = Resources.Load<GameObject>("MissingInputCanvas");
            if (prefab != null)
            {
                Instantiate(prefab);
            }
        }

        protected float GetAxisRaw(string axisName)
        {
            if (assumeInputsDefined)
            {
                return Input.GetAxisRaw(axisName);
            }
            else
            {
                try
                {
                    return Input.GetAxisRaw(axisName);
                }
                catch (System.ArgumentException)
                {
                    ReportMissingInput();
                    return 0;
                }
            }
        }

        protected float GetAxis(string axisName)
        {
            if (assumeInputsDefined)
            {
                return Input.GetAxis(axisName);
            }
            else
            {
                try
                {
                    return Input.GetAxis(axisName);
                }
                catch (System.ArgumentException)
                {
                    ReportMissingInput();
                    return 0;
                }
            }
        }

        protected bool GetButton(string buttonName)
        {
            if (assumeInputsDefined)
            {
                return Input.GetButton(buttonName);
            }
            else
            {
                try
                {
                    return Input.GetButton(buttonName);
                }
                catch (System.ArgumentException)
                {
                    ReportMissingInput();
                    return false;
                }
            }
        }

        protected bool GetButtonDown(string buttonName)
        {
            if (assumeInputsDefined)
            {
                return Input.GetButtonDown(buttonName);
            }
            else
            {
                try
                {
                    return Input.GetButtonDown(buttonName);
                }
                catch (System.ArgumentException)
                {
                    ReportMissingInput();
                    return false;
                }
            }
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
            if (GetButtonDown(input))
            {
                // If pressed this frame, count as press:
                durationHeld = Time.deltaTime;
                isRepeating = false;
                return true;
            }
            else if (GetButton(input))
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

        /// <summary>
        /// Determines if using joystick or keyboard+mouse by checking mouse movement and joystick axis input.
        /// </summary>
        protected virtual void CheckInputDevices()
        {
            if (IsUsingJoystick)
            {
                if (Input.mousePresent && ((Input.mousePosition - lastMousePosition).magnitude > MouseMovementThreshold))
                {
                    lastMousePosition = Input.mousePosition;
                    IsUsingJoystick = false;
                }
            }
            else if (!(string.IsNullOrEmpty(joystickHorizontalAxis) || string.IsNullOrEmpty(joystickVerticalAxis)))
            {
                if (Mathf.Abs(GetAxis(joystickHorizontalAxis)) >= inputDeadZone ||
                    Mathf.Abs(GetAxis(joystickVerticalAxis)) >= inputDeadZone)
                {
                    lastMousePosition = Input.mousePosition;
                    IsUsingJoystick = true;
                }
            }
        }

        /// <summary>
        /// No bounce feedback implemented for built-in input manager.
        /// </summary>
        public virtual void BounceFeedback()
        { 
        }

    }
}

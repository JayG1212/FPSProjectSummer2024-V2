// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.GridController
{

    public enum MovementCommands { Forward, Backward, TurnLeft, TurnRight, StrafeLeft, StrafeRight }

    public enum GridControllerUpdateMode { Realtime, TurnBased }

    /// <summary>
    /// Main Grid Controller class. Handles grid-based movement.
    /// Has a static property Instance.
    /// </summary>
    [AddComponentMenu("Grid Controller/Grid Controller")]
    public class GridController : MonoBehaviour
    {

        #region Fields

        [Header("Movement")]
        [Tooltip("Realtime: Controller processes input immediately.\nTurn-Based: Controller stores last input until you manually call ProcessTurn() method.")]
        [SerializeField] private GridControllerUpdateMode updateMode = GridControllerUpdateMode.Realtime;
        [Tooltip("Size of each grid square. Controller moves this many units when moving to a new square.")]
        [SerializeField] private int squareSize = 1;
        [Tooltip("Offset grid by this distance (e.g., if it's not perfectly aligned to (0,0)). Only used by Enforce Grid Position.")]
        [SerializeField] private Vector3 gridOffset = Vector3.zero;
        [Tooltip("After moving, ensure controller is in center of square.")]
        [SerializeField] private bool enforceGridPosition = true;
        [Tooltip("Turn left/right over this duration in seconds. Not used if Snap Movement is ticked.")]
        [SerializeField] private float rotationDuration = 0.2f;
        [Tooltip("Move to new square over this duration in seconds. Not used if Snap Movement is ticked.")]
        [SerializeField] private float movementDuration = 0.2f;
        [Tooltip("Jump to new position instead of transitioning smoothly from current position to new position.")]
        [SerializeField] private bool snapMovement = false;
        [Tooltip("Allow player to queue movement inputs to be executed when movement allows. Only applies to Runtime update mode. Turn-based always uses most recent input.")]
        [SerializeField] private bool queueInput = true;

        [Header("Head Bob")]
        [Tooltip("Adjust camera to simulate head bob when moving. Ignored when Snap Movement is ticked.")]
        [SerializeField] private bool enableHeadBob = false;
        [EnableIfTrue("enableHeadBob")]
        [Tooltip("When bobbing head, point camera to a fixed point this far ahead.")]
        [SerializeField] private float headBobFocusDistance = 15f;
        [EnableIfTrue("enableHeadBob")]
        [Tooltip("Perform this many bobs with each movement into a new square.")]
        [SerializeField] private int headBobsPerSquare = 1;
        [EnableIfTrue("enableHeadBob")]
        [Tooltip("Height of head bob.")]
        [SerializeField] private float headBobHeight = 0.1f;

        [Header("Crouching")]
        [Tooltip("Allow player to enter and exit crouch.")]
        [SerializeField] private bool allowCrouch = false;
        [EnableIfTrue("allowCrouch")]
        [Tooltip("When crouched, move camera down to this proportion of original camera height.")]
        [SerializeField] private float crouchScaleY = 0.625f;
        [EnableIfTrue("allowCrouch")]
        [Tooltip("Smoothly transition camera from current height to new height over this duration in seconds. Not used if Snap Movement is ticked.")]
        [SerializeField] private float crouchTransitionDuration = 0.1f;
        [EnableIfTrue("allowCrouch")]
        [Tooltip("Reduce foot volume to this proportion of original volume when crouching.")]
        [SerializeField] private float crouchingFootstepVolume = 0.25f;

        [Header("Turning")]
        [Tooltip("Allow turning only when CanMove property is true.")]
        [SerializeField] private bool turnCountsAsMove = true;
        [Tooltip("Play footstep sound at this proportion of original volume when turning.")]
        [SerializeField] private float turningFootstepVolume = 0.1f;

        [Header("Collisions")]
        [Tooltip("To determine if direction of movement is blocked, run boxcast centered this high from player's origin.")]
        [SerializeField] private float boxcastHeight = 0.6f;
        [Tooltip("Size of boxcast.")]
        [SerializeField] private Vector3 boxcastExtents = new Vector3(0.25f, 0.25f, 0.25f);
        [Tooltip("Block movement if slope is steeper than this angle.")]
        [Range(0, 89)]
        [SerializeField] private float maxSlope = 45f;
        [Tooltip("Additional offset from Boxcast Height at which to start downward raycast to detect floor/slopes.")]
        [SerializeField] private float floorDetectionOffset = 0f;
        [Tooltip("Raycast from (origin + Boxcast Height + Floor Detection Offset) down this far to detect floor.")]
        [SerializeField] private float floorRaycastDistance = 10f;
        [Tooltip("If boxcast hits object on this layer mask, register as hit with wall or other movement-blocking barrier.")]
        [SerializeField] private LayerMask wallLayerMask = ~0 - (1 << 1) - (1 << 2) - (1 << 4) - (1 << 5); // TransparentFX, IgnoreRaycast, Water, UI
        [Tooltip("Raycast down to detect floor on this layer mask.")]
        [SerializeField] private LayerMask floorLayerMask = ~0 - (1 << 1) - (1 << 2) - (1 << 4) - (1 << 5); // TransparentFX, IgnoreRaycast, Water, UI
        [Tooltip("When hitting wall, bounce in direction of movement and back by this proportion of full square size.")]
        [SerializeField] private float bounceAmount = 0.125f;
        [Tooltip("When hitting wall while trying to move backward, bounce by this proportion of full square size.")]
        [SerializeField] private float bounceBackwardAmount = 0.08f;

        [Header("Debug")]
        [Tooltip("Show raycast gizmos in Scene view.")]
        [SerializeField] private bool showRaycasts = false;

        [Header("Runtime Info")]
        [ReadOnly] [SerializeField] private CompassDirection compassDirection;
        [ReadOnly] [SerializeField] private bool canMove = true;
        [ReadOnly] [SerializeField] private bool isMoving = false;
        [ReadOnly] [SerializeField] private bool isCrouching = false;

#pragma warning disable 0414 // Only used internally, so suppress warning.
        [HideInInspector] [SerializeField] private bool checkForAuxiliaryComponents = true;
#pragma warning restore 0414

        private const int MaxHits = 8;
        private RaycastHit[] hits = new RaycastHit[MaxHits];
        private Queue<MovementCommands> movementCommandQueue = new Queue<MovementCommands>();
        private Transform cachedTransform; // Caching transform is still slightly faster than using optimized transform property.
        private Transform cameraTransform;
        private Transform cameraParent;
        private float standingCameraY;

        private Coroutine crouchCoroutine;
        private Surface currentSurface = null;
        private WaitForSeconds movementDurationDelay = null;
        private int frameCountFromLastMove = 0;
        private float leftoverDeltaFromLastMove;
        private Vector3 moveVectorFromLastMove;

        private const float ShowRaycastsDuration = 3;
        private Vector3? wallCollisionPosition = null;
        private Vector3? floorCollisionPosition = null;

        #endregion

        #region Events

        /// <summary>
        /// Invoked prior to leaving a square. Parameter is square that controller is about to leave.
        /// </summary>
        public event System.Action<Vector3> ExitingPosition = null;

        /// <summary>
        /// Invoked after entering a new square. Parameter is square that controller just entered.
        /// </summary>
        public event System.Action<Vector3> EnteredPosition = null;

        /// <summary>
        /// Invoked prior to rotating in place. Paramneter is controller's current position.
        /// </summary>
        public event System.Action<Vector3> Turning = null;

        /// <summary>
        /// Rotated in place. Parameter is controller's current position.
        /// </summary>
        public event System.Action<Vector3> Turned = null;

        /// <summary>
        /// Strafed (side-stepped) to a new square. Parameter is square that controller just entered.
        /// </summary>
        public event System.Action<Vector3> Strafed = null;

        /// <summary>
        /// Entered crouch.
        /// </summary>
        public event System.Action<Vector3> Crouched = null;

        /// <summary>
        /// Exited crouch.
        /// </summary>
        public event System.Action<Vector3> Uncrouched = null;

        /// <summary>
        /// Tried to move into a square that's blocked (e.g., by wall).
        /// </summary>
        public event System.Action<Surface> Blocked = null;

        #endregion

        #region Public Accessor Properties For Private Variables

        public IGridControllerInput Input { get; private set; }
        public IGridControllerAudio Audio { get; private set; }

        /// <summary>
        /// Realtime: Controller processes input immediately.\nTurn-Based: Controller stores last input until you manually call ProcessTurn() method.
        /// </summary>
        public GridControllerUpdateMode UpdateMode { get => updateMode; set => updateMode = value; }

        /// <summary>
        /// Size of each grid square. Controller moves this many units when moving to a new square.
        /// </summary>
        public int SquareSize { get => squareSize; set => squareSize = value; }

        /// <summary>
        /// Offset grid by this distance (e.g., if it's not perfectly aligned to (0,0)). Only used by Enforce Grid Position.
        /// </summary>
        public Vector3 GridOffset { get => gridOffset; set => gridOffset = value; }

        /// <summary>
        /// After moving, ensure controller is in center of square.
        /// </summary>
        public bool EnforceGridPosition { get => enforceGridPosition; set => enforceGridPosition = value; }

        /// <summary>
        /// Turn left/right over this duration in seconds. Not used if Snap Movement is ticked.
        /// </summary>
        public float RotationDuration { get => rotationDuration; set => rotationDuration = value; }

        /// <summary>
        /// Move to new square over this duration in seconds. Not used if Snap Movement is ticked.
        /// </summary>
        public float MovementDuration { get => movementDuration; set => movementDuration = value; }

        /// <summary>
        /// Jump to new position instead of transitioning smoothly from current position to new position.
        /// </summary>
        public bool SnapMovement { get => snapMovement; set => snapMovement = value; }

        /// <summary>
        /// Adjust camera to simulate head bob when moving. Ignored when Snap Movement is ticked.
        /// </summary>
        public bool EnableHeadBob { get => enableHeadBob; set => enableHeadBob = value; }

        /// <summary>
        /// When bobbing head, point camera to a fixed point this far ahead.
        /// </summary>
        public float HeadBobFocusDistance { get => headBobFocusDistance; set => headBobFocusDistance = value; }

        /// <summary>
        /// Perform this many bobs with each movement into a new square.
        /// </summary>
        public int HeadBobsPerSquare { get => headBobsPerSquare; set => headBobsPerSquare = value; }

        /// <summary>
        /// Height of head bob.
        /// </summary>
        public float HeadBobHeight { get => headBobHeight; set => headBobHeight = value; }

        /// <summary>
        /// Allow player to enter and exit crouch.
        /// </summary>
        public bool AllowCrouch { get => allowCrouch; set => allowCrouch = value; }

        /// <summary>
        /// When crouched, move camera down to this proportion of original camera height.
        /// </summary>
        public float CrouchScaleY { get => crouchScaleY; set => crouchScaleY = value; }

        /// <summary>
        /// Smoothly transition camera from current height to new height over this duration in seconds. Not used if Snap Movement is ticked.
        /// </summary>
        public float CrouchTransitionDuration { get => crouchTransitionDuration; set => crouchTransitionDuration = value; }

        /// <summary>
        /// Reduce foot volume to this proportion of original volume when crouching.
        /// </summary>
        public float CrouchingFootstepVolume { get => crouchingFootstepVolume; set => crouchingFootstepVolume = value; }

        /// <summary>
        /// If true, queue input for processing in sequence. If false, accept one input at a time.
        /// </summary>
        public bool QueueInput { get => queueInput; set => queueInput = value; }

        /// <summary>
        /// Compass direction that grid controller is currently facing.
        /// </summary>
        public CompassDirection CompassDirection => compassDirection;

        /// <summary>
        /// To determine if direction of movement is blocked, run boxcast centered this high from player's origin.
        /// </summary>
        public float BoxcastHeight { get => boxcastHeight; set => boxcastHeight = value; }

        /// <summary>
        /// Size of boxcast for obstacle detection.
        /// </summary>
        public Vector3 BoxcastExtents { get => boxcastExtents; set => boxcastExtents = value; }

        /// <summary>
        /// Block movement if slope is steeper than this angle.
        /// </summary>
        public float MaxSlope { get => MaxSlope; set => MaxSlope = value; }

        /// <summary>
        /// Additional offset from Boxcast Height at which to start downward raycast to detect floor/slopes.
        /// </summary>
        public float FloorDetectionOffset { get => floorDetectionOffset; set => floorDetectionOffset = value; }

        /// <summary>
        /// Raycast from (origin + Boxcast Height + Floor Detection Offset) down this far to detect floor.
        /// </summary>
        public float FloorRaycastDistance { get => floorRaycastDistance; set => floorRaycastDistance = value; }

        /// <summary>
        /// Detect wall colliders on these layers.
        /// </summary>
        public LayerMask WallLayerMask { get => wallLayerMask; set => wallLayerMask = value; }

        /// <summary>
        /// Detect floor colliders on these layers.
        /// </summary>
        public LayerMask FloorLayerMask { get => floorLayerMask; set => floorLayerMask = value; }

        /// <summary>
        /// When trying to move forward or strafing into a wall, play bounce animation this distance.
        /// </summary>
        public float BounceAmount { get => bounceAmount; set => bounceAmount = value; }

        /// <summary>
        /// When trying to move backward into a wall, play bounce animation this distance.
        /// </summary>
        public float BounceBackwardAmount { get => bounceBackwardAmount; set => bounceBackwardAmount = value; }

        /// <summary>
        /// When in turn-based mode, if true process a single turn as a move. If false,
        /// process turns for free.
        /// </summary>
        public bool TurnCountsAsMove { get => turnCountsAsMove; set => turnCountsAsMove = value; }

        /// <summary>
        /// Play footstep sound at this proportion of original volume when turning.
        /// </summary>
        public float TurningFootstepVolume { get => turningFootstepVolume; set => turningFootstepVolume = value; }

        /// <summary>
        /// Surface detected on floor of current grid position.
        /// </summary>
        public Surface CurrentFloorSurface => currentSurface;

        /// <summary>
        /// Show debug gizmos for floor, wall, and ceiling detection raycasts.
        /// </summary>
        public bool ShowRaycasts { get => showRaycasts; set => showRaycasts = value; }

        /// <summary>
        /// Specifies whether the grid controller is allowed to move or not.
        /// </summary>
        public bool CanMove
        {
            get => canMove;
            set { canMove = value; if (!canMove) Stop(); }
        }

        /// <summary>
        /// True if the grid controller is allowed to move or TurnCountsAsMove is false.
        /// </summary>
        public bool CanTurn
        {
            get { return CanMove || !turnCountsAsMove; }
        }

        /// <summary>
        /// True if the grid controller is currently in the process of moving to a new square.
        /// </summary>
        public bool IsMoving => isMoving;

        /// <summary>
        /// True if the grid controller is in the crouched position (vs. standing).
        /// </summary>
        public bool IsCrouching => isCrouching;

        /// <summary>
        /// Last attempted movement vector, which may have been unsuccessful if blocked.
        /// Also includes vertical movement for crouching and standing.
        /// </summary>
        public Vector3 LastAttemptedMoveVector { get; protected set; } = Vector3.zero;

        /// <summary>
        /// Checks if any movement commands are currently queued.
        /// </summary>
        public bool AreAnyCommandsWaitingInQueue { get { return movementCommandQueue.Count > 0; } }

        /// <summary>
        /// Reference to the most recent GridController to initialize itself in Awake.
        /// </summary>
        public static GridController Instance { get; protected set; }

        #endregion

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            // Initialize static data in case domain reloads are disabled:
            Instance = null;
        }
#endif

        protected virtual void Awake()
        {
            Instance = this;
            cachedTransform = transform;
            Input = GetComponent<IGridControllerInput>();
            Audio = GetComponent<IGridControllerAudio>();
            cameraTransform = GetComponentInChildren<Camera>().transform;
            standingCameraY = (cameraTransform != null) ? cameraTransform.localPosition.y : 1;
            CreateCameraParent();
            if (cameraTransform == null) Debug.LogWarning("GridController requires a camera. No camera found in GridController's hierarchy.", this);
            if (Input == null) Debug.LogWarning("GridController requires an input component. Add GridControllerInput if you're using Unity's built-in input manager.", this);
            if (Audio == null) Debug.LogWarning("GridController recommends an audio component. Add GridControllerAudio to use Unity's built-in audio manager.", this);
        }

        protected virtual void Start()
        {
            compassDirection = CompassDirectionUtility.AngleToCompassDirection(cachedTransform.rotation.eulerAngles.y);
        }

        protected virtual void Update()
        {
            switch (updateMode)
            {
                case GridControllerUpdateMode.Realtime:
                    if (!IsMoving) ProcessMovementCommandQueue();
                    if (CanMove) ReadMovementInput();
                    break;
                case GridControllerUpdateMode.TurnBased:
                    if (CanMove) ReadMovementInput();
                    // When turn-based, move in ProcessTurn().
                    break;
            }
        }

        /// <summary>
        /// Stop movement and clear any queued movement commands. Often called
        /// when an encounter occurs such as entering combat.
        /// </summary>
        public void Stop()
        {
            ClearMovementCommandQueue();
            isMoving = false;
        }

        /// <summary>
        /// Process queued movement command. Call to process movement when update mode is turn-based.
        /// </summary>
        public void ProcessTurn()
        {
            if (updateMode == GridControllerUpdateMode.TurnBased)
            {
                ProcessMovementCommandQueue();
            }
        }

        /// <summary>
        /// Instantly move controller to new position and compass direction.
        /// </summary>
        public void Teleport(Vector3 position, CompassDirection compassDirection)
        {
            StopAllCoroutines();
            ExitingPosition?.Invoke(cachedTransform.position);
            this.compassDirection = compassDirection;
            cachedTransform.SetPositionAndRotation(GetGridPosition(position),
                Quaternion.Euler(0, CompassDirectionUtility.CompassDirectionToAngle(compassDirection), 0));
            EnteredPosition?.Invoke(position);
            Stop();
        }

        /// <summary>
        /// Instantly change controller's compass direction.
        /// </summary>
        public virtual void SetCompassDirection(CompassDirection newCompassDirection)
        {
            if (newCompassDirection != compassDirection)
            {
                Turning?.Invoke(cachedTransform.position);
                compassDirection = newCompassDirection;
                var newRotation = Quaternion.Euler(0, CompassDirectionUtility.CompassDirectionToAngle(newCompassDirection), 0);
                cachedTransform.rotation = newRotation;
                Turned?.Invoke(cachedTransform.position);
            }
        }

        /// <summary>
        /// Toggle between crouching and standing.
        /// </summary>
        public virtual void ToggleCrouching()
        {
            SetCrouching(!isCrouching);
        }

        /// <summary>
        /// Crouch if true, stand if false.
        /// </summary>
        public virtual void SetCrouching(bool value)
        {
            if (allowCrouch && cameraTransform != null)
            {
                Surface surface;
                var wantsToStand = value == false;
                LastAttemptedMoveVector = (wantsToStand ? Vector3.up : Vector3.down) * GetCrouchToStandDistance();
                if (isCrouching && wantsToStand && IsBlockedAbove(out surface))
                {
                    Blocked?.Invoke(surface);
                    if (surface != null) surface.OnHit.Invoke();
                }
                else
                {
                    isCrouching = value;
                    if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
                    var y = standingCameraY * (isCrouching ? crouchScaleY : 1);
                    crouchCoroutine = StartCoroutine(MoveCameraToHeight(y));
                    if (isCrouching)
                    {
                        Crouched?.Invoke(cachedTransform.position);
                    }
                    else
                    {
                        Uncrouched?.Invoke(cachedTransform.position);
                    }
                }
            }
        }

        /// <summary>
        /// Play audio when turning in place.
        /// </summary>
        public virtual void PlayTurningAudio()
        {
            Audio?.PlayFootstep(currentSurface, turningFootstepVolume);
        }

        /// <summary>
        /// Clears all pending movement commands.
        /// </summary>
        protected virtual void ClearMovementCommandQueue()
        {
            movementCommandQueue.Clear();
        }

        /// <summary>
        /// Add a movement command to the queue. In realtime mode, Update will process 
        /// movement commands in queue order. In turn-based mode, the queue will only
        /// contain the most recently-queued command, and it will be processing when
        /// you manually call ProcessTurn.
        /// </summary>
        public void EnqueueMovementCommand(MovementCommands command)
        {
            if (CanMove)
            {
                switch (updateMode)
                {
                    case GridControllerUpdateMode.TurnBased:
                        movementCommandQueue.Clear(); // Turn-based always uses only most recent input.
                        break;
                    case GridControllerUpdateMode.Realtime:
                        if (!queueInput && movementCommandQueue.Count > 0) return; // If not queueing, don't accept input.
                        break;
                }
                movementCommandQueue.Enqueue(command);
            }
        }

        /// <summary>
        /// Rounds a world space position to the center of the nearest square. 
        /// Leaves Y position untouched to accommodate ramps.
        /// </summary>
        public virtual Vector3 ToGridPosition(Vector3 position)
        {
            return MathUtility.ToNearestIntMultiple(position, squareSize) + gridOffset;
        }

        protected virtual Vector3 GetGridPosition(Vector3 position)
        {
            return enforceGridPosition
                ? ToGridPosition(position)
                : position;
        }

        /// <summary>
        /// Queues movement commands if any movement inputs are pressed.
        /// Also toggles crouching if crouch input is pressed.
        /// </summary>
        protected virtual void ReadMovementInput()
        {
            if (Input == null) return;

            if (Input.InputRepeatReleased) ClearMovementCommandQueue();

            if (Input.ForwardPressed && CanMove) EnqueueMovementCommand(MovementCommands.Forward);
            else if (Input.BackwardPressed && CanMove) EnqueueMovementCommand(MovementCommands.Backward);
            else if (Input.TurnLeftPressed && CanTurn) EnqueueMovementCommand(MovementCommands.TurnLeft);
            else if (Input.TurnRightPressed && CanTurn) EnqueueMovementCommand(MovementCommands.TurnRight);
            else if (Input.StrafeLeftPressed && CanMove) EnqueueMovementCommand(MovementCommands.StrafeLeft);
            else if (Input.StrafeRightPressed && CanMove) EnqueueMovementCommand(MovementCommands.StrafeRight);

            if (Input.CrouchPressed) ToggleCrouching();
        }

        /// <summary>
        /// Processes the next movement command in the queue.
        /// </summary>
        protected virtual void ProcessMovementCommandQueue()
        {
            if (movementCommandQueue.Count == 0) return;
            var command = movementCommandQueue.Dequeue();
            switch (command)
            {
                case MovementCommands.Forward:
                    TryMove(Vector3.forward);
                    break;
                case MovementCommands.Backward:
                    TryMove(Vector3.back);
                    break;
                case MovementCommands.TurnLeft:
                    TryRotate(true);
                    break;
                case MovementCommands.TurnRight:
                    TryRotate(false);
                    break;
                case MovementCommands.StrafeLeft:
                    TryMove(Vector3.left);
                    break;
                case MovementCommands.StrafeRight:
                    TryMove(Vector3.right);
                    break;
            }
        }

        protected virtual void TryRotate(bool left)
        {
            if (!CanTurn) return;
            StartCoroutine(Rotate(CompassDirectionUtility.RotateCompassDirection(CompassDirection, left)));
        }

        protected virtual IEnumerator Rotate(CompassDirection newCompassDirection)
        {
            Turning?.Invoke(cachedTransform.position);
            isMoving = true;
            PlayTurningAudio();
            compassDirection = newCompassDirection;
            var originalRotation = cachedTransform.rotation;
            var newRotation = Quaternion.Euler(0, CompassDirectionUtility.CompassDirectionToAngle(CompassDirection), 0);
            if (!snapMovement && rotationDuration > 0)
            {
                float elapsed = 0;
                while (elapsed < rotationDuration)
                {
                    var t = elapsed / rotationDuration;
                    cachedTransform.rotation = Quaternion.Lerp(originalRotation, newRotation, t);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
            }
            cachedTransform.rotation = newRotation;
            isMoving = false;
            Turned?.Invoke(cachedTransform.position);
        }

        /// <summary>
        /// If blocked, bounces controller. Otherwise moves controller in specified diration.
        /// </summary>
        protected virtual void TryMove(Vector3 direction)
        {
            var moveVector = squareSize * CompassDirectionUtility.GetMoveVector(direction, CompassDirection);
            var moveCompassDirection = CompassDirectionUtility.GetMoveCompassDirection(moveVector);
            LastAttemptedMoveVector = moveVector;
            Surface surface;
            if (IsBlocked(moveVector, out surface))
            {
                Blocked?.Invoke(surface);
                if (surface != null) surface.OnHit.Invoke();
                StartCoroutine(Bounce(moveVector, direction == Vector3Int.back, surface));
            }
            else
            {
                StartCoroutine(Move(moveVector, moveCompassDirection));
            }
        }

        /// <summary>
        /// Get the centerpoint of the boxcast used to check for walls and floors.
        /// This will be lower when crouching.
        /// </summary>
        protected virtual Vector3 GetCurrentBoxcastCenter(Vector3 position)
        {
            if (isCrouching)
            {
                return position + crouchScaleY * boxcastHeight * Vector3.up;
            }
            else
            {
                return position + boxcastHeight * Vector3.up;
            }
        }

        /// <summary>
        /// Get the size of the boxcast used to check for walls and floors.
        /// This will be smaller when crouching.
        /// </summary>
        protected virtual Vector3 GetCurrentBoxcastExtents()
        {
            if (isCrouching)
            {
                return new Vector3(boxcastExtents.x, crouchScaleY * boxcastExtents.y, boxcastExtents.z);
            }
            else
            {
                return boxcastExtents;
            }
        }

        /// <summary>
        /// Returns the vertical distance from crouching to standing.
        /// </summary>
        protected virtual float GetCrouchToStandDistance()
        {
            return standingCameraY - (standingCameraY * crouchScaleY);
        }

        /// <summary>
        /// Performs boxcast to determine if movement direction is blocked.
        /// If blocked, returns Surface on blocking object.
        /// </summary>
        public virtual bool IsBlocked(Vector3 moveVector, out Surface surface)
        {
            var center = GetCurrentBoxcastCenter(cachedTransform.position);
            var extents = GetCurrentBoxcastExtents();
            var direction = moveVector.normalized;
            var numHits = Physics.BoxCastNonAlloc(center, extents, direction, hits, 
                Quaternion.identity, squareSize, wallLayerMask, QueryTriggerInteraction.Ignore);
            var hit = numHits > 0;
            if (showRaycasts)
            {
                Debug.DrawRay(center, direction * (extents.x + squareSize), Color.green, ShowRaycastsDuration);
                wallCollisionPosition = null;
            }
            surface = null;
            for (int i = 0; i < numHits; i++)
            {
                surface = hits[i].collider.GetComponentInChildren<Surface>();
                if (surface != null)
                {
                    // If we hit a surface, determine the slope:
                    RaycastHit raycastHitInfo;
                    if (Physics.Raycast(center, moveVector.normalized, out raycastHitInfo, squareSize, wallLayerMask))
                    {
                        if (!Mathf.Approximately(0, raycastHitInfo.normal.y))
                        {
                            var angle = Vector3.Angle(-moveVector, raycastHitInfo.normal);
                            if (angle <= maxSlope)
                            {
                                hit = false;
                            }
                        }
                    }
                    if (showRaycasts)
                    {
                        Debug.DrawRay(center, direction * raycastHitInfo.distance, Color.red, ShowRaycastsDuration);
                        wallCollisionPosition = hits[i].point;
                    }
                }
                if (surface != null) break;
            }
            return hit;
        }

        /// <summary>
        /// Performs boxcast to determine if ability to stand from crouch is blocked.
        /// If blocked, returns Surface on blocking object.
        /// </summary>
        public virtual bool IsBlockedAbove(out Surface surface)
        {
            var standingBoxcastCenter = cachedTransform.position + boxcastHeight * Vector3.up;
            var crouchToStandDistance = GetCrouchToStandDistance();
            var numHits = Physics.BoxCastNonAlloc(standingBoxcastCenter, boxcastExtents, Vector3.up, hits, 
                Quaternion.identity, crouchToStandDistance, wallLayerMask, QueryTriggerInteraction.Ignore);
            var hit = numHits > 0;
            if (showRaycasts)
            {
                Debug.DrawRay(standingBoxcastCenter, Vector3.up * (boxcastExtents.y + crouchToStandDistance), Color.green, ShowRaycastsDuration);
            }
            surface = null;
            for (int i = 0; i < numHits; i++)
            {
                surface = hits[i].collider.GetComponentInChildren<Surface>();
                if (surface != null)
                {
                    if (showRaycasts)
                    {
                        Debug.DrawRay(standingBoxcastCenter, Vector3.up * (boxcastExtents.y + crouchToStandDistance), Color.red, ShowRaycastsDuration);
                    }
                    break;
                }
            }
            return hit;
        }

        /// <summary>
        /// Performs boxcast to determine surface below position to play surface's footstep sound.
        /// Also returns position of surface to snap controller to floor.
        /// Casts from ceiling down floorRaycastDistance units.
        /// </summary>
        protected virtual Surface GetSurfaceUnderPosition(Vector3 position, out Vector3 hitPoint)
        {
            var center = GetCurrentBoxcastCenter(position);// cachedTransform.position);
            var extents = GetCurrentBoxcastExtents();
            var castOrigin = center + Vector3.up * floorDetectionOffset;
            var numHits = Physics.BoxCastNonAlloc(castOrigin, extents, Vector3.down, hits, Quaternion.identity, 
                floorRaycastDistance, floorLayerMask, QueryTriggerInteraction.Ignore);
            var hitFloor = numHits > 0;
            hitPoint = hitFloor ? hits[0].point : position;
            if (showRaycasts)
            {
                Debug.DrawRay(castOrigin, Vector3.down * floorRaycastDistance, Color.green, ShowRaycastsDuration);
                if (hitFloor)
                {
                    Debug.DrawLine(castOrigin, hitPoint, Color.red, ShowRaycastsDuration);
                    floorCollisionPosition = hitPoint;
                }
                else
                {
                    floorCollisionPosition = null;
                }
            }
            Surface surface = null;
            for (int i = 0; i < numHits; i++)
            {
                var hit = hits[i];
                if (hit.collider.isTrigger) continue;
                if (hit.point.y > hitPoint.y) hitPoint = hit.point;
                if (surface == null)
                {
                    surface = hit.collider.GetComponentInChildren<Surface>();
                }
            }
            return surface;
        }

        protected virtual IEnumerator Move(Vector3 moveVector, CompassDirection moveHeading)
        {
            ExitingPosition?.Invoke(cachedTransform.position);
            isMoving = true;
            var originalPosition = cachedTransform.position;
            var newPosition = new Vector3(originalPosition.x + moveVector.x, originalPosition.y, originalPosition.z + moveVector.z);
            newPosition = GetGridPosition(newPosition);

            Vector3 hitPoint;
            currentSurface = GetSurfaceUnderPosition(newPosition, out hitPoint);
            if (currentSurface != null)
            {
                var volume = isCrouching ? crouchingFootstepVolume : 1;
                Audio?.PlayFootstep(currentSurface, volume);
            }
            newPosition.y = hitPoint.y;

            if (!snapMovement && movementDuration > 0)
            {
                // If not snapping movement, lerp to new position over time.
                // If we're continuing movement from last frame (e.g., move button held down), include leftover
                // time from last frame.
                var isContinuingMoveInSameDirection = Time.frameCount == (frameCountFromLastMove + 1) && 
                    Vector3.Equals(moveVector, moveVectorFromLastMove);
                float elapsed = isContinuingMoveInSameDirection ? leftoverDeltaFromLastMove : 0;
                while (elapsed < movementDuration)
                {
                    var t = elapsed / movementDuration;
                    cachedTransform.position = Vector3.Lerp(originalPosition, newPosition, t);

                    if (enableHeadBob)
                    {
                        // If using head bob, determine current Y offset of head.
                        // Adjust camera to that height, and make the camera look
                        // at a fixed focus point in the distance.
                        var radians = 2 * Mathf.PI * t * headBobsPerSquare;
                        var bobY = Mathf.Sin(radians) * headBobHeight;
                        cameraTransform.localPosition += new Vector3(0, bobY, 0);
                        cameraTransform.LookAt(GetFocusPoint());
                    }

                    yield return null;
                    elapsed += Time.deltaTime;
                }
                frameCountFromLastMove = Time.frameCount;
                leftoverDeltaFromLastMove = elapsed - movementDuration;
                moveVectorFromLastMove = moveVector;
                if (enableHeadBob)
                {
                    // Reset head bob values at end of movement:
                    cameraTransform.localPosition = Vector3.zero;
                    cameraTransform.localRotation = Quaternion.identity;
                }
            }
            cachedTransform.position = newPosition;

            if (snapMovement && movementDuration > 0)
            {
                // If snapping movement, still apply movementDuration delay so player can't move infinitely fast:
                if (movementDurationDelay == null) movementDurationDelay = new WaitForSeconds(movementDuration);
                yield return movementDurationDelay;
            }

            isMoving = false;
            if (moveVector == Vector3Int.left || moveVector == Vector3Int.right)
            {
                Strafed?.Invoke(newPosition);
            }
            EnteredPosition?.Invoke(newPosition);
        }

        protected virtual IEnumerator Bounce(Vector3 moveVector, bool isRetreating, Surface surface)
        {
            Input?.BounceFeedback();
            Audio?.PlayBounce(surface);
            isMoving = true;
            var originalPosition = GetGridPosition(cachedTransform.position);
            var moveProportion = isRetreating ? bounceBackwardAmount : bounceAmount;
            var bouncePosition = new Vector3(originalPosition.x + moveVector.x * moveProportion, originalPosition.y, originalPosition.z + moveVector.z * moveProportion);
            if (!snapMovement && movementDuration > 0)
            {
                var bounceDuration = movementDuration / 2;
                float elapsed = 0;
                while (elapsed < bounceDuration)
                {
                    var t = elapsed / bounceDuration;
                    cachedTransform.position = Vector3.Lerp(originalPosition, bouncePosition, t);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
                elapsed = 0;
                while (elapsed < bounceDuration)
                {
                    var t = elapsed / bounceDuration;
                    cachedTransform.position = Vector3.Lerp(bouncePosition, originalPosition, t);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
                cachedTransform.position = originalPosition;
            }
            isMoving = false;
        }

        protected virtual IEnumerator MoveCameraToHeight(float destinationY)
        {
            if (!snapMovement && crouchTransitionDuration > 0)
            {
                var originalY = cameraParent.localPosition.y;
                float elapsed = 0;
                while (elapsed < crouchTransitionDuration)
                {
                    var t = elapsed / crouchTransitionDuration;
                    var y = Mathf.Lerp(originalY, destinationY, t);
                    cameraParent.localPosition = new Vector3(cameraParent.localPosition.x, y, cameraParent.localPosition.z);
                    yield return null;
                    elapsed += Time.deltaTime;
                }
            }
            cameraParent.localPosition = new Vector3(cameraParent.localPosition.x, destinationY, cameraParent.localPosition.z);
            crouchCoroutine = null;
        }

        /// <summary>
        /// Make the camera a child of an empty GameObject so we can offset the camera to simulate head bob.
        /// </summary>
        protected virtual void CreateCameraParent()
        {
            if (cameraTransform == null) return;
            cameraParent = new GameObject("CameraParent").transform;
            cameraParent.transform.SetParent(transform);
            cameraParent.transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
            cameraTransform.SetParent(cameraParent, true);
        }

        /// <summary>
        /// When walking with head bob, look at this fixed-height point in the distance.
        /// (When walking, humans naturally adjust their gaze slightly up or down to counter
        /// the height changes in their footsteps.)
        /// </summary>
        protected virtual Vector3 GetFocusPoint()
        {
            return cameraParent.position + transform.forward * headBobFocusDistance;
        }

        private void OnDrawGizmos()
        {
            if (wallCollisionPosition != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(wallCollisionPosition.Value, 0.25f);
            }
            if (floorCollisionPosition != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(floorCollisionPosition.Value, 0.25f);
            }
        }

    }
}

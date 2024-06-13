using UnityEngine;
using UnityEngine.Events;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Add to a GridController to expose UnityEvents that you can configure
    /// in the inspector (versus using the C# events directly).
    /// </summary>
    [AddComponentMenu("Grid Controller/Grid Controller Events")]
    [RequireComponent(typeof(GridController))]
    public class GridControllerEvents : MonoBehaviour
    {

        [SerializeField] private UnityEvent<Vector3> onExitPosition = new UnityEvent<Vector3>();
        [SerializeField] private UnityEvent<Vector3> onEnterPosition = new UnityEvent<Vector3>();
        [SerializeField] private UnityEvent<Vector3> onBeginTurn = new UnityEvent<Vector3>();
        [SerializeField] private UnityEvent<Vector3> onEndTurn = new UnityEvent<Vector3>();
        [SerializeField] private UnityEvent<Vector3> onStrafe = new UnityEvent<Vector3>();
        [SerializeField] private UnityEvent<Vector3> onCrouch = new UnityEvent<Vector3>();
        [SerializeField] private UnityEvent<Vector3> onUncrouch = new UnityEvent<Vector3>();
        [SerializeField] private UnityEvent<Surface> onBlocked = new UnityEvent<Surface>();

        public UnityEvent<Vector3> OnExitPosition => onExitPosition;
        public UnityEvent<Vector3> OnEnterPosition => onEnterPosition;
        public UnityEvent<Vector3> OnBeginTurn => onBeginTurn;
        public UnityEvent<Vector3> OnEndTurn => onEndTurn;
        public UnityEvent<Vector3> OnStrafe => onStrafe;
        public UnityEvent<Vector3> OnCrouch => onCrouch;
        public UnityEvent<Vector3> OnUncrouch => onUncrouch;
        public UnityEvent<Surface> OnBlocked => onBlocked;

        private GridController gridController;

        protected virtual void Awake()
        {
            gridController = GetComponent<GridController>();
        }

        protected virtual void Start()
        {
            if (gridController == null) return;
            gridController.ExitingPosition += OnGridControllerExitingPosition;
            gridController.EnteredPosition += OnGridControllerEnteredPosition;
            gridController.Turning += OnGridControllerTurning;
            gridController.Turned += OnGridControllerTurned;
            gridController.Strafed += OnGridControllerStrafed;
            gridController.Crouched += OnGridControllerCrouched;
            gridController.Uncrouched += OnGridControllerUncrouched;
            gridController.Blocked += OnGridControllerBlocked;
        }

        protected virtual void OnDestroy()
        {
            if (gridController == null) return;
            gridController.ExitingPosition -= OnGridControllerExitingPosition;
        }

        protected void OnGridControllerExitingPosition(Vector3 vector3)
        {
            if (enabled) onExitPosition.Invoke(vector3);
        }

        private void OnGridControllerEnteredPosition(Vector3 vector3)
        {
            if (enabled) onEnterPosition.Invoke(vector3);
        }

        private void OnGridControllerTurning(Vector3 vector3)
        {
            if (enabled) onBeginTurn.Invoke(vector3);
        }

        private void OnGridControllerTurned(Vector3 vector3)
        {
            if (enabled) onEndTurn.Invoke(vector3);
        }

        private void OnGridControllerStrafed(Vector3 vector3)
        {
            if (enabled) onStrafe.Invoke(vector3);
        }

        private void OnGridControllerCrouched(Vector3 vector3)
        {
            if (enabled) onCrouch.Invoke(vector3);
        }

        private void OnGridControllerUncrouched(Vector3 vector3)
        {
            if (enabled) onUncrouch.Invoke(vector3);
        }

        private void OnGridControllerBlocked(Surface surface)
        {
            if (enabled) onBlocked.Invoke(surface);
        }

    }
}

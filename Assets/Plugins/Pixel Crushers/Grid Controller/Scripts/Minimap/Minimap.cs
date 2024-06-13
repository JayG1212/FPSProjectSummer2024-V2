using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Handles the minimap.
    /// </summary>
    public class Minimap : MonoBehaviour
    {

        [Tooltip("To customize what the minimap sees, set this camera's Culling Mask and Size.")]
        [SerializeField] private Camera minimapCamera;

        public Camera MinimapCamera => minimapCamera;

        protected virtual void Start()
        {
            minimapCamera.transform.SetParent(GridController.Instance.transform, false);
        }

    }

}

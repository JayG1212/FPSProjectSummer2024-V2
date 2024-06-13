// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Assign a SurfaceType to a GameObject. GridController can use
    /// the Surface to play sounds when walking on the GameObject or
    /// bouncing against it.
    /// </summary>
    [AddComponentMenu("Grid Controller/Surface")]
    public class Surface : MonoBehaviour
    {

        [SerializeField] private SurfaceType surfaceType;
        [SerializeField] private UnityEvent onHit;

        public SurfaceType SurfaceType => surfaceType;
        public UnityEvent OnHit => onHit;

    }
}

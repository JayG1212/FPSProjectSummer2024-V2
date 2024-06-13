// Copyright (c) Pixel Crushers. All rights reserved.

#if FMOD

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Associates audio clips with a surface.
    /// </summary>
    [CreateAssetMenu(fileName = "New FMOD Surface Type", menuName = "Grid Controller/FMOD Surface Type")]
    public class FMODSurfaceType : SurfaceType
    {

        [Tooltip("FMOD event to play when walking on this surface.")]
        [SerializeField] private FMODUnity.EventReference footstep;

        [Tooltip("FMOD event to play when bouncing against this surface..")]
        [SerializeField] private FMODUnity.EventReference bounce;

        public FMODUnity.EventReference Footstep => footstep;
        public FMODUnity.EventReference Bounce => bounce;

    }
}

#endif

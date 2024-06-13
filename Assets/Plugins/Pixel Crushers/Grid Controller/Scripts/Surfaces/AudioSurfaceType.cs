// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Associates audio clips with a surface.
    /// </summary>
    [CreateAssetMenu(fileName = "New Audio Surface Type", menuName = "Grid Controller/Audio Surface Type")]
    public class AudioSurfaceType : SurfaceType
    {

        [Tooltip("Audio clips to play when walking on this surface.")]
        [SerializeField] private AudioClip[] footsteps;

        [Tooltip("Audio clips to play when bouncing against this surface.")]
        [SerializeField] private AudioClip[] bounces;

        public AudioClip[] Footsteps => footsteps;
        public AudioClip[] Bounces => bounces;

    }
}

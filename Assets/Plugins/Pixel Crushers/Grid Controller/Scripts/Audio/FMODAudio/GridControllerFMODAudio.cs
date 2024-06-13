// Copyright (c) Pixel Crushers. All rights reserved.

#if FMOD

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// FMOD implementation for Grid Controller audio.
    /// </summary>
    public class GridControllerFMODAudio : MonoBehaviour, IGridControllerAudio
    {

        [Tooltip("Footsteps to play when moving from one square to the next.")]
        public FMODUnity.EventReference footstep;

        [Tooltip("Player has bounced off wall.")]
        public FMODUnity.EventReference bounce;

        [Tooltip("Door is opening.")]
        public FMODUnity.EventReference door;

        public virtual void PlayFootstep(Surface surface, float volume)
        {
            if (surface == null)
            {
                FMODUnity.RuntimeManager.PlayOneShot(footstep, transform.position);
            }
            else if (surface.SurfaceType is FMODSurfaceType fmodSurfaceType)
            {
                FMODUnity.RuntimeManager.PlayOneShot(fmodSurfaceType.Footstep, surface.transform.position);
            }
            else
            {
                FMODUnity.RuntimeManager.PlayOneShot(footstep, surface.transform.position);
            }
        }

        public virtual void PlayBounce(Surface surface)
        {
            if (surface == null)
            {
                FMODUnity.RuntimeManager.PlayOneShot(bounce, transform.position);
            }
            else if (surface.SurfaceType is FMODSurfaceType fmodSurfaceType)
            {
                FMODUnity.RuntimeManager.PlayOneShot(fmodSurfaceType.Bounce, surface.transform.position);
            }
            else
            {
                FMODUnity.RuntimeManager.PlayOneShot(bounce, surface.transform.position);
            }
        }

        public virtual void PlayDoor(AudioInfo audioInfo, Vector3 position)
        {
            FMODUnity.RuntimeManager.PlayOneShot(door, position);
        }

    }
}

#endif

// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Generic audio interface that Grid Controller uses to play feedback audio
    /// such as footsteps and bouncing against walls. Grid Controller ships with
    /// an implementation that uses Unity's built-in audio manager.
    /// </summary>
    public interface IGridControllerAudio
    {

        /// <summary>
        /// Play a footstep sound on the specified surface, at the 
        /// specified volume. (Volume may be reduce if crouching.)
        /// </summary>
        void PlayFootstep(Surface surface, float volume);

        /// <summary>
        /// Play a bounce sound on the specified surface. Invoked
        /// when bouncing against barriers such as walls.
        /// </summary>
        void PlayBounce(Surface surface);

        /// <summary>
        /// Play a door sound at a specified position.
        /// </summary>
        void PlayDoor(AudioInfo audioInfo, Vector3 position);

    }
}

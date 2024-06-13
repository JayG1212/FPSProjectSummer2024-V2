// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Implements IGridControllerAudio using Unity's built-in audio manager.
    /// </summary>
    [AddComponentMenu("Grid Controller/Grid Controller Audio")]
    [RequireComponent(typeof(GridController))]
    public class GridControllerAudio : MonoBehaviour, IGridControllerAudio
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioSource bounceAudioSource;
        [Tooltip("Door Audio Source should be on a separate GameObject such as a child GameObject so it can move to door positions.")]
        [SerializeField] private AudioSource doorAudioSource;

        protected virtual void Awake()
        {
            if (footstepAudioSource == null) footstepAudioSource = GetComponent<AudioSource>();
            if (bounceAudioSource == null) bounceAudioSource = GetComponent<AudioSource>();
            if (footstepAudioSource == null) footstepAudioSource = AddAudioSource();
            if (bounceAudioSource == null) bounceAudioSource = AddAudioSource();
            if (doorAudioSource == null) doorAudioSource = AddAudioSource(true);
        }

        protected AudioSource AddAudioSource(bool asChild = false)
        {
            var go = asChild ? new GameObject("DoorAudioSource") : gameObject;
            if (asChild) go.transform.SetParent(transform);
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 1; // 3D
            return audioSource;
        }

        /// <summary>
        /// If surface is AudioSurfaceType, play one of its footstep audio clips.
        /// </summary>
        public virtual void PlayFootstep(Surface surface, float volume)
        {
            if (surface != null && surface.SurfaceType is AudioSurfaceType)
            {
                PlayAudio((surface.SurfaceType as AudioSurfaceType).Footsteps, footstepAudioSource, volume);
            }
        }

        /// <summary>
        /// If surface is AudioSurfaceType, play one of its bounce audio clips.
        /// </summary>
        /// <param name="surface"></param>
        public virtual void PlayBounce(Surface surface)
        {
            if (surface != null && surface.SurfaceType is AudioSurfaceType)
            {
                PlayAudio((surface.SurfaceType as AudioSurfaceType).Bounces, bounceAudioSource, bounceAudioSource.volume);
            }
        }

        /// <summary>
        /// Move the door audio source to the specified position and play the audioInfo's audio clip.
        /// </summary>
        public virtual void PlayDoor(AudioInfo audioInfo, Vector3 position)
        {
            if (audioInfo is AudioClipInfo)
            {
                doorAudioSource.transform.position = position;
                PlayAudio((audioInfo as AudioClipInfo).AudioClip, doorAudioSource, doorAudioSource.volume);
            }
        }

        /// <summary>
        /// Randomly choose a clip from audioClips and play it through audioSource at the specified volume.
        /// </summary>
        public virtual void PlayAudio(AudioClip[] audioClips, AudioSource audioSource, float volume = 1)
        {
            if (audioClips == null) return;
            var audioClip = audioClips[Random.Range(0, audioClips.Length)];
            if (audioClip == null) return;
            PlayAudio(audioClip, audioSource, volume);
        }

        /// <summary>
        /// Play an audio clip through audioSource at the specified volume.
        /// </summary>
        public virtual void PlayAudio(AudioClip audioClip, AudioSource audioSource, float volume = 1)
        {
            if (audioClip == null) return;
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
        }

    }
}

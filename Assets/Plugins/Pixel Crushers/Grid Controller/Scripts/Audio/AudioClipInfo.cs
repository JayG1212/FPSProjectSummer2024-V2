// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Reference to an audio clip.
    /// </summary>
    [CreateAssetMenu(fileName = "New Audio Clip Info", menuName = "Grid Controller/Audio Clip Info")]
    public class AudioClipInfo : AudioInfo
    {

        [SerializeField] private AudioClip audioClip;

        public AudioClip AudioClip => audioClip;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// SurfaceTypes can be assigned to GameObjects' Surface components
    /// to define attributes such as sounds to play when walking on
    /// surface or bouncing against it.
    /// </summary>
    public abstract class SurfaceType : ScriptableObject
    {
    }
}

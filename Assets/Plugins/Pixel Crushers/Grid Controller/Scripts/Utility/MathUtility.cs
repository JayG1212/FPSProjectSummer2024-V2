// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.GridController
{

    public static class MathUtility
    {

        /// <summary>
        /// Rounds a world space position to the center of the nearest integer multiple. 
        /// Leaves Y position untouched to accommodate ramps.
        /// Example: (1, 2, 4.5) with integer multiple 5 ==> (0, 2, 5).
        /// </summary>
        public static Vector3 ToNearestIntMultiple(Vector3 position, int intMultiple)
        {
            var x = ToNearestIntMultiple(position.x, intMultiple);
            var z = ToNearestIntMultiple(position.z, intMultiple);
            return new Vector3(x, position.y, z);
        }

        /// <summary>
        /// Rounds a float to the nearest integer multiple.
        /// Example: 0.5 with integer multiple 5 ==> 0.
        /// </summary>
        public static float ToNearestIntMultiple(float f, int intMultiple)
        {
            return Mathf.Round(f / (float)intMultiple) * intMultiple;
        }

    }
}

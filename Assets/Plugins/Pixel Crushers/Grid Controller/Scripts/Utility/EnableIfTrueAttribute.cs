// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Adds [EnableIfTrue] attribute to editor. Use to make an inspector
    /// field editable only if another property is true.
    /// </summary>
    public class EnableIfTrueAttribute : PropertyAttribute
    {

        public string OtherPropertyName { get; private set; }

        public EnableIfTrueAttribute(string otherPropertyName)
        {
            OtherPropertyName = otherPropertyName;
        }

    }

}

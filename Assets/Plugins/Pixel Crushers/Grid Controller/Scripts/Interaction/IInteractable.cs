// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Interface for interactables.
    /// </summary>
    public interface IInteractable
    {

        void Interact(Vector3 playerPosition, CompassDirection playerCompassDirection);

    }
}

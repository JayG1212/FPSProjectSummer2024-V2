// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// "Virtual controller" interface that Grid Controller uses for input.
    /// Grid Controller ships with an implementation named GridControllerInput
    /// that uses Unity's built-in input manager, and other implementations.
    /// </summary>
    public interface IGridControllerInput
    {
        
        /// <summary>
        /// Player is using joystick.
        /// </summary>
        bool IsUsingJoystick { get; }

        /// <summary>
        /// Player pressed input to move forward one square.
        /// </summary>
        bool ForwardPressed { get; }

        /// <summary>
        /// Player pressed input to move backward one square.
        /// </summary>
        bool BackwardPressed { get; }

        /// <summary>
        /// Player pressed input to turn left.
        /// </summary>
        bool TurnLeftPressed { get; }

        /// <summary>
        /// Player pressed input to turn right.
        /// </summary>
        bool TurnRightPressed { get; }

        /// <summary>
        /// Player pressed input to strafe (side step) to the left one square.
        /// </summary>
        bool StrafeLeftPressed { get; }

        /// <summary>
        /// Player pressed input to strafe (side step) to the right one square.
        /// </summary>
        bool StrafeRightPressed { get; }

        /// <summary>
        /// Player pressed input to enter or exit crouch.
        /// </summary>
        bool CrouchPressed { get; }

        /// <summary>
        /// Input has been held down to repeat and was just released this frame.
        /// </summary>
        bool InputRepeatReleased { get; }

        /// <summary>
        /// Player is holding free look input.
        /// </summary>
        bool FreeLook { get; }

        /// <summary>
        /// Free look X rotation.
        /// </summary>
        float LookX { get; }

        /// <summary>
        /// Free look Y rotation.
        /// </summary>
        float LookY { get; }

        /// <summary>
        /// Player pressed input to recenter to pre-free look rotation.
        /// </summary>
        bool Recenter { get; }

        /// <summary>
        /// Player pressed input to activate interactable in front of player.
        /// </summary>
        bool Interact { get; }

        /// <summary>
        /// Player pressed mouse button to activate interactable at mouse position.
        /// </summary>
        bool MouseInteract { get; }

        /// <summary>
        /// Current mouse position.
        /// </summary>
        Vector2 MousePosition { get; }

        /// <summary>
        /// Called when player bounces against a barrier such as a wall.
        /// </summary>
        void BounceFeedback();

    }
}

// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Utility methods to work with CompassDirections.
    /// </summary>
    public static class CompassDirectionUtility
    {

        /// <summary>
        /// Converts a compass direction to an Euler angle (0, 90, 180, or 270 degrees).
        /// </summary>
        public static float CompassDirectionToAngle(CompassDirection compassDirection)
        {
            switch (compassDirection)
            {
                default:
                case CompassDirection.North: return 0;
                case CompassDirection.East: return 90;
                case CompassDirection.South: return 180;
                case CompassDirection.West: return 270;
            }
        }

        /// <summary>
        /// Converts an angle to the closest compass direction.
        /// </summary>
        public static CompassDirection AngleToCompassDirection(float angle)
        {
            // Try to get angle in range [-180, 180].
            if (angle < -180)
            {
                angle += 360;
            }
            else if (angle > 180)
            {
                angle -= 360;
            }

            // Return closest compass direction:
            if (-45f <= angle && angle <= 45)
            {
                return CompassDirection.North;
            }
            else if (45 <= angle && angle <= 135)
            {
                return CompassDirection.East;
            }
            else if (-135 <= angle && angle <= -45)
            {
                return CompassDirection.West;
            }
            else
            {
                return CompassDirection.South;
            }
        }

        /// <summary>
        /// Returns a new compass direction by turning the current compass direction left or right.
        /// </summary>
        public static CompassDirection RotateCompassDirection(CompassDirection compassDirection, bool turnLeft)
        {
            var intCompassDirection = (int)compassDirection;
            return turnLeft ? (CompassDirection)((intCompassDirection == 0) ? 3 : (intCompassDirection - 1))
                : (CompassDirection)((intCompassDirection + 1) % 4);
        }

        /// <summary>
        /// Returns the movement vector given an unoriented direction vector and a compass direction.
        /// </summary>
        /// <param name="direction">Relative, unoriented direction vector where x is left/right, z is forward/back.</param>
        /// <param name="compassDirection">Compute movement vector from this compass direction.</param>
        public static Vector3 GetMoveVector(Vector3 direction, CompassDirection compassDirection)
        {
            switch (compassDirection)
            {
                default:
                case CompassDirection.North: return direction;
                case CompassDirection.East: return new Vector3(direction.z, 0, -direction.x);
                case CompassDirection.South: return new Vector3(-direction.x, 0, -direction.z);
                case CompassDirection.West: return new Vector3(-direction.z, 0, direction.x);
            }
        }

        /// <summary>
        /// Returns the compass direction represented by a direction of movement on the X-Z plane.
        /// </summary>
        public static CompassDirection GetMoveCompassDirection(Vector3 moveVector)
        {
            return (moveVector.x < 0) ? CompassDirection.West
                : (moveVector.x > 0) ? CompassDirection.East
                : (moveVector.z < 1) ? CompassDirection.South
                : CompassDirection.North;
        }

    }
}

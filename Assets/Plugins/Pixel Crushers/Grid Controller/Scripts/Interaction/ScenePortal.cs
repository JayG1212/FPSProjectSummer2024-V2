// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// When the grid controller enters the collider, changes scenes using
    /// a SceneChanger.
    /// </summary>
    [AddComponentMenu("Grid Controller/Scene Portal")]
    public class ScenePortal : EntryExitTrigger
    {

        [Tooltip("Scene to move to.")]
        [SerializeField] private string destinationSceneName;

        [Tooltip("Name of GameObject to move Grid Controller to in destination scene.")]
        [SerializeField] private string spawnpoint;

        [Tooltip("Compass direction to spawn into.")]
        [SerializeField] private CompassDirection spawnDirection;

        public string DestinationSceneName { get => destinationSceneName; set => destinationSceneName = value; }
        public string Spawnpoint { get => spawnpoint; set => spawnpoint = value; }
        public CompassDirection SpawnDirection {  get => spawnDirection; set => spawnDirection = value; }

        protected override void HandleGridControllerEnter(GridController gridController)
        {
            base.HandleGridControllerEnter(gridController);
            UsePortal();
        }

        public virtual void UsePortal()
        {
            GridController.Instance.CanMove = false;
            SceneChanger.Instance.GotoScene(destinationSceneName, spawnpoint, spawnDirection);
        }

    }
}

// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PixelCrushers.GridController
{

    /// <summary>
    /// Persistent singleton that handles scene changes.
    /// </summary>
    public class SceneChanger : MonoBehaviour
    {

        [Tooltip("Create fullscreen cover canvas with this sort order.")]
        [SerializeField] protected int canvasSortOrder = 32760;
        [Tooltip("Fade out and in over this many seconds.")]
        [SerializeField] protected float fadeDuration = 1f;
        [SerializeField] protected Color fadeColor = Color.black;
        [Tooltip("Load scenes asynchronously. If unticked, pause game while loading.")]
        [SerializeField] protected bool loadAsync = true;
        [SerializeField] protected UnityEvent onLeaveScene = new UnityEvent();
        [SerializeField] protected UnityEvent onEnterScene = new UnityEvent();

        protected CanvasGroup canvasGroup = null;
        protected string spawnpointName;
        protected CompassDirection spawnDirection;
        private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

        public event System.Action LeavingScene = null;
        public event System.Action EnteredScene = null;

        private static SceneChanger instance = null;
        private static bool isQuitting = false;
        private static object objectLock = new object();

        public static SceneChanger Instance
        {
            get
            {
                if (isQuitting) return null;
                lock (objectLock)
                {
                    if (instance == null)
                    {
                        instance = new GameObject("SceneChanger").AddComponent<SceneChanger>();
                        instance.Initialize();
                    }
                    return instance;
                }
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SubsystemRegistration()
        {
            instance = null;
            isQuitting = false;
        }
#endif

        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this) instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected virtual void Initialize()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeCanvas();
        }

        protected virtual void InitializeCanvas()
        {
            if (canvasGroup != null) return;
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            if (canvasGroup != null) return;
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                canvas = new GameObject("Canvas").AddComponent<Canvas>();
                canvas.sortingOrder = canvasSortOrder;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.transform.SetParent(transform);
                var image = new GameObject("Image").AddComponent<UnityEngine.UI.Image>();
                image.transform.SetParent(canvas.transform, false);
                image.rectTransform.anchorMin = Vector2.zero;
                image.rectTransform.anchorMax = Vector2.one;
                image.sprite = null;
                image.color = fadeColor;
            }
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.gameObject.SetActive(false);
            canvasGroup.alpha = 0;
        }

        public virtual void GotoScene(string sceneName, string spawnpointName, CompassDirection spawnDirection)
        {
            StartCoroutine(GotoSceneCoroutine(sceneName, spawnpointName, spawnDirection));
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(EnterSceneCoroutine());
        }

        protected virtual IEnumerator GotoSceneCoroutine(string sceneName, string spawnpointName, CompassDirection spawnDirection)
        {
            this.spawnpointName = spawnpointName;
            this.spawnDirection = spawnDirection;

            LeavingScene?.Invoke();
            onLeaveScene.Invoke();

            // Fade out:
            canvasGroup.gameObject.SetActive(true);
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
                yield return null;
                elapsed += Time.deltaTime;
            }
            canvasGroup.alpha = 1;

            if (loadAsync)
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        protected virtual IEnumerator EnterSceneCoroutine()
        { 
            EnteredScene?.Invoke();
            onEnterScene.Invoke();

            // Move grid controller to spawnpoint:
            yield return endOfFrame;
            if (!string.IsNullOrEmpty(spawnpointName))
            { 
                var spawnpoint = GameObject.Find(spawnpointName);
                if (spawnpoint == null)
                {
                    Debug.LogWarning($"Spawnpoint GameObject '{spawnpointName}' not found in scene.");
                }
                else if (GridController.Instance == null)
                {
                    Debug.LogWarning($"Can't move to {spawnpoint}. No Grid Controller instance in scene.");
                }
                else
                {
                    GridController.Instance.Teleport(spawnpoint.transform.position, spawnDirection);
                    GridController.Instance.CanMove = false;
                }
            }

            // Fade in:
            canvasGroup.gameObject.SetActive(true);
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
                yield return null;
                elapsed += Time.deltaTime;
            }
            canvasGroup.alpha = 0;

            GridController.Instance.CanMove = true;
        }

    }

}

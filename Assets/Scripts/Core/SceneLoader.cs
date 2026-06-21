using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigitalCircus.Core
{
    /// <summary>
    /// Centralized asynchronous scene loading with an optional progress callback.
    /// Async loading keeps the main thread responsive on phones (no multi-second hitch
    /// when entering a level) and gives us a single place to show a loading screen.
    ///
    /// Usage: <c>SceneLoader.Instance.Load("Episode01", onProgress: p =&gt; bar.value = p);</c>
    /// It is a lazily-created persistent singleton, so any scene can call it.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _instance;

        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SceneLoader]");
                    _instance = go.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>True while a load is in flight (prevents double-loads from button spam).</summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Loads a scene asynchronously. Guards against missing Build Settings entries and
        /// against overlapping loads. <paramref name="onProgress"/> reports 0…1.
        /// </summary>
        public void Load(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[SceneLoader] Load already in progress — ignoring request.");
                return;
            }

            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"[SceneLoader] Scene '{sceneName}' is not in Build Settings. " +
                    "Add it under File ▸ Build Settings.");
                return;
            }

            StartCoroutine(LoadRoutine(sceneName, onProgress, onComplete));
        }

        private IEnumerator LoadRoutine(string sceneName, Action<float> onProgress, Action onComplete)
        {
            IsLoading = true;

            // Persist before tearing down the current scene so progress is never lost.
            GameManager.Instance.PersistSave();

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            // Unity reports 0…0.9 during load, then needs activation to reach 1.
            while (op.progress < 0.9f)
            {
                onProgress?.Invoke(op.progress / 0.9f);
                yield return null;
            }

            onProgress?.Invoke(1f);
            op.allowSceneActivation = true;
            yield return op;

            IsLoading = false;
            onComplete?.Invoke();
        }
    }
}

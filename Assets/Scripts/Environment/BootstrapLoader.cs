using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class BootstrapLoader : MonoBehaviour
{
    [Header("Systems (XR, Input, Managers)")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset systemsSceneAsset; // drag Systems.unity here (XR rig lives here)
#endif
    [SerializeField, HideInInspector] private string systemsSceneName; // cached for runtime

    [Header("Environment Scene")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset environmentSceneAsset; // drag your env scene here
#endif
    [SerializeField, HideInInspector] private string environmentSceneName; // cached for runtime

    [Header("Options")]
    [Tooltip("Unload any open additive scenes (keep only Bootstrap, and keep Systems if already loaded) before loading.")]
    [SerializeField] private bool closeOtherOpenScenesOnStart = true;

    [Tooltip("After loading, set the last feature scene as the active scene (falls back to Environment if none).")]
    [SerializeField] private bool setLastFeatureActive = true;

    [Tooltip("If true, Systems scene will be loaded first and never unloaded by this loader.")]
    [SerializeField] private bool keepSystemsLoaded = true;

#if UNITY_EDITOR
    private void OnValidate()
    {
        systemsSceneName = systemsSceneAsset != null
            ? Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(systemsSceneAsset))
            : null;

        environmentSceneName = environmentSceneAsset != null
            ? Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(environmentSceneAsset))
            : null;
    }
#endif

    private void Awake()
    {
        // Do NOT unload here anymore. Just kick off the init.
        StartCoroutine(InitializeAndLoad());
    }

    private IEnumerator InitializeAndLoad()
    {
        // Wait one frame so the Editor finishes initializing inspectors
        yield return null;

    #if UNITY_EDITOR
        // Ensure the Editor isn't selecting an object in a scene you're about to unload
        UnityEditor.Selection.activeObject = this.gameObject;
        UnityEditor.SceneView.RepaintAll();
    #endif

        if (closeOtherOpenScenesOnStart)
        {
            var keepBootstrap = gameObject.scene;
            var keepSystemsByName = keepSystemsLoaded ? systemsSceneName : null;

            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s == keepBootstrap) continue;
                if (!string.IsNullOrEmpty(keepSystemsByName) && s.name == keepSystemsByName) continue;
                SceneManager.UnloadSceneAsync(s);
            }
        }

        // Now run your existing loader
        yield return StartCoroutine(LoadRoutine());
    }


    private void Start()
    {
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        // --- Validate build settings ---
        if (!string.IsNullOrEmpty(systemsSceneName) && !IsInBuild(systemsSceneName))
        {
            Debug.LogError($"[Bootstrap] Systems scene '{systemsSceneName}' is NOT in Build Settings.");
            yield break;
        }
        if (!string.IsNullOrEmpty(environmentSceneName) && !IsInBuild(environmentSceneName))
        {
            Debug.LogError($"[Bootstrap] Environment scene '{environmentSceneName}' is NOT in Build Settings.");
            yield break;
        }

        // find all FeatureScene components in Bootstrap (even if inactive)
        var features = FindObjectsOfType<FeatureScene>(true)
                      .Where(f => f.loadOnStart && !string.IsNullOrEmpty(f.SceneName))
                      .Distinct()
                      .ToList();

        foreach (var f in features)
        {
            if (!IsInBuild(f.SceneName))
            {
                Debug.LogError($"[Bootstrap] Feature scene '{f.SceneName}' is NOT in Build Settings.");
                yield break;
            }
        }

        // --- Load Systems FIRST (and keep it) ---
        if (keepSystemsLoaded && !string.IsNullOrEmpty(systemsSceneName))
        {
            if (!SceneManager.GetSceneByName(systemsSceneName).isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(systemsSceneName, LoadSceneMode.Additive);
                Debug.Log($"[Bootstrap] Loaded systems: {systemsSceneName}");
            }
        }

        // --- Load Environment ---
        Scene envScene = default;
        if (!string.IsNullOrEmpty(environmentSceneName) &&
            !SceneManager.GetSceneByName(environmentSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(environmentSceneName, LoadSceneMode.Additive);
            envScene = SceneManager.GetSceneByName(environmentSceneName);
            Debug.Log($"[Bootstrap] Loaded environment: {environmentSceneName}");
        }
        else if (!string.IsNullOrEmpty(environmentSceneName))
        {
            envScene = SceneManager.GetSceneByName(environmentSceneName);
        }

        // --- Load Features ---
        Scene lastFeature = default;
        foreach (var f in features)
        {
            if (!SceneManager.GetSceneByName(f.SceneName).isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(f.SceneName, LoadSceneMode.Additive);
                lastFeature = SceneManager.GetSceneByName(f.SceneName);
                Debug.Log($"[Bootstrap] Loaded feature: {f.SceneName}");
            }
        }

        // --- Active Scene selection ---
        if (setLastFeatureActive && lastFeature.IsValid())
        {
            SceneManager.SetActiveScene(lastFeature);
        }
        else if (envScene.IsValid())
        {
            SceneManager.SetActiveScene(envScene);
        }
        // else leave Bootstrap as active if neither loaded

        yield break;
    }

    private static bool IsInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class FeatureScene : MonoBehaviour
{
    [Header("Assign a Feature SceneAsset (Editor only)")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif

    [SerializeField, HideInInspector] private string sceneName; // used at runtime
    public bool loadOnStart = true;

    public string SceneName => sceneName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // cache the scene name so builds don't depend on UnityEditor types
        sceneName = sceneAsset != null
            ? Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sceneAsset))
            : null;
    }
#endif
}
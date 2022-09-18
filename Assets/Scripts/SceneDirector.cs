using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDirector : MonoBehaviour
{
    private ConcurrentBag<Scene> scenesList = new ConcurrentBag<Scene>();

    private void Awake()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name != "RootScene")
                scenesList.Add(scene);
        }
        if (scenesList.IsEmpty)
            Push("TitleScene");
    }

    private IEnumerator clearAllScenes()
    {
        foreach (var scene in scenesList)
        {
            var op = SceneManager.UnloadSceneAsync(scene.name);
            yield return op;
        }
        yield return Resources.UnloadUnusedAssets();
    }

    private IEnumerator changeSceneTo(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return op;
        Scene newScene;
        bool isValid;
        do
        {
            newScene = SceneManager.GetSceneByName(sceneName);
            isValid = newScene.IsValid();
            yield return isValid;
        } while (!isValid);
        scenesList.Add(newScene);
        yield return SceneManager.SetActiveScene(newScene);
    }

    private IEnumerator removeScene(string sceneName)
    {
        var op = SceneManager.UnloadSceneAsync(sceneName);
        yield return op;
        yield return Resources.UnloadUnusedAssets();
    }

    public void Clear()
    {
        StartCoroutine(clearAllScenes());
        scenesList.Clear();
    }

    public void Push(string sceneName)
    {
        StartCoroutine(changeSceneTo(sceneName));
    }

    public void Pop(string sceneName)
    {
        StartCoroutine(removeScene(sceneName));
        Scene removedScene;
        if (scenesList.TryTake(out removedScene))
            Debug.LogWarning("Remove a scene is failed.");
    }
}

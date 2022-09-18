using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IChangingSceneListener
{
    void Clear();
    void Pop(string sceneName);
    void Push(string sceneName);
}

public class SceneDirector : MonoBehaviour, IChangingSceneListener
{
    private ConcurrentStack<Scene> scenesList = new ConcurrentStack<Scene>();

    private void Awake()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name != "RootScene")
                scenesList.Push(scene);
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
        Scene newScene;
        bool isValid;
        do
        {
            newScene = SceneManager.GetSceneByName(sceneName);
            isValid = newScene.IsValid();
            if (isValid)
            {
                yield return isValid;
            }
            else
            {
                var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                yield return op;
            }
        } while (!isValid);
        scenesList.Push(newScene);
        yield return SceneManager.SetActiveScene(newScene);
    }

    private IEnumerator removeScene(string sceneName)
    {
        var op = SceneManager.UnloadSceneAsync(sceneName);
        yield return op;
        yield return Resources.UnloadUnusedAssets();
        Scene nextScene;
        var result = scenesList.TryPeek(out nextScene);
        SceneManager.SetActiveScene(nextScene);
        yield return result;
    }

    public void Clear()
    {
        StartCoroutine(clearAllScenes());
        scenesList.Clear();
    }

    public void Pop(string sceneName)
    {
        StartCoroutine(removeScene(sceneName));
        Scene removedScene;
        if (scenesList.TryPop(out removedScene))
            Debug.LogWarning("Remove a scene is failed.");
    }

    public void Push(string sceneName)
    {
        StartCoroutine(changeSceneTo(sceneName));
    }
}

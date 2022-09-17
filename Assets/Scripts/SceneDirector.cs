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
        ClearAndChangeSceneTo("TitleScene");
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

    public void ClearAndChangeSceneTo(string sceneName)
    {
        StartCoroutine(clearAllScenes());
        scenesList.Clear();
        StartCoroutine(changeSceneTo(sceneName));
    }
}

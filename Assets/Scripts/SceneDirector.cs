using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IChangingSceneListener
{
    void Clear();
    void Pop();
    void Push(string sceneName);
}

public class SceneDirector : MonoBehaviour, IChangingSceneListener
{
    private void Awake()
    {
        bool isSceneListEmpty = true;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name != "RootScene")
                isSceneListEmpty = false;
        }
        if (isSceneListEmpty)
            Push("TitleScene");
    }

    private IEnumerator clearAllScenes()
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name != "RootScene")
            {
                var op = SceneManager.UnloadSceneAsync(scene);
                yield return op;
            }
        }
        yield return Resources.UnloadUnusedAssets();
    }

    private IEnumerator changeSceneTo(string sceneName)
    {
        Scene newScene;
        bool isValid;
        do
        {
            // この初期化や判定の順番を換えると、シーンが二重に読み込まれるなど、意図した挙動にならない。
            newScene = SceneManager.GetSceneByName(sceneName);
            isValid = newScene.IsValid();
            if (isValid)
            {
                yield return true;
            }
            else
            {
                var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                yield return op;
            }
        } while (!isValid);
        yield return SceneManager.SetActiveScene(newScene);
    }

    private IEnumerator removeScene()
    {
        var index = SceneManager.sceneCount - 1;
        var op = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index--));
        yield return op;
        yield return Resources.UnloadUnusedAssets();
        Scene nextScene = SceneManager.GetSceneAt(index);
        var result = SceneManager.SetActiveScene(nextScene);
        yield return result;
    }

    public void Clear()
    {
        StartCoroutine(clearAllScenes());
    }

    public void Pop()
    {
        StartCoroutine(removeScene());
    }

    public void Push(string sceneName)
    {
        StartCoroutine(changeSceneTo(sceneName));
    }
}

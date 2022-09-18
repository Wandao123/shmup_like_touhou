using UnityEngine;
using UnityEngine.SceneManagement;

// 参考：https://kan-kikuchi.hatenablog.com/entry/ManagerSceneAutoLoader
public class RootSceneAutoLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadRootScene()
    {
        if (!SceneManager.GetSceneByName("RootScene").IsValid())
            SceneManager.LoadScene("RootScene", LoadSceneMode.Additive);
    }

    public static IChangingSceneListener GetListener()
    {
        Scene rootScene = SceneManager.GetSceneByName("RootScene");
        foreach (var go in rootScene.GetRootGameObjects())
        {
            SceneDirector sceneDirector = go.GetComponent<SceneDirector>();
            if (sceneDirector != null)
                return sceneDirector;
        }
        return null;
    }
}

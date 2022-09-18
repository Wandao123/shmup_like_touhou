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
}

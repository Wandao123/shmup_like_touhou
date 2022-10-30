using UnityEngine;
using UnityEngine.SceneManagement;

// 参考：https://kan-kikuchi.hatenablog.com/entry/ManagerSceneAutoLoader
public class RootSceneAutoLoader
{
    /// <summary>実行時にRootSceneを読み込む。</summary>
    /// <remarks>指定した属性の御蔭で、任意のシーンを読み込んだ際に実行される。</remarks>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadRootScene()
    {
        if (!SceneManager.GetSceneByName("RootScene").IsValid())
            SceneManager.LoadScene("RootScene", LoadSceneMode.Additive);
    }

    /// <summary>画面管理クラスのインスタンスを取得。</summary>
    /// <return>取得に成功すればそのインスタンス、失敗すればnullを返す</return>
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

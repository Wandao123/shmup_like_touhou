using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private List<Button> _menu;

    private void Awake()
    {
        _menu[0].Select();
    }

    public void OnStartButtonClicked()
    {
        Scene rootScene = SceneManager.GetSceneByName("RootScene");
        foreach (var go in rootScene.GetRootGameObjects())
        {
            SceneDirector sceneDirector = go.GetComponent<SceneDirector>();
            if (sceneDirector != null)
            {
                sceneDirector.ClearAndChangeSceneTo("GameScene");
                break;
            }
        }
    }

    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

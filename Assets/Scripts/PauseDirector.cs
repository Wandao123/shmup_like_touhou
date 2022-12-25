using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseDirector : MonoBehaviour
{
    private IChangingSceneListener _listener;
    [SerializeField]
    private List<Button> _menu;

    private void Awake()
    {
        _menu[0].Select();
    }

    private void Start()
    {
        _listener = RootSceneAutoLoader.GetListener();
    }

    public void OnReturnButtonClicked()
    {
        _listener.Pop();
        Scene rootScene = SceneManager.GetSceneByName("GameScene");
        foreach (var go in rootScene.GetRootGameObjects())
        {
            GameDirector gameDirector = go.GetComponent<GameDirector>();
            if (gameDirector != null)
            {
                gameDirector.enabled = true;
                break;
            }
        }
    }

    public void OnExitButtonClicked()
    {
        _listener.Clear();
        _listener.Push("TitleScene");
    }
}

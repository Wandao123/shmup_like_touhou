using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleDirector : MonoBehaviour
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

    public void OnStartButtonClicked()
    {
        _listener.Clear();
        _listener.Push("GameScene");
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

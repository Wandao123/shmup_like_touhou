using UnityEngine;
using UnityEngine.UI;

public class FrameRateManager : MonoBehaviour
{
    private const int FPS = 60;
    private Text _frameRate;
    private float _averageOfFPS = 0.0f;
    private uint _countedFrames = 0;
    private float _timeBeforeFPS = 0.0f;

    public float AverageOfFPS
    {
        get => _averageOfFPS;
    }

    private void Awake()
    {
        Application.targetFrameRate = FPS;
        _frameRate = GameObject.Find("FrameRate").GetComponent<Text>();
    }

    private void Update()
    {
        // 参考：https://www.create-forever.games/framerate/
        float _currentTime = Time.realtimeSinceStartup;
        ++_countedFrames;
        if (_countedFrames % FPS == 0)
        {
            var temp = (_currentTime - _timeBeforeFPS) / FPS;
            if (temp != 0.0f)
                _averageOfFPS = 1.0f / temp;
            else
                _averageOfFPS = 0.0f;
            _timeBeforeFPS = _currentTime;
            _frameRate.text = "FPS " + _averageOfFPS.ToString("f1");
        }
    }
}

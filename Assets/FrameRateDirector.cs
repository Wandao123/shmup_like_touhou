using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameRateDirector : MonoBehaviour
{
    private const int FPS = 60;
    private GameObject frameRate;
    private uint countedFrames;
    private float timeBeforeFPS;

    // Start is called before the first frame update
    void Start()
    {
        this.frameRate = GameObject.Find("FrameRate");
        countedFrames = 0;
        timeBeforeFPS = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // éQçlÅFhttps://www.create-forever.games/framerate/
        float currentTime = Time.realtimeSinceStartup;
        ++countedFrames;
        if (countedFrames % FPS == 0)
        {
            float averageOfFPS = 0.0f;
            var temp = (currentTime - timeBeforeFPS) / FPS;
            if (temp != 0.0f)
                averageOfFPS = 1.0f / temp;
            else
                averageOfFPS = 0.0f;
            timeBeforeFPS = currentTime;
            this.frameRate.GetComponent<Text>().text = "FPS " + averageOfFPS.ToString("f2");
        }
    }

    void Awake()
    {
        Application.targetFrameRate = FPS;
    }
}

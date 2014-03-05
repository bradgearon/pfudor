using UnityEngine;
using System.Collections;

public class Pauser : MonoBehaviour
{
    private bool paused = false;
    private bool pauseDown;
    public GameObject pausePanel;

    void OnClick()
    {
        pauseDown = true;
    }

    void OnTogglePause()
    {
        paused = !paused;
        pausePanel.SetActive(paused);
        AudioListener.pause = paused;
    }

    // Update is called once per frame
    void Update()
    {
        if (pauseDown)
        {
            OnTogglePause();
        }

        Time.timeScale = paused ? 0 : 1;
        pauseDown = false;
    }
}

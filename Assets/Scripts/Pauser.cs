using UnityEngine;
using System.Collections;

public class Pauser : MonoBehaviour
{
    private bool paused = false;
    private bool pauseDown;


    void OnFingerDown(FingerDownEvent e)
    {
        Debug.Log(e.Selection);

        if (e.Selection != null && e.Selection.GetInstanceID() == this.gameObject.GetInstanceID())
        {
            pauseDown = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pauseDown)
        {
            paused = !paused;
            AudioListener.pause = paused;
        }

        Time.timeScale = paused ? 0 : 1;
        pauseDown = false;
    }
}

using UnityEngine;
using System.Collections;

public class DefaultDown : MonoBehaviour
{
    public GameObject playerControl;
    private bool gameOver;
    private PfudorSceneManager sceneManager;
    public ScreenRaycaster raycaster;

    void Awake()
    {
        sceneManager = FindObjectOfType<PfudorSceneManager>();
        /*
        var fingerDown = gameObject.AddComponent<FingerDownDetector>();
        fingerDown.MessageTarget = gameObject;
        fingerDown.OnFingerDown += OnFingerDown;
        fingerDown.Raycaster = raycaster;
         */
    }

    public void OnClick()
    {
        Debug.Log("OnDefaultDown - OnClick");
        if (playerControl != null && !gameOver)
        {
            playerControl.SendMessage("OnDefaultDown");
        }
        else
        {
            sceneManager.SendMessage("OnDefaultDown");
        }
    }

    void GameOver()
    {
        gameOver = true;
    }

    void OnFingerDown(FingerEvent e)
    {
        if (e.Selection == null || e.Selection.gameObject != gameObject) return;

        if (gameOver)
        {
            sceneManager.SendMessage("OnDefaultDown");
        }
        else if(playerControl != null)
        {
            playerControl.SendMessage("OnDefaultDown");
        }
    }
}

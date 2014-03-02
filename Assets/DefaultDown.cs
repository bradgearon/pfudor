using UnityEngine;
using System.Collections;

public class DefaultDown : MonoBehaviour
{
    public GameObject playerControl;
    private bool gameOver;
    private SceneManager sceneManager;

    void Awake()
    {
        sceneManager = FindObjectOfType<SceneManager>();
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

using UnityEngine;
using System.Collections;

public class Starter : MonoBehaviour
{
    private SceneManager sceneManager;
    void Awake()
    {
        sceneManager = FindObjectOfType<SceneManager>();
    }
    void OnFingerDown(FingerEvent e)
    {
        if (e.Selection == null || e.Selection.gameObject != gameObject) return;
        sceneManager.SendMessage("OnStartDown");
    }
}

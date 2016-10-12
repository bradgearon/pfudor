using System;
using UnityEngine;
using System.Collections;

public class PauseModal : MonoBehaviour
{

    public GameObject playButton;
    public GameObject quitButton;
    public GameObject soundButton;

    private Pauser pauser;
    private TweenHeight playTween;

    // Use this for initialization
    void Awake()
    {
        pauser = FindObjectOfType<Pauser>();
        playTween = playButton.transform.parent.GetComponent<TweenHeight>();
        UIEventListener.Get(playButton).onClick = OnPauseClick;
        UIEventListener.Get(quitButton).onClick = OnQuitClick;
    }

    private void OnQuitClick(GameObject go)
    {
        Debug.Log("quit clcked");
        Application.Quit();
    }

    private void OnPauseClick(GameObject go)
    {
        playTween.onFinished.Clear();
        playTween.onFinished.Add(new EventDelegate(TweenReveresed));
        enableChildren(false);
        playTween.Play(false);
    }

    void OnEnable()
    {
        playTween.onFinished.Clear();
        playTween.onFinished.Add(new EventDelegate(TweenComplete));
        playTween.Play(true);
    }

    public void TweenComplete()
    {
        enableChildren(true);
    }

    private void TweenReveresed()
    {
        pauser.SendMessage("OnTogglePause");
    }

    private void enableChildren(bool enable)
    {
        pauser.gameObject.SetActive(!enable);
        playButton.SetActive(enable);
        soundButton.SetActive(enable);
        quitButton.SetActive(enable);
    }


}

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

    // Update is called once per frame
    void OnFingerDown(FingerEvent e)
    {
        Debug.Log(e.Selection);

        if (e.Selection == null) return;
        if (e.Selection == playButton)
        {
            playTween.onFinished.Clear();
            playTween.onFinished.Add(new EventDelegate(TweenReveresed));
            enableChildren(false);
            playTween.Play(false);
        }

        if (e.Selection == quitButton)
        {
            Application.Quit();
        }
    }


}

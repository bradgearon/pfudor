using UnityEngine;
using System.Collections;

public class Muter : MonoBehaviour
{
    public UISprite muteSprite;
    public UISprite soundSprite;

    public void Awake()
    {
        var mute = PlayerPrefs.GetInt("mute", 0) != 0;
        Camera.main.GetComponent<AudioSource>().mute = mute;
        updateMute(mute);
    }

    public void OnClick()
    {
        Debug.Log("mute clicked");
        var mute = Camera.main.GetComponent<AudioSource>().mute = !Camera.main.GetComponent<AudioSource>().mute;
        PlayerPrefs.SetInt("mute", mute ? 1 : 0);
        PlayerPrefs.Save();
        updateMute(mute);
    }

    private void updateMute(bool mute)
    {
        muteSprite.gameObject.SetActive(mute);
        soundSprite.gameObject.SetActive(!mute);
    }

}

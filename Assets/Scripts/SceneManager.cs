using System;
using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    public string mainScene = "default";
    public Color mainSceneBackground;
    public float transitionAmount = 5f;
    private float transition;
    private Color startColor;
    private bool loaded;
    private bool loading;
    private bool starting;
    public UIWidget signin;
    public UIWidget spin;
    public UIWidget signedIn;
    public UIWidget startButton;

    public UIWidget[] titleObjects;
    public UIWidget[] playObjects;
    public FingerDownDetector defaultTapRecognizer;
    public GameObject[] gameOverObjects;

    private GameObject player;
    private bool gameOver;
    private int autoAuth = 0;
    private ScoreManager scoreManager;
    private bool viewLeaders;

    void Awake()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    // Use this for initialization
    void Start()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            autoAuth = PlayerPrefs.GetInt("autoAuth", 0);

            if (autoAuth == 1)
            {
                SignIn();
            }
        }
        else
        {
            signedIn.gameObject.SetActive(true);
            signin.gameObject.SetActive(false);
        }
    }

    public void SignIn()
    {
        GameManager.Instance.Authenticate();
    }

    void OnStartDown()
    {
        if (!starting)
        {
            OnLoadMain();
        }
    }

    public void HideLeaders()
    {
        scoreManager.SendMessage("hideScores");
        foreach (var so in playObjects)
        {
            so.gameObject.SetActive(false);
        }

        foreach (var so in titleObjects)
        {
            so.gameObject.SetActive(true);
        }
    }

    void OnDefaultDown()
    {
        if (gameOver)
        {
            Application.LoadLevel("title");
        }
    }

    void OnLoadMain()
    {
        startColor = Camera.main.backgroundColor;
        starting = true;
    }

    public void ShowLeaders()
    {
        if (GameManager.Instance.Authenticated)
        {
            GameManager.Instance.ShowLeaderboardUI();
        }
        else
        {
            foreach (var so in titleObjects)
            {
                so.gameObject.SetActive(false);
            }
            viewLeaders = true;
            scoreManager.SendMessage("displayScores");
        }
    }

    void GameOver()
    {
        gameOver = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!loaded)
        {
            if (signin.enabled)
            {
                if (GameManager.Instance.Authenticated || GameManager.Instance.Authenticating)
                {
                    signin.enabled = false;
                    Debug.Log("setting signin inactive");
                    signin.gameObject.SetActive(false);
                }
            }
            else
            {
                if (!loaded && !loading &&
                    !GameManager.Instance.Authenticated && !GameManager.Instance.Authenticating)
                {
                    Debug.Log("setting signin active");
                    signin.gameObject.SetActive(true);
                }
            }

            spin.enabled = GameManager.Instance.Authenticating;

            if (GameManager.Instance.Authenticated)
            {
                if (!signedIn.gameObject.activeSelf)
                {
                    signedIn.gameObject.SetActive(true);
                }
            }
        }

        if (starting)
        {
            if (!loading)
            {
                if (transition < transitionAmount)
                {
                    transition += Time.deltaTime;
                    var totalDelta = transition / transitionAmount;

                    foreach (var to in titleObjects)
                    {
                        to.alpha = (1 - totalDelta * 2);
                    }
                    Camera.main.backgroundColor = Color.Lerp(startColor, mainSceneBackground, totalDelta);
                }
                else
                {
                    loading = true;
                }
            }

            if (!loaded && loading)
            {
                foreach (var so in playObjects)
                {
                    so.gameObject.SetActive(true);
                }

                foreach (var to in titleObjects)
                {
                    to.gameObject.SetActive(false);
                }

                Application.LoadLevelAdditive(mainScene);
                loaded = true;
                signin.gameObject.SetActive(false);
            }
        }

        if (gameOver)
        {
            foreach (var so in playObjects)
            {
                so.gameObject.SetActive(false);
            }

            foreach (var go in gameOverObjects)
            {
                go.gameObject.SetActive(true);
            }
        }

    }


}

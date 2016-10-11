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
    private bool tweening;
    private bool showLeaders
;

    void Awake()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    // Use this for initialization
    void Start()
    {
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            autoAuth = PlayerPrefs.GetInt("autoAuth", 0);

            if (autoAuth == 1)
            {
                SignIn();
            }
        }
        else
        {
            signin.gameObject.SetActive(false);
            setOnSigninClick();
        }
    }

    private void setOnSigninClick()
    {
        signedIn.gameObject.SetActive(true);
        var button = signin.transform.parent.GetComponent<UIButton>();
        button.onClick.Clear();
        button.onClick.Add(new EventDelegate(ShowLeaders));
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SignIn()
    {
        GameManager.Instance.Authenticate();
    }

    public void OnStartDown()
    {
        if (!starting)
        {
            OnLoadMain();
        }
    }

    public void HideLeaders()
    {
        scoreManager.SendMessage("hideScores");
        SetSceneObjects(true, false, false);
    }

    void OnDefaultDown()
    {
        if (gameOver)
        {
            Application.LoadLevel("title");
        }
        if (!loaded)
        {
            HideLeaders();
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
            showLeaders = true;
            SetSceneObjects(false, false, false);
            scoreManager.SendMessage("displayScores");
        }
    }

    public void ShowAchievments()
    {
        if (GameManager.Instance.Authenticated)
        {
            GameManager.Instance.ShowAchievementsUI();
        }
        else
        {
            SetSceneObjects(false, false, false);
            scoreManager.SendMessage("displayAchievements");
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
                    setOnSigninClick();
                }
            }
        }

        if (starting)
        {
            if (!tweening)
            {
                foreach (var titleObject in titleObjects)
                {
                    var tween = titleObject.gameObject.AddComponent<TweenAlpha>();
                    tween.from = titleObject.alpha;
                    tween.to = 0;
                }
                tweening = true;
            }

            if (!loading)
            {
                if (transition < transitionAmount)
                {
                    transition += Time.deltaTime;
                    var totalDelta = transition / transitionAmount;
                    Camera.main.backgroundColor = Color.Lerp(startColor, mainSceneBackground, totalDelta);
                }
                else
                {
                    loading = true;
                }
            }

            if (!loaded && loading)
            {
                SetSceneObjects(false, true, false);
                Application.LoadLevelAdditive(mainScene);
                loaded = true;
            }
        }

        if (gameOver)
        {
            SetSceneObjects(false, false, true);
        }

    }

    void SetSceneObjects(bool title, bool play, bool gameOver)
    {
        foreach (var so in titleObjects)
        {
            so.gameObject.SetActive(title);
        }

        foreach (var so in playObjects)
        {
            so.gameObject.SetActive(play);
        }

        foreach (var go in gameOverObjects)
        {
            go.gameObject.SetActive(gameOver);
        }
    }


}

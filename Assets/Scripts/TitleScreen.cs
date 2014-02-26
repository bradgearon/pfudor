using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour
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

    public UIWidget[] titleObjects;
    public UIWidget[] playObjects;
    public GameObject[] gameOverObjects;

    private GameObject player;
    private bool gameOver;
    private int autoAuth = 0;

    // Use this for initialization
    void Start()
    {

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var autoAuth = PlayerPrefs.GetInt("autoAuth", 0);

            if (autoAuth == 1)
            {
                SignIn();
            }
        }
        else
        {
            signin.gameObject.SetActive(false);

        }

        var screenRaycaster = FingerGestures.Instance.GetComponent<ScreenRaycaster>();
        screenRaycaster.Cameras[0] = FindObjectOfType<UICamera>().camera;

        var fingerDown = gameObject.AddComponent<FingerDownDetector>();
        fingerDown.Raycaster = screenRaycaster;
    }

    public void SignIn()
    {
        GameManager.Instance.Authenticate();
    }

    void OnFingerDown(FingerDownEvent e)
    {
        if (e.Selection == null && !starting)
        {
            OnLoadMain();
        }

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
        GameManager.Instance.ShowLeaderboardUI();
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

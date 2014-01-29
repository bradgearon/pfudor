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

    public UIWidget[] titleObjects;
    public UIWidget[] playObjects;
    public GameObject[] gameOverObjects;

    private GameObject player;
    private bool gameOver;

    // Use this for initialization
    void Start()
    {
        var screenRaycaster = FingerGestures.Instance.GetComponent<ScreenRaycaster>();
        screenRaycaster.Cameras[0] = FindObjectOfType<UICamera>().camera;
        GetComponent<FingerDownDetector>().Raycaster = screenRaycaster;
    }

    void OnFingerDown(FingerDownEvent e)
    {
        if (!starting)
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

    void GameOver()
    {
        gameOver = true;
    }

    // Update is called once per frame
    void Update()
    {
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

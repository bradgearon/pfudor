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

    // Use this for initialization
    void Start()
    {

    }

    void OnFingerDown(FingerDownEvent e)
    {
        OnLoadMain();
    }

    void OnLoadMain()
    {
        startColor = Camera.main.backgroundColor;
        starting = true;
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
                Application.LoadLevelAdditive(mainScene);
                loaded = true;
            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelOneBossManager : MonoBehaviour
{
    public Color mainSceneBackground;
    public float transitionAmount = 5f;
    private float transition;
    private Color startColor;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnTweenInBoss() {
        StartCoroutine("OnLoadBossScene");
    }

    public IEnumerator OnLoadBossScene()
    {
        yield return new WaitForSeconds(1);

        do
        {
            transition += Time.deltaTime;
            var totalDelta = transition / transitionAmount;
            Camera.main.backgroundColor = Color.Lerp(startColor, mainSceneBackground, totalDelta);
        }
        while (transition < transitionAmount);

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene("pillows");
    }

}

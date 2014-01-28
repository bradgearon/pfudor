using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public float scoreScale;

    public UILabel scoreLabel;
    private int score;
    private bool started;
    private int totalTime;
    

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!started)
        {
            started = scoreLabel.gameObject.activeSelf;
        }
        else
        {
            totalTime += Mathf.CeilToInt(Time.deltaTime);
            score = Mathf.CeilToInt(totalTime * scoreScale);
            scoreLabel.text = score.ToString();
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Text;

public class ScoreManager : MonoBehaviour
{
    public float scoreScale;

    public UILabel scoreLabel;
    public UITable scoreTable;

    private int score;
    private bool started;
    private int totalTime;
    private bool gameOver;

    private GameObject sceneManager;

    // Use this for initialization
    void Start()
    {
        sceneManager = FindObjectOfType<TitleScreen>().gameObject;
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
            if (!gameOver)
            {
                totalTime += Mathf.CeilToInt(Time.deltaTime);
                score = Mathf.CeilToInt(totalTime * scoreScale);
                scoreLabel.text = score.ToString();
            }
        }
    }

    void GameOver()
    {
        if (!gameOver)
        {
            gameOver = true;
            updateScores();
        }
    }

    void updateScores()
    {

        var scoreText = PlayerPrefs.GetString("scores", JsonSerializer.SerializeToString(new List<Scores>()));
        var scores = JsonSerializer.DeserializeFromString<List<Scores>>(scoreText);

        scores.Add(new Scores { Name = "Player", Score = score });
        var scoresOrdered = scores.OrderByDescending(s => s.Score).Take(10);
        scoreTable.children.Clear();
        var font = this.scoreLabel.bitmapFont;

        foreach (var s in scoresOrdered)
        {
            var nameLabel = NGUITools.AddWidget<UILabel>(scoreTable.gameObject);
            var scoreLabel = NGUITools.AddWidget<UILabel>(scoreTable.gameObject);

            nameLabel.bitmapFont = font;
            nameLabel.width = 100;
            nameLabel.height = 50;
            nameLabel.text = s.Name;

            scoreLabel.bitmapFont = font;
            scoreLabel.width = 100;
            scoreLabel.height = 50;
            scoreLabel.text = s.Score + string.Empty;
        }

        scoreTable.Reposition();
        scoreText = JsonSerializer.SerializeToString(scoresOrdered.ToList());

        Debug.Log(scoreText);
        PlayerPrefs.SetString("scores", scoreText);
        PlayerPrefs.Save();

        sceneManager.SendMessage("GameOver");
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_METRO
// using System.Runtime.Serialization.Json;
#else
using System.Runtime.Serialization.Formatters.Binary;
#endif

using UnityEngine;
using UnityEngine.SocialPlatforms;


public class ScoreManager : MonoBehaviour
{
    public float scoreScale;
    public UILabel scoreLabel;
    public UITable scoreTable;
    public long highScore;

    private int score;
    private bool started;
    private int totalTime;
    private bool gameOver;

    private GameObject sceneManager;
    private ILeaderboard leaderboard;

#if UNITY_METRO
    // DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(List<Scores>));
#else
    [NonSerialized]
    BinaryFormatter bf = new BinaryFormatter();
    private bool scoresLoaded;
    private int highScoreRank;
#endif

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
        List<Scores> scores;

        var scoreText = PlayerPrefs.GetString("scores");
        if (!string.IsNullOrEmpty(scoreText))
        {
            try
            {
                var mbytes = Convert.FromBase64String(scoreText);

                using (var m = new MemoryStream(mbytes))
                {
#if UNITY_METRO
                // scores = json.ReadObject(m) as List<Scores>;
                scores = new List<Scores>();
#else
                    scores = bf.Deserialize(m) as List<Scores>;
#endif
                }
            }
            catch (Exception ex)
            {
                scores = new List<Scores>();
            }
        }
        else
        {
            scores = new List<Scores>();
        }

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

        using (var m = new MemoryStream())
        {
#if UNITY_METRO
            // json.WriteObject(m, scores);
#else
            bf.Serialize(m, scores);
            scoreText = Convert.ToBase64String(m.GetBuffer());
#endif
        }

        PlayerPrefs.SetString("scores", scoreText);
        PlayerPrefs.Save();


        if (Social.localUser.authenticated)
        {
            GameManager.Instance.PostToLeaderboard(score);
        }

        sceneManager.SendMessage("GameOver");
    }

}

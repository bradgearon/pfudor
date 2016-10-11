using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GooglePlayGames.BasicApi;
using Newtonsoft.Json;

#if UNITY_METRO
// using System.Runtime.Serialization.Json;
#else
using System.Runtime.Serialization.Formatters.Binary;
#endif

using UnityEngine;


public class ScoreManager : MonoBehaviour
{
    public AchievementMeta[] defaultAchievements;
    public float scoreScale;
    public UILabel scoreLabel;
    public UILabel jumpCountLabel;
    public UITable scoreTable;
    public UILabel scoreTableHeader;
    public UITable achievementsTable;
    public UILabel achievementsTableHeader;
    public UIButton buttonPrefab;
    public UISprite spritePrefab;
    public AdLauncher adLauncher;
    private UnityEngine.Random random = new UnityEngine.Random();

    public long highScore;
    public int fontSize = 32;

    private int score;
    private bool started;
    private int totalTime;
    private bool gameOver;

    private GameObject sceneManager;

    private List<Scores> scores = new List<Scores>();
    private List<SavedAchievement> savedAchievements = new List<SavedAchievement>();

    private ILookup<string, AchievementMeta> achievements;

    // Use this for initialization
    void Start()
    {
        sceneManager = FindObjectOfType<SceneManager>().gameObject;
        loadAchievements();
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

    void loadScores()
    {
        if (scores == null || scores.Count < 1)
        {
            scores = loadObject<List<Scores>>("scores");
        }
    }

    public void loadAchievements()
    {
        if (achievements == null || achievements.Count == 0)
        {
            achievements = defaultAchievements.ToLookup(a => a.Id);
        }

        if (GameManager.Instance.Authenticated)
        {
            var loaded = GameManager.Instance.GetAchievements();
            foreach (var achievement in loaded)
            {
                var localAchievement = achievements[achievement.id].FirstOrDefault();
                if (localAchievement != null)
                {
                    localAchievement.IsUnlocked = achievement.completed;

                    savedAchievements.Add(new SavedAchievement
                    {
                        Id = localAchievement.Id,
                        IsUnlocked = localAchievement.IsUnlocked,
                        RewardEnabled = localAchievement.IsUnlocked
                    });
                }
            }
            saveObject("achievements", savedAchievements);
        }
        else
        {
            savedAchievements = loadObject<List<SavedAchievement>>("achievements");
            if (savedAchievements.Count == 0)
            {
                savedAchievements = (from achievement in defaultAchievements
                                     select new SavedAchievement
                                     {
                                         Id = achievement.Id,
                                         IsUnlocked = false
                                     }).ToList();
                saveObject("achievements", savedAchievements);
            }

            foreach (var achievement in savedAchievements)
            {
                var localAchievement = achievements[achievement.Id].FirstOrDefault();
                if (localAchievement != null)
                {
                    localAchievement.IsUnlocked = achievement.IsUnlocked;
                }
            }
        }
    }

    void hideScores()
    {
        scoreTable.gameObject.SetActive(false);
        scoreTableHeader.gameObject.SetActive(false);

        achievementsTable.gameObject.SetActive(false);
        achievementsTableHeader.gameObject.SetActive(false);
    }

    void displayAchievements()
    {
        achievementsTable.gameObject.SetActive(true);
        achievementsTableHeader.gameObject.SetActive(true);

        loadAchievements();

        achievementsTable.columns = 4;
        achievementsTable.GetChildList().Clear();
        for (int i = 0; i < achievementsTable.gameObject.transform.childCount; i++)
        {
            Destroy(achievementsTable.gameObject.transform.GetChild(i).gameObject);
        }

        var font = scoreLabel.bitmapFont;
        foreach (var achievementItem in achievements.AsEnumerable())
        {
            var nameLabel = NGUITools.AddWidget<UILabel>(achievementsTable.gameObject);
            var descriptionLabel = NGUITools.AddWidget<UILabel>(achievementsTable.gameObject);
            var unlockedLabel = NGUITools.AddWidget<UILabel>(achievementsTable.gameObject);
            var achievement = achievementItem.FirstOrDefault();

            if (!(achievement.IsRevealed || achievement.IsUnlocked))
            {
                continue;
            }

            var saved = savedAchievements.FirstOrDefault(a => string.Compare(a.Id, achievement.Id, StringComparison.OrdinalIgnoreCase) == 0);

            var rewardSprite = NGUITools.AddWidget<UISprite>(achievementsTable.gameObject);
            if (achievement.IsUnlocked)
            {
                rewardSprite.color = saved.RewardEnabled ? buttonPrefab.disabledColor : buttonPrefab.defaultColor;
            }
            else
            {
                rewardSprite.enabled = false;
            }

            rewardSprite.width = 50;
            rewardSprite.height = 50;
            rewardSprite.name = "Label";
            rewardSprite.atlas = spritePrefab.atlas;
            rewardSprite.spriteName = spritePrefab.spriteName;

            rewardSprite.gameObject.AddComponent<BoxCollider>();
            rewardSprite.autoResizeBoxCollider = true;
            rewardSprite.ResizeCollider();

            UIEventListener.Get(rewardSprite.gameObject).onClick += go =>
            {
                Debug.Log("on click " + saved.RewardEnabled);
                saved.RewardEnabled = !saved.RewardEnabled;
                rewardSprite.color = saved.RewardEnabled ? buttonPrefab.disabledColor : buttonPrefab.defaultColor;
                saveAchievements();
            };


            nameLabel.bitmapFont = font;
            nameLabel.fontSize = fontSize;
            nameLabel.width = 200;
            nameLabel.height = 50;
            nameLabel.text = achievement.Name;

            descriptionLabel.bitmapFont = font;
            descriptionLabel.fontSize = fontSize;
            descriptionLabel.width = 400;
            descriptionLabel.height = 100;
            descriptionLabel.text = achievement.Description;

            unlockedLabel.bitmapFont = font;
            unlockedLabel.fontSize = fontSize;
            unlockedLabel.width = 100;
            unlockedLabel.height = 50;
            unlockedLabel.text = achievement.IsUnlocked ? "Unlocked" : "Locked";
        }

        achievementsTable.Reposition();
    }

    void displayScores()
    {
        scoreTable.gameObject.SetActive(true);
        scoreTableHeader.gameObject.SetActive(true);

        loadScores();
        scoreTable.GetChildList().Clear();
        scoreTable.columns = 2;

        for (int i = 0; i < scoreTable.gameObject.transform.childCount; i++)
        {
            Destroy(scoreTable.gameObject.transform.GetChild(i).gameObject);
        }

        Debug.Log("score count" + scores.Count);
        var scoresOrdered = scores.OrderByDescending(s => s.Score).Take(10).ToList();
        Debug.Log("scores ordered count " + scoresOrdered.Count());

        if (scoresOrdered.Count == 0)
        {
            scoresOrdered.Add(new Scores() { Name = "None" });
        }

        var font = scoreLabel.bitmapFont;

        foreach (var s in scoresOrdered)
        {
            Debug.Log("score " + s.Name + " " + s.Score);

            var nameLabel = NGUITools.AddWidget<UILabel>(scoreTable.gameObject);
            var valueLabel = NGUITools.AddWidget<UILabel>(scoreTable.gameObject);

            nameLabel.bitmapFont = font;
            nameLabel.fontSize = fontSize;
            nameLabel.width = 100;
            nameLabel.height = 50;
            nameLabel.text = s.Name;

            valueLabel.bitmapFont = font;
            valueLabel.fontSize = fontSize;
            valueLabel.width = 100;
            valueLabel.height = 50;
            if (s.Score > 0)
            {
                valueLabel.text = s.Score + string.Empty;
            }
        }

        scoreTable.Reposition();
    }


    void saveAchievements()
    {
        saveObject("achievements", savedAchievements);
    }

    void saveScores()
    {
        saveObject("scores", scores);
    }

    T loadObject<T>(string name) where T : new()
    {
        var objectText = PlayerPrefs.GetString(name);
        var result = default(T);

        if (!string.IsNullOrEmpty(objectText))
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(objectText);
            }
            catch (Exception ex)
            {
                result = new T();
            }
        }
        else
        {
            result = new T();
        }

        return result;
    }

    void saveObject<T>(string name, T toSave)
    {
        var objectText = string.Empty;
        try
        {
            objectText = JsonConvert.SerializeObject(toSave);
        }
        catch (Exception e)
        {
            Debug.Log("Error serializing: " + e.ToString());
        }

        PlayerPrefs.SetString(name, objectText);
        PlayerPrefs.Save();
    }

    void updateScores()
    {
        var showAd = UnityEngine.Random.Range(0, 1);
        if (showAd < .77)
        {
            adLauncher.ShowAdPlacement();
        }

        Debug.Log("score loading");
        loadScores();
        Debug.Log("score adding");
        var name = "Player";

        if (GameManager.Instance.Authenticated)
        {
            name = Social.localUser.userName;
        }

        scores.Add(new Scores { Name = name, Score = score });
        Debug.Log("score displaying");
        displayScores();
        Debug.Log("score saving");
        saveScores();

        foreach (var achievementItem in achievements)
        {
            var achievement = achievementItem.FirstOrDefault();
            if (!achievement.IsUnlocked && achievement.minScore < score)
            {
                achievement.IsRevealed = true;
                achievement.IsUnlocked = true;

                var saved = savedAchievements.FirstOrDefault(
                    a => string.Compare(a.Id, achievement.Id, StringComparison.OrdinalIgnoreCase) == 0);

                if (saved != null)
                {
                    saved.IsUnlocked = true;
                    saved.RewardEnabled = true;
                }

                if (Social.localUser.authenticated)
                {
                    Social.ReportProgress(achievement.Id, 100,
                        b => Debug.Log("Achievement " + achievement.Id + " unlocked"));
                }
            }
        }

        saveAchievements();


        if (Social.localUser.authenticated)
        {
            GameManager.Instance.PostToLeaderboard(score);
        }

        sceneManager.SendMessage("GameOver");
    }

    public void ActivateAchievements()
    {
        var customization = FindObjectOfType<SpriteCustomization>();
        if (customization == null) return;
        foreach (var localAchievement in achievements)
        {
            var achievement = localAchievement.FirstOrDefault();
            var savedAchievement = savedAchievements.FirstOrDefault(a => 
                string.Compare(a.Id, achievement.Id, StringComparison.OrdinalIgnoreCase) == 0);
            if(savedAchievement == null)
            {
                continue;
            }
            
            if (achievement.IsUnlocked && savedAchievement.RewardEnabled)
            {
                customization.SendMessage(achievement.activateMessage);
            }


        }


    }
}

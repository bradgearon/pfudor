using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[Serializable]
public class AchievementMeta
{
    public string Id = "";

    public int minScore;
    public string activateMessage;

    public bool IsIncremental = false;
    public bool IsRevealed = false;
    public bool IsUnlocked = false;
    public int CurrentSteps = 0;
    public int TotalSteps = 0;
    public string Description = "";
    public string Name = "";

}

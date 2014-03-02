using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GooglePlayGames.BasicApi;
using UnityEngine;

[Serializable]
public class AchievementMeta : Achievement
{
    public int minScore;
    public string activateMessage;
}

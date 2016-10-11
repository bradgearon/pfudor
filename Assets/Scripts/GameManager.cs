using GooglePlayGames.BasicApi;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using System.Linq;
using wds.pink.fluffy;
using UnityEngine.SocialPlatforms;

#if !UNITY_ANDROID && !UNITY_IOS
using UnityEngine.SocialPlatforms.Impl;
#endif

public class GameManager
{
    private static GameManager sInstance = new GameManager();
    private int mLevel = 0;
    private bool mAuthenticating;

    public bool playGameDebugEnabled = true;

    // what is the highest score we have posted to the leaderboard?
    private int mHighestPostedScore = 0;

    // cloud save callbacks
    private IAchievement[] achievements;

    public static GameManager Instance
    {
        get
        {
            return sInstance;
        }
    }

    void ReportAllProgress()
    {

    }

    public void RestartLevel()
    {
        ReportAllProgress();
    }

#if UNITY_ANDROID || UNITY_IOS
    public void Authenticate()
    {
        if (Authenticated || mAuthenticating)
        {
            Debug.LogWarning("Ignoring repeated call to Authenticate().");
            return;
        }

        // Enable/disable logs on the PlayGamesPlatform
        PlayGamesPlatform.DebugLogEnabled = playGameDebugEnabled;

        // Activate the Play Games platform. This will make it the default
        // implementation of Social.Active
        PlayGamesPlatform.Activate();

        // Set the default leaderboard for the leaderboards UI
        var platform = ((PlayGamesPlatform)Social.Active);
        
        platform.SetDefaultLeaderboardForUI(Constants.leaderboard_longest_bouncers);

        // Sign in to Google Play Games
        mAuthenticating = true;
        Social.localUser.Authenticate((bool success) =>
        {
            mAuthenticating = false;
            if (success)
            {
                PlayerPrefs.SetInt("autoAuth", 1);
                platform.LoadAchievements(achievements =>
                {
                    this.achievements = achievements.Cast<PlayGamesAchievement>().ToArray();
                });
            }
            else
            {
                // no need to show error message (error messages are shown automatically
                // by plugin)
                Debug.LogWarning("Failed to sign in with Google Play Games.");
            }
        });
    }
#else
    public void Authenticate() {
        mAuthenticating = false;
    }
#endif

    public IAchievement[] GetAchievements()
    {
        if (achievements == null)
        {
            achievements = new IAchievement[0];
        }
        return achievements;
    }
 
    // Data was successfully loaded from the cloud
    public void OnStateLoaded(bool success, int slot, byte[] data)
    {
        Debug.Log("Cloud load callback, success=" + success);
        if (success)
        {

        }
        else
        {
            Debug.LogWarning("Failed to load from cloud. Network problems?");
        }

        // regardless of success, this is the end of the auth process
        mAuthenticating = false;

        // report any progress we have to report
        ReportAllProgress();
    }

    public bool Authenticating
    {
        get
        {
            return mAuthenticating;
        }
    }

    public bool Authenticated
    {
        get
        {
            return Social.Active.localUser.authenticated;
        }
    }

    public void SignOut()
    {
#if UNITY_ANDROID || UNITY_IOS
        ((PlayGamesPlatform)Social.Active).SignOut();
#endif
    }

    public void ShowLeaderboardUI()
    {
        if (Authenticated)
        {
            Social.ShowLeaderboardUI();
        }
    }

    public void ShowAchievementsUI()
    {
        if (Authenticated)
        {
            Social.ShowAchievementsUI();
        }
    }

    public void PostToLeaderboard(int score)
    {
        if (Authenticated && score > mHighestPostedScore)
        {
            // post score to the leaderboard
            Social.ReportScore(score, Constants.leaderboard_longest_bouncers, (bool success) => { });
            mHighestPostedScore = score;
        }
    }


    public byte[] OnStateConflict(int slot, byte[] localData, byte[] serverData)
    {
        throw new System.NotImplementedException();
    }

    public void OnStateSaved(bool success, int slot)
    {
        throw new System.NotImplementedException();
    }


}
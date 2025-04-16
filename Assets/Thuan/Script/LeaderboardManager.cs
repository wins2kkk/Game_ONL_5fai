using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayFabLeaderboardManager : MonoBehaviour
{
    public static PlayFabLeaderboardManager Instance;

    [Header("Leaderboard UI")]
    public GameObject LeaderboardPanel;
    public Transform LeaderboardContent;
    public GameObject LeaderboardEntryPrefab;

    private bool _leaderboardShown = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ShowLeaderboard(int score)
    {
        if (_leaderboardShown) return;
        _leaderboardShown = true;

        SubmitScore(score);

        GetLeaderboard((leaderboard) =>
        {
            if (leaderboard == null) return;

            LeaderboardPanel.SetActive(true);

            // Clear old entries
            foreach (Transform child in LeaderboardContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var entry in leaderboard)
            {
                GameObject go = Instantiate(LeaderboardEntryPrefab, LeaderboardContent);
                TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();

                string playerName = string.IsNullOrEmpty(entry.DisplayName) ? "Unknown" : entry.DisplayName;
                int chickenKills = entry.StatValue;

                texts[0].text = $"{entry.Position + 1}. {playerName}";
                texts[1].text = $"🐔 x {chickenKills}";
            }

        });
    }

    public void SubmitScore(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "ChickenKills", Value = score }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => Debug.Log("Score submitted."),
            error => Debug.LogError("Submit score failed: " + error.GenerateErrorReport()));
    }

    public void GetLeaderboard(Action<List<PlayerLeaderboardEntry>> callback)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "ChickenKills",
            StartPosition = 0,
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(request,
            result => callback(result.Leaderboard),
            error =>
            {
                Debug.LogError("Get leaderboard failed: " + error.GenerateErrorReport());
                callback(null);
            });
    }
}

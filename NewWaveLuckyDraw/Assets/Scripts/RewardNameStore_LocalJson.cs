using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RewardNameStore_LocalJson : MonoBehaviour
{
    [SerializeField] private string rewardsFolderName = "rewards";
    [SerializeField] private string fileName = "reward_names.json";

    private string FolderPath => Path.Combine(Application.persistentDataPath, rewardsFolderName);
    private string FilePath => Path.Combine(FolderPath, fileName);

    [Serializable]
    private class RewardNameEntry
    {
        public string rewardId;
        public string rewardName;
    }

    [Serializable]
    private class RewardNameData
    {
        public List<RewardNameEntry> entries = new List<RewardNameEntry>();
    }

    private Dictionary<string, string> map = new Dictionary<string, string>();

    public void EnsureFolder()
    {
        try
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("RewardNameStore: EnsureFolder failed: " + e.Message);
        }
    }

    public void Load()
    {
        EnsureFolder();
        map.Clear();

        if (!File.Exists(FilePath))
            return;

        try
        {
            string json = File.ReadAllText(FilePath);
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<RewardNameData>(json);
            if (data?.entries == null) return;

            foreach (var e in data.entries)
            {
                if (string.IsNullOrEmpty(e.rewardId)) continue;
                map[e.rewardId] = e.rewardName ?? "";
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("RewardNameStore: Load failed: " + e.Message);
        }
    }

    public void Save()
    {
        EnsureFolder();

        try
        {
            var data = new RewardNameData();
            foreach (var kv in map)
            {
                data.entries.Add(new RewardNameEntry
                {
                    rewardId = kv.Key,
                    rewardName = kv.Value
                });
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("RewardNameStore: Save failed: " + e.Message);
        }
    }

    public bool TryGetName(string rewardId, out string rewardName)
    {
        return map.TryGetValue(rewardId, out rewardName);
    }

    public void SetName(string rewardId, string rewardName)
    {
        if (string.IsNullOrEmpty(rewardId)) return;
        map[rewardId] = rewardName ?? "";
    }

    public void RemoveName(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId)) return;
        if (map.ContainsKey(rewardId)) map.Remove(rewardId);
    }

    [ContextMenu("Log Reward Names File Path")]
    private void LogPath()
    {
        Debug.Log("RewardNameStore file: " + FilePath);
    }
}

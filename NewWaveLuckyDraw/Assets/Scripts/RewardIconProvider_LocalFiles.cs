using System;
using System.IO;
using UnityEngine;

public class RewardIconProvider_LocalFiles : MonoBehaviour
{
    [SerializeField] private string rewardFolderName = "rewards";

    private string RewardFolderPath => Path.Combine(Application.persistentDataPath, rewardFolderName);

    public void EnsureFolder()
    {
        try
        {
            if (!Directory.Exists(RewardFolderPath))
                Directory.CreateDirectory(RewardFolderPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"RewardIconProvider_LocalFiles: Cannot create folder. {e.Message}");
        }
    }

    public void ApplyOverrideIfExists(RewardSlotView slot)
    {
        if (slot == null) return;

        EnsureFolder();

        string path = FindExistingRewardImagePath(slot.rewardId);
        if (string.IsNullOrEmpty(path))
        {
            slot.ApplyDefaultIcon();
            return;
        }

        var spr = LoadSpriteFromFile(path);
        slot.SetIcon(spr);
    }

    public void ReloadAll(RewardSlotView[] slots)
    {
        EnsureFolder();
        if (slots == null) return;

        foreach (var s in slots)
            ApplyOverrideIfExists(s);
    }

    public bool SetOverrideFromFile(string rewardId, string sourceFilePath)
    {
        if (string.IsNullOrEmpty(rewardId)) return false;
        if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath)) return false;

        EnsureFolder();

        string ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
        {
            Debug.LogWarning("RewardIconProvider_LocalFiles: .png/.jpg/.jpeg");
            return false;
        }

        try
        {
            DeleteIfExists(Path.Combine(RewardFolderPath, rewardId + ".png"));
            DeleteIfExists(Path.Combine(RewardFolderPath, rewardId + ".jpg"));
            DeleteIfExists(Path.Combine(RewardFolderPath, rewardId + ".jpeg"));

            string destPath = Path.Combine(RewardFolderPath, rewardId + ext);
            File.Copy(sourceFilePath, destPath, true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"RewardIconProvider_LocalFiles: SetOverrideFromFile failed. {e.Message}");
            return false;
        }
    }

    public void ClearOverride(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId)) return;

        DeleteIfExists(Path.Combine(RewardFolderPath, rewardId + ".png"));
        DeleteIfExists(Path.Combine(RewardFolderPath, rewardId + ".jpg"));
        DeleteIfExists(Path.Combine(RewardFolderPath, rewardId + ".jpeg"));
    }

    private void DeleteIfExists(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* ignore */ }
    }

    private string FindExistingRewardImagePath(string rewardId)
    {
        if (string.IsNullOrEmpty(rewardId)) return null;

        string p1 = Path.Combine(RewardFolderPath, rewardId + ".png");
        if (File.Exists(p1)) return p1;

        string p2 = Path.Combine(RewardFolderPath, rewardId + ".jpg");
        if (File.Exists(p2)) return p2;

        string p3 = Path.Combine(RewardFolderPath, rewardId + ".jpeg");
        if (File.Exists(p3)) return p3;

        return null;
    }

    private Sprite LoadSpriteFromFile(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes == null || bytes.Length == 0) return null;

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes, false)) return null;

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"RewardIconProvider_LocalFiles: LoadSpriteFromFile failed: {e.Message}");
            return null;
        }
    }

    [ContextMenu("Log Reward Folder Path")]
    private void LogPath()
    {
        Debug.Log($"Reward override folder: {RewardFolderPath}");
    }
}

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RewardIconChangeButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RewardIconProvider_LocalFiles iconProvider;
    [SerializeField] private LuckyDrawSpinner spinnerToRefresh;

    [Header("Which reward to change (must match RewardSlotView.rewardId)")]
    [SerializeField] private string rewardId;

    [Header("Optional UI")]
    [SerializeField] private Button button;
    [SerializeField] private bool autoDisableWhilePicking = true;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OnClickPick);
    }

    public void OnClickPick()
    {
        if (iconProvider == null)
        {
            Debug.LogWarning("RewardIconChangeButton: iconProvider not set");
            return;
        }

        if (string.IsNullOrEmpty(rewardId))
        {
            Debug.LogWarning("RewardIconChangeButton: rewardId is empty");
            return;
        }

        if (autoDisableWhilePicking && button != null)
            button.interactable = false;

        NativeFilePicker.PickFile((path) =>
        {
            if (autoDisableWhilePicking && button != null)
                button.interactable = true;

            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("NativeFilePicker: canceled or permission denied");
                return;
            }

            Debug.Log("Picked file path/uri: " + path);
            StartCoroutine(CopyToSandboxAndApply(path));

        }, "image/*");
    }

    private IEnumerator CopyToSandboxAndApply(string pickedPathOrUri)
    {
        if (iconProvider == null) yield break;

        iconProvider.EnsureFolder();

        string ext = Path.GetExtension(pickedPathOrUri).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            ext = ".png";

        string destPath = Path.Combine(
            Application.persistentDataPath,
            "rewards",
            rewardId + ext
        );

#if UNITY_ANDROID && !UNITY_EDITOR
        if (pickedPathOrUri.StartsWith("content://"))
        {
            using (var req = UnityWebRequest.Get(pickedPathOrUri))
            {
                req.downloadHandler = new DownloadHandlerBuffer();
                yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    Debug.LogWarning("Failed to read content uri: " + req.error);
                    yield break;
                }

                byte[] bytes = req.downloadHandler.data;
                if (bytes == null || bytes.Length == 0)
                {
                    Debug.LogWarning("Picked file is empty");
                    yield break;
                }

                try
                {
                    File.WriteAllBytes(destPath, bytes);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("WriteAllBytes failed: " + e.Message);
                    yield break;
                }
            }
        }
        else
        {
            if (!File.Exists(pickedPathOrUri))
            {
                Debug.LogWarning("Picked file not found: " + pickedPathOrUri);
                yield break;
            }

            try
            {
                File.Copy(pickedPathOrUri, destPath, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("File.Copy failed: " + e.Message);
                yield break;
            }
        }
#else
        if (!File.Exists(pickedPathOrUri))
        {
            Debug.LogWarning("Picked file not found: " + pickedPathOrUri);
            yield break;
        }

        try
        {
            File.Copy(pickedPathOrUri, destPath, true);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("File.Copy failed: " + e.Message);
            yield break;
        }
#endif

        bool ok = iconProvider.SetOverrideFromFile(rewardId, destPath);
        if (!ok)
        {
            Debug.LogWarning("SetOverrideFromFile failed");
            yield break;
        }

        if (spinnerToRefresh != null)
            spinnerToRefresh.ReloadRewardImages();

        Debug.Log($"Reward icon updated for id: {rewardId}");
    }

    public void ClearOverride()
    {
        if (iconProvider == null || string.IsNullOrEmpty(rewardId)) return;

        iconProvider.ClearOverride(rewardId);

        if (spinnerToRefresh != null)
            spinnerToRefresh.ReloadRewardImages();
    }
}

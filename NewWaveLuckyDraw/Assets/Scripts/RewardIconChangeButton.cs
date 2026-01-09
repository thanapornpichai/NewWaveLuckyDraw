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
    [SerializeField] private Button pickButton;  
    [SerializeField] private Button resetButton;  
    [SerializeField] private bool autoDisableWhilePicking = true;

    private void Awake()
    {
        if (pickButton == null) pickButton = GetComponent<Button>();
        if (pickButton != null) pickButton.onClick.AddListener(OnClickPick);

        if (resetButton != null) resetButton.onClick.AddListener(ClearOverride);
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

        if (autoDisableWhilePicking && pickButton != null)
            pickButton.interactable = false;

        NativeFilePicker.PickFile((path) =>
        {
            if (autoDisableWhilePicking && pickButton != null)
                pickButton.interactable = true;

            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("NativeFilePicker: canceled or permission denied");
                return;
            }

            Debug.Log("Picked file path/uri: " + path);
            StartCoroutine(CopyToSandboxAndRefresh(path));

        }, "image/*");
    }

    private IEnumerator CopyToSandboxAndRefresh(string pickedPathOrUri)
    {
        iconProvider.EnsureFolder();

        string ext = Path.GetExtension(pickedPathOrUri).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            ext = ".png";

        string folder = Path.Combine(Application.persistentDataPath, "rewards");
        string destPath = Path.Combine(folder, rewardId + ext);

        SafeDelete(Path.Combine(folder, rewardId + ".png"));
        SafeDelete(Path.Combine(folder, rewardId + ".jpg"));
        SafeDelete(Path.Combine(folder, rewardId + ".jpeg"));

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

                File.WriteAllBytes(destPath, bytes);
            }
        }
        else
        {
            if (!File.Exists(pickedPathOrUri))
            {
                Debug.LogWarning("Picked file not found: " + pickedPathOrUri);
                yield break;
            }
            File.Copy(pickedPathOrUri, destPath, true);
        }
#else
        if (!File.Exists(pickedPathOrUri))
        {
            Debug.LogWarning("Picked file not found: " + pickedPathOrUri);
            yield break;
        }
        File.Copy(pickedPathOrUri, destPath, true);
#endif

        Debug.Log($"Override saved: {destPath}");

        if (spinnerToRefresh != null)
            spinnerToRefresh.ReloadRewardImages();
    }

    public void ClearOverride()
    {
        if (iconProvider == null || string.IsNullOrEmpty(rewardId))
            return;

        iconProvider.ClearOverride(rewardId);

        if (spinnerToRefresh != null)
            spinnerToRefresh.ReloadRewardImages();

        Debug.Log($"Override cleared. Back to default for id: {rewardId}");
    }

    private void SafeDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardNameEditButton : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RewardNameStore_LocalJson store;
    [SerializeField] private RewardSlotView targetSlot;

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;

    private string defaultNameFromInspector;

    private void Awake()
    {
        if (targetSlot != null)
            defaultNameFromInspector = targetSlot.rewardName;

        if (saveButton != null) saveButton.onClick.AddListener(Save);
        if (resetButton != null) resetButton.onClick.AddListener(ResetToDefault);

        SyncInputFromSlot();
    }

    public void SyncInputFromSlot()
    {
        if (inputField == null || targetSlot == null) return;
        inputField.text = targetSlot.rewardName;
    }

    public void Save()
    {
        if (store == null || targetSlot == null || inputField == null) return;
        if (string.IsNullOrEmpty(targetSlot.rewardId)) return;

        string newName = inputField.text?.Trim();
        targetSlot.SetRewardName(newName);

        store.Load();
        store.SetName(targetSlot.rewardId, newName);
        store.Save();

        Debug.Log($"Saved reward name: {targetSlot.rewardId} => {newName}");
    }

    public void ResetToDefault()
    {
        if (store == null || targetSlot == null) return;
        if (string.IsNullOrEmpty(targetSlot.rewardId)) return;

        targetSlot.SetRewardName(defaultNameFromInspector);

        store.Load();
        store.RemoveName(targetSlot.rewardId);
        store.Save();

        if (inputField != null)
            inputField.text = defaultNameFromInspector;

        Debug.Log($"Reset reward name to default: {targetSlot.rewardId}");
    }
}

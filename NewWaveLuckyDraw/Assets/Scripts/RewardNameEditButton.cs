using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardNameEditButton : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RewardSlotSelectorDropdown selector;
    [SerializeField] private RewardNameStore_LocalJson store;

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;

    private readonly Dictionary<string, string> defaultNameById = new Dictionary<string, string>();

    private void Awake()
    {
        if (saveButton != null) saveButton.onClick.AddListener(Save);
        if (resetButton != null) resetButton.onClick.AddListener(ResetToDefault);

        CacheDefaultsFromSlots();
        SyncInputFromSelected();
    }

    private RewardSlotView GetSelectedSlot()
    {
        if (selector == null)
        {
            Debug.LogWarning("RewardNameEditButton: selector not set");
            return null;
        }

        var slot = selector.CurrentSlot;
        if (slot == null)
        {
            //Debug.LogWarning("RewardNameEditButton: No slot selected");
            return null;
        }

        if (string.IsNullOrEmpty(slot.rewardId))
        {
            Debug.LogWarning("RewardNameEditButton: selected slot rewardId is empty");
            return null;
        }

        return slot;
    }

    private void CacheDefaultsFromSlots()
    {
        var slot = GetSelectedSlot();
        if (slot == null) return;

        if (!defaultNameById.ContainsKey(slot.rewardId))
            defaultNameById[slot.rewardId] = slot.rewardName;
    }

    public void SyncInputFromSelected()
    {
        var slot = GetSelectedSlot();
        if (slot == null || inputField == null) return;

        if (!defaultNameById.ContainsKey(slot.rewardId))
            defaultNameById[slot.rewardId] = slot.rewardName;

        inputField.text = slot.rewardName;
    }

    public void Save()
    {
        var slot = GetSelectedSlot();
        if (store == null || slot == null || inputField == null) return;

        string newName = inputField.text?.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("RewardNameEditButton: name is empty");
            return;
        }

        if (!defaultNameById.ContainsKey(slot.rewardId))
            defaultNameById[slot.rewardId] = slot.rewardName;

        slot.SetRewardName(newName);

        store.Load();
        store.SetName(slot.rewardId, newName);
        store.Save();

        if (selector != null)
        {
            selector.RebuildOptions();      
            selector.SyncValueToCurrent(); 
        }

        Debug.Log($"Saved reward name: {slot.rewardId} => {newName}");
    }

    public void ResetToDefault()
    {
        var slot = GetSelectedSlot();
        if (store == null || slot == null) return;

        if (!defaultNameById.TryGetValue(slot.rewardId, out var defaultName))
            defaultName = slot.rewardName;

        slot.SetRewardName(defaultName);

        store.Load();
        store.RemoveName(slot.rewardId);
        store.Save();

        if (inputField != null)
            inputField.text = defaultName;

        if (selector != null)
        {
            selector.RebuildOptions();
            selector.SyncValueToCurrent();
        }

        Debug.Log($"Reset reward name to default: {slot.rewardId}");
    }

}

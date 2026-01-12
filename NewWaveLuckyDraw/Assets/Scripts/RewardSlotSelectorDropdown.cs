using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardSlotSelectorDropdown : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private RewardSlotView[] slots;

    public RewardSlotView CurrentSlot { get; private set; }

    private void Awake()
    {
        if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();

        RebuildOptions();

        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (slots != null && slots.Length > 0)
            SetCurrentByIndex(0);
    }

    public void RebuildOptions()
    {
        if (dropdown == null) return;

        var prev = CurrentSlot; 

        dropdown.ClearOptions();

        var opts = new List<TMP_Dropdown.OptionData>();
        var list = new List<RewardSlotView>();

        if (slots != null)
        {
            foreach (var s in slots)
            {
                if (s == null) continue;
                list.Add(s);
                opts.Add(new TMP_Dropdown.OptionData($"{s.rewardId} - {s.rewardName}"));
            }
        }

        dropdown.AddOptions(opts);

        CurrentSlot = prev != null ? prev : (list.Count > 0 ? list[0] : null);
        int idx = (CurrentSlot != null) ? list.IndexOf(CurrentSlot) : 0;
        if (idx < 0) idx = 0;

        dropdown.SetValueWithoutNotify(idx);
        dropdown.RefreshShownValue();
    }


    private void OnDropdownChanged(int index)
    {
        SetCurrentByIndex(index);
    }

    private void SetCurrentByIndex(int dropdownIndex)
    {
        var list = new List<RewardSlotView>();
        foreach (var s in slots)
            if (s != null) list.Add(s);

        if (list.Count == 0) return;

        int clamped = Mathf.Clamp(dropdownIndex, 0, list.Count - 1);
        CurrentSlot = list[clamped];
    }

    public void SetSlots(RewardSlotView[] newSlots)
    {
        slots = newSlots;
        RebuildOptions();
        SetCurrentByIndex(0);
    }

    public void SyncValueToCurrent()
    {
        if (dropdown == null || slots == null || CurrentSlot == null) return;

        var list = new List<RewardSlotView>();
        foreach (var s in slots)
            if (s != null) list.Add(s);

        int index = list.IndexOf(CurrentSlot);
        if (index < 0) index = 0;

        dropdown.SetValueWithoutNotify(index);
        dropdown.RefreshShownValue();
    }

}

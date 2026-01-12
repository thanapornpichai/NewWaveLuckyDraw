using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardSlotSelectorDropdown : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Dropdown dropdown;

    [Header("Slots (from Registry)")]
    [SerializeField] private RewardSlotsRegistry slotsRegistry;

    public RewardSlotView CurrentSlot { get; private set; }

    private readonly List<RewardSlotView> cachedList = new List<RewardSlotView>();

    private void Awake()
    {
        if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();

        if (slotsRegistry != null)
            slotsRegistry.OnSlotsChanged += HandleSlotsChanged;

        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void Start()
    {
        slotsRegistry?.RefreshSlots();
        RebuildOptions();

        if (CurrentSlot == null && cachedList.Count > 0)
            SetCurrentByIndex(0);
    }

    private void OnDestroy()
    {
        if (slotsRegistry != null)
            slotsRegistry.OnSlotsChanged -= HandleSlotsChanged;

        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void HandleSlotsChanged()
    {
        RebuildOptions();
    }

    public void RebuildOptions()
    {
        if (dropdown == null) return;

        var prev = CurrentSlot;

        dropdown.ClearOptions();
        cachedList.Clear();

        var opts = new List<TMP_Dropdown.OptionData>();

        if (slotsRegistry != null && slotsRegistry.Slots != null)
        {
            var slots = slotsRegistry.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                var s = slots[i];
                if (s == null) continue;

                cachedList.Add(s);
                opts.Add(new TMP_Dropdown.OptionData($"{s.rewardId} - {s.rewardName}"));
            }
        }

        dropdown.AddOptions(opts);

        if (prev != null && cachedList.Contains(prev))
            CurrentSlot = prev;
        else
            CurrentSlot = (cachedList.Count > 0) ? cachedList[0] : null;

        int idx = (CurrentSlot != null) ? cachedList.IndexOf(CurrentSlot) : 0;
        if (idx < 0) idx = 0;

        dropdown.SetValueWithoutNotify(idx);
        dropdown.RefreshShownValue();
    }

    private void OnDropdownChanged(int index)
    {
        SetCurrentByIndex(index);
    }

    public void SetCurrentByIndex(int dropdownIndex)
    {
        if (cachedList.Count == 0) return;

        int clamped = Mathf.Clamp(dropdownIndex, 0, cachedList.Count - 1);
        CurrentSlot = cachedList[clamped];
    }

    public void RefreshLabelsKeepSelection()
    {
        var prev = CurrentSlot;
        RebuildOptions();
        CurrentSlot = prev != null && cachedList.Contains(prev) ? prev : CurrentSlot;
        SyncValueToCurrent();
    }

    public void SyncValueToCurrent()
    {
        if (dropdown == null || CurrentSlot == null || cachedList.Count == 0) return;

        int index = cachedList.IndexOf(CurrentSlot);
        if (index < 0) index = 0;

        dropdown.SetValueWithoutNotify(index);
        dropdown.RefreshShownValue();
    }

    public IReadOnlyList<RewardSlotView> GetCurrentList()
    {
        return cachedList;
    }
}

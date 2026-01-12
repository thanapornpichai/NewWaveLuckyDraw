using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardSlotsRegistry : MonoBehaviour
{
    [Header("Where slots live (parent transform)")]
    [SerializeField] private Transform slotsRoot;

    [Header("Options")]
    [SerializeField] private bool includeInactive = true;
    [SerializeField] private bool autoRefreshOnEnable = true;

    private readonly List<RewardSlotView> slots = new List<RewardSlotView>();

    public event Action OnSlotsChanged;

    public IReadOnlyList<RewardSlotView> Slots => slots;

    private void OnEnable()
    {
        if (autoRefreshOnEnable)
            RefreshSlots();
    }

    public void SetRoot(Transform newRoot, bool refresh = true)
    {
        slotsRoot = newRoot;
        if (refresh) RefreshSlots();
    }

    [ContextMenu("Refresh Slots")]
    public void RefreshSlots()
    {
        slots.Clear();

        if (slotsRoot == null)
        {
            Debug.LogWarning("RewardSlotsRegistry: slotsRoot not set");
            OnSlotsChanged?.Invoke();
            return;
        }

        var found = slotsRoot.GetComponentsInChildren<RewardSlotView>(includeInactive);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null)
                slots.Add(found[i]);
        }

        OnSlotsChanged?.Invoke();
    }

    public RewardSlotView GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }

    public int IndexOf(RewardSlotView slot)
    {
        return slot == null ? -1 : slots.IndexOf(slot);
    }
}

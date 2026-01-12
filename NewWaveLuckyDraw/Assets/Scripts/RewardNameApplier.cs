using UnityEngine;

public class RewardNameApplier : MonoBehaviour
{
    [SerializeField] private RewardNameStore_LocalJson store;

    [Header("Slots (from Registry)")]
    [SerializeField] private RewardSlotsRegistry slotsRegistry;

    private void Awake()
    {
        if (slotsRegistry != null)
            slotsRegistry.OnSlotsChanged += HandleSlotsChanged;
    }

    private void OnDestroy()
    {
        if (slotsRegistry != null)
            slotsRegistry.OnSlotsChanged -= HandleSlotsChanged;
    }

    private void Start()
    {
        slotsRegistry?.RefreshSlots();
        Apply();
    }

    private void HandleSlotsChanged()
    {
        Apply();
    }

    public void Apply()
    {
        if (store == null)
        {
            Debug.LogWarning("RewardNameApplier: store is null");
            return;
        }

        if (slotsRegistry == null || slotsRegistry.Slots == null)
        {
            Debug.LogWarning("RewardNameApplier: slotsRegistry is null");
            return;
        }

        store.Load();

        var slots = slotsRegistry.Slots;
        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            if (s == null) continue;
            if (string.IsNullOrEmpty(s.rewardId)) continue;

            if (store.TryGetName(s.rewardId, out var name) && !string.IsNullOrEmpty(name))
                s.SetRewardName(name);  
            else
                s.RefreshNameUI();
        }
    }
}

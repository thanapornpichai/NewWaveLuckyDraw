using UnityEngine;

public class RewardNameApplier : MonoBehaviour
{
    [SerializeField] private RewardNameStore_LocalJson store;
    [SerializeField] private RewardSlotView[] slots;

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        if (store == null || slots == null) return;

        store.Load();

        foreach (var s in slots)
        {
            if (s == null) continue;
            if (string.IsNullOrEmpty(s.rewardId)) continue;

            if (store.TryGetName(s.rewardId, out var name) && !string.IsNullOrEmpty(name))
                s.SetRewardName(name);
            else
                s.RefreshNameUI();
        }
    }
}

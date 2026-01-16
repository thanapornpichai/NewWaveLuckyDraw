using System.Text;
using TMPro;
using UnityEngine;

public class LuckyDrawLandingStats : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RewardSlotsRegistry slotsRegistry;
    [SerializeField] private TMP_Text statsText;

    [Header("Prefs Keys")]
    [SerializeField] private string prefsKeyPrefix = "LuckyDrawLand_";
    [SerializeField] private string prefsTotalKey = "LuckyDrawLand_TOTAL";

    private string KeyFor(string rewardId)
    {
        string id = string.IsNullOrEmpty(rewardId) ? "UNKNOWN" : rewardId;
        return prefsKeyPrefix + id;
    }

    private void OnEnable()
    {
        if (slotsRegistry != null)
            slotsRegistry.OnSlotsChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (slotsRegistry != null)
            slotsRegistry.OnSlotsChanged -= RefreshUI;
    }

    public void RecordLanding(RewardSlotView landedSlot)
    {
        if (landedSlot == null) return;

        string key = KeyFor(landedSlot.rewardId);
        int count = PlayerPrefs.GetInt(key, 0);
        PlayerPrefs.SetInt(key, count + 1);

        int total = PlayerPrefs.GetInt(prefsTotalKey, 0);
        PlayerPrefs.SetInt(prefsTotalKey, total + 1);

        PlayerPrefs.Save();
        RefreshUI();
    }

    public int GetCount(RewardSlotView slot)
    {
        if (slot == null) return 0;
        return PlayerPrefs.GetInt(KeyFor(slot.rewardId), 0);
    }

    public int GetTotal() => PlayerPrefs.GetInt(prefsTotalKey, 0);

    public void ResetAllStats()
    {
        if (slotsRegistry != null && slotsRegistry.Slots != null)
        {
            for (int i = 0; i < slotsRegistry.Slots.Count; i++)
            {
                var s = slotsRegistry.Slots[i];
                if (s == null) continue;
                PlayerPrefs.DeleteKey(KeyFor(s.rewardId));
            }
        }

        PlayerPrefs.DeleteKey(prefsTotalKey);
        PlayerPrefs.Save();
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (statsText == null) return;

        var sb = new StringBuilder(512);

        sb.AppendLine($"Total Landings: {GetTotal()}");
        sb.AppendLine("---------------");

        if (slotsRegistry == null || slotsRegistry.Slots == null || slotsRegistry.Slots.Count == 0)
        {
            sb.AppendLine("(no slots)");
            statsText.text = sb.ToString();
            return;
        }

        for (int i = 0; i < slotsRegistry.Slots.Count; i++)
        {
            var s = slotsRegistry.Slots[i];
            if (s == null) continue;

            string name = string.IsNullOrEmpty(s.rewardName) ? s.gameObject.name : s.rewardName;
            sb.AppendLine($"{i:00}. {name} ({s.rewardId}) = {GetCount(s)}");
        }

        statsText.text = sb.ToString();
    }
}

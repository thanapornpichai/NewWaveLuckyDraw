using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardSlotView : MonoBehaviour
{
    [Header("Identity")]
    public string rewardId;       
    public string rewardName;     

    [Header("Weight (for chance)")]
    public int weight = 10;

    [Header("UI References")]
    public Image bulbImage;
    public Image frameImage;
    public Image iconImage;

    [Header("Optional Name Text")]
    [SerializeField] private TMP_Text nameText;

    [Header("Default Icon")]
    public Sprite defaultSprite;

    public void ApplyDefaultIcon()
    {
        if (iconImage == null) return;
        iconImage.sprite = defaultSprite;
        iconImage.preserveAspect = true;
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null) return;
        iconImage.sprite = sprite != null ? sprite : defaultSprite;
        iconImage.preserveAspect = true;
    }

    public void SetRewardName(string newName)
    {
        rewardName = newName;
        RefreshNameUI();
    }

    public void RefreshNameUI()
    {
        if (nameText != null)
            nameText.text = rewardName;
    }

    public void ResetVisual(Color idleBulbColor)
    {
        if (bulbImage != null) bulbImage.color = idleBulbColor;
        if (frameImage != null) frameImage.gameObject.SetActive(false);
    }

    public void SetRunning(Color runningBulbColor, Color runningFrameColor)
    {
        if (bulbImage != null) bulbImage.color = runningBulbColor;

        if (frameImage != null)
        {
            frameImage.gameObject.SetActive(true);
            frameImage.color = runningFrameColor;
        }
    }

    public void SetWin(Color winBulbColor, Color winFrameColor)
    {
        if (bulbImage != null) bulbImage.color = winBulbColor;

        if (frameImage != null)
        {
            frameImage.gameObject.SetActive(true);
            frameImage.color = winFrameColor;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardSlotView : MonoBehaviour
{
    [Header("Identity")]
    public string rewardId;
    public string rewardName;

    [Header("Weight (chance)")]
    public int weight = 10;

    [Header("UI")]
    public Image bulbImage;
    public Image frameImage;
    public Image iconImage;

    [Header("Optional Name Text")]
    [SerializeField] private TMP_Text nameText;

    [Header("Reward Icon Default")]
    public Sprite defaultSprite;

    [Header("Bulb Sprites")]
    public Sprite bulbIdleSprite;

    public Sprite bulbRunSprite;

    public Sprite bulbWinSprite;

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
        if (bulbImage != null)
        {
            if (bulbIdleSprite != null) bulbImage.sprite = bulbIdleSprite;

            bulbImage.color = Color.white;
        }

        if (frameImage != null)
            frameImage.gameObject.SetActive(false);
    }

    public void SetRunning(Color runningBulbColor, Color runningFrameColor)
    {
        if (bulbImage != null)
        {
            if (bulbRunSprite != null) bulbImage.sprite = bulbRunSprite;
            bulbImage.color = Color.white;
        }

        if (frameImage != null)
        {
            frameImage.gameObject.SetActive(true);
            frameImage.color = runningFrameColor;
        }
    }

    public void SetWin(Color winBulbColor, Color winFrameColor)
    {
        if (bulbImage != null)
        {
            if (bulbWinSprite != null) bulbImage.sprite = bulbWinSprite;
            else if (bulbRunSprite != null) bulbImage.sprite = bulbRunSprite;

            bulbImage.color = Color.white;
        }

        if (frameImage != null)
        {
            frameImage.gameObject.SetActive(true);
            frameImage.color = winFrameColor;
        }
    }
}

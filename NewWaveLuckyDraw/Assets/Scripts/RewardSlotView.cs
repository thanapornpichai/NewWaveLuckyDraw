using System;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlotView : MonoBehaviour
{
    [Header("Data")]
    public string rewardId = "reward_0";
    public string rewardName = "Reward";

    [Header("UI")]
    public Image iconImage;
    public Image bulbImage;
    public Image frameImage;

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

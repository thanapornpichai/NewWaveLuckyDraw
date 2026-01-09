using UnityEngine;
using UnityEngine.UI;

public class RewardSlotView : MonoBehaviour
{
    [Header("Reward Data")]
    public string rewardId;     
    public string rewardName;

    [Min(0)] public int weight = 10;

    [Header("UI")]
    public Image bulbImage;
    public Image frameImage;
    public Image iconImage;
    public Sprite defaultSprite;

    public void ApplyDefaultIcon()
    {
        if (iconImage != null)
        {
            iconImage.sprite = defaultSprite;
            iconImage.preserveAspect = true;
        }
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null) return;

        iconImage.sprite = sprite != null ? sprite : defaultSprite;
        iconImage.preserveAspect = true;
    }

    public void ResetVisual(Color idleColor)
    {
        if (bulbImage != null) bulbImage.color = idleColor;
        if (frameImage != null) frameImage.gameObject.SetActive(false);
    }

    public void SetRunning(Color bulbColor, Color frameColor)
    {
        if (bulbImage != null) bulbImage.color = bulbColor;
        if (frameImage != null)
        {
            frameImage.gameObject.SetActive(true);
            frameImage.color = frameColor;
        }
    }

    public void SetWin(Color bulbColor, Color frameColor)
    {
        if (bulbImage != null) bulbImage.color = bulbColor;
        if (frameImage != null)
        {
            frameImage.gameObject.SetActive(true);
            frameImage.color = frameColor;
        }
    }
}

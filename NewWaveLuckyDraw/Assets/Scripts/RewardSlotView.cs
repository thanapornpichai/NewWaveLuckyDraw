using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardSlotView : MonoBehaviour
{
    [Header("Identity")]
    public string rewardId;
    public string rewardName;

    [Header("Chance Weight")]
    public int weight = 10;

    [Header("Stock")]
    public int quantity = 1;
    [SerializeField] private TMP_Text qtyText;

    [Header("Stock Persistence")]
    [SerializeField] private int defaultQuantity = 1;
    [SerializeField] private string qtyPrefsKeyPrefix = "LuckyDrawQty_";

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

    [Header("Per Slot Colors")]
    public Color runningFrameColor = Color.red;
    public Color winFrameColor = Color.green;

    [Header("Unavailable Visual")]
    [SerializeField] private bool grayOutWhenUnavailable = true;
    [SerializeField] private Color unavailableTint = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private GameObject unavailableOverlay;

    private bool isUnavailable;

    private string QtyKey
    {
        get
        {
            string id = string.IsNullOrEmpty(rewardId) ? gameObject.name : rewardId;
            return qtyPrefsKeyPrefix + id;
        }
    }

    public int GetDefaultQuantity() => defaultQuantity;

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

    public void LoadQuantityFromPrefs()
    {
        int loaded = PlayerPrefs.GetInt(QtyKey, defaultQuantity);
        quantity = Mathf.Max(0, loaded);
        RefreshQuantityUI();
    }

    public void SaveQuantityToPrefs()
    {
        PlayerPrefs.SetInt(QtyKey, Mathf.Max(0, quantity));
        PlayerPrefs.Save();
    }

    public void SetQuantity(int newQuantity)
    {
        quantity = Mathf.Max(0, newQuantity);
        RefreshQuantityUI();
        SaveQuantityToPrefs();
    }

    public void SetQuantityAndSave(int newQuantity)
    {
        SetQuantity(newQuantity);
    }

    public void ResetQuantityToDefaultAndSave()
    {
        quantity = Mathf.Max(0, defaultQuantity);
        RefreshQuantityUI();
        SaveQuantityToPrefs();
    }

    public void RefreshQuantityUI()
    {
        if (qtyText != null)
            qtyText.text = quantity.ToString();

        UpdateQuantityVisibility();
    }

    public void SetQuantityVisible(bool visible)
    {
        if (qtyText != null)
            qtyText.gameObject.SetActive(visible);
    }

    public void SetUnavailable(bool unavailable)
    {
        isUnavailable = unavailable;

        if (unavailableOverlay != null)
            unavailableOverlay.SetActive(unavailable);

        ApplyUnavailableTint();
    }

    private void ApplyUnavailableTint()
    {
        if (!grayOutWhenUnavailable) return;

        Color tint = isUnavailable ? unavailableTint : Color.white;

        if (iconImage != null) iconImage.color = tint;
        if (bulbImage != null) bulbImage.color = tint;
    }
    private void UpdateQuantityVisibility()
    {
        bool visible = quantity > 0;

        if (qtyText != null)
            qtyText.gameObject.SetActive(visible);
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

        ApplyUnavailableTint();
    }

    public void SetRunning(Color runningBulbColor, Color fallbackRunningFrameColor)
    {
        if (bulbImage != null)
        {
            if (bulbRunSprite != null) bulbImage.sprite = bulbRunSprite;
            bulbImage.color = Color.white;
        }

        if (frameImage != null)
        {
            frameImage.gameObject.SetActive(true);
            Color c = runningFrameColor.a > 0f ? runningFrameColor : fallbackRunningFrameColor;
            frameImage.color = c;
        }

        ApplyUnavailableTint();
    }

    public void SetWin(Color winBulbColor, Color fallbackWinFrameColor)
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
            Color c = winFrameColor.a > 0f ? winFrameColor : fallbackWinFrameColor;
            frameImage.color = c;
        }

        ApplyUnavailableTint();
    }
}

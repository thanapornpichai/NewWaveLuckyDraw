using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LuckyDrawSpinner : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private RewardSlotView[] slots;

    [Header("Optional: Icon Provider (Local Files)")]
    [SerializeField] private RewardIconProvider_LocalFiles iconProvider;

    [Header("Colors")]
    [SerializeField] private Color idleBulbColor = Color.white;
    [SerializeField] private Color runningBulbColor = new Color(1f, 0.95f, 0.3f, 1f);
    [SerializeField] private Color runningFrameColor = new Color(1f, 0.95f, 0.3f, 1f);
    [SerializeField] private Color winBulbColor = Color.green;
    [SerializeField] private Color winFrameColor = Color.green;

    [Header("Spin Timing")]
    [SerializeField] private float startStepDelay = 0.04f;
    [SerializeField] private float endStepDelay = 0.12f;
    [SerializeField] private int minCycles = 1;
    [SerializeField] private int maxCycles = 2;
    [SerializeField] private int extraStepsMin = 0;
    [SerializeField] private int extraStepsMax = 3;

    [Header("Result Popup")]
    [SerializeField] private GameObject resultPopupRoot;
    [SerializeField] private RectTransform popupContent;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image resultIcon;
    [SerializeField] private float showPopupDelay = 1f;

    [Header("Popup Tween")]
    [SerializeField] private float popupStartScale = 0.6f;
    [SerializeField] private float popupOpenDuration = 0.4f;
    [SerializeField] private float popupCloseDuration = 0.25f;
    [SerializeField] private Ease popupOpenEase = Ease.OutBack;
    [SerializeField] private Ease popupCloseEase = Ease.InBack;

    [Header("Spin Button")]
    [SerializeField] private Button spinButton;

    [Header("Spin Button Tween")]
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 0.6f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource popupLoopSource;
    [SerializeField] private AudioClip spinClickSfx;
    [SerializeField] private AudioClip tickStepSfx;
    [SerializeField] private AudioClip popupLoopSfx;

    private bool isSpinning;
    private int currentIndex = -1;

    private Tween spinButtonTween;
    private RectTransform spinButtonRect;
    private Tween popupTween;

    private void Awake()
    {
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(Spin);
            spinButtonRect = spinButton.GetComponent<RectTransform>();
        }

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (popupLoopSource == null)
            popupLoopSource = gameObject.AddComponent<AudioSource>();

        popupLoopSource.loop = true;

        ReloadRewardImages();
        ResetAllVisuals();

        if (resultPopupRoot != null) resultPopupRoot.SetActive(false);
        SetSpinButtonState(true);
    }

    public void ReloadRewardImages()
    {
        if (iconProvider != null)
            iconProvider.ReloadAll(slots);
    }

    public void Spin()
    {
        if (isSpinning) return;
        if (resultPopupRoot != null && resultPopupRoot.activeSelf) return;

        if (slots == null || slots.Length == 0) return;

        isSpinning = true;
        SetSpinButtonState(false);

        if (spinClickSfx != null)
            sfxSource.PlayOneShot(spinClickSfx);

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        ResetAllVisuals();

        int targetIndex = GetWeightedRandomIndex();
        int count = slots.Length;

        if (currentIndex < 0)
            currentIndex = Random.Range(0, count);

        int cycles = Random.Range(minCycles, maxCycles + 1);
        int extraSteps = Random.Range(extraStepsMin, extraStepsMax + 1);
        int offset = GetForwardDistance(currentIndex, targetIndex, count);
        int totalSteps = cycles * count + offset + extraSteps;

        for (int i = 0; i < totalSteps; i++)
        {
            int next = (currentIndex + Random.Range(1, 4)) % count;
            HighlightRunning(next);
            currentIndex = next;

            if (tickStepSfx != null)
                sfxSource.PlayOneShot(tickStepSfx);

            float t = i / (float)totalSteps;
            float delay = Mathf.Lerp(startStepDelay, endStepDelay, t);
            yield return new WaitForSeconds(delay);
        }

        currentIndex = targetIndex;
        HighlightWin(currentIndex);

        yield return new WaitForSeconds(showPopupDelay);
        ShowResultPopup(currentIndex);

        isSpinning = false;
    }

    private int GetWeightedRandomIndex()
    {
        int total = 0;
        foreach (var s in slots)
            if (s != null)
                total += Mathf.Max(0, s.weight);

        if (total <= 0)
        {
            Debug.LogWarning("All weights are 0. Fallback to uniform random.");
            return Random.Range(0, slots.Length);
        }

        int rand = Random.Range(0, total);
        int sum = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            sum += Mathf.Max(0, slots[i].weight);
            if (rand < sum) return i;
        }

        return 0;
    }

    private void ShowResultPopup(int index)
    {
        var slot = slots[index];

        if (resultText != null)
            resultText.text = slot.rewardName;

        if (resultIcon != null && slot.iconImage != null)
        {
            resultIcon.sprite = slot.iconImage.sprite;
            resultIcon.preserveAspect = true;
        }

        if (popupLoopSfx != null)
        {
            popupLoopSource.clip = popupLoopSfx;
            popupLoopSource.Play();
        }

        resultPopupRoot.SetActive(true);
        popupContent.localScale = Vector3.one * popupStartScale;

        popupTween?.Kill();
        popupTween = popupContent
            .DOScale(1f, popupOpenDuration)
            .SetEase(popupOpenEase);
    }

    public void ClosePopup()
    {
        if (popupLoopSource != null) popupLoopSource.Stop();

        popupTween?.Kill();
        popupTween = popupContent
            .DOScale(popupStartScale, popupCloseDuration)
            .SetEase(popupCloseEase)
            .OnComplete(() =>
            {
                resultPopupRoot.SetActive(false);
                SetSpinButtonState(true);
            });
    }

    private void ResetAllVisuals()
    {
        foreach (var s in slots)
            if (s != null)
                s.ResetVisual(idleBulbColor);
    }

    private void HighlightRunning(int index)
    {
        ResetAllVisuals();
        slots[index].SetRunning(runningBulbColor, runningFrameColor);
    }

    private void HighlightWin(int index)
    {
        ResetAllVisuals();
        slots[index].SetWin(winBulbColor, winFrameColor);
    }

    private void SetSpinButtonState(bool enable)
    {
        if (spinButton != null)
            spinButton.interactable = enable;

        if (spinButtonRect == null) return;

        if (enable)
        {
            spinButtonTween?.Kill();
            spinButtonTween = spinButtonRect
                .DOScale(pulseScale, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            spinButtonTween?.Kill();
            spinButtonRect.localScale = Vector3.one;
        }
    }

    private int GetForwardDistance(int from, int to, int count)
    {
        int d = to - from;
        if (d < 0) d += count;
        return d;
    }
}

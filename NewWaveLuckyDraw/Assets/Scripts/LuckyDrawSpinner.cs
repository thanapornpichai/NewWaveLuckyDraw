using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LuckyDrawSpinner : MonoBehaviour
{
    [Header("Slots (RewardSlotView Components)")]
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
    [SerializeField, Range(0f, 1f)] private float spinClickVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float tickStepVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float popupLoopVolume = 1f;

    private bool isSpinning;
    private int currentIndex = -1;
    private Coroutine spinRoutine;

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
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (popupLoopSource == null)
            popupLoopSource = gameObject.AddComponent<AudioSource>();

        popupLoopSource.loop = true;
        popupLoopSource.playOnAwake = false;

        ApplyRewardIcons();
        ResetAllVisuals();

        if (resultPopupRoot != null)
            resultPopupRoot.SetActive(false);

        SetSpinButtonState(true);
    }

    public void ReloadRewardImages()
    {
        ApplyRewardIcons();
    }

    private void ApplyRewardIcons()
    {
        if (slots == null) return;

        foreach (var s in slots)
            if (s != null) s.ApplyDefaultIcon();

        if (iconProvider != null)
            iconProvider.ReloadAll(slots);
    }

    public void Spin()
    {
        if (isSpinning) return;
        if (resultPopupRoot != null && resultPopupRoot.activeSelf) return;

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("LuckyDrawSpinner: slots empty");
            return;
        }

        PlayOneShot(spinClickSfx, spinClickVolume);

        ForceHidePopup();
        SetSpinButtonState(false);

        isSpinning = true;

        if (spinRoutine != null) StopCoroutine(spinRoutine);
        spinRoutine = StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        ResetAllVisuals();

        int count = slots.Length;
        int targetIndex = Random.Range(0, count);

        int cycles = Random.Range(minCycles, maxCycles + 1);
        int extraSteps = Random.Range(extraStepsMin, extraStepsMax + 1);

        if (currentIndex < 0)
            currentIndex = Random.Range(0, count);

        int offsetToTarget = GetForwardDistance(currentIndex, targetIndex, count);
        int totalSteps = (cycles * count) + offsetToTarget + extraSteps;

        for (int step = 1; step <= totalSteps; step++)
        {
            int jump = Random.Range(1, 4);
            int nextIndex = (currentIndex + jump) % count;

            HighlightRunning(nextIndex);
            currentIndex = nextIndex;

            PlayOneShot(tickStepSfx, tickStepVolume);

            float t = step / (float)totalSteps;
            float delay = Mathf.Lerp(startStepDelay, endStepDelay, EaseOutQuad(t));
            yield return new WaitForSeconds(delay);
        }

        currentIndex = targetIndex;
        HighlightWin(currentIndex);

        yield return new WaitForSeconds(showPopupDelay);
        ShowResultPopup(currentIndex);

        isSpinning = false;
        spinRoutine = null;
    }

    private void ResetAllVisuals()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            slots[i].ResetVisual(idleBulbColor);
        }
    }

    private void HighlightRunning(int index)
    {
        ResetAllVisuals();

        if (slots == null || index < 0 || index >= slots.Length) return;
        if (slots[index] == null) return;

        slots[index].SetRunning(runningBulbColor, runningFrameColor);
    }

    private void HighlightWin(int index)
    {
        ResetAllVisuals();

        if (slots == null || index < 0 || index >= slots.Length) return;
        if (slots[index] == null) return;

        slots[index].SetWin(winBulbColor, winFrameColor);
    }

    private void ShowResultPopup(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length)
        {
            SetSpinButtonState(true);
            return;
        }

        var slot = slots[index];
        if (slot == null)
        {
            SetSpinButtonState(true);
            return;
        }

        if (resultText != null)
            resultText.text = slot.rewardName;

        if (resultIcon != null)
        {
            var spr = slot.iconImage != null ? slot.iconImage.sprite : slot.defaultSprite;
            resultIcon.sprite = spr;
            resultIcon.preserveAspect = true;
        }

        if (resultPopupRoot == null || popupContent == null)
        {
            SetSpinButtonState(true);
            return;
        }

        SetSpinButtonState(false);

        resultPopupRoot.SetActive(true);

        popupTween?.Kill();
        popupContent.localScale = Vector3.one * popupStartScale;

        popupTween = popupContent
            .DOScale(1f, popupOpenDuration)
            .SetEase(popupOpenEase);

        PlayPopupLoop();
    }

    public void ClosePopup()
    {
        StopPopupLoop();

        if (resultPopupRoot == null || popupContent == null)
        {
            if (resultPopupRoot != null) resultPopupRoot.SetActive(false);
            SetSpinButtonState(true);
            return;
        }

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

    private void ForceHidePopup()
    {
        popupTween?.Kill();
        StopPopupLoop();

        if (resultPopupRoot != null) resultPopupRoot.SetActive(false);
        if (popupContent != null) popupContent.localScale = Vector3.one;
    }

    private void SetSpinButtonState(bool enable)
    {
        if (spinButton != null)
            spinButton.interactable = enable;

        if (enable) StartSpinButtonPulse();
        else StopSpinButtonPulse();
    }

    private void StartSpinButtonPulse()
    {
        if (spinButtonRect == null) return;

        spinButtonTween?.Kill();
        spinButtonRect.localScale = Vector3.one;

        spinButtonTween = spinButtonRect
            .DOScale(pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopSpinButtonPulse()
    {
        spinButtonTween?.Kill();
        spinButtonTween = null;

        if (spinButtonRect != null)
            spinButtonRect.localScale = Vector3.one;
    }

    private int GetForwardDistance(int from, int to, int count)
    {
        int d = to - from;
        if (d < 0) d += count;
        return d;
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }


    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    private void PlayPopupLoop()
    {
        if (popupLoopSource == null || popupLoopSfx == null) return;

        if (popupLoopSource.isPlaying && popupLoopSource.clip == popupLoopSfx) return;

        popupLoopSource.Stop();
        popupLoopSource.clip = popupLoopSfx;
        popupLoopSource.volume = popupLoopVolume;
        popupLoopSource.loop = true;
        popupLoopSource.Play();
    }

    private void StopPopupLoop()
    {
        if (popupLoopSource == null) return;
        if (popupLoopSource.isPlaying) popupLoopSource.Stop();
        popupLoopSource.clip = null;
    }
}

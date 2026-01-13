using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LuckyDrawSpinner : MonoBehaviour
{
    [Header("Slots Registry")]
    [SerializeField] private RewardSlotsRegistry slotsRegistry;

    [Header("Icon Provider")]
    [SerializeField] private RewardIconProvider_LocalFiles iconProvider;

    [Header("Colors")]
    [SerializeField] private Color idleBulbColor = Color.white;
    [SerializeField] private Color runningBulbColor = new Color(1f, 0.95f, 0.3f, 1f);
    [SerializeField] private Color runningFrameColor = new Color(1f, 0.95f, 0.3f, 1f);
    [SerializeField] private Color winBulbColor = Color.green;
    [SerializeField] private Color winFrameColor = Color.green;
    [SerializeField] private Color failBulbColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color failFrameColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Spin Timing")]
    [SerializeField] private float startStepDelay = 0.04f;
    [SerializeField] private float endStepDelay = 0.12f;
    [SerializeField] private int minCycles = 3;
    [SerializeField] private int maxCycles = 5;
    [SerializeField] private int extraStepsMin = 0;
    [SerializeField] private int extraStepsMax = 3;
    [SerializeField] private int finalSlowSteps = 3;                 
    [SerializeField] private float finalSlowDelayMultiplier = 2.2f;

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
    [SerializeField] private float pulseDuration = 0.6f;

    [Header("Spin Button Scale")]
    [SerializeField] private float spinButtonBaseScale = 0.48f;
    [SerializeField] private float spinButtonPulseScale = 0.6f;

    [Header("Result Particles")]
    [SerializeField] private GameObject[] winParticles;

    [Header("Fail RewardId")]
    [SerializeField] private string[] failRewardIds;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource popupLoopSource;
    [SerializeField] private AudioClip spinClickSfx;
    [SerializeField] private AudioClip tickStepSfx;
    [SerializeField] private AudioClip winSfx;    
    [SerializeField] private AudioClip failSfx;

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
            if (spinButtonRect != null)
                spinButtonRect.localScale = Vector3.one * spinButtonBaseScale;
        }

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (popupLoopSource == null)
            popupLoopSource = gameObject.AddComponent<AudioSource>();

        popupLoopSource.loop = true;

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

        ReloadRewardImages();
        ResetAllVisuals();

        if (resultPopupRoot != null) resultPopupRoot.SetActive(false);
        SetSpinButtonState(true);
    }

    private void HandleSlotsChanged()
    {
        int count = SlotCount;
        if (count <= 0) currentIndex = -1;
        else currentIndex = Mathf.Clamp(currentIndex, 0, count - 1);

        ReloadRewardImages();
        ResetAllVisuals();
        RefreshAllSlotTexts();
    }

    private int SlotCount => (slotsRegistry != null && slotsRegistry.Slots != null) ? slotsRegistry.Slots.Count : 0;

    private RewardSlotView GetSlot(int index)
    {
        if (slotsRegistry == null) return null;
        return slotsRegistry.GetSlot(index);
    }

    private RewardSlotView[] GetSlotsArray()
    {
        if (slotsRegistry == null || slotsRegistry.Slots == null) return null;
        var list = slotsRegistry.Slots;
        var arr = new RewardSlotView[list.Count];
        for (int i = 0; i < arr.Length; i++) arr[i] = list[i];
        return arr;
    }

    public void ReloadRewardImages()
    {
        if (iconProvider == null) return;

        var arr = GetSlotsArray();
        if (arr != null)
            iconProvider.ReloadAll(arr);
    }

    public void Spin()
    {
        if (isSpinning) return;
        if (resultPopupRoot != null && resultPopupRoot.activeSelf) return;
        if (SlotCount <= 0) return;

        isSpinning = true;
        SetSpinButtonState(false);

        if (spinClickSfx != null)
            sfxSource.PlayOneShot(spinClickSfx);

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        ResetAllVisuals();

        int count = SlotCount;
        if (count <= 0)
        {
            isSpinning = false;
            yield break;
        }

        int targetIndex = GetWeightedRandomIndex();

        if (currentIndex < 0)
            currentIndex = Random.Range(0, count);

        int cycles = Random.Range(minCycles, maxCycles + 1);

        int offsetToTarget = GetForwardDistance(currentIndex, targetIndex, count);

        int slowSteps = Mathf.Clamp(finalSlowSteps, 1, Mathf.Max(1, count - 1));

        int preOffsetSteps = Mathf.Max(0, offsetToTarget - slowSteps);

        int extraSteps = Random.Range(extraStepsMin, extraStepsMax + 1);

        int totalRandomSteps = cycles * count + preOffsetSteps + extraSteps;
        int totalAllSteps = totalRandomSteps + slowSteps;

        for (int i = 0; i < totalRandomSteps; i++)
        {
            int jump = Random.Range(1, 4);
            int next = (currentIndex + jump) % count;

            HighlightRunning(next);
            currentIndex = next;

            if (tickStepSfx != null)
                sfxSource.PlayOneShot(tickStepSfx);

            float t = (totalAllSteps <= 1) ? 1f : (i / (float)(totalAllSteps - 1));
            float delay = Mathf.Lerp(startStepDelay, endStepDelay, t);

            yield return new WaitForSeconds(delay);
        }

        for (int k = 0; k < slowSteps; k++)
        {
            int next = (currentIndex + 1) % count;

            HighlightRunning(next);
            currentIndex = next;

            if (tickStepSfx != null)
                sfxSource.PlayOneShot(tickStepSfx);

            float u = (slowSteps <= 1) ? 1f : (k / (float)(slowSteps - 1)); 
            float baseDelay = Mathf.Lerp(startStepDelay, endStepDelay, 1f); 
            float delay = Mathf.Lerp(baseDelay, baseDelay * finalSlowDelayMultiplier, u);

            yield return new WaitForSeconds(delay);
        }

        currentIndex = targetIndex;

        var landedSlot = GetSlot(currentIndex);
        bool isFail = IsFailSlot(landedSlot);

        if (isFail) HighlightFail(currentIndex);
        else HighlightWin(currentIndex);

        yield return new WaitForSeconds(showPopupDelay);
        ShowResultPopup(currentIndex, isFail);

        isSpinning = false;
    }

    private int GetWeightedRandomIndex()
    {
        int count = SlotCount;
        if (count <= 0) return 0;

        int total = 0;
        for (int i = 0; i < count; i++)
        {
            var s = GetSlot(i);
            if (s != null)
                total += Mathf.Max(0, s.weight);
        }

        if (total <= 0)
            return Random.Range(0, count);

        int rand = Random.Range(0, total);
        int sum = 0;

        for (int i = 0; i < count; i++)
        {
            var s = GetSlot(i);
            if (s == null) continue;

            sum += Mathf.Max(0, s.weight);
            if (rand < sum) return i;
        }

        return 0;
    }

    private void ShowResultPopup(int index, bool isFail)
    {
        var slot = GetSlot(index);
        if (slot == null) return;

        if (resultText != null)
            resultText.text = slot.rewardName;

        if (resultIcon != null && slot.iconImage != null)
        {
            resultIcon.sprite = slot.iconImage.sprite;
            resultIcon.preserveAspect = true;
        }

        if (resultPopupRoot != null)
            resultPopupRoot.SetActive(true);

        SetWinParticles(!isFail);

        if (sfxSource != null)
        {
            var clip = isFail ? failSfx : winSfx;
            if (clip != null)
            {
                sfxSource.Stop();
                sfxSource.PlayOneShot(clip, 1f);
            }
        }

        if (popupContent != null)
        {
            popupContent.localScale = Vector3.one * popupStartScale;

            popupTween?.Kill();
            popupTween = popupContent
                .DOScale(1f, popupOpenDuration)
                .SetEase(popupOpenEase);
        }
    }

    public void ClosePopup()
    {
        SetWinParticles(false);

        if (popupLoopSource != null)
            popupLoopSource.Stop();

        if (popupContent == null || resultPopupRoot == null)
        {
            if (resultPopupRoot != null)
                resultPopupRoot.SetActive(false);

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

    private void ResetAllVisuals()
    {
        int count = SlotCount;
        for (int i = 0; i < count; i++)
        {
            var s = GetSlot(i);
            if (s != null) s.ResetVisual(idleBulbColor);
        }
    }

    private void HighlightRunning(int index)
    {
        ResetAllVisuals();
        var s = GetSlot(index);
        if (s != null) s.SetRunning(runningBulbColor, runningFrameColor);
    }

    private void HighlightWin(int index)
    {
        ResetAllVisuals();
        var s = GetSlot(index);
        if (s != null) s.SetWin(winBulbColor, winFrameColor);
    }

    private void HighlightFail(int index)
    {
        ResetAllVisuals();
        var s = GetSlot(index);
        if (s != null) s.SetRunning(failBulbColor, failFrameColor);
    }

    private void SetSpinButtonState(bool enable)
    {
        if (spinButton != null)
            spinButton.interactable = enable;

        if (spinButtonRect == null) return;

        spinButtonTween?.Kill();

        if (enable)
        {
            spinButtonRect.localScale = Vector3.one * spinButtonBaseScale;

            spinButtonTween = spinButtonRect
                .DOScale(spinButtonPulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            spinButtonRect.localScale = Vector3.one * spinButtonBaseScale;
        }
    }

    private int GetForwardDistance(int from, int to, int count)
    {
        int d = to - from;
        if (d < 0) d += count;
        return d;
    }

    public void RefreshAllSlotTexts()
    {
        int count = SlotCount;
        for (int i = 0; i < count; i++)
        {
            var s = GetSlot(i);
            if (s != null) s.RefreshNameUI();
        }
    }

    private bool IsFailSlot(RewardSlotView slot)
    {
        if (slot == null) return false;
        if (failRewardIds == null || failRewardIds.Length == 0) return false;

        string id = slot.rewardId;

        if (string.IsNullOrEmpty(id)) return false;

        for (int i = 0; i < failRewardIds.Length; i++)
        {
            if (!string.IsNullOrEmpty(failRewardIds[i]) && failRewardIds[i] == id)
                return true;
        }
        return false;
    }

    private void SetWinParticles(bool active)
    {
        if (winParticles == null) return;

        for (int i = 0; i < winParticles.Length; i++)
        {
            if (winParticles[i] != null)
                winParticles[i].SetActive(active);
        }
    }
}

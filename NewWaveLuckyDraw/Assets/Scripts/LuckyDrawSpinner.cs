using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LuckyDrawSpinner : MonoBehaviour
{
    [Serializable]
    public class RewardSlot
    {
        public string rewardName;
        public Image bulbImage;
        public Image frameImage;
    }

    [Header("Slots")]
    [SerializeField] private List<RewardSlot> slots = new List<RewardSlot>();

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
    [SerializeField] private float showPopupDelay = 1f;

    [Header("Popup Tween")]
    [SerializeField] private float popupStartScale = 0.6f;
    [SerializeField] private float popupOpenDuration = 0.4f;
    [SerializeField] private float popupCloseDuration = 0.25f;
    [SerializeField] private Ease popupOpenEase = Ease.OutBack;
    [SerializeField] private Ease popupCloseEase = Ease.InBack;

    [Header("UI Button")]
    [SerializeField] private Button spinButton;

    [Header("Spin Button Tween")]
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 0.6f;

    private bool isSpinning;
    private int currentIndex = -1;
    private Coroutine spinRoutine;

    private Tween spinButtonTween;
    private RectTransform spinButtonRect;

    private Tween popupTween;

    private void Awake()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(Spin);

        if (spinButton != null)
            spinButtonRect = spinButton.GetComponent<RectTransform>();

        ResetAllVisuals();

        if (resultPopupRoot != null)
            resultPopupRoot.SetActive(false);

        SetSpinButtonState(true);
    }

    public void Spin()
    {
        if (isSpinning) return;

        if (resultPopupRoot != null && resultPopupRoot.activeSelf) return;

        if (slots == null || slots.Count == 0)
        {
            Debug.LogWarning("LuckyDrawSpinner: slots empty");
            return;
        }

        ForceHidePopup();

        SetSpinButtonState(false);

        isSpinning = true;

        if (spinRoutine != null)
            StopCoroutine(spinRoutine);

        spinRoutine = StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        ResetAllVisuals();

        int targetIndex = UnityEngine.Random.Range(0, slots.Count);

        int cycles = UnityEngine.Random.Range(minCycles, maxCycles + 1);
        int extraSteps = UnityEngine.Random.Range(extraStepsMin, extraStepsMax + 1);

        if (currentIndex < 0)
            currentIndex = UnityEngine.Random.Range(0, slots.Count);

        int offsetToTarget = GetForwardDistance(currentIndex, targetIndex, slots.Count);
        int totalSteps = (cycles * slots.Count) + offsetToTarget + extraSteps;

        for (int step = 1; step <= totalSteps; step++)
        {
            int jump = UnityEngine.Random.Range(1, 4);
            int nextIndex = (currentIndex + jump) % slots.Count;

            HighlightRunning(nextIndex);
            currentIndex = nextIndex;

            float t = step / (float)totalSteps;
            float delay = Mathf.Lerp(startStepDelay, endStepDelay, EaseOutQuad(t));

            yield return new WaitForSeconds(delay);
        }

        currentIndex = targetIndex;
        HighlightWin(currentIndex);

        yield return new WaitForSeconds(showPopupDelay);
        ShowResultPopup(slots[currentIndex].rewardName);

        isSpinning = false;
        spinRoutine = null;
    }

    private void ResetAllVisuals()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].bulbImage != null)
                slots[i].bulbImage.color = idleBulbColor;

            if (slots[i].frameImage != null)
                slots[i].frameImage.gameObject.SetActive(false);
        }
    }

    private void HighlightRunning(int index)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].bulbImage != null)
                slots[i].bulbImage.color = idleBulbColor;

            if (slots[i].frameImage != null)
                slots[i].frameImage.gameObject.SetActive(false);
        }

        if (slots[index].bulbImage != null)
            slots[index].bulbImage.color = runningBulbColor;

        if (slots[index].frameImage != null)
        {
            slots[index].frameImage.gameObject.SetActive(true);
            slots[index].frameImage.color = runningFrameColor;
        }
    }

    private void HighlightWin(int index)
    {
        ResetAllVisuals();

        if (slots[index].bulbImage != null)
            slots[index].bulbImage.color = winBulbColor;

        if (slots[index].frameImage != null)
        {
            slots[index].frameImage.gameObject.SetActive(true);
            slots[index].frameImage.color = winFrameColor;
        }
    }

    private void ShowResultPopup(string rewardName)
    {
        if (resultText != null)
            resultText.text = rewardName;

        if (resultPopupRoot == null || popupContent == null)
        {
            Debug.LogWarning("LuckyDrawSpinner: resultPopupRoot or popupContent not setting");
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
    }

    public void ClosePopup()
    {
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

        if (resultPopupRoot != null)
            resultPopupRoot.SetActive(false);

        if (popupContent != null)
            popupContent.localScale = Vector3.one;
    }

    private void SetSpinButtonState(bool enable)
    {
        if (spinButton != null)
            spinButton.interactable = enable;

        if (enable)
            StartSpinButtonPulse();
        else
            StopSpinButtonPulse();
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
}

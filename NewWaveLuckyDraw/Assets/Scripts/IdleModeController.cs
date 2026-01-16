using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class IdleModeController : MonoBehaviour
{
    [Header("Idle Settings")]
    [SerializeField] private float idleDelay = 10f;
    [SerializeField] private bool ignoreInputWhenPopupOpen = false;

    [Header("Idle Allowed (Only Normal Screen)")]
    [SerializeField] private GameObject[] blockIdleWhenAnyActive;

    [Header("Popup (Invite)")]
    [SerializeField] private GameObject invitePopupRoot;
    [SerializeField] private RectTransform invitePopupContent;
    [SerializeField] private float popupStartScale = 0.85f;
    [SerializeField] private float popupEnterDuration = 0.35f;
    [SerializeField] private Ease popupEnterEase = Ease.OutBack;

    [Header("Popup Loop Animation")]
    [SerializeField] private float moveX = 25f;
    [SerializeField] private float moveY = 10f;
    [SerializeField] private float moveDuration = 1.2f;
    [SerializeField] private float pulseScale = 1.05f;
    [SerializeField] private float pulseDuration = 0.8f;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip idleBgm;
    [SerializeField] private float bgmFadeIn = 0.35f;
    [SerializeField] private float bgmFadeOut = 0.25f;
    [SerializeField] private float bgmVolume = 1f;

    public event Action<bool> OnIdleChanged;

    private float lastInputTime;
    private bool isIdle;

    private Tween popupEnterTween;
    private Sequence popupLoopSeq;
    private Tween bgmFadeTween;

    private Vector2 popupBaseAnchoredPos;

    private int runtimeBlockCount = 0;

    private void Awake()
    {
        lastInputTime = Time.unscaledTime;

        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0f;

        if (invitePopupRoot != null)
            invitePopupRoot.SetActive(false);

        if (invitePopupContent != null)
            popupBaseAnchoredPos = invitePopupContent.anchoredPosition;
    }

    private void OnEnable()
    {
        lastInputTime = Time.unscaledTime;
        ForceExitIdle();
    }

    private void OnDisable()
    {
        ForceExitIdle();
    }

    private void Update()
    {
        if (!IsIdleAllowedNow())
        {
            lastInputTime = Time.unscaledTime;

            if (isIdle) ExitIdle();
            return;
        }

        bool hasAnyInput =
            Input.GetMouseButtonDown(0) ||
            Input.touchCount > 0 ||
            Input.anyKeyDown;

        if (hasAnyInput)
        {
            if (!ignoreInputWhenPopupOpen || !IsPointerOverUI())
                RegisterUserActivity();
        }

        if (!isIdle && (Time.unscaledTime - lastInputTime >= idleDelay))
            EnterIdle();
    }

    private bool IsIdleAllowedNow()
    {
        if (runtimeBlockCount > 0) return false;

        if (blockIdleWhenAnyActive != null)
        {
            for (int i = 0; i < blockIdleWhenAnyActive.Length; i++)
            {
                var go = blockIdleWhenAnyActive[i];
                if (go != null && go.activeInHierarchy)
                    return false;
            }
        }

        return true;
    }

    public void RegisterUserActivity()
    {
        lastInputTime = Time.unscaledTime;

        if (isIdle)
            ExitIdle();
    }

    public void PushBlock()
    {
        runtimeBlockCount++;
        if (runtimeBlockCount < 0) runtimeBlockCount = 0;

        lastInputTime = Time.unscaledTime;
        if (isIdle) ExitIdle();
    }

    public void PopBlock()
    {
        runtimeBlockCount--;
        if (runtimeBlockCount < 0) runtimeBlockCount = 0;

        lastInputTime = Time.unscaledTime;
    }

    private void EnterIdle()
    {
        isIdle = true;
        OnIdleChanged?.Invoke(true);

        if (invitePopupRoot != null)
            invitePopupRoot.SetActive(true);

        if (invitePopupContent != null)
        {
            popupEnterTween?.Kill();
            popupLoopSeq?.Kill();

            invitePopupContent.anchoredPosition = popupBaseAnchoredPos;
            invitePopupContent.localScale = Vector3.one * popupStartScale;

            popupEnterTween = invitePopupContent
                .DOScale(1f, popupEnterDuration)
                .SetEase(popupEnterEase)
                .SetUpdate(true)
                .OnComplete(StartPopupLoop);
        }

        PlayIdleBgm();
    }

    private void ExitIdle()
    {
        isIdle = false;
        OnIdleChanged?.Invoke(false);

        popupEnterTween?.Kill();
        popupLoopSeq?.Kill();

        if (invitePopupRoot != null)
            invitePopupRoot.SetActive(false);

        StopIdleBgm();
    }

    private void StartPopupLoop()
    {
        if (invitePopupContent == null) return;

        popupLoopSeq?.Kill();
        popupLoopSeq = DOTween.Sequence().SetUpdate(true);

        Vector2 p0 = popupBaseAnchoredPos;
        Vector2 p1 = p0 + new Vector2(moveX, moveY);
        Vector2 p2 = p0 + new Vector2(-moveX, -moveY);

        popupLoopSeq
            .Append(invitePopupContent.DOAnchorPos(p1, moveDuration).SetEase(Ease.InOutSine))
            .Append(invitePopupContent.DOAnchorPos(p2, moveDuration).SetEase(Ease.InOutSine))
            .Append(invitePopupContent.DOAnchorPos(p0, moveDuration * 0.8f).SetEase(Ease.InOutSine))
            .SetLoops(-1);

        popupLoopSeq.Join(
            invitePopupContent.DOScale(pulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
        );
    }

    private void PlayIdleBgm()
    {
        if (bgmSource == null || idleBgm == null) return;

        bgmFadeTween?.Kill();

        bgmSource.clip = idleBgm;
        if (!bgmSource.isPlaying)
            bgmSource.Play();

        bgmFadeTween = DOTween.To(() => bgmSource.volume, v => bgmSource.volume = v, bgmVolume, bgmFadeIn)
            .SetEase(Ease.Linear)
            .SetUpdate(true);
    }

    private void StopIdleBgm()
    {
        if (bgmSource == null) return;

        bgmFadeTween?.Kill();

        bgmFadeTween = DOTween.To(() => bgmSource.volume, v => bgmSource.volume = v, 0f, bgmFadeOut)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            });
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    public void ForceExitIdle()
    {
        lastInputTime = Time.unscaledTime;

        if (isIdle)
            ExitIdle();
        else
        {
            popupEnterTween?.Kill();
            popupLoopSeq?.Kill();

            if (invitePopupRoot != null)
                invitePopupRoot.SetActive(false);

            bgmFadeTween?.Kill();
            if (bgmSource != null)
            {
                bgmSource.volume = 0f;
                bgmSource.Stop();
                bgmSource.clip = null;
            }
        }
    }

    public bool IsIdle => isIdle;
}

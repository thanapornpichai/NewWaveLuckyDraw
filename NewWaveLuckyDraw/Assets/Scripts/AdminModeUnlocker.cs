using UnityEngine;

public class AdminModeUnlocker : MonoBehaviour
{
    [Header("Admin Mode Target")]
    [SerializeField] private GameObject adminModeObject;

    [Header("Unlock Settings")]
    [SerializeField] private int requiredClicks = 3;
    [SerializeField] private float maxIntervalBetweenClicks = 1.2f;

    private int clickCount = 0;
    private float lastClickTime = -999f;

    public void OnSecretButtonClicked()
    {
        float now = Time.unscaledTime;

        if (now - lastClickTime > maxIntervalBetweenClicks)
        {
            clickCount = 0;
        }

        clickCount++;
        lastClickTime = now;

        if (clickCount >= requiredClicks)
        {
            OpenAdminMode();
            ResetClicks();
        }
    }

    private void OpenAdminMode()
    {
        if (adminModeObject != null)
            adminModeObject.SetActive(true);
    }

    private void ResetClicks()
    {
        clickCount = 0;
        lastClickTime = -999f;
    }
}

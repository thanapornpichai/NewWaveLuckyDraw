using UnityEngine;

public class AdminModeUnlocker : MonoBehaviour
{
    [Header("Admin Mode Target")]
    [SerializeField] private GameObject adminModeObject;

    [Header("Idle Integration")]
    [SerializeField] private IdleModeController idleController;

    [Header("Unlock Settings")]
    [SerializeField] private int requiredClicks = 3;
    [SerializeField] private float maxIntervalBetweenClicks = 1.2f;

    private int clickCount = 0;
    private float lastClickTime = -999f;

    private bool isAdminOpen = false;

    public void OnSecretButtonClicked()
    {
        idleController?.RegisterUserActivity();

        float now = Time.unscaledTime;

        if (now - lastClickTime > maxIntervalBetweenClicks)
            clickCount = 0;

        clickCount++;
        lastClickTime = now;

        if (clickCount >= requiredClicks)
        {
            OpenAdminMode();
            ResetClicks();
        }
    }

    public void OpenAdminMode()
    {
        if (isAdminOpen) return;
        isAdminOpen = true;

        if (adminModeObject != null)
            adminModeObject.SetActive(true);

        if (idleController != null)
        {
            idleController.PushBlock();
            idleController.ForceExitIdle();    
            idleController.RegisterUserActivity(); 
        }
    }

    public void CloseAdminMode()
    {
        if (!isAdminOpen) return;
        isAdminOpen = false;

        if (adminModeObject != null)
            adminModeObject.SetActive(false);

        if (idleController != null)
        {
            idleController.PopBlock();
            idleController.RegisterUserActivity();
        }
    }

    private void ResetClicks()
    {
        clickCount = 0;
        lastClickTime = -999f;
    }
}

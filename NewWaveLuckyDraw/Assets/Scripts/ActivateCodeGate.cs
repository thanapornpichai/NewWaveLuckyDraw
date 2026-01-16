using TMPro;
using UnityEngine;

public class ActivateCodeGate : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text messageText;

    [Header("Screens")]
    [SerializeField] private GameObject activateScreen;
    [SerializeField] private GameObject gameScreen;

    [Header("Idle Controller (Disable until Activated)")]
    [SerializeField] private IdleModeController idleController;

    [Header("Valid Code")]
    [SerializeField] private string validCode = "ActivateNewWaveGame";

    [Header("PlayerPrefs")]
    [SerializeField] private string activatedKey = "APP_ACTIVATED";

    [Header("Messages")]
    [SerializeField] private string wrongMessage = "The activation code is invalid.";
    [SerializeField] private string emptyMessage = "Activation code is required.";

    private void Awake()
    {
        if (messageText != null)
            messageText.text = "";
    }

    private void Start()
    {
        if (IsActivated())
            ShowGame();
        else
            ShowActivate();
    }

    public void OnSubmit()
    {
        if (codeInput == null) return;

        string input = (codeInput.text ?? "").Trim();

        if (string.IsNullOrEmpty(input))
        {
            ShowError(emptyMessage);
            return;
        }

        if (input == validCode)
        {
            SetActivated(true);
            ClearMessage();
            ShowGame();
        }
        else
        {
            ShowError(wrongMessage);
        }
    }

    public void OnCancel()
    {
        if (codeInput != null)
        {
            codeInput.text = "";
            codeInput.ActivateInputField();
        }

        ClearMessage();
    }

    private bool IsActivated()
    {
        return PlayerPrefs.GetInt(activatedKey, 0) == 1;
    }

    private void SetActivated(bool on)
    {
        PlayerPrefs.SetInt(activatedKey, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ShowActivate()
    {
        if (activateScreen != null) activateScreen.SetActive(true);
        if (gameScreen != null) gameScreen.SetActive(false);

        if (idleController != null)
        {
            idleController.ForceExitIdle();
            idleController.enabled = false; 
        }

        if (codeInput != null)
        {
            codeInput.text = "";
            codeInput.ActivateInputField();
        }

        ClearMessage();
    }

    private void ShowGame()
    {
        if (activateScreen != null) activateScreen.SetActive(false);
        if (gameScreen != null) gameScreen.SetActive(true);

        if (idleController != null)
        {
            idleController.enabled = true;
            idleController.RegisterUserActivity();
        }
    }

    private void ShowError(string msg)
    {
        if (messageText == null) return;
        messageText.color = Color.red;
        messageText.text = msg;
    }

    private void ClearMessage()
    {
        if (messageText == null) return;
        messageText.text = "";
    }

    public void Debug_ResetActivation()
    {
        PlayerPrefs.DeleteKey(activatedKey);
        PlayerPrefs.Save();
        ShowActivate();
    }
}

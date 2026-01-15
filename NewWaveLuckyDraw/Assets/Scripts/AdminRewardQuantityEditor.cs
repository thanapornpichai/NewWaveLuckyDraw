using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdminRewardQuantityEditor : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RewardSlotSelectorDropdown selector;
    [SerializeField] private TMP_InputField qtyInput;
    [SerializeField] private LuckyDrawSpinner spinner;

    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;

    private void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(OnClickSave);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnClickReset);

        if (qtyInput != null)
            qtyInput.onSubmit.AddListener(_ => OnClickSave());
    }

    private void OnDestroy()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnClickSave);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnClickReset);

        if (qtyInput != null)
            qtyInput.onSubmit.RemoveAllListeners();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        var slot = selector != null ? selector.CurrentSlot : null;
        if (slot == null || qtyInput == null) return;

        qtyInput.SetTextWithoutNotify(slot.quantity.ToString());
    }

    private void OnClickSave()
    {
        var slot = selector != null ? selector.CurrentSlot : null;
        if (slot == null || qtyInput == null) return;

        if (!int.TryParse(qtyInput.text, out int q))
            q = slot.quantity;

        slot.SetQuantityAndSave(q);

        if (spinner != null)
            spinner.AdminRefreshAll();

        RefreshUI();
    }

    private void OnClickReset()
    {
        var slot = selector != null ? selector.CurrentSlot : null;
        if (slot == null) return;

        slot.ResetQuantityToDefaultAndSave();

        if (spinner != null)
            spinner.AdminRefreshAll();

        RefreshUI();
    }
}

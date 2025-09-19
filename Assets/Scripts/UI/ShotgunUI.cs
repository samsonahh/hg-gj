using TMPro;
using UnityEngine;

public class ShotgunUI : UIPanel
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private Shotgun shotgun;

    private void OnEnable()
    {
        // Subscribe to UIManager events
        UIManager.Instance.OnPanelChanged += HandleShowPanel;
        UIManager.Instance.OnUIClose += HandleHideAllPanels;
        StartCoroutine(SubscribeWhenReady());
    }

    private void OnDisable()
    {
        if (shotgun != null)
            shotgun.OnAmmoChanged -= UpdateAmmoUI;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnPanelChanged -= HandleShowPanel;
            UIManager.Instance.OnUIClose -= HandleHideAllPanels;
        }
    }

    private System.Collections.IEnumerator SubscribeWhenReady()
    {
        yield return new WaitForEndOfFrame();

        shotgun = GetComponentInParent<Shotgun>();
        if (shotgun == null)
            shotgun = Object.FindFirstObjectByType<Shotgun>();

        if (shotgun != null)
        {
            shotgun.OnAmmoChanged += UpdateAmmoUI;
            UpdateAmmoUI(shotgun.CurrentAmmo, shotgun.MaxAmmo);
        }
    }

    private void UpdateAmmoUI(int current, int max)
    {
        if (shotgun != null && shotgun.InfiniteAmmo)
        {
            if (ammoText != null)
                ammoText.text = $"Ammo: \u221E";
        }
        else
        {
            if (ammoText != null)
                ammoText.text = $"Ammo: {current} / {max}";
        }
    }

    /// <summary>
    /// Hides the ammo text UI element.
    /// </summary>
    public void HideAmmoText()
    {
        if (ammoText != null)
            ammoText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the ammo text UI element.
    /// </summary>
    public void ShowAmmoText()
    {
        if (ammoText != null)
            ammoText.gameObject.SetActive(true);
    }

    private void HandleShowPanel(UIPanel panel)
    {
        // Hide ammo text when any panel is shown
        HideAmmoText();
    }

    private void HandleHideAllPanels()
    {
        // Show ammo text when all panels are hidden
        ShowAmmoText();
    }

    // Implementation of abstract Init() from UIPanel
    private protected override void Init()
    {
        
    }
}
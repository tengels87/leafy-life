using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemController : MonoBehaviour
{
    public Image imageItemIcon;
    public TextMeshProUGUI textItemCosts;
    public Image imageDisabledOverlay;

    private bool isEnabled;
    private Button panelAsButton;

    void Start()
    {
        setEnabled(false);
    }

    void Update()
    {
        
    }

    public void setEnabled(bool val) {
        isEnabled = val;

        imageDisabledOverlay.gameObject.SetActive(!val);
        if (panelAsButton != null) {
            panelAsButton.enabled = val;
        }
    }

    public void setOnClickedListener(UnityEngine.Events.UnityAction callback) {
        if (panelAsButton == null) {
            panelAsButton = this.gameObject.GetComponent<Button>();
        }

        if (panelAsButton != null && panelAsButton.onClick.GetPersistentEventCount() == 0) {
            panelAsButton.onClick.AddListener(callback);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemController : MonoBehaviour
{
    public Image imageItemIcon;
    public TextMeshProUGUI textItemName;
    public TextMeshProUGUI textItemCosts;

    private Button panelAsButton;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void setOnSelectedListener(UnityEngine.Events.UnityAction callback) {
        if (panelAsButton == null) {
            panelAsButton = this.gameObject.GetComponent<Button>();

            if (panelAsButton != null) {
                panelAsButton.onClick.AddListener(callback);
            }
        }
    }
}

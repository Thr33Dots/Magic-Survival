using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PurchaseScript : MonoBehaviour
{
    public CustomizeMenuManager customizeMenuManager;
    CustomizeMenuManager.CustomizationSelections[] customizationSelections;

    public Image characterImage;
    public TextMeshProUGUI characterName;

    public void UnlockPrize()
    {
        customizationSelections = customizeMenuManager.GetCustomizationSelectionsArray();

        int picked = Random.Range(1, customizationSelections.Length);

        PlayerPrefs.SetInt("customization" + picked, 0);

        characterImage.sprite = customizationSelections[picked].image;
        characterName.text = customizationSelections[picked].name;
    }
}

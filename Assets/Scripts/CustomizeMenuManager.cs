using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CustomizeMenuManager : MonoBehaviour
{
    public Scrollbar scrollbar;
    public GameObject characterSelect;
    public RectTransform content;
    public Transform startingPos;
    public float spacing;
    int selectAmount;
    int selectedCharacter;

    [System.Serializable]
    public struct CustomizationSelections
    {
        public string name;
        public Sprite image;
        public bool isUnlocked;
        [System.NonSerialized]
        public bool isSelected;
    }

    public CustomizationSelections[] customizationSelections;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.GetInt("customization" + 0, 0);
        selectedCharacter = PlayerPrefs.GetInt("PlayerSkin", 0);
        selectAmount = customizationSelections.Length;
        SetUpCustomization();
    }

    void SetUpCustomization()
    {
        scrollbar.value         = 0;
        scrollbar.numberOfSteps = 0;
        content.offsetMin       = new Vector2(0, 0);
        content.offsetMax       = new Vector2(selectAmount * spacing, 0);

        startingPos.GetComponent<RectTransform>().anchoredPosition = new Vector2(spacing / 2, 0);

        SetUpCharacterSelections();
    }

    void SetCustomizeUnlocked()
    {
        customizationSelections[0].isUnlocked = true;

        for (int i = 1; i < selectAmount; i++)
        {
            customizationSelections[i].isUnlocked = (PlayerPrefs.GetInt("customization" + i, 1) == 1) ? false : true;
        }
    }

    void SetUpCharacterSelections()
    {
        SetCustomizeUnlocked();
        foreach (Transform child in startingPos)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < selectAmount; i++)
        {
            GameObject selectObj = Instantiate(characterSelect, Vector3.zero, Quaternion.identity);
            selectObj.transform.SetParent(startingPos);
            selectObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * spacing, 0);

            CharacterSelectScript characterSelectScript = selectObj.GetComponent<CharacterSelectScript>();

            // Is selected character
            if (i == selectedCharacter)
                customizationSelections[i].isSelected = true;

            characterSelectScript.GetName().text    = customizationSelections[i].name;
            characterSelectScript.GetImage().sprite = customizationSelections[i].image;
            characterSelectScript.HideButtons(customizationSelections[i].isUnlocked);
            characterSelectScript.Selected(customizationSelections[i].isSelected);
            characterSelectScript.SetNum(i);
        }
    }

    public CustomizationSelections GetCustomizationSelections(int _num)
    {
        return customizationSelections[_num];
    }
    public CustomizationSelections[] GetCustomizationSelectionsArray()
    {
        return customizationSelections;
    }

    public void CharacterSelected(int _num)
    {
        customizationSelections[selectedCharacter].isSelected = false;
        selectedCharacter = _num;
        customizationSelections[selectedCharacter].isSelected = true;
        PlayerPrefs.SetInt("PlayerSkin", selectedCharacter);

        SetUpCharacterSelections();
    }
}

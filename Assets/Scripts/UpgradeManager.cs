using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public PlayerScript player;
    public UpgradeStats upgradeStats;

    public GameObject[] optionButtons;
    public TextMeshProUGUI[] upgradeNameText;
    public TextMeshProUGUI[] upgradeDescriptionText;
    public TextMeshProUGUI[] tierText;
    public Image[] upgradeIcons;
    public TextMeshProUGUI skipText;
    public Animator descriptionBox;
    public TextMeshProUGUI descriptionText;

    public int skipCoin;
    public int chanceForLastReappear;
    public int chanceForPrevReappear;

    const int amountOfOptions = 3;
    const int maxTiers = 5;

    int previousPick = 0;
    int queueOfUpgrades = 0;

    int[] options = { 0, 0, 0 };
    int[] arrayOfUpgrades;

    public void QueueUpgrades()
    {
        queueOfUpgrades++;

        if (queueOfUpgrades <= 1)
        {
            SelectOptions();
        }
    }

    public bool CheckQueue()
    {
        if (queueOfUpgrades > 0)
        {
            SelectOptions();
        }
        return queueOfUpgrades > 0;
    }

    public void SelectOptions()
    {
        skipText.text = "Skip +$" + skipCoin;

        List<int> listOfUpgrades = new List<int>();
        List<int> previousUpgrades = new List<int>();

        for (int i = 0; i < upgradeStats.GetUpgradeStats().Count; i++)
        {
            if (upgradeStats.GetUpgradeStats()[i].tierLevel <= maxTiers)
            {
                listOfUpgrades.Add(i);

                if (upgradeStats.GetUpgradeStats()[i].tierLevel > 0)
                {
                    previousUpgrades.Add(i);
                }
            }
        }

        arrayOfUpgrades = listOfUpgrades.ToArray();

        for (int i = 0; i < amountOfOptions; i++)
        {
            //pick random upgrade
            int num = arrayOfUpgrades[Random.Range(0, arrayOfUpgrades.Length)];

            PickOption(i, num);
        }

        int random = Random.Range(0, 100);

        if (random < chanceForLastReappear && System.Array.IndexOf(arrayOfUpgrades, previousPick) != -1)
        {
            int randOption = Random.Range(0, amountOfOptions);
            PickOption(randOption, previousPick);
        }
        else if (random < chanceForPrevReappear)
        {
            int randOption = Random.Range(0, amountOfOptions);
            int randUpgrade = previousUpgrades[Random.Range(0, previousUpgrades.Count)];

            if (System.Array.IndexOf(arrayOfUpgrades, randUpgrade) != -1)
            {
                PickOption(randOption, randUpgrade);
            }
        }
    }

    void PickOption(int _i, int _num)
    {
        //remove that upgrade from the list of upgrades to choose from
        int indexOfNum = System.Array.IndexOf(arrayOfUpgrades, _num);
        arrayOfUpgrades[indexOfNum] = arrayOfUpgrades[arrayOfUpgrades.Length - 1];
        System.Array.Resize(ref arrayOfUpgrades, arrayOfUpgrades.Length - 1);

        options[_i] = _num;

        UpgradeStats.upgradeTiers upgradeTier = upgradeStats.GetUpgradeStats()[options[_i]];

        upgradeNameText[_i].text = upgradeTier.upgradeName;
        tierText[_i].text = "" + Mathf.Clamp(upgradeTier.tierLevel + 1, 0, 5);
        //upgradeDescriptionText[i].text = upgradeTier.upgradeDescription;
    }

    public void UpgradeButtonPressed(int _number)
    {
        queueOfUpgrades--;
        previousPick = options[_number];
        UpgradeStats.upgradeTiers upgradeTier = upgradeStats.GetUpgradeStats()[options[_number]];

        int i = Mathf.Clamp(upgradeTier.tierLevel + 1, 0, 5);
        upgradeStats.GetUpgradeStats()[options[_number]].SetUpgradeTier(i);

        player.Upgrade(upgradeTier.upgrade, upgradeTier.positiveUpgrade, upgradeTier.negativeUpgrade);


        descriptionBox.SetTrigger("Play");
        descriptionText.text = upgradeTier.upgradeDescription;
    }

    public void SkipPressed()
    {
        //player.IncreaseCoins(skipCoin);
        //player.Upgrade(PlayerScript.UPGRADES.none, 0, 0);
    }
}

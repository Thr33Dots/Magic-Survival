using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class MenuUIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI highScoreText;
    public PlayerScript player;
    public UIManager uiManager;
    public EnemySpawner enemySpawner;
    public CoinSpawner coinSpawner;
    public GameObject prizePrompt;
    public GameObject purchasePrompt;
    public PurchaseScript purchaseScript;
    public PrizeScript prizeScript;
    public PoolingManager poolingManager;
    public TextMeshProUGUI[] coinTexts;

    public int coinsToPurchase;

    [Header("Audio")]
    public GameObject BGM;

    public void RestartPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdateScoreText(int _score)
    {
        scoreText.text = "Score: " + _score;
    }
    public void UpdateHighScoreText(int _highScore)
    {
        highScoreText.text = "HighScore: " + _highScore;
    }
    public void StartPressed()
    {
        player.StartPressed();
        enemySpawner.StartPressed();
        coinSpawner.StartPressed();
        poolingManager.SpawnIntialObjects();
    }
    public void CustomizePressed()
    {
        uiManager.ShowCustomizeUI(true);
        uiManager.ShowPlayScreen(false);
    }
    public void SettingsPressed()
    {
        uiManager.ShowSettingUI(true);
        uiManager.ShowPlayScreen(false);
    }
    public void BackPressed()
    {
        uiManager.ShowPlayScreen(true);
        uiManager.ShowCustomizeUI(false);
        uiManager.ShowPurchaseUI(false);
        uiManager.ShowSettingUI(false);
    }
    public void AcceptPressed()
    {
        uiManager.ShowPurchaseUI(false);
        uiManager.ShowPrizeUI(false);
    }
    public void PurchasePressed()
    {
        uiManager.ShowPurchaseUI(true);
        PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - coinsToPurchase);
        purchasePrompt.SetActive(PlayerPrefs.GetInt("Coins") >= coinsToPurchase);
        purchaseScript.UnlockPrize();
        UpdateCoinTexts();
    }
    public void PrizePressed()
    {
        uiManager.ShowPrizeUI(true);
        prizePrompt.SetActive(false);
        prizeScript.GivePrize();
        PlayerPrefs.SetInt("Day", System.DateTime.Today.Day);
        UpdateCoinTexts();
    }
    public void GameOver(int _coins)
    {
        purchasePrompt.SetActive(_coins >= coinsToPurchase);
        prizePrompt.SetActive(PlayerPrefs.GetInt("Day", 0) != System.DateTime.Today.Day);
        UpdateCoinTexts();
    }

    void UpdateCoinTexts()
    {
        for(int i = 0; i < coinTexts.Length; i++)
        {
            coinTexts[i].text = "Coins: " + PlayerPrefs.GetInt("Coins");
        }
    }

    public void ToggleMusic()
    {
        BGM.SetActive(!BGM.activeSelf);
    }
    public void ToggleSound()
    {

    }
}
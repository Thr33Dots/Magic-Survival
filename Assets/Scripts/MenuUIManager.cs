using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUIManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI highScoreText;
    public PlayerScript player;
    public UIManager uiManager;
    public EnemySpawner enemySpawner;
    public ObstacleSpawner obstacleSpawner;
    public CoinSpawner coinSpawner;
    public GameObject prizePrompt;
    public GameObject purchasePrompt;
    public GameObject watchAdPrompt;
    public PurchaseScript purchaseScript;
    public PrizeScript prizeScript;
    public PoolingManager poolingManager;
    public TextMeshProUGUI[] coinTexts;

    public int coinsToPurchase;

    [Header("Audio")]
    public GameObject BGM;
    public bool hasSound = true;
    public GameObject MusicButton;
    public GameObject SoundButton;

    [Header("Ad Settings")]
    public string _androidGameId;
    public string _iOSGameId;
    public bool _testMode = true;
    public int coinsFromWatchingAd;
    public int chanceForAd;
    public int chanceToShowWatchAd;

    string kRemoveAds = "removeads";
    static IStoreController m_StoreController;
    static IExtensionProvider m_StoreExtensionProvider;
    string _gameId;
    string _adUnitId;
    bool isIOS;

    enum EADTYPE
    {
        nothing,
        coin,
    }

    EADTYPE adType;

    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        isIOS = Application.platform == RuntimePlatform.IPhonePlayer;
        _gameId = (isIOS)
            ? _iOSGameId
            : _androidGameId;
        Advertisement.Initialize(_gameId, _testMode);
    }
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

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
        obstacleSpawner.StartPressed();
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
    public void ShowAd()
    {
        _adUnitId = (isIOS) ? "Interstitial_iOS" : "Interstitial_Android";
        adType = EADTYPE.nothing;
        Advertisement.Show(_adUnitId, this);
    }
    public void WatchAd()
    {
        _adUnitId = (isIOS) ? "Rewarded_iOS" : "Rewarded_Android";
        Debug.Log("Showing Ad: " + _adUnitId);
        adType = EADTYPE.coin;
        Advertisement.Show(_adUnitId, this);
    }
    public void ShowBanner()
    {
        Advertisement.Banner.Show("bannerPlacement");
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        // Optionally execute code if the Ad Unit successfully loads content.

        switch (adType)
        {
            case EADTYPE.coin:
                Debug.Log($"LOADED");
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + coinsFromWatchingAd);
                break;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) { }

    public void InitializePurchasing()
    {
        // TODO: uncomment and fix
        /*if (IsInitialized())
        {
            return;
        }
        // Collects products and store-specific configuration details
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // Add a product with a Unity IAP ID, type
        builder.AddProduct(kRemoveAds, ProductType.Consumable);
        //Initialize Unity IAP with the specified listener and configuration
        // Store Controller and Extension provider are set.
        UnityPurchasing.Initialize(this, builder);*/
    }
    public void BuyRemoveAds(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log("Purchasing Product");
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("Product either is not found or not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (Equals(args.purchasedProduct.definition.id, kRemoveAds))
        {
            Debug.Log("Purchasing Product");
            PlayerPrefs.SetInt("boughtAds", 1);
        }
        else
        {
            Debug.Log("Unrecognized Product");
        }
        return PurchaseProcessingResult.Complete;
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: { 1}", product.definition.storeSpecificId, failureReason));
    }
    private bool IsInitialized()
    {
        // Check if both the Purchasing references are set
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void GameOver(int _coins)
    {
        purchasePrompt.SetActive(_coins >= coinsToPurchase);
        watchAdPrompt.SetActive(Random.Range(0, 100) < chanceToShowWatchAd);
        prizePrompt.SetActive(PlayerPrefs.GetInt("Day", 0) != System.DateTime.Today.Day);
        UpdateCoinTexts();

        if(Random.Range(0, 100) < chanceForAd)
        {
            ShowAd();
        }
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
        if (BGM.activeSelf)
        {
            Color Red = new Color(0.78f, 0.36f, 0.38f, 1.0f);
            MusicButton.GetComponent<Image>().color = Red;

            BGM.SetActive(false);
        }
        else
        {
            Color Green = new Color(0.56f, 0.74f, 0.45f, 1.0f);
            MusicButton.GetComponent<Image>().color = Green;

            BGM.SetActive(true);
        }
    }
    public void ToggleSound()
    {
        if (hasSound)
        {
            Color Red = new Color(0.78f, 0.36f, 0.38f, 1.0f);
            SoundButton.GetComponent<Image>().color = Red;

            hasSound = false;
        }
        else
        {
            Color Green = new Color(0.56f, 0.74f, 0.45f, 1.0f);
            SoundButton.GetComponent<Image>().color = Green;

            hasSound = true;
        }
    }
}
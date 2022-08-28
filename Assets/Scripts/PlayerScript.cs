using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerScript : MonoBehaviour
{
    public GameObject projectile;
    public GameObject spinningSawObject;
    public GameObject sentryObject;
    public GameObject spikeObject;
    public GameObject heads;

    public GameObject playerModel;
    public Animator modelAnimator;

    public Transform muzzleVFX;

    public InGameUIManager inGameUI;
    public MenuUIManager menuUI;
    public UpgradeManager upgradeManager;
    public UIManager uiManager;
    public UpgradeStats upgradeStats;
    public PlayerMovement playerMovement;
    public Joystick aimingJoystick;
    public PoolingManager poolingManager;
    public EnemySpawner enemySpawner;

    public CustomizeMenuManager customizeMenuManager;

    public int xpToLevelUp;
    public float xpIncr;
    public float maxHealthLevelUp;
    public float speedLevelUp;

    int xp;
    int level;
    int coins;
    int score;
    float health;

    float fireRateTimer;
    float sentriesFireRateTimer;
    float lightningTimer;
    float spikeSpawnTimer;
    float regenerationTimer;

    bool paused = true;

    bool playingOnComputer = false;
    bool playingOnPhone = false;

    List<GameObject> spinningSaws = new List<GameObject>();
    List<GameObject> sentries = new List<GameObject>();

    [System.Serializable]
    public struct UpgradableStats
    {
        [Header("Player stats")]
        public float playerSpeed; 
        public float magnetStrength; 

        [Header("Health stats")]
        public float maxHealth;
        public float regeneration;

        [Header("Projectile stats")]
        public int projectileSpeed;
        public int projectilePierce;
        public int criticalChance;
        public int projectiles;
        public float bulletDamage;
        public float bulletsSize;
        public float bulletRange;
        public float bulletKnockback;

        public float fireRate;
        public float accuracy;

        [Header("Projectile special stats")]
        public float homingStrength;
        public float explosionSize;
        public float explosionDamage;
        public float damageDistance;

        [Header("Special stats")]
        public float sawSpinSpeed;
        public float sentrySpinSpeed;
        public float sentryFireRate;
        public float lightningRate;
        public float lightningDamage;
        public float spikeDestroyDuration;
        public float spikeSpawnRate;
    }

    public UpgradableStats upgradableStats;

    public enum UPGRADES
    {
        none,
        playerSpeed,
        maxHealth,
        projectileSpeed,
        fireRate,
        piercing,
        spread,
        magnet,
        knockback,
        glassCannon,
        homing,
        critical,
        sniper,
        extraProjectile,
        submachineGun,
        regeneration,
        explosion,
        spinningSaw,
        sentry,
        jackOfAllTrades,
        distanceDamage,
        lightningStrike,
        spike,
    };
    public enum ANIMATIONS
    {
        Idle,
        Walk,
        Die,
    };

    [Header("Audio")]
    public AudioClip SFX_Collect;
    public AudioClip SFX_Death;
    public AudioClip SFX_Menu_Pop_up;

    private void Start()
    {
        Time.timeScale = 1;
        health = upgradableStats.maxHealth;
        regenerationTimer = upgradableStats.regeneration;
        xp = 0;
        level = 0;
        score = 0;
        coins = PlayerPrefs.GetInt("Coins", 0);
        fireRateTimer = upgradableStats.fireRate;
        sentriesFireRateTimer = upgradableStats.sentryFireRate;
        lightningTimer = upgradableStats.lightningRate;
        spikeSpawnTimer = upgradableStats.spikeSpawnRate;

        //update UI
        inGameUI.UpdateHealthBar(health, upgradableStats.maxHealth);
        inGameUI.UpdateXPBar(xp, xpToLevelUp);
        inGameUI.UpdateLevelText(level);
        inGameUI.UpdateCoinText(coins);
        inGameUI.UpdateScoreText(score);

        playerMovement.UpdateMovemnentSpeed(upgradableStats.playerSpeed);

        SetPaused(true);
        SetHead();

        #if UNITY_STANDALONE_WIN
            playingOnComputer = true;
        #endif
        #if UNITY_IOS || UNITY_ANDROID || UNITY_IPHONE
            playingOnPhone = true;
        #endif

        if(playingOnComputer)
        {
            aimingJoystick.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!paused)
        {
            fireRateTimer -= Time.deltaTime;
            sentriesFireRateTimer -= Time.deltaTime;
            lightningTimer -= Time.deltaTime;
            spikeSpawnTimer -= Time.deltaTime;

            if (playingOnComputer)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    playerModel.transform.LookAt(hit.point);
                    playerModel.transform.localEulerAngles = new Vector3(0, playerModel.transform.localEulerAngles.y, 0);
                }
            }
            if (playingOnPhone && aimingJoystick.Direction.magnitude != 0)
            {
                playerModel.transform.localEulerAngles = new Vector3(0, Angle(aimingJoystick.Direction) - 45, 0);
            }

            if ((aimingJoystick.Direction.magnitude != 0 && playingOnPhone) || (Input.GetMouseButton(0) && playingOnComputer))
            {
                if (fireRateTimer <= 0)
                {
                    int amount = upgradableStats.projectiles;
                    int angleRange = 5 + ((amount - 1) * 20);
                    int newAngle = (amount == 1) ? 0 : angleRange / (amount - 1);
                    for(int i = 0; i < amount; i++)
                    {
                        Vector3 angle = playerModel.transform.rotation.eulerAngles;
                        angle += new Vector3(0, (i * newAngle) + (-angleRange / 2), 0);
                        FireProjectile(angle, transform.position);
                    }

                    fireRateTimer = upgradableStats.fireRate;
                }
            }
            if (upgradableStats.regeneration > 0)
            {
                regenerationTimer -= Time.deltaTime;

                if(regenerationTimer <= 0)
                {
                    regenerationTimer = 1; //every 1 seconds
                    UpdateHealth(upgradableStats.regeneration);
                }
            }

            for (int i = 0; i < spinningSaws.Count; i++)
            {
                spinningSaws[i].transform.localEulerAngles += new Vector3(0, Time.deltaTime * upgradableStats.sawSpinSpeed, 0);
            }            

            if(sentries.Count > 0)
            {
                for (int i = 0; i < sentries.Count; i++)
                {
                    sentries[i].transform.localEulerAngles += new Vector3(0, Time.deltaTime * upgradableStats.sentrySpinSpeed, 0);
                }

                if (sentriesFireRateTimer <= 0)
                {
                    sentriesFireRateTimer = upgradableStats.sentryFireRate;

                    for (int i = 0; i < sentries.Count; i++)
                    {
                        Vector3 newAngle = new Vector3(
                            sentries[i].transform.localEulerAngles.x,
                            sentries[i].transform.localEulerAngles.y + 45,
                            sentries[i].transform.localEulerAngles.z);
                        FireProjectile(newAngle, sentries[i].transform.GetChild(0).position);
                    }
                }
            }

            if (upgradableStats.lightningRate > 0 && lightningTimer <= 0)
            {
                lightningTimer = 10 - upgradableStats.lightningRate;

                List<GameObject> activeEnemies = enemySpawner.GetActiveEnemies();
                if (activeEnemies.Count > 0)
                {
                    int randomEnemy = Random.Range(0, activeEnemies.Count);

                    StartCoroutine(activeEnemies[randomEnemy].GetComponent<EnemyScript>().LightningStrike(upgradableStats.lightningDamage));
                }
            }
            if (upgradableStats.spikeSpawnRate > 0 && spikeSpawnTimer <= 0)
            {
                spikeSpawnTimer = 5 - upgradableStats.spikeSpawnRate;
                Destroy(Instantiate(spikeObject, transform.position, Quaternion.identity), upgradableStats.spikeDestroyDuration);
            }

            //DEBUG
            if (Input.GetKey(KeyCode.Q))
            {
                IncreaseXP(xpToLevelUp - xp);
            }
            if (Input.GetKey(KeyCode.E))
            {
                for(int i = 1; i < customizeMenuManager.GetCustomizationSelectionsArray().Length; i++)
                {
                    PlayerPrefs.SetInt("customization" + i, 1);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("xp"))
        {
            IncreaseXP(other.GetComponent<XPScript>().GetXPGain());
            poolingManager.DespawnObject(other.gameObject);
            //Destroy(other.gameObject);
        }
        if(other.CompareTag("Coin"))
        {
            IncreaseCoins(1);
            poolingManager.DespawnObject(other.gameObject);
            //Destroy(other.gameObject);
        }
    }

    public void SetPaused(bool _pause)
    {
        paused = _pause;

        playerMovement.SetPaused(_pause);

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<EnemyScript>().SetPaused(_pause);
        }
    }

    public void StartPressed()
    {
        SetPaused(false);
        uiManager.ShowPlayScreen(false);
        uiManager.ShowInGameUI(true);
    }

    public void UpdateHealth(float _damage)
    {
        health += _damage;
        inGameUI.UpdateHealthBar(health, upgradableStats.maxHealth);

        if(health <= 0)
        {
            PlayerDied();
        }
    }

    void PlayerDied()
    {
        if (menuUI.hasSound)
        {
            AudioSource.PlayClipAtPoint(SFX_Death, new Vector3(0, 0, 0), 1.0f);
        }

        if (score > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", score);
        }

        PlayAnimation(ANIMATIONS.Die);

        uiManager.ShowGameOverScreen(true);
        uiManager.ShowInGameUI(false);
        uiManager.ShowUpgradeUI(false);
        menuUI.UpdateScoreText(score);
        menuUI.UpdateHighScoreText(PlayerPrefs.GetInt("HighScore"));
        menuUI.GameOver(coins);
        Time.timeScale = 0;
    }

    void IncreaseXP(int _xp)
    {
        if (menuUI.hasSound)
        {
            AudioSource.PlayClipAtPoint(SFX_Collect, new Vector3(0, 0, 0), 1.0f);
        }

        xp += _xp;

        if(xp >= xpToLevelUp)
        {
            LeveledUp();
        }
        inGameUI.UpdateXPBar(xp, xpToLevelUp);
        inGameUI.UpdateLevelText(level);
    }
    public void IncreaseCoins(int _coin)
    {
        if (menuUI.hasSound)
        {
            AudioSource.PlayClipAtPoint(SFX_Collect, new Vector3(0, 0, 0), 1.0f);
        }

        coins += _coin;

        PlayerPrefs.SetInt("Coins", coins);

        inGameUI.UpdateCoinText(coins);
    }

    public void IncreaseScore(int _score)
    {
        score += _score;
        inGameUI.UpdateScoreText(score);
    }

    void LeveledUp()
    {
        xp = 0;
        level++;

        upgradableStats.maxHealth   += maxHealthLevelUp;
        upgradableStats.playerSpeed += speedLevelUp;
        UpdateStats();

        xpToLevelUp = (int)Mathf.Ceil(xpToLevelUp * xpIncr);

        if (menuUI.hasSound)
        {
            AudioSource.PlayClipAtPoint(SFX_Menu_Pop_up, new Vector3(0, 0, 0), 1.0f);
        }

        uiManager.ShowUpgradeUI(true);
        upgradeManager.QueueUpgrades();
        //SetPaused(true);
        //Time.timeScale = 0;
    }

    float Angle(Vector2 vector2)
    {
        if (vector2.x < 0)
        {
            return 360 - (Mathf.Atan2(vector2.x, vector2.y) * Mathf.Rad2Deg * -1);
        }
        else
        {
            return Mathf.Atan2(vector2.x, vector2.y) * Mathf.Rad2Deg;
        }
    }

    void FireProjectile(Vector3 _direction, Vector3 _pos)
    {
        //GameObject projectileObj = Instantiate(projectile, _pos, Quaternion.identity);
        GameObject projectileObj = poolingManager.SpawnObject(PoolingManager.PoolingEnum.Bullet, _pos, Quaternion.identity);
        projectileObj.transform.localEulerAngles = _direction;
        projectileObj.GetComponent<ProjectileScript>().FireProjectile(
            upgradableStats.projectileSpeed,
            upgradableStats.bulletRange,
            upgradableStats.bulletDamage / upgradableStats.projectiles,
            upgradableStats.projectilePierce,
            upgradableStats.accuracy,
            upgradableStats.homingStrength,
            upgradableStats.explosionSize
            );

        muzzleVFX.rotation = Quaternion.Euler(_direction);
        muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Play();

        //GameObject muzzleObj = Instantiate(muzzleVFX, _pos, Quaternion.Euler(_direction));
        //muzzleObj.transform.SetParent(transform);
        //Destroy(muzzleObj, muzzleObj.transform.GetChild(0).GetComponent<ParticleSystem>().main.duration);
    }

    public void SetHead()
    {
        foreach(Transform child in heads.transform)
        {
            child.gameObject.SetActive(false);
        }

        heads.transform.GetChild(PlayerPrefs.GetInt("PlayerSkin")).gameObject.SetActive(true);
    }

    public UpgradableStats GetUpgradableStats()
    {
        return upgradableStats;
    }

    void UpdateMaxHealth()
    {
        if(health >= upgradableStats.maxHealth)
        {
            health = upgradableStats.maxHealth;
        }
    }

    void AddSpinningSaw()
    {
        GameObject sawObj = Instantiate(spinningSawObject, transform.position, Quaternion.identity);
        sawObj.transform.parent = gameObject.transform;
        spinningSaws.Add(sawObj);

        float angle = 360 / spinningSaws.Count;

        //reset all spinning saws 
        for(int i = 0; i < spinningSaws.Count; i++)
        {
            spinningSaws[i].transform.localEulerAngles = new Vector3(0, angle * i, 0);
        }
    }
    void AddSentry()
    {
        GameObject sentryObj = Instantiate(sentryObject, transform.position, Quaternion.identity);
        sentryObj.transform.parent = gameObject.transform;
        sentries.Add(sentryObj);

        float angle = 360 / sentries.Count;

        //reset all spinning saws 
        for(int i = 0; i < sentries.Count; i++)
        {
            sentries[i].transform.localEulerAngles = new Vector3(0, angle * i, 0);
        }
    }

    public void Upgrade(UPGRADES _upgrade, float _positiveUpgrade, float _negativeUpgrade)
    {
        SetPaused(false);
        Time.timeScale = 1;

        switch (_upgrade)
        {
            case UPGRADES.playerSpeed:
                upgradableStats.playerSpeed     += _positiveUpgrade;
                break;

            case UPGRADES.maxHealth:
                upgradableStats.maxHealth       += (int)_positiveUpgrade;
                upgradableStats.playerSpeed     /= _negativeUpgrade;
                break;

            case UPGRADES.projectileSpeed:
                upgradableStats.projectileSpeed += (int)_positiveUpgrade;
                break;

            case UPGRADES.fireRate:
                upgradableStats.fireRate        += _positiveUpgrade;
                break;

            case UPGRADES.piercing:
                upgradableStats.projectilePierce += (int)_positiveUpgrade;
                break;

            case UPGRADES.spread:
                upgradableStats.accuracy        += _positiveUpgrade;
                upgradableStats.bulletDamage    *= _negativeUpgrade;
                break;

            case UPGRADES.magnet:
                upgradableStats.magnetStrength  += _positiveUpgrade;
                break;

            case UPGRADES.knockback:
                upgradableStats.bulletKnockback += _positiveUpgrade;
                break;

            case UPGRADES.glassCannon:
                upgradableStats.bulletDamage    *= _positiveUpgrade;
                upgradableStats.maxHealth       /= _negativeUpgrade;
                break;

            case UPGRADES.homing:
                upgradableStats.homingStrength  += _positiveUpgrade;
                break;

            case UPGRADES.critical:
                upgradableStats.criticalChance  += (int)_positiveUpgrade;
                break;

            case UPGRADES.sniper:
                upgradableStats.bulletRange     += _positiveUpgrade;
                upgradableStats.fireRate        += _negativeUpgrade;
                break;

            case UPGRADES.extraProjectile:
                upgradableStats.projectiles     += (int)_positiveUpgrade;
                upgradableStats.bulletDamage    += _negativeUpgrade;
                break;

            case UPGRADES.submachineGun:
                upgradableStats.fireRate        /= _positiveUpgrade;
                upgradableStats.bulletRange     *= _negativeUpgrade;
                break;

            case UPGRADES.regeneration:
                upgradableStats.regeneration    += _positiveUpgrade;
                break;

            case UPGRADES.explosion:
                upgradableStats.explosionSize   += 1 + (_positiveUpgrade * 0.5f);
                break;

            case UPGRADES.spinningSaw:
                upgradableStats.sawSpinSpeed    += _positiveUpgrade;
                AddSpinningSaw();
                break;

            case UPGRADES.sentry:
                upgradableStats.sentrySpinSpeed += _positiveUpgrade;
                AddSentry();
                break;

            case UPGRADES.jackOfAllTrades:
                upgradableStats.maxHealth       *= _positiveUpgrade;
                upgradableStats.playerSpeed     *= _positiveUpgrade;
                upgradableStats.bulletDamage    *= _positiveUpgrade;
                break;

            case UPGRADES.distanceDamage:
                upgradableStats.damageDistance  += _positiveUpgrade;
                break;

            case UPGRADES.lightningStrike:
                upgradableStats.lightningRate   += _positiveUpgrade;
                break;

            case UPGRADES.spike:
                upgradableStats.spikeSpawnRate  += _positiveUpgrade;
                break;
        }

        UpdateStats();

        uiManager.ShowUpgradeUI(upgradeManager.CheckQueue());
    }

    public void PlayAnimation(ANIMATIONS _animation)
    {
        switch(_animation)
        {
            case ANIMATIONS.Idle:
                modelAnimator.SetBool("IsWalking", false);
                break;

            case ANIMATIONS.Walk:
                modelAnimator.SetBool("IsWalking", true);
                break;

            case ANIMATIONS.Die:
                modelAnimator.SetBool("IsDead", true);
                break;
        }
    }

    public PoolingManager GetPoolingManager()
    {
        return poolingManager;
    }

    void UpdateStats()
    {
        UpdateMaxHealth();
        inGameUI.UpdateHealthBar(health, upgradableStats.maxHealth);
        playerMovement.UpdateMovemnentSpeed(upgradableStats.playerSpeed);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float maxHealth;
    public float damage;
    public float speed;
    public float range;
    public float attackWait;
    public float despawnWait;
    public int scoreIncrease;
    public int xp;

    bool paused = false;
    bool isDead = false;

    const float critcalTimer = 1.5f;

    int layerMask = 1 << 8;

    float t;
    float health;

    GameObject player;
    PlayerScript playerScript;
    PoolingManager poolingManager;
    Rigidbody rb;
    public Animator enemyAnimator;
    public GameObject xpPellet;
    public GameObject criticalText;
    public GameObject impactVFX;

    [Header("Audio")]
    public AudioSource SFX_Destroy;

    public enum ANIMATIONS
    {
        Walk,
        Attack,
        Die
    }

    private void Start()
    {
        player         = GameObject.FindGameObjectWithTag("Player");
        poolingManager = GameObject.Find("PoolingManager").GetComponent<PoolingManager>();
        playerScript   = player.GetComponent<PlayerScript>();
        rb             = GetComponent<Rigidbody>();
        t              = attackWait;
        Init();
    }

    public void Init()
    {
        t      = attackWait;
        health = maxHealth;
        isDead = false;

        GetComponent<BoxCollider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    private void FixedUpdate()
    {
        if (!paused && !isDead)
        {
            //rotate to look at player
            transform.LookAt(player.transform);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

            //move towards player
            //transform.position += transform.forward * Time.deltaTime * speed;

            rb.AddForce(transform.forward * Time.deltaTime * speed, ForceMode.Force);

            t -= Time.deltaTime;

            if (t <= 0)
            {
                RaycastHit hit;
                Debug.DrawRay(transform.position, transform.forward, Color.green, 1);

                if (Physics.Raycast(transform.position, transform.forward, out hit, range, layerMask))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        PlayAnimation(ANIMATIONS.Attack);
                        playerScript.UpdateHealth(damage);
                    }
                }

                t = attackWait + Random.Range(0.00f, 0.15f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Projectile"))
        {
            HitByBullet(other.gameObject);
        }
        if(other.CompareTag("Saw"))
        {
            DamageEnemy(playerScript.GetUpgradableStats().bulletDamage, true);
        }
        if(other.CompareTag("Explosion"))
        {
            DamageEnemy(playerScript.GetUpgradableStats().explosionDamage, true);
        }
    }

    void HitByBullet(GameObject _bullet)
    {
        ProjectileScript projectile = _bullet.GetComponent<ProjectileScript>();
        projectile.StartExplosion(transform.position);
        DamageEnemy(projectile.GetDamage(), false);
        Knockback();

        Vector3 direction = transform.position - _bullet.transform.position;
        Destroy(Instantiate(impactVFX, _bullet.transform.position, Quaternion.LookRotation(-direction)), impactVFX.GetComponent<ParticleSystem>().main.duration);

        if (projectile.GetPiercing() <= 0)
        {
            poolingManager.DespawnObject(_bullet);
            //Destroy(_bullet);
        }
        else
        {
            projectile.SetPiercing();
        }
    }

    void Knockback()
    {
        rb.AddForce(-transform.forward * playerScript.GetUpgradableStats().bulletKnockback, ForceMode.Force);
    }

    public void SetPaused(bool _pause)
    {
        paused = _pause;
    }

    public void DamageEnemy(float _damage, bool _set)
    {
        int rand = Random.Range(0, 100);

        if(rand < playerScript.GetUpgradableStats().criticalChance)
        {
            _damage *= 2;
            Destroy(Instantiate(criticalText, transform.position, Quaternion.identity), critcalTimer);
        }

        if(!_set && playerScript.GetUpgradableStats().damageDistance > 0)
        {
            //float newDamage = _damage * ((Vector3.Distance(player.transform.position, transform.position) * playerScript.GetUpgradableStats().damageDistance));
            _damage += (Vector3.Distance(player.transform.position, transform.position) / 10) * playerScript.GetUpgradableStats().damageDistance;
            print("_damage: " + _damage);
        }

        health -= _damage;

        if (health <= 0)
        {
            EnemyDied();    
        }
    }

    void EnemyDied()
    {
        AudioSource.PlayClipAtPoint(SFX_Destroy.clip, new Vector3(0, 0, 0), 1.0f);

        playerScript.IncreaseScore(scoreIncrease);
        GameObject xpObj = poolingManager.SpawnObject(PoolingManager.PoolingEnum.XP, transform.position, Quaternion.Euler(0, 45, 0));
        xpObj.GetComponent<XPScript>().Init();
        xpObj.GetComponent<XPScript>().SetXPGain(xp);
        PlayAnimation(ANIMATIONS.Die);
        isDead = true;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        StartCoroutine(DepsawnWait(despawnWait));
    }

    IEnumerator DepsawnWait(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        poolingManager.DespawnObject(this.gameObject);
    }

    public void PlayAnimation(ANIMATIONS _animation)
    {
        switch (_animation)
        {
            case ANIMATIONS.Attack:
                enemyAnimator.SetTrigger("Attack");
                break;

            case ANIMATIONS.Die:
                enemyAnimator.SetBool("IsDead", true);
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    float projectileSpeed;
    float destroyTimer;
    float damage;
    float homingStrength;
    int piercing;
    bool homing = false;
    Transform closestEnemy;
    Rigidbody rb;

    private void Start()
    {
        homing = (homingStrength != 0);
        rb = GetComponent<Rigidbody>();
    }

    public void FireProjectile(float _projectileSpeed, float _destroyTimer, float _damage, int _piercing, float _accuracy, float _homing)
    {
        projectileSpeed = _projectileSpeed;
        destroyTimer    = _destroyTimer;
        damage          = _damage;
        piercing        = _piercing;
        homingStrength  = _homing;
        homing          = (homingStrength != 0);

        if (homing)
        {
            float closestDistance = Mathf.Infinity;

            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < closestDistance)
                {
                    closestEnemy = enemy.transform;
                    closestDistance = distance;
                }
            }
        }

        transform.forward += new Vector3(Random.Range(-_accuracy, _accuracy), 0, 0);
        StartCoroutine(DestroyWait());
    }

    private void FixedUpdate()
    {
        //transform.position += transform.forward * Time.deltaTime * projectileSpeed;
        rb.velocity = transform.forward * projectileSpeed;

        if (homing && closestEnemy != null)
        {
            Vector3 direction = closestEnemy.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, homingStrength * Time.deltaTime));
        }
    }

    public float GetDamage()
    {
        return damage;
    }

    public int GetPiercing()
    {
        return piercing;
    }
    public void SetPiercing()
    {
        piercing--;
        damage /= 2;
    }

    private IEnumerator DestroyWait()
    {
        yield return new WaitForSeconds(destroyTimer);
        Destroy(gameObject);
    }
}

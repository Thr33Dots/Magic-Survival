using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    Transform player;
    public float despawnDistance;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance > despawnDistance)
        {
            player.GetComponent<PlayerScript>().GetPoolingManager().DespawnObject(gameObject);
            //Destroy(gameObject);
        }
    }
}

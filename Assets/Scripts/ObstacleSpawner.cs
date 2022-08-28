using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public float spawnRate;
    public float radius;
    public int maxSpawn;
    public float spawnChance;

    public GameObject ObstacleObj;
    public PoolingManager poolingManager;

    private int MaxTries = 100;
    private int tries = 0;

    GameObject player;

    public void StartPressed()
    {
        StartCoroutine(WaitToSpawn());
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private IEnumerator WaitToSpawn()
    {
        yield return new WaitForSeconds(spawnRate);

        SpawnObstacle();

        StartCoroutine(WaitToSpawn());
    }

    void SpawnObstacle()
    {

        float angle;
        Vector3 pos;
        bool foundSpot = false;

        do
        {
            angle = Random.Range(0, 2f * Mathf.PI);
            pos = player.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            RaycastHit hit;

            if (Physics.Raycast(pos, -transform.up, out hit, 2))
            {
                if (hit.transform.CompareTag("Ground"))
                {
                    if (Mathf.PerlinNoise(Mathf.Round(pos.x) * 100, Mathf.Round(pos.z) * 100) < spawnChance)
                    {
                        foundSpot = true;
                    }
                }
            }

            tries++;

        } while (!foundSpot && tries < MaxTries);

        GameObject ObstacleObj = poolingManager.SpawnObject(PoolingManager.PoolingEnum.Obstacles, pos, Quaternion.Euler(0, 45, 0));
        //coinObj.GetComponent<CoinScript>().Init();
        //Instantiate(coinObj, pos, Quaternion.Euler(0, 45, 0));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    [System.Serializable]
    public struct PoolingSystems
    {
        public PoolingEnum name;
        public GameObject gameObjectToSpawn;
        public int IntialToSpawn;
    }

    public PoolingSystems[] poolingSystems;
    List<GameObject> parentPools = new List<GameObject>();

    public enum PoolingEnum
    {
        Coins,
        XP,
        Bullet,
        Enemy1,
        Enemy2,
        Enemy3,
        Enemy4,
    }

    public void SpawnIntialObjects()
    {
        for (int i = 0; i < poolingSystems.Length; i++)
        {
            GameObject parent = new GameObject(poolingSystems[i].name.ToString() + "PoolSystem");
            parentPools.Add(parent);
            parentPools[i].transform.parent = transform;

            for (int j = 0; j < poolingSystems[i].IntialToSpawn; j++)
            {
                GameObject obj = Instantiate(poolingSystems[i].gameObjectToSpawn, new Vector3(0, 0, 0), Quaternion.identity);
                obj.transform.parent = parent.transform;
                obj.SetActive(false);
            }
        }
    }

    public GameObject SpawnObject(PoolingEnum _poolingEnum, Vector3 _position, Quaternion _rotation)
    {
        GameObject spawnObj = FindFreeObject(parentPools[(int)_poolingEnum].transform);

        if (spawnObj != null)
        {
            spawnObj.SetActive(true);
            spawnObj.transform.position = _position;
            spawnObj.transform.rotation = _rotation;
        }

        return spawnObj;
    }

    public void DespawnObject(GameObject _object)
    {
        _object.SetActive(false);
        _object.transform.position = new Vector3(0, 0, 0);
    }

    GameObject FindFreeObject(Transform _parent)
    {
        foreach(Transform child in _parent)
        {
            if(!child.gameObject.activeSelf)
            {
                return child.gameObject;
            }
        }

        Debug.LogError($"Couldn't find a free object in {_parent.name} pool (try increasing the size)");
        return null;
    }

    /*PoolingSystems FindPoolingSystem(PoolingEnum _poolingEnum)
    {
        for(int i = 0; i < poolingSystems.Length; i++)
        {
            if (poolingSystems[i].name == _poolingEnum)
                return poolingSystems[i];
        }

        Debug.LogError("Couldn't find enum");
        return poolingSystems[0];
    }*/
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunker : MonoBehaviour
{
    private const int MaxNumberChunks = 9;  //For Each Chunk Around the Center Chunk
    private const float OverlapSphereRadius = 10.0f;    //Completely Arbitrary but seems to work

    private const int BorderlayerId = 6;
    private const int BorderlayerMask = 1 << BorderlayerId;

    private GameObject CurrentChunk;
    private float ChunkXSize;
    private float ChunkZSize;

    public List<GameObject> WorldChunks = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Set Center Chunk
        CurrentChunk = WorldChunks[0];

        //Set Scale Diff (If placed flush to center the Top Right Corner will give Positive Size Values)
        ChunkXSize = WorldChunks[3].transform.position.x;
        ChunkZSize = WorldChunks[3].transform.position.z;

    }

    private void OnTriggerEnter(Collider other)
    {
        WorldReArranger(other);
    }

    void WorldReArranger(Collider _other)
    {
        if (CurrentChunk.name != _other.transform.parent.name)
        {
            //Update Current Chunk
            for (int i = 0; i < MaxNumberChunks; i++)
            {
                if (WorldChunks[i] == _other.transform.parent.gameObject)
                {
                    //Position in Array (To Match the Chunk Locations)
                    WorldChunks[0] = _other.transform.parent.gameObject;
                    WorldChunks[i] = CurrentChunk;

                    Transform TempPos = WorldChunks[i].transform;
                    WorldChunks[i].transform.position = WorldChunks[0].transform.position;
                    WorldChunks[0].transform.position = TempPos.position;

                    CurrentChunk = WorldChunks[0];

                    break;
                }
            }

            for (int i = 0; i < MaxNumberChunks; i++)
            {
                ChunkPositioner(i);
            }

            //isRechunking = true;
        }
    }

    void ChunkPositioner(int _i)
    {
        if (_i == 1)
        {
            //Top Left
            float Xpos = WorldChunks[0].transform.position.x - (WorldChunks[_i].transform.localScale.x * ChunkXSize);
            float Zpos = WorldChunks[0].transform.position.z + (WorldChunks[_i].transform.localScale.z * ChunkZSize);

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos);
        }

        if (_i == 2)
        {
            //Top
            float Xpos = WorldChunks[0].transform.position.x;
            float Zpos = WorldChunks[0].transform.position.z + (WorldChunks[_i].transform.localScale.z * ChunkZSize);

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos); 
        }

        if (_i == 3)
        {
            //Top Right
            float Xpos = WorldChunks[0].transform.position.x + (WorldChunks[_i].transform.localScale.x * ChunkXSize);
            float Zpos = WorldChunks[0].transform.position.z + (WorldChunks[_i].transform.localScale.z * ChunkZSize);

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos);
        }

        if (_i == 4)
        {
            //Left
            float Xpos = WorldChunks[0].transform.position.x - (WorldChunks[_i].transform.localScale.x * ChunkXSize);
            float Zpos = WorldChunks[0].transform.position.z;

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos);
        }

        if (_i == 5)
        {
            //Right
            float Xpos = WorldChunks[0].transform.position.x + (WorldChunks[_i].transform.localScale.x * ChunkXSize);
            float Zpos = WorldChunks[0].transform.position.z;

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos);
        }

        if (_i == 6)
        {
            //Bottom Left
            float Xpos = WorldChunks[0].transform.position.x - (WorldChunks[_i].transform.localScale.x * ChunkXSize);
            float Zpos = WorldChunks[0].transform.position.z - (WorldChunks[_i].transform.localScale.z * ChunkZSize);

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos);
        }

        if (_i == 7)
        {
            //Bottom
            float Xpos = WorldChunks[0].transform.position.x;
            float Zpos = WorldChunks[0].transform.position.z - (WorldChunks[_i].transform.localScale.z * ChunkZSize);

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos);
        }

        if (_i == 8)
        {
            //Bottom Right
            float Xpos = WorldChunks[0].transform.position.x + (WorldChunks[_i].transform.localScale.x * ChunkXSize);
            float Zpos = WorldChunks[0].transform.position.z - (WorldChunks[_i].transform.localScale.z * ChunkZSize);

            WorldChunks[_i].transform.position = new Vector3(Xpos, 0, Zpos);
        }
    }
}

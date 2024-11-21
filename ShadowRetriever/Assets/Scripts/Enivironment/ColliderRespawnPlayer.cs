using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderRespawnPlayer : MonoBehaviour
{
    public Vector3 spawnPoint;
    private RoomRespawnManager rrm;
    private AudioHandler audioHandler;

    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (audioHandler != null)
                audioHandler.Play("PlayerDie");
            other.transform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            other.transform.position = spawnPoint;
            OneWayCollider[] oneWayColliders = GameObject.FindObjectsOfType<OneWayCollider>();
            foreach (OneWayCollider collider in oneWayColliders)
            {
                collider.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            }
        }
    }

    void Awake()
    {
        //Requires a gameobject called RoomRespawnManager to have the RoomRespawnManager
        rrm = GameObject.Find("RoomRespawnManager").GetComponent<RoomRespawnManager>();
        audioHandler = GetComponent<AudioHandler>();
    }

    void Update()
    {
        if (rrm.playerSpawnLocation != spawnPoint)
        {
            UpdateSpawnPos();
        }
    }

    private void UpdateSpawnPos()
    {
        spawnPoint = rrm.playerSpawnLocation;
        // Debug.Log($"{spawnPoint}, {name}");
    }
}

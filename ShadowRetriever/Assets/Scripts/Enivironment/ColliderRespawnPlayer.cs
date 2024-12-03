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
            StartCoroutine(RespawnPlayer(other));
            
            // Reset one-way colliders
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


    public IEnumerator RespawnPlayer(Collider2D player)
    {
        player.GetComponent<PlayerController>().canMove = false;
        // Trigger the fade animation
        player.GetComponent<PlayerController>().animator.SetTrigger("Fade");
        // Stop player movement and disable gravity
        Rigidbody2D rb = player.transform.GetComponent<Rigidbody2D>();
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0.0f;

        // Wait for the fade animation to complete
        yield return new WaitForSeconds(0.5f);

        // Gradually move the player to the spawn point
        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = spawnPoint;
        float duration = 1.0f; // Time in seconds for the movement
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration; // Normalized time
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null; // Wait for the next frame
        }

        // Ensure the player ends exactly at the spawn point
        player.transform.position = targetPosition;

        // Restore gravity and re-enable player controls
        rb.gravityScale = 1.0f;
        player.GetComponent<PlayerController>().canMove = true;
    }

}

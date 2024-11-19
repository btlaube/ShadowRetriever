using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayCollider : MonoBehaviour
{
    
    private BoxCollider2D boxCol;

    void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            // Check if player is above this collider
            if (other.transform.position.y - transform.position.y > 0.0f)
            {
                boxCol.isTrigger = false;
            }
            else{
                boxCol.isTrigger = true;
            }
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        // SwitchOff();
    }

    public void SwitchOff()
    {
        boxCol.isTrigger = true;
    }

    public void SwitchOn()
    {
        boxCol.isTrigger = false;
    }

}

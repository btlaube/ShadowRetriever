using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEventTrigger : MonoBehaviour
{
    public UnityEvent myEvent;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            myEvent.Invoke();
            Destroy(gameObject);
        }
    }
}

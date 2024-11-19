using UnityEngine;
using UnityEngine.Events;

public class InteractEventTrigger : MonoBehaviour
{
    public UnityEvent myEvent;

    [SerializeField] private float interactDistance;
    private Transform player;
    private SpriteRenderer sr;

    void Awake()
    {
        player = GameObject.Find("Player").transform;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //Check if player is within range
        if (Vector2.Distance(transform.position, player.position) < interactDistance)
        {
            // Become interactable
            if (sr != null)
                sr.enabled = true;
            // Check input from hardcoded key "X" 
            if (Input.GetKeyUp(KeyCode.X))
            {
                Interact();
                sr.enabled = false;
            }
        }
        else
        {
            // Become uninteractable
            if (sr != null)
                sr.enabled = false;
        }
    }

    private void Interact()
    {
        myEvent.Invoke();
    }
}
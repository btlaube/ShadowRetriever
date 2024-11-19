using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;
    
    private DialogueManager dialogueManager;

    void Awake()
    {
        dialogueManager = GameObject.Find("DialogueCanvas").GetComponent<DialogueManager>();
    }

    public void TriggerDialogue()
    {
        dialogueManager.StartDialogue(dialogue);
    }
}

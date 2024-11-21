using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroCutsceneManager : MonoBehaviour
{
    private PlayerController playerController;
    private DialogueTrigger dialogeOne;
    private DialogueTrigger dialogueTwo;
    private bool dialogueOneTriggered;
    private bool dialogueTwoTriggered;

    void Awake()
    {
        dialogeOne = transform.Find("DialogueOne").GetComponent<DialogueTrigger>();
        dialogueTwo = transform.Find("DialogueTwo").GetComponent<DialogueTrigger>();
    }

    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;
        StartCoroutine(IntroCutscene());
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Return) && dialogueOneTriggered)
        {
            if (!dialogueTwoTriggered)
                StartCoroutine(SpawnPlayer());
        }
    }

    public IEnumerator IntroCutscene()
    {
        yield return new WaitForSeconds(2.0f);
        if (dialogeOne != null)
            dialogeOne.TriggerDialogue();
        dialogueOneTriggered = true;
    }

    public IEnumerator SpawnPlayer()
    {
        dialogueTwoTriggered = true;
        playerController.SpawnPlayer();

        // Wait for player Spawn Animation
        yield return new WaitForSeconds(2.0f);

        //Start second dialogue
        if (dialogueTwo != null)
            dialogueTwo.TriggerDialogue();
    }

}

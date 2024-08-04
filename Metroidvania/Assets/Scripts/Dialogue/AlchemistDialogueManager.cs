using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemistDialogueManager : MonoBehaviour
{
    [SerializeField] private Dialogue[] zeroAbilityDialogues;
    [SerializeField] private Dialogue[] oneAbilityDialogues;

    [SerializeField] private int zeroAbilityIndex;
    [SerializeField] private int oneAbilityIndex;

    private DialogueManager dialogueManager;
    private PlayerController playerController;

    void Awake()
    {
        dialogueManager = GameObject.Find("DialogueCanvas").GetComponent<DialogueManager>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    public void StartDialogue()
    {
        if (playerController.hasDoubleJump)
        {
            // End Game dialogue
            Debug.Log("End game");
            GameObject.Find("EndCutsceneManager").GetComponent<EndCutsceneManager>().StartEndCutscene();
        }
        else if (playerController.hasWallCling)
        {
            // One Ability dialogue
            dialogueManager.StartDialogue(oneAbilityDialogues[oneAbilityIndex]);
            oneAbilityIndex++;
            if (oneAbilityIndex == oneAbilityDialogues.Length)
                oneAbilityIndex = 0;

        }
        else
        {
            // No ability dialogue
            dialogueManager.StartDialogue(zeroAbilityDialogues[zeroAbilityIndex]);
            zeroAbilityIndex++;
            if (zeroAbilityIndex == zeroAbilityDialogues.Length)
                zeroAbilityIndex = 0;
        }
    }
}

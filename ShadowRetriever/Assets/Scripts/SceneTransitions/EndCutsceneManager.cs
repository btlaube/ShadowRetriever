using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCutsceneManager : MonoBehaviour
{
    private DialogueTrigger startDialogue;
    private DialogueTrigger returnDialogue;
    private DialogueTrigger refuseDialogue;
    public int startDialogueProgress;
    public int refuseDialogueProgress;
    public int returnDialogueProgress;
    private Animator canvasAnimator;

    private bool startDialogueTriggered;
    private bool returnDialogueTriggered;
    private bool refuseDialogueTriggered;

    private Animator playerAnimator;
    private Animator alchemistAnimator;

    void Awake()
    {
        startDialogue = transform.Find("StartDialogue").GetComponent<DialogueTrigger>();
        returnDialogue = transform.Find("ReturnDialogue").GetComponent<DialogueTrigger>();
        refuseDialogue = transform.Find("RefuseDialogue").GetComponent<DialogueTrigger>();
        canvasAnimator = GameObject.Find("EndGameChoiceCanvas").GetComponent<Animator>();

        playerAnimator = GameObject.Find("Player").GetComponent<Animator>();
        alchemistAnimator = GameObject.Find("Alchemist").GetComponent<Animator>();
    }


    void Update()
    {

        if (refuseDialogueTriggered)
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                refuseDialogueProgress++;
                if (refuseDialogueProgress == 2)
                {
                    // Refuse dialogue done
                    // Refuse animations
                    Debug.Log("refuse finished");
                    StartCoroutine(EndOfRefuseCutscene());
                }
            }
        }
        else if (returnDialogueTriggered)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("return finished");
                StartCoroutine(EndOfReturnCutscene());
            }
        }
        else if (startDialogueTriggered)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                startDialogueProgress++;
                if (startDialogueProgress == 4)
                {
                    if (canvasAnimator != null)
                        canvasAnimator.SetTrigger("Start");
                }
            }
        }
    }

    public void StartEndCutscene()
    {
        if (startDialogue != null)
            startDialogue.TriggerDialogue();
        startDialogueTriggered = true;
    }

    public void PlayReturnCutscene()
    {
        if (returnDialogue != null)
        {
            returnDialogue.TriggerDialogue();
            returnDialogueTriggered = true;
        }
        
    }

    public void PlayRefuseCutscene()
    {
        if (refuseDialogue != null)
        {
            refuseDialogue.TriggerDialogue();
            refuseDialogueTriggered = true;
        }
    }

    public IEnumerator EndOfReturnCutscene()
    {
        playerAnimator.SetTrigger("Fade");

        yield return new WaitForSeconds(2.0f);

        LevelLoader.instance.LoadScene(2);
    }

    public IEnumerator EndOfRefuseCutscene()
    {
        Debug.Log("Animate player and alchemist");
        alchemistAnimator.SetTrigger("Fade");

        yield return new WaitForSeconds(2.0f);

        LevelLoader.instance.LoadScene(2);
    }

}

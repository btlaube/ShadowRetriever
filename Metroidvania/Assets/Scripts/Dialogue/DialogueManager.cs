using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Animator dialogueAnimator;
    
    // [SerializeField] private Animator AiAnimator;
    [SerializeField] private PlayerController playerMovement;
    private Queue<string> sentences;    

    private AudioHandler audioHandler;

    void Awake()
    {
        audioHandler = GetComponent<AudioHandler>();
    }

    void Start()
    {
        sentences = new Queue<string>();
    }

    void Update()
    {
        // if(SceneManager.GetActiveScene().buildIndex == 3) {
        //     AiAnimator = GameObject.Find("AI").GetComponent<Animator>();
        playerMovement = GameObject.Find("Player").GetComponent<PlayerController>();
        // }        
        if (Input.GetKeyUp(KeyCode.Return))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueAnimator.SetTrigger("Open");

        // AiAnimator.SetBool("Talking", true);
        playerMovement.enabled = false;

        nameText.text = dialogue.name;
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        // DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        else
        {
            string sentence = sentences.Dequeue();
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence));
        }
    }

    IEnumerator TypeSentence (string sentence)
    {
        if (audioHandler != null)
            audioHandler.Play("DialogueAudio");
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.01f);
        }
        if (audioHandler != null)
            audioHandler.Stop("DialogueAudio");
    }

    void EndDialogue()
    {
        nameText.text = "";
        dialogueText.text = "";
        dialogueAnimator.SetTrigger("Close");
        // AiAnimator.SetBool("Talking", false);
        playerMovement.enabled = true;
    }
}

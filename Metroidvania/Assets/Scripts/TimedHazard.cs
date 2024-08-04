using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedHazard : MonoBehaviour
{
    public float onDuration;
    public float offDuration;

    public Sprite onSprite;
    public Sprite offSprite;
    private SpriteRenderer sr;
    private Animator animator;
    private AudioHandler audioHandler;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioHandler = GetComponent<AudioHandler>();
        InvokeRepeating("TriggerOnOff", 0.0f, offDuration);
    }

    public void TriggerOnOff()
    {
        StartCoroutine("PulseHazardOnOff");
    }

    public IEnumerator PulseHazardOnOff()
    {
        audioHandler.Play("On");
        GetComponent<BoxCollider2D>().enabled = true;
        sr.sprite = onSprite;
        animator.SetBool("On", true);
        animator.SetBool("Off", false);

        yield return new WaitForSeconds(onDuration);

        audioHandler.Play("Off");
        GetComponent<BoxCollider2D>().enabled = false;
        sr.sprite = offSprite;
        animator.SetBool("On", false);
        animator.SetBool("Off", true);
    }
}

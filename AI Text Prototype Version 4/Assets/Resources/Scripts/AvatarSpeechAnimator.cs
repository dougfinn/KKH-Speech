using UnityEngine;

public class AvatarSpeechAnimator : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public int talkingAnimationCount = 5;

    private bool wasTalking = false;

    public bool isTalking;

    void Update()
    {
        if (audioSource.clip != null)
        {
            Debug.Log("Audio is playing");
        }

        if (isTalking && !wasTalking)
        {
            int randomIndex = Random.Range(0, talkingAnimationCount);
            animator.SetInteger("TalkingIndex", randomIndex);
            animator.SetBool("Talking", true);
        }
        else if (!isTalking && wasTalking)
        {
            Debug.Log("Audio stopped playing");
            animator.SetBool("Talking", false);
        }

        wasTalking = isTalking;
    }
}
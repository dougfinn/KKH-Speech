using UnityEngine;

public class AvatarSpeechAnimator : MonoBehaviour
{
    public Animator animator; // Assign in Inspector
    public AudioSource audioSource; // Assign in Inspector

    private bool wasTalking = false;

    void Update()
    {
        bool isTalking = audioSource.clip;
        if (audioSource.clip != null)
        {
            Debug.Log("Audio is playing");
        }

        if (isTalking && !wasTalking)
        {
            animator.SetBool("Talking", true); // Or SetTrigger("Talking")
        }
        else if (!isTalking && wasTalking)
        {
            animator.SetBool("Talking", false);
        }

        wasTalking = isTalking;
    }
}
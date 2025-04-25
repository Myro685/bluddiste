using UnityEngine;

public class FootstepSoundController : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;
    public AudioClip footstepSound;
    public float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody nenalezen na objektu s FootstepSoundController! Skript očekává, že je připojený na objekt s Rigidbody (např. Player).");
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (footstepSound != null)
        {
            audioSource.clip = footstepSound;
            audioSource.loop = false;
        }
        else
        {
            Debug.LogWarning("FootstepSound není přiřazený v FootstepSoundController!");
        }
    }

    void Update()
    {
        if (rb == null) return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        bool isMoving = velocity.magnitude > 0.1f;

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f && footstepSound != null)
            {
                audioSource.Play();
                footstepTimer = footstepInterval;
            }
        } else {
            audioSource.Stop();
        }
    }
}
using UnityEngine;

/// <summary>
/// Řídí přehrávání zvuku kroků při pohybu hráče.
/// </summary>
public class FootstepSoundController : MonoBehaviour
{
    private Rigidbody rb;                  // Reference na Rigidbody hráče
    private AudioSource audioSource;       // AudioSource pro přehrávání zvuku kroků
    public AudioClip footstepSound;        // Zvukový klip pro krok
    public float footstepInterval = 0.5f;  // Interval mezi kroky (v sekundách)
    private float footstepTimer = 0f;      // Časovač pro další krok

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
            audioSource = gameObject.AddComponent<AudioSource>(); // Přidá AudioSource pokud chybí
        }

        if (footstepSound != null)
        {
            audioSource.clip = footstepSound;
            audioSource.loop = false; // Zvuk kroků se nepřehrává ve smyčce
        }
        else
        {
            Debug.LogWarning("FootstepSound není přiřazený v FootstepSoundController!");
        }
    }

    void Update()
    {
        if (rb == null) return;

        Vector3 velocity = rb.linearVelocity; // Získá rychlost pohybu
        velocity.y = 0; // Ignoruje pohyb ve vertikálním směru
        bool isMoving = velocity.magnitude > 0.1f; // Kontrola, zda se hráč pohybuje

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f && footstepSound != null)
            {
                audioSource.Play(); // Přehrát zvuk kroku
                footstepTimer = footstepInterval; // Resetovat časovač
            }
        }
        else
        {
            audioSource.Stop(); // Zastavit zvuk, pokud se hráč nehýbe
        }
    }
}
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public AudioClip ghostSound;
    private AudioSource audioSource;
    private Transform player;

    void Start()
    {
        // Zajistíme, že AudioSource existuje
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D zvuk
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 10f;
            audioSource.loop = true;
        }

        if (ghostSound != null)
        {
            audioSource.clip = ghostSound;
        }
        else
        {
            Debug.LogWarning("GhostSound není přiřazený v EnemyController!");
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Hráč s tagem 'Player' nebyl nalezen!");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Kontrola přímé viditelnosti (line of sight)
        bool isWallBetween = CheckWallBetween();

        // Zapneme nebo vypneme zvuk podle přítomnosti zdi
        if (isWallBetween && distance <= audioSource.maxDistance)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        else
        {
            if (!audioSource.isPlaying && ghostSound != null)
            {
                audioSource.Play();
            }
        }

        
    }

    bool CheckWallBetween()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Raycast od ducha k hráči
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, distanceToPlayer))
        {
            // Pokud raycast narazí na zeď, vrátíme true
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("Zeď detekována mezi duchem a hráčem!");
                return true;
            }
        }

        Debug.Log("Žádná zeď mezi duchem a hráčem – přímá viditelnost.");
        return false;
    }
}
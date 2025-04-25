using UnityEngine;

/// <summary>
/// Řídí chování ducha – přehrává zvuk, pokud je mezi hráčem a duchem zeď.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public AudioClip ghostSound;         // Zvuk ducha
    private AudioSource audioSource;     // AudioSource pro přehrávání zvuku
    private Transform player;            // Reference na hráče
    private PlayerController playerController; // Reference na skript hráče

    void Start()
    {
        // Najdi nebo přidej AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 10f;
            audioSource.loop = true;
            Debug.Log("AudioSource přidán na objekt Enemy.");
        }

        // Nastav zvuk, pokud je přiřazen
        if (ghostSound != null)
        {
            audioSource.clip = ghostSound;
            Debug.Log($"GhostSound přiřazen: {ghostSound.name}, délka: {ghostSound.length} sekund.");
        }
        else
        {
            Debug.LogWarning("GhostSound není přiřazený v EnemyController!");
        }

        // Najdi hráče a jeho skript
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Hráč s tagem 'Player' nebyl nalezen!");
        }
        else
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController nenalezen na hráči!");
            }
        }
    }

    void Update()
    {
        // Kontrola platnosti referencí a stavu hry
        if (player == null || playerController == null) return;
        if (Time.timeScale == 0f) return;

        // Pokud je hráč mrtvý nebo má plné zdraví, nic nedělej
        if (playerController.GetCurrentHealth() <= 0 || playerController.GetCurrentHealth() >= playerController.GetMaxHealth()) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log($"Vzdálenost od hráče: {distance}");

        bool isWallBetween = CheckWallBetween();
        Debug.Log($"Zeď mezi hráčem a duchem: {isWallBetween}");

        // Pokud je mezi hráčem a duchem zeď a jsou dostatečně blízko, zastav zvuk
        if (isWallBetween && distance <= audioSource.maxDistance)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Zvuk ducha zastaven – přímá viditelnost nebo příliš velká vzdálenost.");
            }    
        }
        else
        {
            // Pokud není zeď a zvuk nehraje, přehraj zvuk
            if (!audioSource.isPlaying && ghostSound != null)
            {
                audioSource.Play();
                Debug.Log("Zvuk ducha přehrán – zeď mezi hráčem a duchem.");
            }
            else if (ghostSound == null)
            {
                Debug.LogWarning("Nelze přehrát zvuk – ghostSound je null!");
            }
            else if (audioSource.isPlaying)
            {
                Debug.Log("Zvuk už hraje, pokračuje...");
            }
        }
    }

    /// <summary>
    /// Zjistí, zda je mezi hráčem a duchem zeď pomocí raycastu.
    /// </summary>
    bool CheckWallBetween()
    {
        if (player == null) return false;

        Vector3 enemyPosition = transform.position + Vector3.up * 0.5f; // Raycast z výšky 0.5
        Vector3 playerPosition = player.position + Vector3.up * 0.5f;   // Raycast na výšku 0.5
        Vector3 directionToPlayer = playerPosition - enemyPosition;
        float distanceToPlayer = directionToPlayer.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(enemyPosition, directionToPlayer.normalized, out hit, distanceToPlayer))
        {
            // Pokud raycast narazí na objekt s tagem "Wall", je mezi hráčem a duchem zeď
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("Zeď detekována mezi duchem a hráčem!");
                return true;
            }
            else
            {
                Debug.Log($"Raycast narazil na: {hit.collider.gameObject.name}, tag: {hit.collider.tag}");
            }
        }
        else
        {
            Debug.Log("Raycast nenarazil na žádný objekt – přímá viditelnost.");
        }

        return false;
    }

    /// <summary>
    /// Zastaví zvuk ducha (např. při výhře nebo smrti hráče).
    /// </summary>
    public void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Zvuk ducha zastaven – hráč vyhrál nebo zemřel.");
        }
    }
}
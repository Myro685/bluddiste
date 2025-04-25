using UnityEngine;

/// <summary>
/// Spravuje hlavní logiku hry – singleton.
/// Sleduje počet sebraných předmětů a řeší výhru.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance
    public int totalItems = 3;                               // Celkový počet sběratelných předmětů ve hře
    public Transform winTeleportPosition;                    // Pozice, kam se hráč teleportuje po výhře
    private int collectedItems = 0;                          // Počet sebraných předmětů

    void Awake()
    {
        // Singleton pattern – zajistí, že existuje jen jedna instance GameManageru
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // GameManager přežije změnu scény
        }
        else
        {
            Destroy(gameObject); // Pokud už existuje, znič tuto instanci
        }
    }

    /// <summary>
    /// Zavolej při sebrání předmětu. Po nasbírání všech spustí výhru.
    /// </summary>
    public void CollectItem()
    {
        collectedItems++;
        if (collectedItems >= totalItems)
        {
            Win();
        }
    }

    /// <summary>
    /// Výhra – teleportuje hráče na výherní pozici.
    /// </summary>
    void Win(){
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && winTeleportPosition != null)
        {
            player.transform.position = winTeleportPosition.position;
        }
    }
}

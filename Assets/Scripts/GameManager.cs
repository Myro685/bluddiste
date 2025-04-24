using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int totalItems = 3; // Celkový počet sbíratelných předmětů
    public Transform winTeleportPosition; // Pozice teleportu pro výhru
    private int collectedItems = 0; // Počet nasbíraných předmětů

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Udržet GameManager při přechodu mezi scénami
        }
        else
        {
            Destroy(gameObject); // Zničit duplikát GameManageru
        }
    }

    public void CollectItem()
    {
        collectedItems++;
        if (collectedItems >= totalItems)
        {
            Win();
        }
    }

    void Win(){
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && winTeleportPosition != null)
        {
            player.transform.position = winTeleportPosition.position;
        }
    }
}

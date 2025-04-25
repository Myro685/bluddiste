using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Řídí pohyb hráče, sbírání předmětů, zdraví, schovávání, výhru/prohru a UI.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;                  // Základní rychlost pohybu
    public float mouseSensitivity = 2f;           // Citlivost myši
    public Transform playerCamera;                // Kamera hráče
    public float wallCheckDistance = 0.6f;        // Vzdálenost pro kontrolu kolize se zdí

    private Rigidbody rb;                         // Rigidbody hráče
    private float xRotation = 0f;                 // Rotace kamery nahoru/dolů
    private CapsuleCollider playerCollider;       // Collider hráče
    private bool isHiding = false;                // Zda je hráč schovaný
    private Vector3 originalColliderCenter;       // Původní střed collideru
    private float originalColliderHeight;         // Původní výška collideru

    public float maxHealth = 100f;                // Maximální zdraví hráče
    private float currentHealth;                  // Aktuální zdraví
    private bool isDead = false;                  // Zda je hráč mrtvý

    private float baseSpeed;                      // Uložená základní rychlost
    private bool isSprinting = false;             // Zda hráč sprintuje
    public float sprintMultiplier = 2f;           // Násobitel rychlosti při sprintu
    public float shakeMagnitude = 0.1f;           // Síla třesení kamery při sprintu
    public float shakeSpeed = 10f;                // Rychlost třesení kamery
    private Vector3 originalCameraPosition;       // Výchozí pozice kamery

    public GameObject deathScreen;                // UI obrazovka smrti
    public Button restartButton;                  // Tlačítko pro restart

    public GameObject winScreen;                  // UI obrazovka výhry
    public Button backToMenuButton;               // Tlačítko pro návrat do menu
    public Button backToMainMenuButton;           // Alternativní tlačítko pro návrat do menu
    private bool hasWon = false;                  // Zda hráč vyhrál
    private int collectedItems = 0;               // Počet sebraných předmětů
    private int maxCollectibles = 5;              // Maximální počet předmětů k sebrání

    public GameObject pauseCanvas;                // UI panel pro pauzu
    private bool isPaused = false;                // Zda je hra pozastavená

    public TextMeshProUGUI collectiblesCounter;   // UI text pro počítadlo předmětů

    private AudioSource audioSource;              // AudioSource pro zvuky hráče
    public AudioClip collectSound;                // Zvuk při sebrání předmětu

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        if (playerCamera == null)
            playerCamera = Camera.main.transform;

        originalColliderCenter = playerCollider.center;
        originalColliderHeight = playerCollider.height;

        Cursor.lockState = CursorLockMode.Locked;
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        baseSpeed = moveSpeed;
        originalCameraPosition = playerCamera.localPosition;

        // Nastavení audia
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Nastavení UI a tlačítek
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }
        else
        {
            Debug.LogError("DeathScreen není přiřazený v PlayerController!");
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("RestartButton není přiřazený v PlayerController!");
        }

        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
        else
        {
            Debug.LogError("WinScreen není přiřazený v PlayerController!");
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(BackToMenu);
        }
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("BackToMenuButton není přiřazený v PlayerController!");
        }

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseCanvas není přiřazený v PlayerController!");
        }

        // Získání počtu collectible z MazeGeneratoru
        MazeGenerator mazeGenerator = Object.FindFirstObjectByType<MazeGenerator>();
        if (mazeGenerator != null)
        {
            maxCollectibles = mazeGenerator.numberOfCollectibles;
            Debug.Log($"Max Collectibles nastaveno na: {maxCollectibles}");
        }
        else
        {
            Debug.LogError("MazeGenerator nenalezen ve scéně!");
        }

        UpdateCollectiblesUI();
        if (collectiblesCounter != null)
        {
            collectiblesCounter.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("CollectiblesCounter není přiřazený v PlayerController!");
        }
    }

    void Update()
    {
        // Pauza
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Pokud je hráč mrtvý, vyhrál nebo je pauza, nic nedělej
        if (isDead || hasWon || isPaused) return;

        // Ovládání kamery myší
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        // Schovávání pod stoličku
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isHiding)
            {
                ExitHiding();
            }
            else
            {
                TryEnterHiding();
            }
        }

        // Sprint
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprinting = true;
            moveSpeed = baseSpeed * sprintMultiplier;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
            moveSpeed = baseSpeed;
        }

        // Efekt třesení kamery při sprintu
        if (isSprinting && !isHiding)
        {
            ApplyCameraShake();
        }
        else
        {
            playerCamera.localPosition = originalCameraPosition;
        }
    }

    void FixedUpdate()
    {
        if (isDead || hasWon || isPaused) return;

        if (!isHiding)
        {
            // Pohyb hráče
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = transform.forward * moveZ + transform.right * moveX;
            moveDirection = moveDirection.normalized * moveSpeed;

            // Kontrola kolize se zdí
            if (moveDirection.magnitude > 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, moveDirection.normalized, out hit, wallCheckDistance))
                {
                    if (hit.collider.CompareTag("Wall"))
                    {
                        Vector3 wallNormal = hit.normal;
                        moveDirection = Vector3.ProjectOnPlane(moveDirection, wallNormal).normalized * moveSpeed;
                    }
                }
            }

            moveDirection.y = rb.linearVelocity.y;
            rb.linearVelocity = moveDirection;
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        rb.angularVelocity = Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        // Detekce stoličky pro schování
        if (other.CompareTag("Chair"))
        {
            Debug.Log("Můžeš se schovat! Stiskni E.");
        }
        // Detekce a sbírání collectible
        else if (other.CompareTag("Collectible"))
        {
            Debug.Log("Detekován Collectible!");
            collectedItems++;
            Destroy(other.gameObject);
            Debug.Log($"Sesbíráno {collectedItems}/{maxCollectibles} předmětů!");

            if (collectSound != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
            else
            {
                Debug.LogWarning("CollectSound není přiřazený v PlayerController!");
            }

            UpdateCollectiblesUI();

            if (collectedItems >= maxCollectibles)
            {
                WinGame();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Opustil oblast stoličky
        if (other.CompareTag("Chair") && !isHiding)
        {
            Debug.Log("Opustil jsi oblast stoličky.");
        }
    }

    // Pokusí se schovat, pokud je poblíž stolička
    void TryEnterHiding()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Chair"))
            {
                EnterHiding();
                break;
            }
        }
    }

    // Schová hráče pod stoličku
    void EnterHiding()
    {
        isHiding = true;
        Debug.Log("Hráč se schoval pod stoličku!");

        playerCollider.height = 0.5f;
        playerCollider.center = new Vector3(0, 0.25f, 0);
        playerCamera.localPosition = new Vector3(0, 0.5f, 0);
    }

    // Vyleze ze schovky
    void ExitHiding()
    {
        isHiding = false;
        Debug.Log("Hráč vylezl ze schovávaní!");

        playerCollider.height = originalColliderHeight;
        playerCollider.center = originalColliderCenter;
        playerCamera.localPosition = new Vector3(0, 1.5f, 0);
    }

    // Vrací, zda je hráč schovaný
    public bool IsHiding()
    {
        return isHiding;
    }

    // Odečte zdraví hráči
    public void TakeDamage(float damage)
    {
        if (isDead || hasWon) return;

        currentHealth -= damage;
        Debug.Log($"Hráč obdržel {damage} poškození! Aktuální zdraví: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Smrt hráče
    void Die()
    {
        isDead = true;
        currentHealth = 0;
        Debug.Log("Hráč zemřel!");

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (collectiblesCounter != null)
            {
                collectiblesCounter.gameObject.SetActive(false);
            }
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Time.timeScale = 0f; 
       
        DisableEnemySounds();
    }

    // Výhra hráče
    void WinGame()
    {
        hasWon = true;
        Debug.Log("Hráč vyhrál!");

        if (winScreen != null)
        {
            winScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (collectiblesCounter != null)
            {
                collectiblesCounter.gameObject.SetActive(false);
            }
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        DisableEnemySounds();
    }

    // Vypne zvuky všech nepřátel ve scéně
    void DisableEnemySounds()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
        {
            enemy.StopSound();
        }
        Debug.Log("Všechny zvuky nepřátel byly vypnuty.");
    }

    // Přepíná pauzu
    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; 
            pauseCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Hra pozastavena.");
        }
        else
        {
            Time.timeScale = 1f; 
            pauseCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Hra odpauzována.");
        }
    }

    // Restartuje aktuální scénu
    void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Návrat do hlavního menu
    void BackToMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }

    // Vrací aktuální zdraví hráče
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // Vrací maximální zdraví hráče
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    // Obnoví kurzor a čas při zničení objektu hráče
    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f; 
    }

    // Efekt třesení kamery při sprintu
    void ApplyCameraShake()
    {
        float shakeOffsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeMagnitude;
        float shakeOffsetY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeMagnitude;

        Vector3 shakeOffset = new Vector3(shakeOffsetX, shakeOffsetY, 0f);
        playerCamera.localPosition = originalCameraPosition + shakeOffset;
    }

    // Aktualizuje UI počítadla sebraných předmětů
    void UpdateCollectiblesUI()
    {
        if (collectiblesCounter != null)
        {
            collectiblesCounter.text = $"Diamanty: {collectedItems}/{maxCollectibles}";
            Debug.Log($"Aktualizován UI text: Collectibles: {collectedItems}/{maxCollectibles}");
        }
    }
}
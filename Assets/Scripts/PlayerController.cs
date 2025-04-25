using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    public float wallCheckDistance = 0.6f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private CapsuleCollider playerCollider;
    private bool isHiding = false;
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    // Zdraví hráče
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    // Sprint a shake kamery
    private float baseSpeed;
    private bool isSprinting = false;
    public float sprintMultiplier = 2f;
    public float shakeMagnitude = 0.1f;
    public float shakeSpeed = 10f;
    private Vector3 originalCameraPosition;

    // Posmrtná obrazovka
    public GameObject deathScreen;
    public Button restartButton;

    // Výherní obrazovka
    public GameObject winScreen;
    public Button backToMenuButton;
    private bool hasWon = false;
    private int collectedItems = 0;
    private int maxCollectibles = 5;

    // UI pro zobrazení collectibles během hry
    public TextMeshProUGUI collectiblesCounter;

    // Zvuk pro sbírání collectibles
    private AudioSource audioSource;
    public AudioClip collectSound;

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

        // Nastavení AudioSource pro sbírání collectibles
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Nastavení posmrtné obrazovky
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

        // Nastavení výherní obrazovky
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
        else
        {
            Debug.LogError("BackToMenuButton není přiřazený v PlayerController!");
        }

        // Nastavení maximálního počtu collectibles podle MazeGenerator
        MazeGenerator mazeGenerator = FindObjectOfType<MazeGenerator>();
        if (mazeGenerator != null)
        {
            maxCollectibles = mazeGenerator.numberOfCollectibles;
            Debug.Log($"Max Collectibles nastaveno na: {maxCollectibles}");
        }
        else
        {
            Debug.LogError("MazeGenerator nenalezen ve scéně!");
        }

        // Inicializace UI textu pro collectibles
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
        if (isDead || hasWon) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

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
        if (isDead || hasWon) return;

        if (!isHiding)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = transform.forward * moveZ + transform.right * moveX;
            moveDirection = moveDirection.normalized * moveSpeed;

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
        if (other.CompareTag("Chair"))
        {
            Debug.Log("Můžeš se schovat! Stiskni E.");
        }
        else if (other.CompareTag("Collectible"))
        {
            Debug.Log("Detekován Collectible!");
            collectedItems++;
            Destroy(other.gameObject);
            Debug.Log($"Sesbíráno {collectedItems}/{maxCollectibles} předmětů!");
            
            // Přehrání zvuku při sebrání collectible
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
        if (other.CompareTag("Chair") && !isHiding)
        {
            Debug.Log("Opustil jsi oblast stoličky.");
        }
    }

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

    void EnterHiding()
    {
        isHiding = true;
        Debug.Log("Hráč se schoval pod stoličku!");

        playerCollider.height = 0.5f;
        playerCollider.center = new Vector3(0, 0.25f, 0);
        playerCamera.localPosition = new Vector3(0, 0.5f, 0);
    }

    void ExitHiding()
    {
        isHiding = false;
        Debug.Log("Hráč vylezl ze schovávaní!");

        playerCollider.height = originalColliderHeight;
        playerCollider.center = originalColliderCenter;
        playerCamera.localPosition = new Vector3(0, 1.5f, 0);
    }

    public bool IsHiding()
    {
        return isHiding;
    }

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
    }

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
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    void ApplyCameraShake()
    {
        float shakeOffsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeMagnitude;
        float shakeOffsetY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeMagnitude;

        Vector3 shakeOffset = new Vector3(shakeOffsetX, shakeOffsetY, 0f);
        playerCamera.localPosition = originalCameraPosition + shakeOffset;
    }

    void UpdateCollectiblesUI()
    {
        if (collectiblesCounter != null)
        {
            collectiblesCounter.text = $"Diamanty: {collectedItems}/{maxCollectibles}";
            Debug.Log($"Aktualizován UI text: Collectibles: {collectedItems}/{maxCollectibles}");
        }
    }
}
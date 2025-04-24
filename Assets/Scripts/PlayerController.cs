using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Rychlost pohybu
    public float mouseSensitivity = 2f; // Citlivost myši
    public Transform playerCamera; // Reference na kameru
    public float wallCheckDistance = 0.6f; // Vzdálenost pro detekci zdi

    private Rigidbody rb;
    private float xRotation = 0f; // Rotace kamery nahoru/dolů
    private CapsuleCollider playerCollider; // Reference na Collider hráče
    private bool isHiding = false; // Zda je hráč schovaný
    private Vector3 originalColliderCenter; // Původní střed Collideru
    private float originalColliderHeight; // Původní výška Collideru

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        if (playerCamera == null)
            playerCamera = Camera.main.transform;

        // Uložit původní hodnoty Collideru
        originalColliderCenter = playerCollider.center;
        originalColliderHeight = playerCollider.height;

        // Zamknout kurzor do středu obrazovky
        Cursor.lockState = CursorLockMode.Locked;

        // Zajistit, že Rigidbody nebude rotovat kvůli fyzice
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Rotace hráče a kamery pomocí myši
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotace kamery nahoru/dolů (osa X)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotace hráče doleva/doprava (osa Y)
        transform.Rotate(Vector3.up * mouseX);

        // Interakce se stoličkou (klávesa E pro skrčení/vyskočení)
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
            moveSpeed *= 2; // Zrychlení při stisknutí Shift
        } else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed /= 2; // Obnovení rychlosti po uvolnění Shift
        }
    }

    void FixedUpdate()
    {
        // Pohyb hráče (WASD/šipky) - pouze pokud není schovaný
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
            // Pokud je schovaný, zastavíme pohyb
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
        // Kontrola, zda je hráč u stoličky
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

        // Snížíme výšku hráče (simulace skrčení)
        playerCollider.height = 0.5f;
        playerCollider.center = new Vector3(0, 0.25f, 0);

        // Snížíme pozici kamery
        playerCamera.localPosition = new Vector3(0, 0.5f, 0);
    }

    void ExitHiding()
    {
        isHiding = false;
        Debug.Log("Hráč vylezl ze schovávaní!");

        // Obnovíme původní hodnoty Collideru a kamery
        playerCollider.height = originalColliderHeight;
        playerCollider.center = originalColliderCenter;
        playerCamera.localPosition = new Vector3(0, 1.5f, 0);
    }

    public bool IsHiding()
    {
        return isHiding;
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
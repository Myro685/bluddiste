using UnityEngine;

/// <summary>
/// Skript pro sběratelný předmět.
/// Po dotyku s hráčem zavolá GameManager a zničí se.
/// V Update se otáčí.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    // Když dojde ke kolizi s jiným objektem
    private void OnTriggerEnter(Collider other)
    {
        // Pokud je to hráč
        if (other.CompareTag("Player"))
        {
            // Informuj GameManager o sebrání předmětu
            GameManager.Instance.CollectItem();
            // Znič tento objekt
            Destroy(gameObject);
        }
    }

    // Každý snímek otáčej předmět kolem osy Y
    void Update()
    {
        transform.Rotate(0, 90 * Time.deltaTime, 0); 
    }
}

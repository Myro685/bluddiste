using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI skript pro zobrazení zdraví hráče pomocí health baru.
/// Dynamicky mění šířku výplně podle aktuálního zdraví.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    public PlayerController player;      // Reference na hráče (PlayerController)
    public Image healthFill;             // UI Image, která zobrazuje výplň zdraví

    private float maxHealth;             // Maximální zdraví hráče
    private float maxWidth;              // Maximální šířka health baru

    void Start()
    {
        // Pokud není hráč nastavený v inspektoru, najdi ho podle tagu
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        // Inicializace maximálních hodnot
        if (player != null)
        {
            maxHealth = player.GetMaxHealth();
            maxWidth = healthFill.rectTransform.sizeDelta.x; 
            UpdateHealthBar(); // Nastaví health bar na začátku
        }

        // Kontrola, zda je nastavený obrázek výplně
        if (healthFill == null)
        {
            Debug.LogError("Health Fill Image není nastavený v HealthBarUI!");
        }
    }

    void Update()
    {
        // Každý snímek aktualizuje health bar podle aktuálního zdraví hráče
        if (player != null)
        {
            UpdateHealthBar();
        }
    }

    /// <summary>
    /// Aktualizuje šířku health baru podle aktuálního zdraví hráče.
    /// </summary>
    void UpdateHealthBar()
    {
        float currentHealth = player.GetCurrentHealth(); // Získá aktuální zdraví
        float healthRatio = currentHealth / maxHealth;   // Poměr aktuálního zdraví k maximu
        float newWidth = maxWidth * healthRatio;         // Nová šířka výplně
        healthFill.rectTransform.sizeDelta = new Vector2(newWidth, healthFill.rectTransform.sizeDelta.y);
    }
}
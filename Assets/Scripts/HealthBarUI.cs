using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerController player; // Reference na hráče
    public Image healthFill; // Reference na Image, které reprezentuje zdraví

    private float maxHealth;
    private float maxWidth; // Maximální šířka Health Image

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        if (player != null)
        {
            maxHealth = player.GetMaxHealth();
            maxWidth = healthFill.rectTransform.sizeDelta.x; // Uložíme si původní šířku
            UpdateHealthBar();
        }

        if (healthFill == null)
        {
            Debug.LogError("Health Fill Image není nastavený v HealthBarUI!");
        }
    }

    void Update()
    {
        if (player != null)
        {
            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        float currentHealth = player.GetCurrentHealth();
        float healthRatio = currentHealth / maxHealth; // Poměr zdraví (0 až 1)
        float newWidth = maxWidth * healthRatio; // Nová šířka podle zdraví
        healthFill.rectTransform.sizeDelta = new Vector2(newWidth, healthFill.rectTransform.sizeDelta.y);
    }
}
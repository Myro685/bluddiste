using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Řídí hlavní menu hry – spouštění hry, zobrazení informací a ukončení.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public Button startButton;         // Tlačítko pro spuštění hry
    public Button infoButton;          // Tlačítko pro zobrazení informací
    public Button exitButton;          // Tlačítko pro ukončení hry
    public GameObject infoPanel;       // Panel s informacemi
    public Button closeInfoButton;     // Tlačítko pro zavření panelu s informacemi

    void Start()
    {
        // Kontrola, zda jsou všechny UI prvky přiřazeny
        if (startButton == null || infoButton == null || exitButton == null || infoPanel == null || closeInfoButton == null)
        {
            Debug.LogError("Některé prvky UI nejsou přiřazeny v MainMenuController!");
            return;
        }

        infoPanel.SetActive(false); // Na začátku je panel s informacemi skrytý

        // Přiřazení funkcí k tlačítkům
        startButton.onClick.AddListener(StartGame);
        infoButton.onClick.AddListener(ShowInfo);
        exitButton.onClick.AddListener(ExitGame);
        closeInfoButton.onClick.AddListener(HideInfo);
    }

    /// <summary>
    /// Spustí hlavní scénu hry.
    /// </summary>
    void StartGame()
    {
        SceneManager.LoadScene("MainScene"); 
    }

    /// <summary>
    /// Zobrazí panel s informacemi.
    /// </summary>
    void ShowInfo()
    {
        infoPanel.SetActive(true); 
    }

    /// <summary>
    /// Skryje panel s informacemi.
    /// </summary>
    void HideInfo()
    {
        infoPanel.SetActive(false); 
    }

    /// <summary>
    /// Ukončí hru.
    /// </summary>
    void ExitGame()
    {
        Application.Quit(); 
        Debug.Log("Hra byla ukončena."); 
    }
}
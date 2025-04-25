using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;
    public Button infoButton;
    public Button exitButton;
    public GameObject infoPanel;
    public Button closeInfoButton;

    void Start()
    {
        // Kontrola přiřazení
        if (startButton == null || infoButton == null || exitButton == null || infoPanel == null || closeInfoButton == null)
        {
            Debug.LogError("Některé prvky UI nejsou přiřazeny v MainMenuController!");
            return;
        }

        // Skryjeme Info panel při startu
        infoPanel.SetActive(false);

        // Přidáme funkce na tlačítka
        startButton.onClick.AddListener(StartGame);
        infoButton.onClick.AddListener(ShowInfo);
        exitButton.onClick.AddListener(ExitGame);
        closeInfoButton.onClick.AddListener(HideInfo);
    }

    void StartGame()
    {
        SceneManager.LoadScene("MainScene"); // Načte herní scénu
    }

    void ShowInfo()
    {
        infoPanel.SetActive(true); // Zobrazí Info panel
    }

    void HideInfo()
    {
        infoPanel.SetActive(false); // Skryje Info panel
    }

    void ExitGame()
    {
        Application.Quit(); // Ukončí hru
        Debug.Log("Hra byla ukončena."); // Debug zpráva (viditelná pouze v editoru)
    }
}
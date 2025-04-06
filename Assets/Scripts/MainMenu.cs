using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    // Play game
    public void Play(){
        SceneManager.LoadScene("MainScene");
    }

    // Quit game
    public void Quit(){
        Application.Quit();
    }
}

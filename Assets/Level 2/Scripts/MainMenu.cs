<<<<<<< Updated upstream:Assets/MainMenu.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class MainMenu : MonoBehaviour
{

    public float playDelay = 3f;

    public void PlayGame()
    {
        StartCoroutine(PlayGameWithDelay());
    }

    private IEnumerator PlayGameWithDelay()
    {
        yield return new WaitForSeconds(playDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
=======
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void QuitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
>>>>>>> Stashed changes:Assets/Level 2/Scripts/MainMenu.cs

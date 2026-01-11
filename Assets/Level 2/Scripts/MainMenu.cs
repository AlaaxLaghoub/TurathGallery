using UnityEngine;
using UnityEngine.SceneManagement;
<<<<<<< Updated upstream
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
=======
public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

>>>>>>> Stashed changes
    }

    public void QuitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
<<<<<<< Updated upstream
}
=======
}
>>>>>>> Stashed changes

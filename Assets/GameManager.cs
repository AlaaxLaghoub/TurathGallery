using UnityEngine;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    //   public GameObject footprints;
    //  public SpriteRenderer animalRenderer;
    // public Sprite correctAnimalSprite;


    public int totalMatches = 3;
    private int correctMatches = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void CheckWin()
    {
        correctMatches++;
        Debug.Log("Correct Match");

        if (correctMatches >= totalMatches)
        {
            Debug.Log("You Win!");
            // Show UI panel, play sound, etc.
        }
    }

    public void OnCorrectMatch()
    {
       // animalRenderer.sprite = correctAnimalSprite;

        // Hide footprints
      //  footprints.SetActive(false);


      
        
        
        StartCoroutine(NextLevelDelay());
    }

    IEnumerator NextLevelDelay()
    {
        yield return new WaitForSeconds(1.5f);
        NextLevel();
    }

    void NextLevel()
    {
        Debug.Log("Next level!");
    }

}

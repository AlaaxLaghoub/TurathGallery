using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class OasisEndZone : MonoBehaviour
{
    [Header("Oasis Settings")]
    public float slowdownRadius = 5f;
    public float completeRadius = 2f;

    [Header("Next Level Settings")]
    public string nextSceneName = ""; // Set this in Inspector to the name of the next level scene

    [Header("References")]
    public BirdManager birdManager; // Assign in Inspector

    public event Action OnOasisReached;

    private PlayerRunnerController player;
    private GameManager3 gameManager;
    private Spawner spawner;
    private bool completed;

    void Start()
    {
        // Find references if not assigned
        if (birdManager == null)
        {
            birdManager = FindObjectOfType<BirdManager>();
        }

        gameManager = GameManager3.Instance; // Use the singleton
        spawner = FindObjectOfType<Spawner>();

        // Debug warning if next scene name is not set
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next scene name is not set in OasisEndZone. Please assign a scene name in the inspector.");
        }
    }

    void Update()
    {
        if (player == null || completed) return;

        float distance = Vector3.Distance(
            player.transform.position,
            transform.position
        );

        if (distance <= slowdownRadius)
        {
            player.SlowDown();
        }

        if (distance <= completeRadius)
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        if (completed) return;

        completed = true;
        Debug.Log("Player reached oasis - Loading next level: " + nextSceneName);

        // Stop player
        if (player != null)
        {
            player.StopGame();
        }

        // Stop game manager (stops score and speed)
        if (gameManager != null)
        {
            gameManager.LevelComplete();
        }

        // Stop obstacle spawning
        if (spawner != null)
        {
            spawner.gameObject.SetActive(false);
        }

        // Handle bird reaching oasis
        if (birdManager != null)
        {
            birdManager.BirdReachOasis();
        }

        // Trigger the event
        OnOasisReached?.Invoke();

        // Load next level immediately
        LoadNextLevel();
    }

    void LoadNextLevel()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Cannot load next level: nextSceneName is not set in the inspector!");
            return;
        }

        // Check if the scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneName == nextSceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"Scene '{nextSceneName}' not found in build settings!");
            return;
        }

        // Load the specified scene
        SceneManager.LoadScene(nextSceneName);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerRunnerController>();
            Debug.Log("Player entered oasis zone");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slowdownRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, completeRadius);
    }
}
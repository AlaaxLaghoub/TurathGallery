using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager3 : MonoBehaviour
{
    public static GameManager3 Instance { get; private set; }

    public float initialGameSpeed = 5f;
    public float gameSpeedIncrease = 0.1f;
    public float gameSpeed { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hiscoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button retryButton;

    private PlayerRunnerController player;
    private Spawner spawner;
    private BirdManager birdManager; // Reference to BirdManager

    private float score;
    public float Score => score;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerRunnerController>();
        spawner = FindObjectOfType<Spawner>();
        birdManager = FindObjectOfType<BirdManager>(); // Get BirdManager

        NewGame();
    }

    public void NewGame()
    {
        // Destroy obstacles
        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        score = 0f;
        gameSpeed = initialGameSpeed;
        enabled = true;

        player.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        UpdateHiscore();

        // Reset oasis if it exists
        OasisEndZone oasis = FindObjectOfType<OasisEndZone>();
        if (oasis != null)
        {
            Debug.Log("Oasis should be reset for new game");
        }

        // Reset bird (this will also reset hit count in BirdManager)
        ResetBird();
    }

    public void GameOver()
    {
        gameSpeed = 0f;
        enabled = false;

        player.gameObject.SetActive(false);
        spawner.gameObject.SetActive(false);

        // Get final hit count from bird manager
        int finalHitCount = 0;
        if (birdManager != null)
        {
            finalHitCount = birdManager.ObstacleHits;
        }

        // Show hit count in game over
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\nFinal Score: {Mathf.FloorToInt(score)}\nObstacle Hits: {finalHitCount}";
            gameOverText.gameObject.SetActive(true);
        }

        retryButton.gameObject.SetActive(true);

        UpdateHiscore();
    }

    private void Update()
    {
        gameSpeed += gameSpeedIncrease * Time.deltaTime;
        score += gameSpeed * Time.deltaTime;
        scoreText.text = Mathf.FloorToInt(score).ToString("D5");
    }

    private void UpdateHiscore()
    {
        float hiscore = PlayerPrefs.GetFloat("hiscore", 0);

        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);
        }

        hiscoreText.text = Mathf.FloorToInt(hiscore).ToString("D5");
    }

    // Called when player hits an obstacle
    public void OnPlayerHit(int hitCount)
    {
        Debug.Log($"GameManager: Player hit count updated: {hitCount}");

        // You can add game-wide effects based on hit count here
        if (hitCount >= 2)
        {
            // Example: Speed penalty after multiple hits
            // gameSpeed = Mathf.Max(initialGameSpeed, gameSpeed * 0.9f);
        }
    }

    // Called when bird flies away (after 3 hits)
    public void OnBirdFled()
    {
        Debug.Log("GameManager: Bird has fled!");

        // Optional: Trigger game over or penalty when bird flees
        // GameOver(); // Uncomment if you want game over when bird leaves
    }

    public void ResetBird()
    {
        BirdManager birdManager = FindObjectOfType<BirdManager>();
        if (birdManager != null)
        {
            birdManager.ResetBird();
        }
    }

    public void LevelComplete()
    {
        Debug.Log("GameManager: Level Complete!");

        // Get final hit count
        int finalHitCount = 0;
        if (birdManager != null)
        {
            finalHitCount = birdManager.ObstacleHits;
        }

        // Stop the game
        gameSpeed = 0f;
        enabled = false; // Stops Update() from running

        // Update high score one last time
        UpdateHiscore();

        // Show level complete UI with hit count
        ShowLevelCompleteUI(finalHitCount);
    }

    private void ShowLevelCompleteUI(int hitCount)
    {
        // Use the existing UI elements to show level complete
        if (gameOverText != null)
        {
            gameOverText.text = $"LEVEL COMPLETE!\nScore: {Mathf.FloorToInt(score)}\nObstacle Hits: {hitCount}";
            gameOverText.color = Color.green;
            gameOverText.gameObject.SetActive(true);
        }

        if (hiscoreText != null)
        {
            // Keep score display
        }

        if (retryButton != null)
        {
            // Optionally change the button text
            TextMeshProUGUI buttonText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "PLAY AGAIN";
            }
            retryButton.gameObject.SetActive(true);
        }
    }

    // Helper method to get current hit count
    public int GetPlayerHitCount()
    {
        if (birdManager != null)
        {
            return birdManager.ObstacleHits;
        }
        return 0;
    }
}
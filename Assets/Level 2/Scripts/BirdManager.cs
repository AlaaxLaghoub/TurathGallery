using UnityEngine;
using TMPro;
using System.Collections;

public class BirdManager : MonoBehaviour
{
    [Header("Bird Settings")]
    public float birdDistanceFromPlayer = 8f;
    public float followSpeed = 5f;

    [Header("Fly Away Settings")]
    public float flyAwaySpeed = 15f;
    public float flyAwayScale = 2f;
    public float scaleUpTime = 0.5f;

    [Header("References")]
    public Transform player;
    public ChaseBirdBehavior birdBehavior;
    public TextMeshProUGUI birdDistanceText;
    public TextMeshProUGUI hitCountText; // Your top-right hit text

    // Private variables
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isActive = true;
    private bool isFollowingPlayer = true;
    private bool isFlyingAway = false;
    private bool hasReachedOasis = false;
    private int obstacleHits = 0;
    private const int maxHits = 3;
    private Coroutine flyAwayCoroutine;

    // Public property to access hit count
    public int ObstacleHits => obstacleHits;
    public int MaxHits => maxHits;

    void Start()
    
    {

        FindHitText();
        UpdateHitCountUI();
        // Find player if not assigned
        if (player == null)
        {
            PlayerRunnerController playerController = FindObjectOfType<PlayerRunnerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }

        if (birdBehavior == null)
        {
            birdBehavior = GetComponent<ChaseBirdBehavior>();
        }

        // Store original values
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        // Initialize hit count display
        UpdateHitCountUI();

        Debug.Log("Bird Manager initialized with " + obstacleHits + " hits");
    }

    void Update()
    {
        if (!isActive || player == null || isFlyingAway || hasReachedOasis || !isFollowingPlayer) return;

        // Calculate target position - ahead of player
        Vector3 targetPosition = new Vector3(
            player.position.x + birdDistanceFromPlayer,
            originalPosition.y,
            originalPosition.z
        );

        // Smoothly move towards target position
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        UpdateDistanceText();
    }

    public void PlayerHitObstacle()
    {
        if (!isActive || isFlyingAway || hasReachedOasis) return;

        obstacleHits++;
        Debug.Log($"Player hit obstacle! Hits: {obstacleHits}/{maxHits}");

        // Update UI immediately
        UpdateHitCountUI();

        // Also update bird distance text to show hits
        UpdateDistanceText();

        if (obstacleHits >= maxHits)
        {
            StartFlyAway();
        }
    }

    // Method to update the hit count UI in top-right corner
    void UpdateHitCountUI()
    {
        if (hitCountText != null)
        {
            hitCountText.text = $"Hits: {obstacleHits}/{maxHits}";

            // Color coding for visual feedback
            UpdateHitCountColor();
        }
        else
        {
            Debug.LogWarning("HitCountText is not assigned in BirdManager!");
        }
    }

    void UpdateHitCountColor()
    {
        if (hitCountText == null) return;

        switch (obstacleHits)
        {
            case 0:
                hitCountText.color = Color.green;
                break;
            case 1:
                hitCountText.color = Color.yellow;
                break;
            case 2:
                hitCountText.color = new Color(1f, 0.5f, 0f); // Orange
                break;
            case 3:
                hitCountText.color = Color.red;
                break;
        }
    }

    void UpdateDistanceText()
    {
        if (birdDistanceText == null || player == null) return;

        float distance = Mathf.Abs(transform.position.x - player.position.x);

        if (hasReachedOasis)
        {
            birdDistanceText.text = "✓ Bird at Oasis!";
            birdDistanceText.color = Color.green;
        }
        else if (isFlyingAway)
        {
            birdDistanceText.text = "✗ Bird Fled!";
            birdDistanceText.color = Color.red;
        }
        else
        {
            // Show both distance and hits in bird text
            birdDistanceText.text = $"Bird: {distance:F1}m | Hits: {obstacleHits}/{maxHits}";

            // Color based on hits for bird text too
            if (obstacleHits == 0)
                birdDistanceText.color = Color.white;
            else if (obstacleHits == 1)
                birdDistanceText.color = Color.yellow;
            else if (obstacleHits == 2)
                birdDistanceText.color = new Color(1f, 0.5f, 0f);
            else
                birdDistanceText.color = Color.red;
        }
    }

    void StartFlyAway()
    {
        if (isFlyingAway || hasReachedOasis) return;

        isFlyingAway = true;
        isFollowingPlayer = false;
        Debug.Log("Bird is flying away!");

        // Update hit text to show bird fled
        if (hitCountText != null)
        {
            hitCountText.text = "BIRD FLED!";
            hitCountText.color = Color.red;
        }

        // Stop the bird's normal bobbing behavior
        if (birdBehavior != null)
        {
            birdBehavior.StopBird();
        }

        // Start fly away sequence
        flyAwayCoroutine = StartCoroutine(FlyAwaySequence());
    }

    IEnumerator FlyAwaySequence()
    {
        Debug.Log("Starting fly away sequence");

        // Step 1: Smooth scale up
        Vector3 targetScale = originalScale * flyAwayScale;
        float elapsed = 0f;

        // Keep original rotation during scaling
        transform.rotation = originalRotation;

        while (elapsed < scaleUpTime)
        {
            float t = elapsed / scaleUpTime;
            t = 1f - Mathf.Pow(1f - t, 3);

            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        // Step 2: Smooth fly away
        float flyAwayDuration = 3f;
        elapsed = 0f;
        Vector3 startPosition = transform.position;

        Vector3 flyAwayTarget = new Vector3(
            startPosition.x + 30f,
            startPosition.y + 10f,
            startPosition.z
        );

        while (elapsed < flyAwayDuration)
        {
            float t = elapsed / flyAwayDuration;
            t = 1f - Mathf.Pow(1f - t, 3);

            transform.position = Vector3.Lerp(startPosition, flyAwayTarget, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Step 3: Fade out
        elapsed = 0f;
        float fadeDuration = 0.5f;
        startPosition = transform.position;
        Vector3 fadePosition = new Vector3(startPosition.x + 5f, startPosition.y + 2f, startPosition.z);

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;

            transform.position = Vector3.Lerp(startPosition, fadePosition, t);
            transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Step 4: Deactivate
        gameObject.SetActive(false);
        isActive = false;
        Debug.Log("Bird has flown away completely");
    }

    // Keep your existing BirdReachOasis and OasisArrivalSequence methods

    public void ResetBird()
    {
        // Stop any running coroutines
        if (flyAwayCoroutine != null)
        {
            StopCoroutine(flyAwayCoroutine);
            flyAwayCoroutine = null;
        }

        // Reset hit count
        obstacleHits = 0;
        isActive = true;
        isFlyingAway = false;
        hasReachedOasis = false;
        isFollowingPlayer = true;

        // Reset transform
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        // Reactivate GameObject
        gameObject.SetActive(true);

        // Reset bird behavior
        if (birdBehavior != null)
        {
            birdBehavior.enabled = true;

            if (birdBehavior.birdSprite != null)
            {
                birdBehavior.birdSprite.color = Color.white;
            }
        }

        // Reset UI
        UpdateHitCountUI();
        UpdateDistanceText();

        Debug.Log("Bird reset complete. Hits: " + obstacleHits);
    }

    // For debugging
    // void OnGUI()
    // {
    //     if (!Application.isPlaying) return;

    //     GUIStyle style = new GUIStyle(GUI.skin.label);
    //     style.fontSize = 14;
    //     style.normal.textColor = Color.white;

    //     Rect rect = new Rect(10, 220, 350, 30);

    //     if (hasReachedOasis)
    //     {
    //         GUI.Label(rect, "Bird Status: AT OASIS", style);
    //     }
    //     else if (isFlyingAway)
    //     {
    //         GUI.Label(rect, "Bird Status: FLYING AWAY", style);
    //     }
    //     else
    //     {
    //         GUI.Label(rect, "Bird Status: FOLLOWING", style);
    //     }

    //     rect.y += 20;
    //     GUI.Label(rect, $"Obstacle Hits: {obstacleHits}/{maxHits}", style);
    // }


    public void BirdReachOasis()
    {
        if (hasReachedOasis || isFlyingAway) return;

        hasReachedOasis = true;
        isFollowingPlayer = false;
        Debug.Log("Bird has reached the oasis!");

        // Stop any current fly away coroutine
        if (flyAwayCoroutine != null)
        {
            StopCoroutine(flyAwayCoroutine);
        }

        // Start oasis arrival sequence
        flyAwayCoroutine = StartCoroutine(OasisArrivalSequence());
    }

    IEnumerator OasisArrivalSequence()
    {
        Debug.Log("Starting oasis arrival sequence");

        // Update UI to show bird reached oasis
        if (hitCountText != null)
        {
            hitCountText.text = "✓ BIRD SAFE!";
            hitCountText.color = Color.green;
        }

        if (birdDistanceText != null)
        {
            birdDistanceText.text = "✓ Bird at Oasis!";
            birdDistanceText.color = Color.green;
        }

        // Fly to a landing position
        Vector3 targetPosition = new Vector3(
            transform.position.x + 3f,
            transform.position.y - 1f,
            transform.position.z
        );

        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Land animation
        elapsed = 0f;
        duration = 0.5f;
        startPosition = transform.position;
        Vector3 finalPosition = new Vector3(startPosition.x, startPosition.y - 0.5f, startPosition.z);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, finalPosition, t);
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * 0.8f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Stop bird behavior
        if (birdBehavior != null)
        {
            birdBehavior.StopBird();
            birdBehavior.enabled = false;
        }

        Debug.Log("Bird has landed at oasis!");
    }
    void FindHitText()
    {
        if (hitCountText != null) return; // Already assigned

        // Look for text with "hit" in the name (case insensitive)
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.text.ToLower().Contains("hit") ||
                text.name.ToLower().Contains("hit") ||
                text.text.Contains("/3")) // Looks like a hit counter
            {
                hitCountText = text;
                Debug.Log($"[BIRD] Found hit text: {text.name} - '{text.text}'");
                break;
            }
        }

        if (hitCountText == null && allTexts.Length > 0)
        {
            // Just use the first text as fallback
            hitCountText = allTexts[0];
            Debug.LogWarning($"[BIRD] Using fallback text: {hitCountText.name}");
        }
    }
}
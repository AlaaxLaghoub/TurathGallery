<<<<<<< Updated upstream
using UnityEngine;
using System.Collections;
=======
// SandObstacle.cs - Attach to sand prefabs
using UnityEngine;
>>>>>>> Stashed changes

public class SandObstacle : MonoBehaviour
{
    [Header("Slow Settings")]
<<<<<<< Updated upstream
    public float slowFactor = 0.5f; // How much to slow the player (0.5 = 50% speed)
    public float slowDuration = 2f; // How long the slow effect lasts

    [Header("Movement Settings")]
    public float moveSpeed = 3f; // Speed at which sand moves left
    public float lifeTime = 5f; // How long before sand disappears
    public float destroyDelay = 2f; // Delay before destroying after hitting player

    [Header("Visual Effects")]
    public Color activeColor = new Color(1f, 0.5f, 0f, 0.7f); // Color when moving
    public Color hitColor = Color.red; // Color when hitting player
    public float fadeOutDuration = 0.5f; // How long fade out takes

    // References
    private PlayerRunnerController playerController;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // State
    private bool hasHitPlayer = false;
    private bool isFadingOut = false;
    private float originalAlpha;

    void Start()
    {
        // Set tag
        gameObject.tag = "SandCluster";

        // Get references
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Setup rigidbody if needed
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // No gravity
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.isKinematic = false; // Use physics for movement
        }

        // Setup collider if needed
=======
    public float slowFactor = 0.5f;
    public float duration = 2f;

    void Start()
    {
        gameObject.tag = "SandCluster";

>>>>>>> Stashed changes
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true; // Sand should be a trigger
<<<<<<< Updated upstream
            // Adjust collider size based on sprite
            if (spriteRenderer != null)
            {
                collider.size = spriteRenderer.bounds.size;
            }
        }

        // Set initial color
        if (spriteRenderer != null)
        {
            originalAlpha = spriteRenderer.color.a;
            spriteRenderer.color = activeColor;
        }

        // Start moving left
        StartMovingLeft();

        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Keep moving left if not fading out
        if (!isFadingOut && rb != null)
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
        }

        // Optional: Rotate slowly for visual effect
        transform.Rotate(0, 0, 30 * Time.deltaTime);
    }

    void StartMovingLeft()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(-moveSpeed, 0);
=======
>>>>>>> Stashed changes
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
<<<<<<< Updated upstream
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            Debug.Log("[SAND] Player entered sand obstacle!");

            // Get player controller if not already
            if (playerController == null)
            {
                playerController = other.GetComponent<PlayerRunnerController>();
            }

            // Apply slow effect to player
            ApplySlowEffect();

            // Visual feedback
            if (spriteRenderer != null)
            {
                StartCoroutine(HitEffect());
            }

            // Start fade out
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    void ApplySlowEffect()
    {
        if (playerController == null || hasHitPlayer) return;

        hasHitPlayer = true;

        // Apply slow effect using the new method with factor
        playerController.SlowDown(slowFactor);

        Debug.Log($"[SAND] Applied slow factor: {slowFactor}");

        // Restore speed after duration
        Invoke(nameof(RestorePlayerSpeed), slowDuration);
    }

    void RestorePlayerSpeed()
    {
        if (playerController != null)
        {
            // Restore to normal run speed
            playerController.runSpeed = playerController.runSpeed / slowFactor;
            playerController.currentSpeed = playerController.runSpeed;
            playerController.targetHorizontalSpeed = playerController.runSpeed;
            Debug.Log($"[SAND] Restored player speed");
        }
    }

    IEnumerator HitEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(0.2f);

        spriteRenderer.color = originalColor;
    }

    IEnumerator FadeOutAndDestroy()
    {
        if (isFadingOut) yield break;

        isFadingOut = true;

        // Stop movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Disable collider to prevent multiple hits
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Fade out effect
        if (spriteRenderer != null)
        {
            float elapsedTime = 0f;
            Color startColor = spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                spriteRenderer.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }
        }

        // Wait before destroying
        yield return new WaitForSeconds(destroyDelay - fadeOutDuration);

        // Destroy the object
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        // Optional: Destroy when off screen
        // Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Draw movement direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.left * 2f);

        // Draw bounds
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        if (GetComponent<Collider2D>() != null)
        {
            Bounds bounds = GetComponent<Collider2D>().bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
=======
        if (other.CompareTag("Player"))
        {
            // Optional: Visual effect when hit
            GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0f, 0.5f);
>>>>>>> Stashed changes
        }
    }
}
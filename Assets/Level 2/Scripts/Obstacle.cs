using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float leftEdge;
    private bool hasCollided = false;

    // Add a Collider2D reference to check
    private Collider2D obstacleCollider;

    private void Start()
    {
        leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x - 2f;
        obstacleCollider = GetComponent<Collider2D>();

        if (obstacleCollider == null)
        {
            Debug.LogWarning("Obstacle has no Collider2D!");
        }
    }

    private void Update()
    {
        transform.position += GameManager3.Instance.gameSpeed * Time.deltaTime * Vector3.left;

        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }

    // Use OnCollisionEnter2D instead of OnTriggerEnter2D for more reliable detection
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Notify BirdManager
            BirdManager birdManager = FindObjectOfType<BirdManager>();
            if (birdManager != null)
            {
                birdManager.PlayerHitObstacle();
            }

            // You can also directly notify the player for visual feedback
            PlayerRunnerController player = collision.gameObject.GetComponent<PlayerRunnerController>();
            if (player != null)
            {
                player.TakeDamage();
            }
        }
    }

    // Also keep trigger detection as backup
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasCollided) return;

        if (collision.CompareTag("Player"))
        {
            hasCollided = true;
            Debug.Log("Player TRIGGERED obstacle!");

            BirdManager birdManager = FindObjectOfType<BirdManager>();
            if (birdManager != null)
            {
                birdManager.PlayerHitObstacle();
            }
        }
    }
}
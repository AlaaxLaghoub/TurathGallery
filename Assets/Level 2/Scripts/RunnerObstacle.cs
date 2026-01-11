// RunnerObstacle.cs
using UnityEngine;

public class RunnerObstacle : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public bool isHighObstacle = false;
    public bool canMoveVertically = false;

    [Header("Visual")]
    public float verticalSpeed = 2f;
    public float verticalAmplitude = 1f;

    private Vector3 startPosition;
    private bool isActive = true;

    void Start()
    {
        startPosition = transform.position;

        // Set appropriate layer for obstacle type
        if (isHighObstacle)
        {
            gameObject.layer = LayerMask.NameToLayer("HighObstacle");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("LowObstacle");
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Move left
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Vertical movement for flying obstacles
        if (canMoveVertically)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * verticalSpeed) * verticalAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Destroy if off screen
        if (transform.position.x < Camera.main.transform.position.x - 15f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Optional: Add hit effect
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(DestroyWithEffect());
        }
    }

    System.Collections.IEnumerator DestroyWithEffect()
    {
        // Add destruction effect here
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}
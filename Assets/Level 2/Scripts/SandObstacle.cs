// SandObstacle.cs - Attach to sand prefabs
using UnityEngine;

public class SandObstacle : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowFactor = 0.5f;
    public float duration = 2f;

    void Start()
    {
        gameObject.tag = "SandCluster";

        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true; // Sand should be a trigger
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Optional: Visual effect when hit
            GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0f, 0.5f);
        }
    }
}
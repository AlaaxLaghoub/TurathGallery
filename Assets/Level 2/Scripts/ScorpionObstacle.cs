// ScorpionObstacle.cs - Attach to scorpion prefabs
using UnityEngine;

public class ScorpionObstacle : MonoBehaviour
{
    void Start()
    {
        gameObject.tag = "Scorpion";

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }
}
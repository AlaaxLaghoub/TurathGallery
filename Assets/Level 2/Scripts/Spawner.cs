using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnableObject
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance;
    }

    public SpawnableObject[] objects;
    public float minSpawnRate = 1f;
    public float maxSpawnRate = 2f;

    // Add these variables
    public float spawnDistanceAhead = 10f; // How far ahead of player to spawn
    public float fixedYPosition = -4.563f; // Fixed Y position for obstacles
    private Transform playerTransform;     // Reference to player

    private void Start()
    {
        // Find the player when the game starts
        PlayerRunnerController player = FindObjectOfType<PlayerRunnerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void OnEnable()
    {
        Invoke(nameof(Spawn), Random.Range(minSpawnRate, maxSpawnRate));
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Spawn()
    {
        if (playerTransform == null) return; // Safety check

        float spawnChance = Random.value;

        foreach (SpawnableObject obj in objects)
        {
            if (spawnChance < obj.spawnChance)
            {
                // Calculate spawn position: ahead of player on X, fixed Y position
                Vector3 spawnPosition = new Vector3(
                    playerTransform.position.x + spawnDistanceAhead, // X: ahead of player
                    fixedYPosition,                                   // Y: fixed at -4.563
                    0f                                               // Z: 0
                );

                GameObject obstacle = Instantiate(obj.prefab, spawnPosition, Quaternion.identity);
                break;
            }

            spawnChance -= obj.spawnChance;
        }

        Invoke(nameof(Spawn), Random.Range(minSpawnRate, maxSpawnRate));
    }
}
// AnimatedCactus.cs - Modified to keep scale consistent
using UnityEngine;

public class AnimatedCactus : MonoBehaviour
{
    [Header("Animation")]
    public float bobSpeed = 1f;
    public float bobAmount = 0.1f;
    public float rotateSpeed = 0.01f;

    [Header("Visual Variety")]
    public Color[] colorVariations;
    // Removed scaleVariations to keep original scale

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private float randomOffset;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        randomOffset = Random.Range(0f, 100f);

        ApplyRandomVariation();
    }

    void Update()
    {
        // Gentle bobbing motion
        float bob = Mathf.Sin((Time.time + randomOffset) * bobSpeed) * bobAmount;
        transform.position = new Vector3(
            transform.position.x,
            startPosition.y + bob,
            transform.position.z
        );

        // Gentle rotation
        transform.Rotate(0, 0, Mathf.Sin(Time.time * 0.5f) * rotateSpeed * Time.deltaTime);
    }

    void ApplyRandomVariation()
    {
        // Random color
        if (colorVariations.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.color = colorVariations[Random.Range(0, colorVariations.Length)];
        }

        // REMOVED scale randomization - keeps original prefab scale

        // Random flip for variety
        if (spriteRenderer != null && Random.value > 0.5f)
        {
            spriteRenderer.flipX = true;
        }
    }

    // Optional: Add spines/flowers
    public void AddDecoration()
    {
        // You could instantiate small flower/thorn sprites as children
        if (Random.value > 0.7f) // 30% chance
        {
            GameObject flower = new GameObject("Flower");
            flower.transform.SetParent(transform);
            flower.transform.localPosition = new Vector3(0, 0.5f, -0.1f);
            flower.transform.localScale = Vector3.one * 0.2f; // Small decoration

            SpriteRenderer flowerRenderer = flower.AddComponent<SpriteRenderer>();
            flowerRenderer.sprite = CreateSimpleSprite();
            flowerRenderer.color = new Color(1f, 0.8f, 0.2f);
            flowerRenderer.sortingOrder = 1;
        }
    }

    Sprite CreateSimpleSprite()
    {
        Texture2D tex = new Texture2D(8, 8);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(4, 4));
                float alpha = 1f - Mathf.Clamp01(dist / 4f);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
    }
}
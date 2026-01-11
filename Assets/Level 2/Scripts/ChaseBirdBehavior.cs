using UnityEngine;

public class ChaseBirdBehavior : MonoBehaviour
{
    [Header("Flight Settings")]
    public float baseSpeed = 4f;
    public float bobHeight = 0.8f;
    public float bobSpeed = 3f;
    public float speedPulseAmount = 1f;
    public float speedPulseFrequency = 2f;

    [Header("Smoothing")]
    [Range(0.1f, 5f)]
    public float smoothTime = 0.5f; // Time to reach target speed/position
    public bool useSmoothing = true;

    [Header("Visual")]
    public SpriteRenderer birdSprite;
    public bool useSubtleColorChange = false;

    private Vector3 startPosition;
    private bool isActive = true;
    private float randomOffset;
    private float currentSpeed;
    private float targetSpeed;
    private float speedVelocity; // For SmoothDamp
    private Vector3 velocity; // For SmoothDamp position

    void Start()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
        currentSpeed = baseSpeed;
        targetSpeed = baseSpeed;

        if (birdSprite == null)
            birdSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isActive) return;

        // Calculate target speed with pulsing
        targetSpeed = baseSpeed + Mathf.Sin((Time.time + randomOffset) * speedPulseFrequency) * speedPulseAmount;

        // Smoothly interpolate to target speed
        if (useSmoothing)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, smoothTime);
        }
        else
        {
            currentSpeed = targetSpeed;
        }

        // Ensure minimum speed
        if (currentSpeed < 0.1f) currentSpeed = 0.1f;

        // Smooth horizontal movement
        Vector3 targetHorizontalPos = transform.position + Vector3.right * currentSpeed * Time.deltaTime;

        // Calculate vertical bobbing with smoothing
        float targetBobY = Mathf.Sin((Time.time + randomOffset) * bobSpeed) * bobHeight;
        Vector3 targetPosition = new Vector3(
            targetHorizontalPos.x,
            startPosition.y + targetBobY,
            transform.position.z
        );

        // Apply smoothing to position
        if (useSmoothing)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime * 0.5f);
        }
        else
        {
            transform.position = targetPosition;
        }

        // Very subtle visual feedback (optional)
        if (birdSprite != null && useSubtleColorChange)
        {
            float speedRatio = Mathf.Abs(targetSpeed - baseSpeed) / speedPulseAmount;
            birdSprite.color = Color.Lerp(Color.white, new Color(1f, 0.98f, 0.96f, 1f), speedRatio * 0.1f);
        }
    }

    // Alternative: Even smoother using Lerp with delta time
    void UpdateSmoothLerp()
    {
        if (!isActive) return;

        // Calculate target speed
        targetSpeed = baseSpeed + Mathf.Sin((Time.time + randomOffset) * speedPulseFrequency) * speedPulseAmount;

        // Smooth speed transition
        float smoothFactor = Mathf.Clamp01(Time.deltaTime / smoothTime);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, smoothFactor);

        // Calculate target position with bobbing
        Vector3 currentPos = transform.position;
        float bobY = Mathf.Sin((Time.time + randomOffset) * bobSpeed) * bobHeight;

        Vector3 targetPos = new Vector3(
            currentPos.x + currentSpeed * Time.deltaTime,
            startPosition.y + bobY,
            currentPos.z
        );

        // Smooth position transition
        transform.position = Vector3.Lerp(currentPos, targetPos, smoothFactor * 2f);
    }

    public void StopBird()
    {
        isActive = false;
        if (birdSprite != null)
        {
            birdSprite.color = Color.gray;
            // Smoothly stop
            StartCoroutine(SmoothStop());
        }
    }

    // Optional: Smooth stopping coroutine
    private System.Collections.IEnumerator SmoothStop()
    {
        float initialSpeed = currentSpeed;
        float stopDuration = 1f;
        float timer = 0f;

        while (timer < stopDuration)
        {
            timer += Time.deltaTime;
            float t = timer / stopDuration;
            currentSpeed = Mathf.Lerp(initialSpeed, 0f, t);
            yield return null;
        }
        currentSpeed = 0f;
    }
}
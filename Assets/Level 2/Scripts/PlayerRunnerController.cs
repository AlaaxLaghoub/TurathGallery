using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerRunnerController : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 6f;
    public float slowedSpeed = 2.5f;
    public float jumpForce = 12f;
    public ParticleSystem dust;


    [Header("Movement Smoothing")]
    public float acceleration = 15f;
    public float maxSpeed = 8f;
    public float groundDeceleration = 10f;
    public float airDeceleration = 5f;

    [Header("Jump Settings")]
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.1f;
    public float jumpCutMultiplier = 0.5f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Animation")]
    public string runAnimationName = "Run";
    public string jumpAnimationName = "Jump";
    public string fallAnimationName = "Fall";
    public string idleAnimationName = "Idle";
    public float jumpAnimationThreshold = 0.5f;
    public float fallAnimationThreshold = -0.5f;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip runSound;
    public AudioClip hurtSound;
    [Range(0f, 1f)]
    public float jumpVolume = 0.7f;
    [Range(0f, 1f)]
    public float runVolume = 0.5f;
    [Range(0f, 1f)]
    public float hurtVolume = 1f;
    public float runSoundPitchMin = 0.9f;
    public float runSoundPitchMax = 1.1f;
    public float minTimeBetweenRunSounds = 0.2f;
    public float runSoundSpeedThreshold = 0.5f;

    [Header("Hurt Settings")]
    public float hurtFlashDuration = 0.1f;
    public int hurtFlashCount = 3;
    public float hurtSlowDuration = 0.5f;

    [Header("UI References")]
    public TextMeshProUGUI hitCountText;

    [HideInInspector]
    public Vector3 playerStartPosition;

    public bool IsActive { get; private set; }
    public bool IsInvincible { get; private set; }

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer playerSprite;
    private AudioSource audioSource;
    private GameManager3 gameManager;
    private BirdManager birdManager;

    // Movement
    public float currentSpeed;
    private bool isGrounded;
    private bool isJumping;
    private bool wasGrounded;

    // Jump timing
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpInputReleased;
    private bool jumpRequested;

    // public ParticleSystem dust;


    // Animation
    private string currentAnimationState;
    private bool isFalling;

    // Smooth velocity tracking
    public float targetHorizontalSpeed;
    public float currentHorizontalVelocity;

    // Hurt state
    private bool isHurting = false;
    private Coroutine hurtCoroutine;

    // Audio tracking
    private float lastRunSoundTime = 0f;
    private bool wasRunning = false;
    private float currentRunSoundDelay = 0f;

    // Hit tracking
    private int hitCount = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerSprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        playerStartPosition = transform.position;
        currentSpeed = runSpeed;
        targetHorizontalSpeed = currentSpeed;

        gameManager = FindObjectOfType<GameManager3>();
        birdManager = FindObjectOfType<BirdManager>();

        Debug.Log("[INIT] Player initialized");
    }

    void Start()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
        }

        Application.targetFrameRate = 60;
        ChangeAnimationState(idleAnimationName);

        StartCoroutine(BriefInvincibility());
        Invoke(nameof(DelayedStart), 0.5f);
    }

    IEnumerator BriefInvincibility()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(0.5f);
        IsInvincible = false;
    }

    void DelayedStart()
    {
        StartGame();
    }

    void Update()
    {
        if (!IsActive)
        {
            if (isGrounded && currentAnimationState != idleAnimationName)
            {
                ChangeAnimationState(idleAnimationName);
            }
            return;
        }

        HandleJumpInput();
        UpdateTimers();
        CheckGround();
        UpdateAnimation();
        HandleRunSounds();

        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[TEST] Manual hurt test triggered");
            TakeDamage();
        }

        DebugDisplay();
    }

    void FixedUpdate()
    {
        if (!IsActive) return;

        HandleSmoothMovement();
        HandleJumpBuffer();
        ApplyBetterJumpPhysics();
    }

    // ================= COLLISION HANDLING =================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsActive || isHurting || IsInvincible) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("[PLAYER] Hit obstacle via collision!");
            HandleObstacleHit();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActive || isHurting || IsInvincible) return;

        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("[PLAYER] Hit obstacle via trigger!");
            HandleObstacleHit();
        }

        if (other.CompareTag("OasisEndZone"))
        {
            Debug.Log("[PLAYER] Reached oasis!");
            if (gameManager != null)
            {
                gameManager.LevelComplete();
            }

            if (birdManager != null)
            {
                birdManager.BirdReachOasis();
            }
        }
    }

    void HandleObstacleHit()
    {
        TakeDamage();

        if (birdManager != null)
        {
            birdManager.PlayerHitObstacle();
        }

        UpdateHitCountText();
    }

    void UpdateHitCountText()
    {
        if (hitCountText != null && birdManager != null)
        {
            hitCount = birdManager.ObstacleHits;
            hitCountText.text = $"Hits: {hitCount}/{birdManager.MaxHits}";

            switch (hitCount)
            {
                case 0:
                    hitCountText.color = Color.green;
                    break;
                case 1:
                    hitCountText.color = Color.yellow;
                    break;
                case 2:
                    hitCountText.color = new Color(1f, 0.5f, 0f);
                    break;
                case 3:
                    hitCountText.color = Color.red;
                    break;
            }
        }
    }

    // ================= AUDIO METHODS =================

    void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    void PlayRunSound()
    {
        if (audioSource != null && runSound != null)
        {
            audioSource.pitch = Random.Range(runSoundPitchMin, runSoundPitchMax);
            audioSource.PlayOneShot(runSound, runVolume);
            lastRunSoundTime = Time.time;
        }
    }

    void PlayHurtSound()
    {
        if (audioSource != null && hurtSound != null)
        {
            Debug.Log("[AUDIO] Playing hurt sound");
            audioSource.PlayOneShot(hurtSound, hurtVolume);
        }
        else
        {
            Debug.LogWarning($"[AUDIO] Cannot play hurt sound. AudioSource: {audioSource != null}, HurtSound: {hurtSound != null}");
        }
    }

    void StopRunSound()
    {
        wasRunning = false;
        lastRunSoundTime = Time.time;
    }

    void HandleRunSounds()
    {
        if (!IsActive || isHurting || isJumping) return;

        float currentSpeedValue = Mathf.Abs(rb.velocity.x);
        bool isRunning = currentSpeedValue > runSoundSpeedThreshold && isGrounded;

        if (isRunning && !wasRunning)
        {
            PlayRunSound();
            wasRunning = true;
        }
        else if (isRunning && wasRunning)
        {
            float speedFactor = Mathf.Clamp01(currentSpeedValue / maxSpeed);
            currentRunSoundDelay = Mathf.Lerp(minTimeBetweenRunSounds * 2f, minTimeBetweenRunSounds, speedFactor);

            if (Time.time - lastRunSoundTime >= currentRunSoundDelay)
            {
                PlayRunSound();
            }
        }
        else if (!isRunning || isJumping)
        {
            StopRunSound();
        }
    }

    // ================= MOVEMENT =================

    void HandleSmoothMovement()
    {
        if (rb == null) return;

        Vector2 currentVelocity = rb.velocity;
        float targetSpeed = targetHorizontalSpeed;
        float accelerationRate = isGrounded ? acceleration : acceleration * 0.5f;
        float decelerationRate = isGrounded ? groundDeceleration : airDeceleration;

        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            currentHorizontalVelocity = Mathf.MoveTowards(
                currentHorizontalVelocity,
                targetSpeed,
                accelerationRate * Time.fixedDeltaTime
            );
        }
        else
        {
            currentHorizontalVelocity = Mathf.MoveTowards(
                currentHorizontalVelocity,
                0f,
                decelerationRate * Time.fixedDeltaTime
            );
        }

        float clampedSpeed = Mathf.Clamp(currentHorizontalVelocity, -maxSpeed, maxSpeed);
        rb.velocity = new Vector2(clampedSpeed, currentVelocity.y);
    }

    // ================= ANIMATION =================

    void UpdateAnimation()
    {
        if (animator == null) return;

        float currentSpeedValue = Mathf.Abs(rb.velocity.x);
        float smoothedSpeed = Mathf.Lerp(animator.GetFloat("Speed"), currentSpeedValue, Time.deltaTime * 10f);
        float smoothedVelocityY = Mathf.Lerp(animator.GetFloat("VelocityY"), rb.velocity.y, Time.deltaTime * 10f);

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VelocityY", smoothedVelocityY);
        animator.SetFloat("Speed", smoothedSpeed);

        if (!isGrounded)
        {
            if (rb.velocity.y > jumpAnimationThreshold)
            {
                ChangeAnimationState(jumpAnimationName);
                isFalling = false;
            }
            else if (rb.velocity.y < fallAnimationThreshold)
            {
                ChangeAnimationState(fallAnimationName);
                isFalling = true;
            }
        }
        else
        {
            if (isFalling)
            {
                ChangeAnimationState(runAnimationName);
                isFalling = false;
            }
            else if (currentSpeedValue > 0.1f)
            {
                ChangeAnimationState(runAnimationName);
            }
            else
            {
                ChangeAnimationState(idleAnimationName);
            }
        }

        if (isGrounded && !wasGrounded && rb.velocity.y <= 0)
        {
            animator.SetTrigger("Land");
        }

        wasGrounded = isGrounded;
    }

    void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;

        if (animator != null)
        {
            animator.Play(newState, -1, 0f);
            currentAnimationState = newState;
        }
    }

    // ================= GROUND CHECK =================

    void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        isGrounded = colliders.Length > 0;

        if (isGrounded && rb.velocity.y <= 0.1f)
        {
            isJumping = false;
            coyoteTimeCounter = coyoteTime;
        }
        else if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    // ================= INPUT HANDLING =================

    void HandleJumpInput()
    {
        bool jumpPressed = Input.GetMouseButtonDown(0) ||
                          Input.GetKeyDown(KeyCode.Space) ||
                          Input.GetKeyDown(KeyCode.W) ||
                          Input.GetKeyDown(KeyCode.UpArrow);

        if (jumpPressed)
        {
            jumpRequested = true;

            dust.Play();

            jumpBufferCounter = jumpBufferTime;
        }

        bool jumpReleased = Input.GetMouseButtonUp(0) ||
                           Input.GetKeyUp(KeyCode.Space) ||
                           Input.GetKeyUp(KeyCode.W) ||
                           Input.GetKeyUp(KeyCode.UpArrow);

        if (jumpReleased && rb.velocity.y > 0)
        {
            jumpInputReleased = true;
        }
    }

    void UpdateTimers()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    // ================= JUMP PHYSICS =================

    void HandleJumpBuffer()
    {
        bool canJump = jumpBufferCounter > 0 &&
                      (isGrounded || coyoteTimeCounter > 0) &&
                      !isJumping;

        if (canJump && jumpRequested)
        {
            ExecuteJump();
            jumpRequested = false;
        }
    }

    void ExecuteJump()
    {
        Vector2 velocity = rb.velocity;
        velocity.y = 0f;
        rb.velocity = velocity;

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        isJumping = true;
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;

        StopRunSound();
        PlayJumpSound();

        // Trigger jump animation directly
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }

    void ApplyBetterJumpPhysics()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && jumpInputReleased)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            jumpInputReleased = false;
        }
    }

    // ================= HURT FEEDBACK =================

    public void TakeDamage()
    {
        if (isHurting || IsInvincible) return;

        Debug.Log("[HURT] Player taking damage");

        PlayHurtSound();
        StopRunSound();

        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
        }
        hurtCoroutine = StartCoroutine(HurtSequence());
    }

    private IEnumerator HurtSequence()
    {
        isHurting = true;
        IsInvincible = true;

        float originalSpeed = currentSpeed;
        currentSpeed = slowedSpeed;
        targetHorizontalSpeed = currentSpeed;

        for (int i = 0; i < hurtFlashCount; i++)
        {
            if (playerSprite != null)
            {
                playerSprite.enabled = false;
            }

            yield return new WaitForSeconds(hurtFlashDuration);

            if (playerSprite != null)
            {
                playerSprite.enabled = true;
            }

            if (i < hurtFlashCount - 1)
            {
                yield return new WaitForSeconds(hurtFlashDuration);
            }
        }

        yield return new WaitForSeconds(hurtSlowDuration);
        currentSpeed = originalSpeed;
        targetHorizontalSpeed = currentSpeed;

        yield return new WaitForSeconds(0.5f);
        IsInvincible = false;

        isHurting = false;
        hurtCoroutine = null;
    }

    // ================= PUBLIC API =================

    public void StartGame()
    {
        Debug.Log("[GAME] Starting player movement");

        IsActive = true;
        currentSpeed = runSpeed;
        targetHorizontalSpeed = currentSpeed;
        currentHorizontalVelocity = 0f;

        if (rb != null)
        {
            rb.simulated = true;
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;
        }

        hitCount = 0;
        UpdateHitCountText();

        ChangeAnimationState(runAnimationName);
    }

    public void StopGame()
    {
        IsActive = false;
        targetHorizontalSpeed = 0f;
        wasRunning = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        ChangeAnimationState(idleAnimationName);
    }

    public void SlowDown(float slowFactor)
    {
        if (isHurting || IsInvincible) return;

        Debug.Log($"[PLAYER] Slowing down by factor {slowFactor}");

        float originalSpeed = currentSpeed;
        currentSpeed = runSpeed * slowFactor;
        targetHorizontalSpeed = currentSpeed;

        // Optional: Visual feedback for slow
        if (playerSprite != null)
        {
            StartCoroutine(SlowVisualEffect());
        }
    }

    // Method 2: Slow down to default slowed speed (for existing code)
    public void SlowDown()
    {
        if (isHurting || IsInvincible) return;

        Debug.Log("[PLAYER] Slowing down to default slowed speed");

        currentSpeed = slowedSpeed;
        targetHorizontalSpeed = currentSpeed;

        // Optional: Visual feedback for slow
        if (playerSprite != null)
        {
            StartCoroutine(SlowVisualEffect());
        }
    }

    private System.Collections.IEnumerator SlowVisualEffect()
    {
        if (playerSprite == null) yield break;

        Color originalColor = playerSprite.color;
        playerSprite.color = new Color(0.5f, 0.5f, 1f, 0.7f); // Blue tint

        yield return new WaitForSeconds(0.3f);

        playerSprite.color = originalColor;
    }
    public void ResetPlayer()
    {
        Debug.Log("[RESET] Resetting player");

        StopGame();
        transform.position = playerStartPosition;
        currentSpeed = runSpeed;
        targetHorizontalSpeed = currentSpeed;
        currentHorizontalVelocity = 0f;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        isJumping = false;
        isGrounded = false;
        wasGrounded = false;
        isFalling = false;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
        jumpRequested = false;
        jumpInputReleased = false;
        wasRunning = false;
        lastRunSoundTime = 0f;
        hitCount = 0;

        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }
        isHurting = false;
        IsInvincible = false;

        if (playerSprite != null)
        {
            playerSprite.enabled = true;
            playerSprite.color = Color.white;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        UpdateHitCountText();
        ChangeAnimationState(idleAnimationName);

        StartCoroutine(BriefInvincibility());
        Invoke(nameof(StartGame), 0.1f);
    }

    // ================= DEBUG =================

    void DebugDisplay()
    {
        if (Time.frameCount % 60 == 0 && IsActive)
        {
            Debug.Log($"[STATUS] Hits: {hitCount}, Hurting: {isHurting}, Invincible: {IsInvincible}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
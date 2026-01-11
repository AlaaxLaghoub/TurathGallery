using UnityEngine;
using System.Collections;

public class DesertScorpion : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 3f;
    public bool startFacingRight = true;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public int attackDamage = 1;

    [Header("Animation")]
    public Animator scorpionAnimator;
    public string walkParam = "IsWalking";
    public string attackParam = "Attack";

    [Header("Visuals")]
    public ParticleSystem dustParticles;
    public ParticleSystem attackParticles;
    public Color attackColor = Color.red;
    public Color normalColor = new Color(0.7f, 0.5f, 0.3f);

    // Private variables
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float moveDirection;
    private bool isPatrolling = true;
    private bool canAttack = true;
    private float lastAttackTime;
    private Transform playerTarget;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        // Set initial direction
        moveDirection = startFacingRight ? 1f : -1f;

        // Set color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        // Try to find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }

        Debug.Log("Scorpion spawned - Ready to patrol!");
    }

    void Update()
    {
        if (!isPatrolling) return;

        // Check for player in range
        if (playerTarget != null && canAttack)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= attackRange)
            {
                Attack();
                return;
            }
        }

        // Continue patrolling
        Patrol();
    }

    void Patrol()
    {
        // Move in current direction
        Vector3 newPosition = transform.position;
        newPosition.x += patrolSpeed * moveDirection * Time.deltaTime;
        transform.position = newPosition;

        // Update animation
        if (scorpionAnimator != null)
        {
            scorpionAnimator.SetBool(walkParam, true);
        }

        // Flip sprite based on direction
        UpdateSpriteFlip();

        // Dust particles when moving
        UpdateDustParticles(true);

        // Check if reached patrol boundary
        if (Mathf.Abs(transform.position.x - startPosition.x) >= patrolDistance)
        {
            TurnAround();
        }
    }

    void Attack()
    {
        isPatrolling = false;
        canAttack = false;
        lastAttackTime = Time.time;

        // Stop moving
        if (scorpionAnimator != null)
        {
            scorpionAnimator.SetBool(walkParam, false);
        }

        // Trigger attack animation
        if (scorpionAnimator != null)
        {
            scorpionAnimator.SetTrigger(attackParam);
        }

        // Stop dust particles
        UpdateDustParticles(false);

        Debug.Log("Scorpion attacks!");

        // Face player if nearby
        if (playerTarget != null)
        {
            bool playerOnRight = playerTarget.position.x > transform.position.x;
            spriteRenderer.flipX = !playerOnRight;
        }

        // Start attack cooldown
        Invoke("ResetAttack", attackCooldown);

        // Resume patrolling after attack animation
        Invoke("ResumePatrol", 1f);
    }

    void ResumePatrol()
    {
        isPatrolling = true;
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    void TurnAround()
    {
        moveDirection *= -1f;
        startPosition = transform.position;

        Debug.Log($"Scorpion turns around at X: {transform.position.x}");
    }

    void UpdateSpriteFlip()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection < 0;
        }
    }

    void UpdateDustParticles(bool isMoving)
    {
        if (dustParticles == null) return;

        if (isMoving)
        {
            if (!dustParticles.isPlaying)
                dustParticles.Play();
        }
        else
        {
            if (dustParticles.isPlaying)
                dustParticles.Stop();
        }
    }

    // Animation Events (called from Animator)
    public void OnAttackWindup()
    {
        Debug.Log("Scorpion winds up for attack!");

        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = attackColor;
        }
    }

    public void OnAttackStrike()
    {
        Debug.Log("Scorpion strikes!");

        // Play attack particles
        if (attackParticles != null)
        {
            attackParticles.Play();
        }

        // Deal damage to player if in range
        DealDamage();
    }

    public void OnAttackRecovery()
    {
        Debug.Log("Scorpion recovers from attack");

        // Return to normal color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    void DealDamage()
    {
        // Check for player in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Scorpion stung the player!");

                // Damage player
                PlayerRunnerController player = hit.GetComponent<PlayerRunnerController>();
                if (player != null)
                {
                    // Call TakeDamage method on player
                    player.TakeDamage();

                    // Also trigger BirdManager if player is hit
                    BirdManager birdManager = FindObjectOfType<BirdManager>();
                    if (birdManager != null)
                    {
                        birdManager.PlayerHitObstacle();
                    }
                }

                break;
            }
        }
    }

    // For player to defeat scorpion (optional)
    public void TakeDamage()
    {
        Debug.Log("Scorpion takes damage!");

        // Flash white
        StartCoroutine(DamageFlash());

        // Stop attacking temporarily
        isPatrolling = false;
        canAttack = false;

        // Resume after 1 second
        Invoke("RecoverFromDamage", 1f);
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = normalColor;
    }

    void RecoverFromDamage()
    {
        isPatrolling = true;
        canAttack = true;
    }

    // Visualize attack range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Vector3 patrolEnd = transform.position;
        patrolEnd.x += patrolDistance * (startFacingRight ? 1 : -1);
        Gizmos.DrawLine(transform.position, patrolEnd);
        Gizmos.DrawWireSphere(patrolEnd, 0.3f);
    }
}
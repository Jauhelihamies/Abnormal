using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Pelaajan jahtaaminen")]
    public float stoppingDistance = 4f;
    public float moveSpeed = 3f;

    [Header("Potkumekaniikka (Kick)")]
    public float kickActivationRange = 2f;
    public int kickDamage = 10;
    public float kickCooldown = 1.5f;
    public float kickKnockbackForce = 12f;
    public float enemyLungeForce = 8f;

    [Header("Esteiden havaitseminen")]
    [Tooltip("Kuinka kaukaa vihollinen huomaa hautakiven edessään ja alkaa potkia sitä")]
    public float obstacleCheckDistance = 1.5f;
    [Tooltip("Valitse tähän taso, jolla hautakivi on (esim. Default tai Ground)")]
    public LayerMask obstacleLayer;

    [Header("Jalkojen kävelyanimaatio")]
    public Transform rightLeg;
    public Transform leftLeg;
    public float walkAnimationSpeed = 10f;
    public float maxLegRotation = 25f;

    private Transform playerTransform;
    private Rigidbody2D enemyRb;
    private Collider2D leftLegCollider;
    private float animationTimer;
    private bool isMoving;
    private bool isFacingRight = true;

    // Potkun sisäiset muuttujat
    private float kickCooldownTimer;
    private bool isKicking = false;
    private float kickAnimationTimer = 0f;
    private Quaternion originalLeftLegRotation;
    private bool hasDealtDamageThisKick = false;
    private bool isAttackingObstacle = false;

    void Awake()
    {
        if (leftLeg != null)
        {
            originalLeftLegRotation = leftLeg.localRotation;
            leftLegCollider = leftLeg.GetComponent<Collider2D>();
            if (leftLegCollider != null) leftLegCollider.enabled = false;
        }
    }

    void Start()
    {
        enemyRb = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerHealth ph = Object.FindFirstObjectByType<PlayerHealth>();
            if (ph != null) player = ph.gameObject;
        }

        if (player != null) playerTransform = player.transform;

        TriggerFakeKick();
    }

    void Update()
    {
        if (kickCooldownTimer > 0) kickCooldownTimer -= Time.deltaTime;

        if (isKicking)
        {
            AnimateKick();
            return;
        }

        // Tarkistetaan ensin, onko edessä hautakivi
        CheckForObstacles();

        // 1. JOS EDESSÄ ON HAUTAKIVI, POTKITAAN SITÄ
        if (isAttackingObstacle)
        {
            isMoving = false;
            if (kickCooldownTimer <= 0)
            {
                StartKick();
            }
            StopWalkingAnimation();
            return;
        }

        // 2. NORMAALI PELAAJAN JAHTAAMINEN (jos tie on vapaa)
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (playerTransform.position.x > transform.position.x && !isFacingRight) Flip();
        else if (playerTransform.position.x < transform.position.x && isFacingRight) Flip();

        if (distanceToPlayer <= kickActivationRange && kickCooldownTimer <= 0)
        {
            StartKick();
            return;
        }

        if (distanceToPlayer > stoppingDistance)
        {
            isMoving = true;
            float directionX = isFacingRight ? 1f : -1f;

            if (enemyRb != null)
            {
                enemyRb.linearVelocity = new Vector2(directionX * moveSpeed, enemyRb.linearVelocity.y);
            }

            animationTimer += Time.deltaTime * walkAnimationSpeed;
            float wave = Mathf.Sin(animationTimer);

            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(0, 0, wave * maxLegRotation);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(0, 0, -wave * maxLegRotation);
        }
        else
        {
            isMoving = false;
            if (enemyRb != null) enemyRb.linearVelocity = new Vector2(0, enemyRb.linearVelocity.y);
            StopWalkingAnimation();
        }
    }

    void CheckForObstacles()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;

        // Luodaan säde vihollisen keskikohdasta eteenpäin
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleCheckDistance, obstacleLayer);

        // Jos säde osuu hautakiveen tai jos vihollinen on jo hyvin lähellä sitä
        if (hit.collider != null && hit.collider.CompareTag("Tombstone") && !hit.collider.isTrigger)
        {
            isAttackingObstacle = true;
        }
        else
        {
            // Jos säde ei jostain syystä osu (esim. korkeusero), käytetään varmuuden vuoksi lähietäisyystunnistusta
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, obstacleCheckDistance, obstacleLayer);
            bool foundObstacle = false;
            foreach (var col in hitColliders)
            {
                if (col.CompareTag("Tombstone") && !col.isTrigger)
                {
                    foundObstacle = true;
                    break;
                }
            }
            isAttackingObstacle = foundObstacle;
        }
    }


    void StartKick()
    {
        isKicking = true;
        kickAnimationTimer = 0f;
        kickCooldownTimer = kickCooldown;
        isMoving = false;
        hasDealtDamageThisKick = false;

        if (leftLeg != null) leftLeg.localRotation = originalLeftLegRotation;
        if (leftLegCollider != null) leftLegCollider.enabled = true;

        if (enemyRb != null)
        {
            float lungeDirection = isFacingRight ? 1f : -1f;
            enemyRb.linearVelocity = new Vector2(0, enemyRb.linearVelocity.y);
            enemyRb.AddForce(new Vector2(lungeDirection * enemyLungeForce, 0f), ForceMode2D.Impulse);
        }
    }

    void AnimateKick()
    {
        if (leftLeg == null) return;

        kickAnimationTimer += Time.deltaTime * 4f;
        float kickWave = Mathf.Sin(kickAnimationTimer * Mathf.PI);
        float maxKickAngle = 80f;
        float kickAngle = isFacingRight ? -kickWave * maxKickAngle : kickWave * maxKickAngle;

        leftLeg.localRotation = Quaternion.Euler(0, 0, kickAngle);

        if (kickAnimationTimer >= 1f)
        {
            isKicking = false;
            leftLeg.localRotation = originalLeftLegRotation;
            if (leftLegCollider != null) leftLegCollider.enabled = false;
        }
    }

    // Kutsutaan triggeristä kun osutaan pelaajaan
    public void OnKickHitPlayer(GameObject player)
    {
        if (hasDealtDamageThisKick) return;
        hasDealtDamageThisKick = true;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null) playerHealth.TakeDamage();

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
            Vector2 force = new Vector2(knockbackDirection.x * kickKnockbackForce, 4f);
            playerMovement.ApplyKnockback(force, 0.25f);
        }
    }

    // Kutsutaan triggeristä kun osutaan hautakiveen
    public void OnKickHitTombstone(GameObject tombstone)
    {
        if (hasDealtDamageThisKick) return;
        hasDealtDamageThisKick = true;

        DestructibleTombstone stone = tombstone.GetComponent<DestructibleTombstone>();
        if (stone != null)
        {
            stone.TakeHit();
        }
    }

    void StopWalkingAnimation()
    {
        if (rightLeg != null) rightLeg.localRotation = Quaternion.Lerp(rightLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        if (leftLeg != null) leftLeg.localRotation = Quaternion.Lerp(leftLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        animationTimer = 0f;
    }

    void TriggerFakeKick()
    {
        isKicking = true;
        kickAnimationTimer = 0f;
        hasDealtDamageThisKick = true;
        if (leftLegCollider != null) leftLegCollider.enabled = false;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        // Piirtää punaisen viivan editorissa, jotta näet kuinka kauas vihollinen näkee hautakivet
        Gizmos.color = Color.red;
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, direction * obstacleCheckDistance);
    }
}

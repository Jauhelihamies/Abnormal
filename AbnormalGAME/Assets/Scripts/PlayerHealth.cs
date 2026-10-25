using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite damagedSprite1;
    public Sprite damagedSprite2;
    public Sprite healingSprite;
    public Sprite gameOverSprite;
    public GameObject EYES;

    [Header("Audio Clips")]
    public AudioClip damageSound;
    public AudioClip healingLoopSound;
    public AudioClip gameOverSound;

    [Header("Effects")]
    public ParticleSystem deathParticles;
    public GameOverUI uiScript; // VIITTAUS UUTEEN UI-SKRIPTIIN

    [Header("Damage Settings")]
    public float damageCooldown = 1.5f;
    private bool isInvulnerable = false;

    [Header("Healing Settings")]
    public float healDuration = 2f;
    public float shakeSpeed = 30f;
    public float shakeAmount = 10f;

    private int currentHealth = 3;
    private bool isHealing = false;
    private float healTimer = 0f;
    private bool isDead = false;

    private PlayerMovement movementScript;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float originalRotationZ;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateSprite();
    }

    void Update()
    {
        if (isDead) return;
        HandleHealingInput();
    }

    void HandleHealingInput()
    {
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        bool squarePressed = Gamepad.current != null && Gamepad.current.buttonWest.isPressed;
        bool isHoldingHeal = ePressed || squarePressed;

        if (isHoldingHeal && currentHealth < 3)
        {
            if (!isHealing)
            {
                isHealing = true;
                originalRotationZ = transform.rotation.eulerAngles.z;
                if (movementScript != null) movementScript.enabled = false;

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }

                if (healingSprite != null) spriteRenderer.sprite = healingSprite;

                if (audioSource != null && healingLoopSound != null)
                {
                    audioSource.clip = healingLoopSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }

            healTimer += Time.deltaTime;
            float zRotation = originalRotationZ + Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.rotation = Quaternion.Euler(0, 0, zRotation);

            if (healTimer >= healDuration)
            {
                currentHealth = 3;
                ResetHealingState();
                UpdateSprite();
            }
        }
        else if (isHealing)
        {
            ResetHealingState();
            UpdateSprite();
        }
    }

    void ResetHealingState()
    {
        isHealing = false;
        healTimer = 0f;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
        }

        transform.rotation = Quaternion.Euler(0, 0, originalRotationZ);
        if (movementScript != null) movementScript.enabled = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isDead && collision.gameObject.CompareTag("Enemy")) TakeDamage();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isDead && other.CompareTag("Enemy")) TakeDamage();
    }

    void TakeDamage()
    {
        if (isInvulnerable) return;
        if (isHealing) ResetHealingState();

        currentHealth--;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (audioSource != null && damageSound != null) audioSource.PlayOneShot(damageSound);
            UpdateSprite();
            StartCoroutine(DamageCooldownRoutine());
        }
    }

    IEnumerator DamageCooldownRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;
        while (elapsed < damageCooldown)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    void UpdateSprite()
    {
        if (isHealing) return;
        switch (currentHealth)
        {
            case 3: spriteRenderer.sprite = normalSprite; break;
            case 2: spriteRenderer.sprite = damagedSprite1; break;
            case 1: spriteRenderer.sprite = damagedSprite2; break;
        }
    }

    void Die()
    {
        isDead = true;
        if (EYES != null) EYES.SetActive(false);
        spriteRenderer.sprite = gameOverSprite;
        transform.rotation = Quaternion.identity;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            if (gameOverSound != null) audioSource.PlayOneShot(gameOverSound);
        }

        if (deathParticles != null) deathParticles.Play();
        if (movementScript != null) movementScript.enabled = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = false;
            rb.centerOfMass = new Vector2(0f, -0.5f);
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), 3f);
            rb.angularVelocity = Random.Range(-100f, 100f);
        }

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1) Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        // Kutsutaan UI-skriptiä käynnistymään
        if (uiScript != null) uiScript.StartGameOverSequence();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead && movementScript != null && ((1 << collision.gameObject.layer) & movementScript.groundLayer) != 0)
        {
            if (rb != null)
            {
                rb.linearDamping = 2.5f;
                rb.angularDamping = 2.5f;
                rb.gravityScale = 2f;
            }
        }
    }
}

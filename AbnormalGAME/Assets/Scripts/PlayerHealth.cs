using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // UUSI: Varmistaa, että hahmolla on äänilähde
public class PlayerHealth : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite damagedSprite1;
    public Sprite damagedSprite2;
    public Sprite healingSprite;
    public Sprite gameOverSprite;

    public GameObject Eyes;
    [Header("Audio Clips")]
    public AudioClip damageSound;      // UUSI: Ääni kun otetaan osumaa
    public AudioClip healingLoopSound; // UUSI: Jatkuva ääni parannuksen aikana (esim. puskassa suhina tai latausääni)
    public AudioClip gameOverSound;    // UUSI: Ääni kun kurpitsa menee muussiksi

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
    private AudioSource audioSource; // UUSI: Komponentti äänten toistamiseen
    private float originalRotationZ;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); // Haetaan äänilähde

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

                // UUSI: Aloitetaan parannusäänen toisto loopilla
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
                Debug.Log("Pelaaja parannettu täyteen!");
            }
        }
        else
        {
            if (isHealing)
            {
                ResetHealingState();
                UpdateSprite();
            }
        }
    }

    void ResetHealingState()
    {
        isHealing = false;
        healTimer = 0f;

        // UUSI: Pysäytetään parannusääni
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
        if (isDead) return;
        if (collision.gameObject.CompareTag("Enemy")) TakeDamage();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("Enemy")) TakeDamage();
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
            // UUSI: Soitetaan osumaääni kerran (PlayOneShot ei katkaise muita ääniä)
            if (audioSource != null && damageSound != null)
            {
                audioSource.PlayOneShot(damageSound);
            }

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
        Eyes.SetActive(false);
        spriteRenderer.sprite = gameOverSprite;
        transform.rotation = Quaternion.identity;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            if (gameOverSound != null)
            {
                audioSource.PlayOneShot(gameOverSound);
            }
        }

        if (movementScript != null) movementScript.enabled = false;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
    }
}

using UnityEngine;
using System.Collections;

public class DestructibleTombstone : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite stage1Normal;
    public Sprite stage2Damaged;
    public Sprite stage3Cracked;
    public Sprite stage4Broken;

    [Header("Rotation Settings")]
    public float rotateAmount = 25f;
    public float rotationDuration = 0.3f; // UUSI: Kuinka monta sekuntia kääntymisliike kestää (pienempi = nopeampi)

    [Header("Cooldown Settings")]
    public float hitCooldown = 0.8f;
    private bool isInvulnerable = false;

    [Header("Audio & Effects")]
    public AudioClip crumbleSound;
    public ParticleSystem dustParticles;

    private int currentStage = 1;
    private bool isDestroyed = false;
    private AudioSource audioSource;
    private float targetZRotation = 0f; // UUSI: Pitää kirjaa kulmasta, johon ollaan kääntymässä

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && stage1Normal != null)
        {
            spriteRenderer.sprite = stage1Normal;
        }

        // Otetaan alkukulma talteen (yleensä 0)
        targetZRotation = transform.rotation.eulerAngles.z;
    }

    public void TakeHit()
    {
        if (isDestroyed || isInvulnerable) return;

        currentStage++;

        // Soitetaan efektit
        if (audioSource != null && crumbleSound != null) audioSource.PlayOneShot(crumbleSound);
        if (dustParticles != null) dustParticles.Play();

        // Päivitetään sprite ja uusi tavoitekulma
        if (currentStage == 2)
        {
            if (stage2Damaged != null) spriteRenderer.sprite = stage2Damaged;
            targetZRotation -= rotateAmount; // Miinus kääntää oikealle
            StartCoroutine(SmoothRotateRoutine(targetZRotation));
        }
        else if (currentStage == 3)
        {
            if (stage3Cracked != null) spriteRenderer.sprite = stage3Cracked;
            targetZRotation -= rotateAmount;
            StartCoroutine(SmoothRotateRoutine(targetZRotation));
        }
        else if (currentStage >= 4)
        {
            BreakCompletely();
            return;
        }

        StartCoroutine(HitCooldownRoutine());
    }

    // UUSI KORUTIINI: Kääntää ristin pehmeästi ja tasaisesti uuteen kulmaan
    IEnumerator SmoothRotateRoutine(float targetZ)
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / rotationDuration;

            // Smootherstep tekee liikkeestä alussa ja lopussa hieman pehmeämmän
            float t = Mathf.SmoothStep(0f, 1f, normalizedTime);

            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation; // Varmistetaan tarkka loppukulma
    }

    IEnumerator HitCooldownRoutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(hitCooldown);
        isInvulnerable = false;
    }

    void BreakCompletely()
    {
        isDestroyed = true;
        if (stage4Broken != null) spriteRenderer.sprite = stage4Broken;

        // Kaadetaan risti maahan sulavasti loppuasentoon (-85 astetta)
        StartCoroutine(SmoothRotateRoutine(-85f));

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        Debug.Log("Hautakivi kaatui ja mureni maahan!");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player"))
        {
            TakeHit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            TakeHit();
        }
    }
}

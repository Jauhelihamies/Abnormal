using UnityEngine;
using System.Collections;

public class DestructibleTombstone : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite stage1Normal;
    public Sprite stage2Damaged;
    public Sprite stage3Cracked;
    public Sprite stage4Broken;

    public float rotateAmount = 25f;
    public float rotationDuration = 0.3f;
    public float hitCooldown = 0.8f;
    private bool isInvulnerable = false;

    [Header("Audio Settings")]
    public AudioClip[] crumbleSounds; // Muutettu taulukoksi, johon voi lisätä useita ääniä
    [Range(0.1f, 3f)] public float minPitch = 0.85f;
    [Range(0.1f, 3f)] public float maxPitch = 1.15f;

    [Header("Effects")]
    public ParticleSystem dustParticles;

    private int currentStage = 1;
    private bool isDestroyed = false;
    private AudioSource audioSource;
    private float targetZRotation = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && stage1Normal != null)
        {
            spriteRenderer.sprite = stage1Normal;
        }

        targetZRotation = transform.rotation.eulerAngles.z;
    }

    public void TakeHit()
    {
        if (isDestroyed || isInvulnerable) return;

        currentStage++;

        // Äänen valinta ja toistaminen satunnaisella pitchillä
        if (audioSource != null && crumbleSounds != null && crumbleSounds.Length > 0)
        {
            // Valitaan satunnainen ääni listalta
            int randomIndex = Random.Range(0, crumbleSounds.Length);
            AudioClip randomClip = crumbleSounds[randomIndex];

            if (randomClip != null)
            {
                // Asetetaan satunnainen pitch
                float randomPitch = Random.Range(minPitch, maxPitch);
                audioSource.pitch = randomPitch;

                // Soitetaan valittu ääni
                audioSource.PlayOneShot(randomClip);
            }
        }

        if (dustParticles != null) dustParticles.Play();

        if (currentStage == 2)
        {
            if (stage2Damaged != null) spriteRenderer.sprite = stage2Damaged;
            targetZRotation -= rotateAmount;
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

    IEnumerator SmoothRotateRoutine(float targetZ)
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / rotationDuration;
            float t = Mathf.SmoothStep(0f, 1f, normalizedTime);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation;
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
        StartCoroutine(SmoothRotateRoutine(-85f));

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player"))
        {
            TakeHit();
        }
    }
}

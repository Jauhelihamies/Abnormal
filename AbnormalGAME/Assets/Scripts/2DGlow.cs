using UnityEngine;
using System.Collections;

public class AdvancedEyeBlinkShader : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer eyeRenderer;

    [Header("Glow Settings")]
    [Range(0f, 10f)] public float minBrightness = 0.3f;
    public float maxBrightness = 1.0f;
    [Range(0f, 1f)] public float noiseStrength = 0.2f;
    public float noiseSpeed = 6f;
    public float glowFadeSpeed = 2f; // UUSI: Kuinka nopeasti hehku laskee takaisin minimiin avautumisen jälkeen

    [Header("Blink Settings")]
    public float blinkChance = 0.15f;
    public float closeDuration = 0.15f;
    public float holdClosedDuration = 0.2f;
    public float openDuration = 0.2f;

    private float seed;
    private bool isBlinking = false;
    private Material eyeMaterial;
    private float currentGlowBrightness;

    private static readonly int BlinkProgressProperty = Shader.PropertyToID("_BlinkProgress");
    private static readonly int GlowBrightnessProperty = Shader.PropertyToID("_GlowBrightness");

    private WaitForSeconds blinkCheckInterval;
    private WaitForSeconds holdClosedWait;

    void Start()
    {
        if (eyeRenderer != null)
        {
            eyeMaterial = eyeRenderer.material;
        }

        seed = Random.Range(0f, 100f);
        blinkCheckInterval = new WaitForSeconds(0.5f);
        holdClosedWait = new WaitForSeconds(holdClosedDuration);

        eyeMaterial.SetFloat(BlinkProgressProperty, 0f);

        // Aloitetaan minimihehkusta
        currentGlowBrightness = minBrightness;
        eyeMaterial.SetFloat(GlowBrightnessProperty, currentGlowBrightness);

        StartCoroutine(BlinkRandomizerLoop());
    }

    void Update()
    {
        if (eyeMaterial == null) return;

        // Jos silmä ei räpsähdä, hehku palautuu hitaasti minimiarvoon
        if (!isBlinking && currentGlowBrightness > minBrightness)
        {
            currentGlowBrightness = Mathf.MoveTowards(currentGlowBrightness, minBrightness, Time.deltaTime * glowFadeSpeed);
        }

        // Kohina pidetään päällä elävöittämään silmää, mutta se lasketaan nykyisestä hehkun tasosta
        float noiseSample = Mathf.PerlinNoise(Time.time * noiseSpeed, seed);
        float noiseOffset = (noiseSample * 2f - 1f) * noiseStrength;

        // Lopullinen kirkkaus perustuu nykyiseen dynaamiseen hehkuun + kohinaan
        float finalBrightness = Mathf.Clamp(currentGlowBrightness + noiseOffset, minBrightness, maxBrightness);
        eyeMaterial.SetFloat(GlowBrightnessProperty, finalBrightness);
    }

    IEnumerator BlinkRandomizerLoop()
    {
        while (true)
        {
            yield return blinkCheckInterval;
            if (!isBlinking && Random.value < blinkChance)
            {
                yield return StartCoroutine(BlinkSequence());
            }
        }
    }

    IEnumerator BlinkSequence()
    {
        isBlinking = true;
        float timer = 0f;

        // 1. Silmä menee kiinni (Hehku voi tässä samalla laskea minimiin)
        while (timer < closeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / closeDuration;
            eyeMaterial.SetFloat(BlinkProgressProperty, progress);

            currentGlowBrightness = Mathf.Lerp(currentGlowBrightness, minBrightness, progress);

            yield return null;
        }
        eyeMaterial.SetFloat(BlinkProgressProperty, 1f);
        currentGlowBrightness = minBrightness;

        // 2. Silmä pysyy kiinni
        yield return holdClosedWait;

        // 3. Silmä avautuu ja HEHKU LÄMÄHTÄÄ TÄYTEEN (maxBrightness)
        timer = 0f;
        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / openDuration;
            eyeMaterial.SetFloat(BlinkProgressProperty, 1f - progress);

            // Hehku nousee maksimiin sitä mukaa kun silmä aukeaa
            currentGlowBrightness = Mathf.Lerp(minBrightness, maxBrightness, progress);

            yield return null;
        }
        eyeMaterial.SetFloat(BlinkProgressProperty, 0f);
        currentGlowBrightness = maxBrightness;

        // Räpsäys on ohi, Update-funktio hoitaa hehkun hitaan himmentämisen takaisin minBrightness-tasolle
        isBlinking = false;
    }

    void OnDestroy()
    {
        if (eyeMaterial != null)
        {
            Destroy(eyeMaterial);
        }
    }
}

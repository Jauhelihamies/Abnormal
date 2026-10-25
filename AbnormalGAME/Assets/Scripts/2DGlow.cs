using UnityEngine;
using System.Collections;

public class AdvancedEyeBlinkShader : MonoBehaviour
{
    public SpriteRenderer eyeRenderer;
    [Range(0f, 10f)] public float minBrightness = 0.3f;
    public float maxBrightness = 1.0f;
    [Range(0f, 1f)] public float noiseStrength = 0.2f;
    public float noiseSpeed = 6f;
    public float glowFadeSpeed = 2f; 
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
        currentGlowBrightness = minBrightness;
        eyeMaterial.SetFloat(GlowBrightnessProperty, currentGlowBrightness);

        StartCoroutine(BlinkRandomizerLoop());
    }

    void Update()
    {
        if (eyeMaterial == null) return;
        if (!isBlinking && currentGlowBrightness > minBrightness)
        {
            currentGlowBrightness = Mathf.MoveTowards(currentGlowBrightness, minBrightness, Time.deltaTime * glowFadeSpeed);
        }
        float noiseSample = Mathf.PerlinNoise(Time.time * noiseSpeed, seed);
        float noiseOffset = (noiseSample * 2f - 1f) * noiseStrength;
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
        yield return holdClosedWait;
        timer = 0f;
        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / openDuration;
            eyeMaterial.SetFloat(BlinkProgressProperty, 1f - progress);
            currentGlowBrightness = Mathf.Lerp(minBrightness, maxBrightness, progress);

            yield return null;
        }
        eyeMaterial.SetFloat(BlinkProgressProperty, 0f);
        currentGlowBrightness = maxBrightness;
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

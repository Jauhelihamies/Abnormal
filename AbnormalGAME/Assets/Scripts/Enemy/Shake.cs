using UnityEngine;

public class OptimoiduObjectShaker : MonoBehaviour
{
    [Header("Tärinän asetukset")]
    [Tooltip("Kuinka voimakkaasti objekti tärisee")]
    public float shakeMagnitude = 0.05f;

    [Tooltip("Kuinka usein uusi tärinäpiste lasketaan (sekunneissa). Pienempi arvo = nopeampi tärinä.")]
    public float tickSpeed = 0.05f;

    [Header("Toiminnan ohjaus")]
    public bool isShaking = true;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private float tickTimer;

    void Start()
    {
        originalPosition = transform.localPosition;
        targetPosition = originalPosition;
    }

    void Update()
    {
        if (isShaking)
        {
            tickTimer += Time.deltaTime;

            // Suoritetaan tärinälogiikka vain, kun "tick" tapahtuu
            if (tickTimer >= tickSpeed)
            {
                tickTimer = 0f; // Nollataan ajastin

                // Lasketaan uusi kohdepiste vain tickin kohdalla
                Vector2 randomOffset = Random.insideUnitCircle * shakeMagnitude;
                targetPosition = originalPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
            }

            // Liikutetaan tasaisesti kohti kohdepistettä (Time.deltaTime pitää liikkeen sulavana)
            // Jaetaan tickSpeedillä, jotta liike ehtii perille ennen seuraavaa tickiä
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, (shakeMagnitude / tickSpeed) * Time.deltaTime);
        }
        else
        {
            // Jos tärinä loppuu, palautetaan objekti suoraan tai pehmeästi paikoilleen
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition, (shakeMagnitude / tickSpeed) * Time.deltaTime);
        }
    }

    public void StartShaking() => isShaking = true;
    public void StopShaking()
    {
        isShaking = false;
        targetPosition = originalPosition;
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class EnemyShotgun : MonoBehaviour
{
    [Header("Ammuksen asetukset")]
    public GameObject pelletPrefab;
    public GameObject shellPrefab;

    [Header("Laukauksen pisteet")]
    public Transform firePoint;
    public Transform shellEjectPoint;

    [Header("Haulikon tasapainotus")]
    public int pelletCount = 5;
    public float pelletSpeed = 12f;
    public float spreadAngle = 15f;

    [Header("Satunnainen tulitusnopeus")]
    public float minFireCooldown = 1.5f;
    public float maxFireCooldown = 4.0f;

    [Header("Turhautumisammunta")]
    [Range(0f, 100f)]
    public float blindFireChance = 20f;

    [Header("Hylsyn fysiikat")]
    public float shellEjectForceX = 3f;
    public float shellEjectForceY = 5f;
    public float shellTorque = 100f;

    [Header("Ääniefektit")]
    public AudioClip reloadBeforeShootSound;
    public AudioClip shootSound;
    public AudioClip shellSound;

    private Transform playerTransform;
    private EnemyAI enemyAI;
    private AudioSource audioSource;
    private float fireTimer;
    private float currentCooldown;
    private bool isPreparingShot = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        enemyAI = GetComponentInParent<EnemyAI>();
        SetRandomCooldown();

        // 🚀 KÄYNNISTETÄÄN ESILÄMMITYS (Prewarm)
        StartCoroutine(PrewarmShotgunRoutine());
    }

    IEnumerator PrewarmShotgunRoutine()
    {
        // 1. ÄÄNIEN ESILÄMMITYS
        // Tallennetaan alkuperäinen äänenvoimakkuus
        float originalVolume = audioSource.volume;
        // Laitetaan ääni nollaan, jotta soitto tapahtuu täysin taustalla näkymättömästi
        audioSource.volume = 0f;

        // Pakotetaan Unity lataamaan äänidatat muistiin soittamalla ne äänettömästi
        if (reloadBeforeShootSound != null) audioSource.PlayOneShot(reloadBeforeShootSound);
        if (shootSound != null) audioSource.PlayOneShot(shootSound);
        if (shellSound != null) audioSource.PlayOneShot(shellSound);

        // Odotetaan yksi frame, että fysiikat ja audiot rekisteröityvät
        yield return new WaitForEndOfFrame();


        audioSource.volume = originalVolume;


        Vector3 spawnFarAway = new Vector3(0, 0, -999f);

        if (pelletPrefab != null)
        {
            GameObject dummyPellet = Instantiate(pelletPrefab, spawnFarAway, Quaternion.identity);
            Destroy(dummyPellet);
        }

        if (shellPrefab != null)
        {
            GameObject dummyShell = Instantiate(shellPrefab, spawnFarAway, Quaternion.identity);
            Destroy(dummyShell);
        }


    }

    void Update()
    {
        if (playerTransform == null || enemyAI == null || isPreparingShot) return;

        fireTimer += Time.deltaTime;

        if (fireTimer >= currentCooldown)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= enemyAI.stoppingDistance && distanceToPlayer > enemyAI.kickActivationRange)
            {
                StartCoroutine(ShootSequenceRoutine(false));
            }
            else if (distanceToPlayer > enemyAI.stoppingDistance)
            {
                if (Random.Range(0f, 100f) <= blindFireChance)
                {
                    StartCoroutine(ShootSequenceRoutine(true));
                }
                else
                {
                    fireTimer = 0f;
                    SetRandomCooldown();
                }
            }
        }
    }

    IEnumerator ShootSequenceRoutine(bool isBlindFire)
    {
        isPreparingShot = true;
        fireTimer = 0f;

        if (audioSource != null && reloadBeforeShootSound != null)
        {
            audioSource.PlayOneShot(reloadBeforeShootSound);
        }

        yield return new WaitForSeconds(0.4f);

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        SpawnPellets(isBlindFire);

        yield return new WaitForSeconds(0.1f);

        EjectShell();

        isPreparingShot = false;
        SetRandomCooldown();
    }

    void SpawnPellets(bool isBlindFire)
    {
        if (firePoint == null || pelletPrefab == null) return;

        Vector2 shootDirection = (isBlindFire || playerTransform == null)
            ? (Vector2)(transform.lossyScale.x > 0 ? Vector2.right : Vector2.left)
            : (Vector2)(playerTransform.position - firePoint.position).normalized;

        float baseAngle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < pelletCount; i++)
        {
            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            float finalAngle = baseAngle + randomSpread;

            GameObject pellet = Instantiate(pelletPrefab, firePoint.position, Quaternion.Euler(0, 0, finalAngle));

            Rigidbody2D pelletRb = pellet.GetComponent<Rigidbody2D>();
            if (pelletRb != null)
            {
                Vector2 dir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
                pelletRb.linearVelocity = dir * pelletSpeed;
            }
        }
    }

    void EjectShell()
    {
        if (shellEjectPoint == null || shellPrefab == null) return;

        GameObject shell = Instantiate(shellPrefab, shellEjectPoint.position, shellEjectPoint.rotation);
        Rigidbody2D shellRb = shell.GetComponent<Rigidbody2D>();

        if (shellRb != null)
        {
            float directionModifier = transform.lossyScale.x > 0 ? -1f : 1f;
            Vector2 ejectForce = new Vector2(shellEjectForceX * directionModifier, shellEjectForceY);

            ejectForce.x += Random.Range(-0.5f, 0.5f);
            ejectForce.y += Random.Range(-0.5f, 0.5f);

            shellRb.AddForce(ejectForce, ForceMode2D.Impulse);
            shellRb.AddTorque(Random.Range(-shellTorque, shellTorque));
        }

        if (audioSource != null && shellSound != null)
        {
            audioSource.PlayOneShot(shellSound);
        }

        Destroy(shell, 5f);
    }

    void SetRandomCooldown()
    {
        currentCooldown = Random.Range(minFireCooldown, maxFireCooldown);
    }
}

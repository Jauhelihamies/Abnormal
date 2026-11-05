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

    [Header("Turhautumisammunta (Rangen ulkopuolella)")]
    [Range(0f, 100f)]
    [Tooltip("Kuinka suurella prosenttina vihollinen ampuu tyhjää rangen ulkopuolella cooldownin täyttyessä")]
    public float blindFireChance = 20f;

    [Header("Hylsyn fysiikat")]
    public float shellEjectForceX = 3f;
    public float shellEjectForceY = 5f;
    public float shellTorque = 100f;

    [Header("Ääniefektit")]
    public AudioClip reloadBeforeShootSound; // 1. Latausääni (klik-klak)
    public AudioClip shootSound;              // 2. Ampumisääni (pum!)
    public AudioClip shellSound;              // 3. Hylsyn kalahdus maahan

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
    }

    void Update()
    {
        if (playerTransform == null || enemyAI == null || isPreparingShot) return;

        fireTimer += Time.deltaTime;

        if (fireTimer >= currentCooldown)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Vaihtoehto A: Pelaaja on rangella -> ammutaan normaalisti
            if (distanceToPlayer <= enemyAI.stoppingDistance && distanceToPlayer > enemyAI.kickActivationRange)
            {
                StartCoroutine(ShootSequenceRoutine(false));
            }
            // Vaihtoehto B: Pelaaja on liian kaukana -> vihollinen voi turhautua ja ampua sokeasti ilmaan!
            else if (distanceToPlayer > enemyAI.stoppingDistance)
            {
                if (Random.Range(0f, 100f) <= blindFireChance)
                {
                    Debug.Log("<color=orange>Vihollinen turhautui ja ampuu sokeasti ilmaan!</color>");
                    StartCoroutine(ShootSequenceRoutine(true)); // true = sokkoammunta
                }
                else
                {
                    // Jos ei turhautunut, nollataan ja arvotaan uusi aika heti ettei koodi jumiudu
                    fireTimer = 0f;
                    SetRandomCooldown();
                }
            }
        }
    }

    // Ääni- ja ampumasekvenssi
    IEnumerator ShootSequenceRoutine(bool isBlindFire)
    {
        isPreparingShot = true;
        fireTimer = 0f;

        // 1. LATAUSÄÄNI ENNEN AMPUMISTA
        if (audioSource != null && reloadBeforeShootSound != null)
        {
            audioSource.PlayOneShot(reloadBeforeShootSound);
        }

        // Odotetaan hetki latauksen aikana ennen kuin pamahtaa (esim. 0.4 sekuntia)
        yield return new WaitForSeconds(0.4f);

        // 2. AMPUMISÄÄNI JA LUODIT
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        SpawnPellets(isBlindFire);

        // Odotetaan pikkuruinen hetki ennen kuin hylsy lentää ja kolahtaa (realistisempaa)
        yield return new WaitForSeconds(0.1f);

        // 3. HYLSY JA HYLSYN ÄÄNI
        EjectShell();

        isPreparingShot = false;
        SetRandomCooldown();
    }

    void SpawnPellets(bool isBlindFire)
    {
        if (firePoint == null || pelletPrefab == null) return;

        Vector2 shootDirection;

        if (isBlindFire || playerTransform == null)
        {
            // Sokkoammunnassa ammutaan suoraan eteenpäin rintamasuunnan mukaan
            shootDirection = transform.lossyScale.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            // Normaalisti tähdätään pelaajaan
            shootDirection = (playerTransform.position - firePoint.position).normalized;
        }

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

        // SOITETAAN HYLSYN ÄÄNI (Kalahdus maahan/ilmaan)
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

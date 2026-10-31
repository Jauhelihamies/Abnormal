using UnityEngine;

public class Pellet : MonoBehaviour
{
    [Header("Elinkaari")]
    [Tooltip("Kuinka monta sekuntia luoti lentää ennen kuin se katoaa automaattisesti")]
    public float lifeTime = 2f;

    void Start()
    {

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Jos luoti osuu pelaajaan
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(); 
            }

            Destroy(gameObject); 
        }

        else if (other.CompareTag("NONE") || other.CompareTag("Tombstone"))
        {
            Destroy(gameObject);
        }
    }
}

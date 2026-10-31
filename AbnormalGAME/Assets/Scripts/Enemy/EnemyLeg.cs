using UnityEngine;

public class EnemyKickTrigger : MonoBehaviour
{
    private EnemyAI enemyAI;

    void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyAI == null) return;

        // Osuminen pelaajaan
        if (other.CompareTag("Player"))
        {
            enemyAI.OnKickHitPlayer(other.gameObject);
        }
        // Osuminen hautakiveen (Varmista, että hautakiven objektilla on tagi "Tombstone")
        else if (other.CompareTag("Tombstone"))
        {
            enemyAI.OnKickHitTombstone(other.gameObject);
        }
    }
}

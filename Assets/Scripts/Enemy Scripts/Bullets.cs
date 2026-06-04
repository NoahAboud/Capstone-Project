using UnityEngine;

public class Bullets : MonoBehaviour
{
    public float lifetime = 5f;
    public int damage = 25;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("Bullet hit: " + other.name);

            }
        }

        Destroy(gameObject);
    }
}
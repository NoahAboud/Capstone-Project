using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;
    private bool isDead = false;

    public GameObject gameOverUI; 

    private void Start()
    {
        currentHealth = maxHealth;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        Time.timeScale = 0f; // Pause the game

        if (gameOverUI != null)
         gameOverUI.SetActive(true);

        //Optional: Disable player movement/shooting here instead of Destroying
        gameObject.SetActive(false); // Hide player instead of destroy
    }
}
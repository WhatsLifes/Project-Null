using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int Health = 100;
    public GameObject bloodyScreen;

    public void TakeDamage(int damage)
    {
        if (Health <= 0)
        {
            print("player died");
            PlayerDeath();
        }
        else
        {
            print("player took damage");
            // Take health from the player
            Health -= damage;

            // code for showing a bloody screen / etc
            StartCoroutine(bloodyScreenEffect());
        }
    }

    private void PlayerDeath()
    {
        // Stops player from doing anything
        GetComponent<SimpleFPS>().enabled = false;
        
        // Death animation
        print("player died");
    }

    // code for the bloody scrren / etc
    private IEnumerator bloodyScreenEffect()
    {
        if (bloodyScreen.activeInHierarchy == false)
        {
            bloodyScreen.SetActive(true);
        }

        var image = bloodyScreen.GetComponentInChildren<Image>();

        // Set the initial alpha value to 1 (fully visible).
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        float duration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the new alpha value using Lerp.
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Update the color with the new alpha value.
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;

            // Increment the elapsed time.
            elapsedTime += Time.deltaTime;

            yield return null;
            // Wait for the next frame.
        }

        if (bloodyScreen.activeInHierarchy)
        {
            bloodyScreen.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(other.gameObject.GetComponent<TestEnemy>().damage);
        }
    }
    
}

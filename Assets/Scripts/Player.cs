using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public int health = 100;
    public int totalCoins = 5;

    public TMP_Text scoreText;
    public TMP_Text healthText;
    public TMP_Text resultText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public Slider healthSlider;
    public AudioClip coinSound;

    int score = 0;
    int groundContacts = 0;      // Ground-пен қанша жерде тиіп тұр
    bool finished = false;       // жеңді немесе жеңілді
    bool isGrounded { get { return groundContacts > 0; } }

    Rigidbody2D rb;
    SpriteRenderer sr;
    Color startColor;

    void Start()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;

        if (scoreText != null) scoreText.text = "Ұпай: " + score;
        if (healthText != null) healthText.text = "Өмір: " + health;
        if (healthSlider != null) healthSlider.value = health;
    }

    void Update()
    {
        if (finished || Time.timeScale == 0f) return;

        float x = GameInput.Horizontal();
        transform.Translate(x * speed * Time.deltaTime, 0, 0);

        if (GameInput.JumpPressed() && isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Ground")) groundContacts++;

        if (c.gameObject.CompareTag("Enemy") && !finished)
        {
            health -= 20;
            if (health < 0) health = 0;
            Debug.Log("Өмір: " + health);

            if (healthText != null) healthText.text = "Өмір: " + health;
            if (healthSlider != null) healthSlider.value = health;
            StartCoroutine(Blink());

            if (health <= 0)
            {
                finished = true;
                gameOverPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Ground"))
            groundContacts = Mathf.Max(0, groundContacts - 1);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin") && !finished)
        {
            score++;
            Debug.Log("Ұпай: " + score);
            if (scoreText != null) scoreText.text = "Ұпай: " + score;

            // дыбыс камераның орнында ойналады, сонда 2D ойында да қатты естіледі
            if (coinSound != null && Camera.main != null)
                AudioSource.PlayClipAtPoint(coinSound, Camera.main.transform.position);

            Destroy(other.gameObject);

            if (score >= totalCoins)
                Win();
        }
    }

    void Win()
    {
        finished = true;

        float t = Time.timeSinceLevelLoad;                       // қанша секундта өттіңіз
        string key = "BestTime_" + SceneManager.GetActiveScene().name;   // әр деңгейге өз кілті
        float best = PlayerPrefs.GetFloat(key, 999f);            // бұрынғы рекорд
        if (t < best)                                            // жаңа рекорд па?
        {
            best = t;
            PlayerPrefs.SetFloat(key, best);
            PlayerPrefs.Save();
        }

        if (resultText != null)
        {
            resultText.text = "Уақыт: " + t.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + " с";
            resultText.text += "\nРекорд: " + best.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + " с";
        }

        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    IEnumerator Blink()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sr.color = startColor;
    }
}

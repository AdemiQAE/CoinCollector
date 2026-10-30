using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TMP_Text recordText;   // тек басты мәзірде толтырылады

    void Start()
    {
        if (recordText == null) return;   // басқа сахналарда өріс бос

        float best = PlayerPrefs.GetFloat("BestTime_Game", 0f);
        if (best > 0f)
            recordText.text = "Рекорд: " + best.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + " с";
        else
            recordText.text = "Рекорд жоқ";
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Шығу");
    }
}

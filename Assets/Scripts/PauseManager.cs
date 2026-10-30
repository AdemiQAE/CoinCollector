using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;

    void Update()
    {
        if (!GameInput.EscapePressed()) return;

        if (pausePanel.activeSelf)
        {
            Resume();
        }
        else if (Time.timeScale > 0f)   // Game Over / Жеңіс кезінде пауза қосылмайды
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}

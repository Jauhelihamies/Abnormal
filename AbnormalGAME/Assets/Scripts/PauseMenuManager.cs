using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Tarvitaan ohjaimen UI-valintoja varten

public class PauseManager : MonoBehaviour
{
    [Header("Valikon osat")]
    [SerializeField] private GameObject pauseMenuCanvas; // PAUSEMENU-hierarkiasta se visuaalinen osa
    [SerializeField] private GameObject firstSelectedButton; // Ensimmäinen valittava nappi ohjaimelle (esim. Continue)

    [Header("Pelaaja viittaus")]
    [SerializeField] private PlayerMovement playerMovement; // Vedä PLAYER-objekti tähän

    private bool isPaused = false;
    private float lastToggleTime = 0f;
    private const float toggleCooldown = 0.3f;

    void Start()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        // Jos pelaajaviittausta ei ole asetettu, yritetään etsiä se automaattisesti
        if (playerMovement == null)
        {
            playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        }
    }

    void Update()
    {
        bool pauseInputPressed = false;

        // 1. NÄPPÄIMISTÖ (ESC-näppäin)
        if (Keyboard.current != null && Keyboard.current.escapeKey.isPressed)
        {
            pauseInputPressed = true;
        }

        // 2. PELIOHJAIN (PS5 / Xbox)
        Gamepad currentGamepad = Gamepad.current;
        if (currentGamepad != null)
        {
            var psGamepad = currentGamepad as UnityEngine.InputSystem.DualShock.DualShockGamepad;

            if (currentGamepad.startButton.isPressed ||
                currentGamepad.buttonEast.isPressed ||
                (psGamepad != null && psGamepad.optionsButton.isPressed))
            {
                pauseInputPressed = true;
            }
        }

        // Vaihdetaan pelin tila, jos cooldown on kulunut
        if (pauseInputPressed && Time.unscaledTime > lastToggleTime + toggleCooldown)
        {
            lastToggleTime = Time.unscaledTime;

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pysäyttää fysiikat ja peliajan

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        // Estetään pelaajan liikkuminen valikon taustalla
        if (playerMovement != null)
            playerMovement.enabled = false;


        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Palauttaa normaalin pelinopeuden

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // Sallitaan pelaajan liikkuminen uudelleen
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);

    }
}

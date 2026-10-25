using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // UUSI: Tarvitaan ohjaimen valintoja varten
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI youDiedText;
    public GameObject restartButton;
    public GameObject mainMenuButton;

    [Header("Cinematic Settings")]
    public float textFadeDuration = 1.5f;
    public float buttonFadeDuration = 1.0f;
    public float textShakeAmount = 5f;
    public float textShakeSpeed = 40f;

    private Vector3 originalTextPosition;
    private bool shouldShake = false;
    private CanvasGroup restartGroup;
    private CanvasGroup mainMenuGroup;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (youDiedText != null) originalTextPosition = youDiedText.transform.localPosition;

        if (restartButton != null) restartGroup = restartButton.GetComponent<CanvasGroup>();
        if (mainMenuButton != null) mainMenuGroup = mainMenuButton.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (shouldShake && youDiedText != null)
        {
            float offsetX = (Mathf.PerlinNoise(Time.time * textShakeSpeed, 0f) * 2f - 1f) * textShakeAmount;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * textShakeSpeed) * 2f - 1f) * textShakeAmount;
            youDiedText.transform.localPosition = originalTextPosition + new Vector3(offsetX, offsetY, 0f);
        }
    }

    public void StartGameOverSequence()
    {
        StartCoroutine(ShowGameOverRoutine());
    }

    IEnumerator ShowGameOverRoutine()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (youDiedText != null)
            {
                Color origColor = youDiedText.color;
                youDiedText.color = new Color(origColor.r, origColor.g, origColor.b, 0f);
                youDiedText.gameObject.SetActive(true);
                shouldShake = true;

                float elapsed = 0f;
                Vector3 startScale = Vector3.one * 0.7f;

                while (elapsed < textFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float normalizedTime = elapsed / textFadeDuration;
                    youDiedText.color = new Color(origColor.r, origColor.g, origColor.b, normalizedTime);
                    youDiedText.transform.localScale = Vector3.Lerp(startScale, Vector3.one, normalizedTime);
                    yield return null;
                }
                youDiedText.color = origColor;
                youDiedText.transform.localScale = Vector3.one;
            }

            yield return new WaitForSeconds(0.5f);

            if (restartButton != null) restartButton.SetActive(true);
            if (mainMenuButton != null) mainMenuButton.SetActive(true);
            if (restartGroup != null) restartGroup.alpha = 0f;
            if (mainMenuGroup != null) mainMenuGroup.alpha = 0f;

            float buttonElapsed = 0f;
            while (buttonElapsed < buttonFadeDuration)
            {
                buttonElapsed += Time.deltaTime;
                float normalizedTime = buttonElapsed / buttonFadeDuration;
                if (restartGroup != null) restartGroup.alpha = normalizedTime;
                if (mainMenuGroup != null) mainMenuGroup.alpha = normalizedTime;
                yield return null;
            }

            if (restartGroup != null) restartGroup.alpha = 1f;
            if (mainMenuGroup != null) mainMenuGroup.alpha = 1f;

            if (EventSystem.current != null && restartButton != null)
            {
                EventSystem.current.SetSelectedGameObject(restartButton);
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}

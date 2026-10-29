using UnityEngine;
using UnityEngine.InputSystem;

public class InteractiveSpriteController : MonoBehaviour
{
    [Header("Tatit (Sprite objektit)")]
    [SerializeField] private Transform leftStick;

    [SerializeField] private float stickMoveRange = 0.5f; 
    [SerializeField] private float snapBackSpeed = 15f; 

    [Header("Painikkeet")]
    [SerializeField] private SpriteRenderer jumpButtonRenderer;
    [SerializeField] private SpriteRenderer pauseButtonRenderer;
    [SerializeField] private SpriteRenderer healButtonRenderer; 

    [Header("Painallus Asetukset")]
    [SerializeField] private Vector3 pressedScaleModifier = new Vector3(0.9f, 0.9f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);


    private Vector3 leftStickStartPos;
    private Vector3 jumpOriginalScale;
    private Color jumpOriginalColor;
    private Vector3 pauseOriginalScale;
    private Color pauseOriginalColor;
    private Vector3 healOriginalScale; // UUSI
    private Color healOriginalColor; // UUSI

    void Start()
    {

        if (leftStick != null) leftStickStartPos = leftStick.localPosition;


        // Tallennetaan painikkeiden alkuperäiset tilat talteen
        if (jumpButtonRenderer != null)
        {
            jumpOriginalScale = jumpButtonRenderer.transform.localScale;
            jumpOriginalColor = jumpButtonRenderer.color;
        }
        if (pauseButtonRenderer != null)
        {
            pauseOriginalScale = pauseButtonRenderer.transform.localScale;
            pauseOriginalColor = pauseButtonRenderer.color;
        }
        if (healButtonRenderer != null) // UUSI
        {
            healOriginalScale = healButtonRenderer.transform.localScale;
            healOriginalColor = healButtonRenderer.color;
        }
    }

    void Update()
    {
        Gamepad currentGamepad = Gamepad.current;

        if (currentGamepad == null)
        {
            ResetInputsToStart();
            return;
        }

        // --- 1. TATTIN LIIKE JA LAPSAHDUS ---
        Vector2 leftInput = currentGamepad.leftStick.ReadValue();


        if (leftStick != null)
        {
            if (leftInput.magnitude > 0.05f)
            {
                Vector3 targetPos = leftStickStartPos + new Vector3(leftInput.x, leftInput.y, 0) * stickMoveRange;
                leftStick.localPosition = targetPos;
            }
            else
            {
                leftStick.localPosition = Vector3.Lerp(leftStick.localPosition, leftStickStartPos, Time.deltaTime * snapBackSpeed);
            }
        }



        // --- 2. JUMP NAPPI (buttonSouth) ---
        if (jumpButtonRenderer != null)
        {
            if (currentGamepad.buttonSouth.isPressed)
            {
                jumpButtonRenderer.transform.localScale = Vector3.Scale(jumpOriginalScale, pressedScaleModifier);
                jumpButtonRenderer.color = pressedColor;
            }
            else
            {
                jumpButtonRenderer.transform.localScale = jumpOriginalScale;
                jumpButtonRenderer.color = jumpOriginalColor;
            }
        }

        // --- 3. PAUSE NAPPI (Options / Start) ---
        if (pauseButtonRenderer != null)
        {
            var psGamepad = currentGamepad as UnityEngine.InputSystem.DualShock.DualShockGamepad;

            bool isPausePressed = currentGamepad.startButton.isPressed ||
                                 currentGamepad.buttonEast.isPressed ||
                                 (psGamepad != null && psGamepad.optionsButton.isPressed);

            if (isPausePressed)
            {
                pauseButtonRenderer.transform.localScale = Vector3.Scale(pauseOriginalScale, pressedScaleModifier);
                pauseButtonRenderer.color = pressedColor;
            }
            else
            {
                pauseButtonRenderer.transform.localScale = pauseOriginalScale;
                pauseButtonRenderer.color = pauseOriginalColor;
            }
        }


        // --- 4. HEAL NAPPI (buttonWest / Neliö / X) ---
        if (healButtonRenderer != null)
        {
            if (currentGamepad.buttonWest.isPressed)
            {
                healButtonRenderer.transform.localScale = Vector3.Scale(healOriginalScale, pressedScaleModifier);
                healButtonRenderer.color = pressedColor;
            }
            else
            {
                healButtonRenderer.transform.localScale = healOriginalScale;
                healButtonRenderer.color = healOriginalColor;
            }
        }
    }

    private void ResetInputsToStart()
    {
        if (leftStick != null) leftStick.localPosition = leftStickStartPos;


        if (jumpButtonRenderer != null)
        {
            jumpButtonRenderer.transform.localScale = jumpOriginalScale;
            jumpButtonRenderer.color = jumpOriginalColor;
        }
        if (pauseButtonRenderer != null)
        {
            pauseButtonRenderer.transform.localScale = pauseOriginalScale;
            pauseButtonRenderer.color = pauseOriginalColor;
        }
        if (healButtonRenderer != null)
        {
            healButtonRenderer.transform.localScale = healOriginalScale;
            healButtonRenderer.color = healOriginalColor;
        }
    }
}

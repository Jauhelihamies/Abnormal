using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    private float horizontalInput;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;
    public float RotateSpeed = 1f;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Jos skripti on jostain syystä deaktivoitu, älä lue syötteitä
        if (!enabled) return;

        horizontalInput = 0f;

        // Näppäimistö
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
        }

        // PS5 Ohjain
        if (Gamepad.current != null)
        {
            float gamepadX = Gamepad.current.leftStick.x.ReadValue();
            float dpadX = Gamepad.current.dpad.x.ReadValue();

            if (Mathf.Abs(gamepadX) > 0.1f) horizontalInput = gamepadX;
            else if (Mathf.Abs(dpadX) > 0.1f) horizontalInput = dpadX;
        }

        // Maatunnistus
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(transform.position - new Vector3(0, 0.5f, 0), checkRadius, groundLayer);
        }

        // Hyppy
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) jumpPressed = true;

        if (isGrounded && jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        // Varmistetaan fysiikkaliikkeen pysäytys, jos skripti ei ole päällä
        if (!enabled) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            transform.Rotate(0, 0, -horizontalInput * moveSpeed * RotateSpeed);
        }
    }

    // UUSI: Kun tämä skripti otetaan pois päältä (esim. parannus alkaa), pysäytetään hahmo seinään
    void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}

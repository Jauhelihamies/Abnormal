using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
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
    public AudioClip jumpSound;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isGrounded;
    private Collider2D playerCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponent<Collider2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!enabled) return;

        horizontalInput = 0f;

        // --- NÄPPÄIMISTÖ ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
        }

        Gamepad activeGamepad = Gamepad.current;
        if (activeGamepad != null)
        {
            // Luetaan tatin ja d-padin x-akselit
            float gamepadX = activeGamepad.leftStick.x.ReadValue();
            float dpadX = activeGamepad.dpad.x.ReadValue();


            if (Mathf.Abs(gamepadX) > 0.1f)
            {
                horizontalInput = gamepadX;
            }
            else if (Mathf.Abs(dpadX) > 0.1f)
            {
                horizontalInput = dpadX;
            }
        }


        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(transform.position - new Vector3(0, 0.5f, 0), checkRadius, groundLayer);
        }

        // HYPPYINPUT
        bool jumpPressed = false;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;


        if (activeGamepad != null && activeGamepad.buttonSouth.wasPressedThisFrame)
            jumpPressed = true;


        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }
    }

    void FixedUpdate()
    {
        if (!enabled) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            transform.Rotate(0, 0, -horizontalInput * moveSpeed * RotateSpeed);
        }
    }

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

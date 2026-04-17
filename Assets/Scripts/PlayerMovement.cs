using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // 🔥 Manual respawn position (set in inspector or updated by portals)
    public Vector2 respawnPosition;

    private bool levelComplete = false;
    public GameObject winText;
    public string ExitSceneName = "ExitScene";

    // ---- External launch override (for trampoline etc.) ----
    private bool externalLaunchActive = false;
    private float externalLaunchEndTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // If respawnPosition not set in inspector, default to player's start position
        if (respawnPosition == Vector2.zero)
            respawnPosition = new Vector2(transform.position.x, transform.position.y);
    }

    // Call this when something (like trampoline) launches the player.
    // During the lock window, normal movement/jump code won't overwrite velocity.
    public void ApplyExternalLaunch(Vector2 newVelocity, float lockDuration)
    {
        rb.linearVelocity = newVelocity;
        externalLaunchActive = true;
        externalLaunchEndTime = Time.time + lockDuration;
    }

    void Update()
    {
        if (levelComplete) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        // If we are being launched by trampoline etc., ignore normal jump
        if (!externalLaunchActive && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        if (levelComplete) return;

        // While external launch is active, do NOT overwrite velocity.
        if (externalLaunchActive)
        {
            if (Time.time >= externalLaunchEndTime)
            {
                externalLaunchActive = false;
            }
            else
            {
                return;
            }
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void LateUpdate()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
    }

    // -------------------------
    // New minimal API for portals:
    // Teleport player to the given 2D target (next-stage entry)
    // and update the player's respawnPosition to that same entry point.
    // -------------------------
    public void TeleportToStage(Vector2 entryPosition2D)
    {
        Vector3 target = new Vector3(entryPosition2D.x, entryPosition2D.y, transform.position.z);

        // teleport via Rigidbody2D for physics correctness
        transform.position = target;
        if (rb != null)
        {
            rb.position = target;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // update manual respawn to the new stage entry
        respawnPosition = entryPosition2D;
    }
    // -------------------------

    void Respawn()
    {
        Vector3 newPos = new Vector3(respawnPosition.x, respawnPosition.y, transform.position.z);

        transform.position = newPos;
        if (rb != null)
        {
            rb.position = newPos;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spike"))
        {
            Respawn();
            return;
        }

        if (other.CompareTag("Exit"))
        {
            LetterInventory li = FindObjectOfType<LetterInventory>();
            if (li != null)
            {
                if (GameState.Instance == null)
                {
                    var go = new GameObject("GameState");
                    go.AddComponent<GameState>();
                }
                GameState.Instance.collectedLettersSnapshot = li.GetCollectedLetters();
            }

            SceneManager.LoadScene(ExitSceneName);
            return;
        }

        // compatibility: if a plain checkpoint object with tag "Checkpoint" exists,
        // use its transform position as respawn (unchanged behavior)
        if (other.CompareTag("Checkpoint"))
        {
            respawnPosition = new Vector2(other.transform.position.x, other.transform.position.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Guard"))
        {
            Respawn();
        }
    }
}
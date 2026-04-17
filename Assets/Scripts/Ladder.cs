using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LadderClimb : MonoBehaviour
{
    [Header("Climb")]
    public float climbSpeed = 3f;
    public bool lockToLadderCenterX = true;

    [Header("Exit / Behavior")]
    public bool autoAttachOnEnter = true;   // if false, requires Up/Down input to start climbing
    public bool holdStillWhenNoInput = true; // if true, player "hangs" on ladder when no input

    [Header("Top Exit")]
    public float topExitPopUp = 0.25f;   // how much to pop above edge
    public float topExitPopForward = 0.25f; // optional push onto platform (x direction)
    public bool popToRight = true;       // set depending on which side platform is

    private Rigidbody2D rb;
    private float defaultGravity;

    private bool inLadderZone = false;
    private bool isClimbing = false;
    private float ladderCenterX = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        float v = Input.GetAxisRaw("Vertical");
        bool wantsClimb = Mathf.Abs(v) > 0.01f;

        // If we don't auto-attach, start climbing only when player presses up/down while in ladder
        if (!autoAttachOnEnter && inLadderZone && wantsClimb)
            StartClimbing();
    }

    void FixedUpdate()
    {
        if (!isClimbing) return;

        float v = Input.GetAxisRaw("Vertical");

        // Vertical climb movement
        rb.linearVelocity = new Vector2(0f, v * climbSpeed);

        // If no input, hang still (prevents sliding/falling)
        if (holdStillWhenNoInput && Mathf.Abs(v) < 0.01f)
            rb.linearVelocity = Vector2.zero;

        // Lock X to ladder center for clean climb
        if (lockToLadderCenterX)
            rb.position = new Vector2(ladderCenterX, rb.position.y);
    }

    void StartClimbing()
    {
        if (isClimbing) return;

        isClimbing = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    void StopClimbing()
    {
        if (!isClimbing) return;

        isClimbing = false;
        rb.gravityScale = defaultGravity;
        // keep current rb.velocity so player can step off naturally
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        /*if (other.CompareTag("LadderTop"))
        {
            Debug.Log("Hit ladder top trigger");
            StopClimbing();

            float dir = popToRight ? 1f : -1f;
            Vector2 p = rb.position;

            rb.position = new Vector2(
                p.x + dir * topExitPopForward,
                p.y + topExitPopUp
            );

            rb.linearVelocity = new Vector2(0f, -0.5f);
            return;
        }*/

        // THEN handle ladder
        if (other.CompareTag("Ladder"))
        {
            inLadderZone = true;
            ladderCenterX = other.bounds.center.x;

            if (autoAttachOnEnter)
                StartClimbing();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Ladder")) return;

        inLadderZone = false;

        // Leaving ladder always stops climbing
        StopClimbing();
    }
}
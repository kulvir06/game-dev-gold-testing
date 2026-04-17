using UnityEngine;

// Attach this to the SAME object that has the TRIGGER collider (TrampolineTrigger).
public class TrampolineLauncher2D : MonoBehaviour
{
    [Header("Center Snap")]
    public bool snapPlayerToCenter = true;

    [Tooltip("How fast to pull player to center (higher = faster).")]
    public float snapSpeed = 25f;

    [Tooltip("Stop snapping when player is this close to center (in units).")]
    public float snapDeadZone = 0.02f;

    [Tooltip("Optional: freeze horizontal velocity while snapping.")]
    public bool zeroPlayerXVelocityWhileSnapping = true;

    [Tooltip("If empty, we will auto-use the Collider2D on this object.")]
    public Collider2D triggerCollider;

    [Header("Direction Meter (visual swing)")]
    public float minAngleDeg = 30f;
    public float maxAngleDeg = 150f;
    public float oscillationSpeed = 0.6f;

    [Header("Launch")]
    public float launchSpeed = 14f;

    [Tooltip("Degrees to rotate the needle so its visual points along the launch axis.")]
    public float needleAngleOffsetDeg = 0f;

    [Tooltip("How long to prevent PlayerMovement from overriding launch velocity.")]
    public float movementLockDuration = 0.75f;

    public float launchCooldown = 0.35f;

    [Header("Needle Visual")]
    public LineRenderer needleLine;
    public float needleLength = 0.8f;
    public Vector2 needleLocalOffset = new Vector2(0f, 1.0f);

    [Header("Needle Transform (authoritative for launch)")]
    [Tooltip("Drag your DirectionNeedle transform here. Launch will follow this transform.")]
    public Transform directionNeedle;

    [Tooltip("If your needle points UP at 0 rotation, enable this. If it points RIGHT at 0 rotation, keep it off.")]
    public bool needleUsesUpAxis = false;

    [Header("Robustness")]
    [Tooltip("Keeps trampoline active briefly even if physics pushes player out of trigger for a moment.")]
    public float insideGraceTime = 0.15f;

    [Tooltip("How long to disable snapping after launch so it doesn't kill X velocity.")]
    public float snapDisableAfterLaunchTime = 0.25f;

    private Rigidbody2D playerRbInside;
    private PlayerMovement playerMovementInside;

    private float currentAngleDeg;
    private float nextAllowedLaunchTime = 0f;

    // Last time we confirmed the player was inside (via Enter/Stay)
    private float lastInsideTime = -999f;

    // Prevent snap right after launch
    private float snapDisabledUntilTime = 0f;

    void Awake()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        if (needleLine != null)
            needleLine.useWorldSpace = false;
    }

    private bool PlayerConsideredInside()
    {
        if (playerRbInside == null) return false;
        return Time.time <= lastInsideTime + insideGraceTime;
    }

    private float GetCenterX()
    {
        if (triggerCollider != null)
            return triggerCollider.bounds.center.x;

        return transform.position.x;
    }

    void Update()
    {
        // Visual swing angle
        float t = Mathf.PingPong(Time.time * oscillationSpeed, 1f);
        currentAngleDeg = Mathf.Lerp(minAngleDeg, maxAngleDeg, t);

        // Rotate the needle transform so launch and visuals are synced
        if (directionNeedle != null)
        {
            directionNeedle.localRotation = Quaternion.Euler(0f, 0f, currentAngleDeg + needleAngleOffsetDeg);
        }

        UpdateNeedleVisual();

        if (!PlayerConsideredInside()) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryLaunch();
        }
    }

    void FixedUpdate()
    {
        if (!snapPlayerToCenter) return;
        if (!PlayerConsideredInside()) return;

        // IMPORTANT: don't interfere with the player immediately after launch
        if (Time.time < snapDisabledUntilTime) return;

        // If player is trying to move, let them walk off the trampoline
        float move = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(move) > 0.1f) return;

        float centerX = GetCenterX();

        float px = playerRbInside.position.x;
        float dx = centerX - px;

        if (Mathf.Abs(dx) <= snapDeadZone) return;

        float newX = Mathf.MoveTowards(px, centerX, snapSpeed * Time.fixedDeltaTime);
        playerRbInside.MovePosition(new Vector2(newX, playerRbInside.position.y));

        if (zeroPlayerXVelocityWhileSnapping)
        {
            // Only safe to do this when not in the post-launch window
            playerRbInside.linearVelocity = new Vector2(0f, playerRbInside.linearVelocity.y);
        }
    }

    // Fallback only (if directionNeedle isn't assigned)
    private Vector2 ComputeVisualDirectionFromAngle()
    {
        float ang = currentAngleDeg;

        // Avoid perfectly flat/downward launches by clamping the angle
        float minSafeAngle = 15f;
        if (ang < minSafeAngle) ang = minSafeAngle;

        float rad = ang * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        dir.Normalize();
        return dir;
    }

    private Vector2 ComputeLaunchDirection()
    {
        Vector2 dir;

        // Authoritative: follow the needle transform
        if (directionNeedle != null)
        {
            dir = needleUsesUpAxis ? (Vector2)directionNeedle.up : (Vector2)directionNeedle.right;
        }
        else
        {
            dir = ComputeVisualDirectionFromAngle();
        }

        dir.Normalize();
        return dir;
    }

    private void TryLaunch()
    {
        if (Time.time < nextAllowedLaunchTime) return;

        Vector2 dir = ComputeLaunchDirection();
        Vector2 launchVel = dir * launchSpeed;

        // Disable snapping so it doesn't kill the horizontal component of the launch
        snapDisabledUntilTime = Time.time + snapDisableAfterLaunchTime;

        // Stop being considered "inside" immediately after launch to prevent any lingering snap
        lastInsideTime = -999f;

        if (playerMovementInside != null)
        {
            playerMovementInside.ApplyExternalLaunch(launchVel, movementLockDuration);
        }
        else if (playerRbInside != null)
        {
            playerRbInside.linearVelocity = launchVel;
        }

        nextAllowedLaunchTime = Time.time + launchCooldown;
    }

    private void UpdateNeedleVisual()
    {
        if (needleLine == null) return;

        // The line is drawn in LOCAL space; transform rotation handles the swing.
        needleLine.useWorldSpace = false;

        Vector3 start = new Vector3(needleLocalOffset.x, needleLocalOffset.y, 0f);
        Vector3 localAxis = needleUsesUpAxis ? Vector3.up : Vector3.right;
        Vector3 end = start + localAxis * needleLength;

        needleLine.positionCount = 2;
        needleLine.SetPosition(0, start);
        needleLine.SetPosition(1, end);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerRbInside = other.attachedRigidbody;
        playerMovementInside = other.GetComponent<PlayerMovement>();
        lastInsideTime = Time.time;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        lastInsideTime = Time.time;

        if (playerRbInside == null) playerRbInside = other.attachedRigidbody;
        if (playerMovementInside == null) playerMovementInside = other.GetComponent<PlayerMovement>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        lastInsideTime = Time.time;
    }
}
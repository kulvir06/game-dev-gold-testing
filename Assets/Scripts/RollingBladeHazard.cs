using UnityEngine;

// Attach to RollingBlade.
// Set pointA and pointB in inspector.
public class RollingBladeHazard : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 360f; // degrees per second

    [Header("Patrol Between Two Points")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;       // units per second
    public float waitTimeAtEnds = 0f;  // seconds to pause at A/B

    private Transform currentTarget;
    private float waitTimer = 0f;

    // If Rigidbody2D exists, we will move using MovePosition (recommended).
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // If you add Rigidbody2D, set it Kinematic in inspector.
        // If rb is null, we'll move via transform.position.

        // Start by moving toward B (or A if B missing)
        if (pointB != null) currentTarget = pointB;
        else currentTarget = pointA;
    }

    void Update()
    {
        // Rotate continuously (visual)
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (pointA == null || pointB == null)
        {
            // No patrol points assigned; do nothing.
            return;
        }

        // Handle pause at ends
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 targetPos = currentTarget.position;

        // Move toward current target
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.fixedDeltaTime);

        if (rb != null)
            rb.MovePosition(newPos);
        else
            transform.position = newPos;

        // If we reached the target, switch direction and optionally wait
        if (Vector2.Distance(newPos, targetPos) < 0.01f)
        {
            if (currentTarget == pointA) currentTarget = pointB;
            else currentTarget = pointA;

            if (waitTimeAtEnds > 0f)
                waitTimer = waitTimeAtEnds;
        }
    }

    // Optional: auto-create points if missing (helps prefab workflow)
    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.08f);
            Gizmos.DrawSphere(pointB.position, 0.08f);
        }
    }
}
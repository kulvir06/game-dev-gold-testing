using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class CheckpointPortal : MonoBehaviour
{
    // Hardcoded 2D entry position for the next stage. Set this in Inspector per-prefab.
    public Vector2 entryPosition;
    public Camera camera;
    public float cameraPosition = 9f; // Distance to move the camera up from the entry position

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Robust: find PlayerMovement on the triggered object or its parents.
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;

        // Teleport player to next-stage entry and update player's respawn to this entry.
        pm.TeleportToStage(entryPosition);
        StopAllCoroutines();
        StartCoroutine(MoveCamera(cameraPosition));
        // Add 9 to Y to keep the camera above the center of the screen.
        //camera.transform.position = new Vector3(camera.transform.position.x, entryPosition.y + 9f, camera.transform.position.z);
    }

    private IEnumerator MoveCamera(float targetY)
    {
        float speed = 4f;

        while (Mathf.Abs(camera.transform.position.y - targetY) > 0.01f)
        {
            float newY = Mathf.Lerp(
                camera.transform.position.y,
                targetY,
                speed * Time.deltaTime
            );

            camera.transform.position = new Vector3(
                camera.transform.position.x,
                newY,
                camera.transform.position.z
            );

            yield return null;
        }

        // Final snap
        camera.transform.position = new Vector3(
            camera.transform.position.x,
            targetY,
            camera.transform.position.z
        );
    }
}
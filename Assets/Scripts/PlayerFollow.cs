using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform

    [Header("Dead Zone")]
    public float upperThreshold = 3f; // Horizontal dead zone threshold
    public float lowerThreshold = -2f; // Horizontal dead zone threshold

    public float verticalOffset = 5f; // Vertical offset to keep the camera above the center of the screen

    [Header("Camera Follow Speed")]
    public float followSpeed = 5f; // Speed at which the camera follows the player

    public float minY = -8f;

    private Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    /*void LateUpdate()
    {
        if (!player || !cam) return;

        float camY = transform.position.y;
        float playerY = player.position.y + verticalOffset; // Apply vertical offset to player's Y position

        float difference = playerY - camY;
        float targetY = camY;

        if (difference > upperThreshold)
            targetY = playerY - upperThreshold;
        else if (difference < lowerThreshold)
            targetY = playerY - lowerThreshold;

        Vector3 newPos = transform.position;
        newPos.y = Mathf.Lerp(camY, targetY, followSpeed * Time.deltaTime);

        float minCamCenterY = minY + cam.orthographicSize;
        newPos.y = Mathf.Max(newPos.y, minCamCenterY);


        transform.position = newPos;
    }*/
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnergyMeter : MonoBehaviour
{
    [Header("UI")]
    public Slider energyBar;

    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float drainPerUnit = 0.2f;       // lower = drains slower
    public float verticalWeight = 0.4f;     // jump/ladder counts, but less than horizontal

    [Header("Filtering")]
    public float minMovementThreshold = 0.01f;
    public float axisDeadZone = 0.1f;

    private float energy;
    private Vector2 lastPos;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        energy = maxEnergy; 
        lastPos = rb ? rb.position : (Vector2)transform.position;

        if (energyBar != null)
        {
            energyBar.minValue = 0f;
            energyBar.maxValue = 1f;
            energyBar.value = 1f; 
        }
    }

    void LateUpdate()
    {
        Vector2 currentPos = rb ? rb.position : (Vector2)transform.position;

        float dx = Mathf.Abs(currentPos.x - lastPos.x);
        float dy = Mathf.Abs(currentPos.y - lastPos.y);
        float movement = dx + dy * verticalWeight;

        // Intent: axis + key fallback
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        bool axisIntent = Mathf.Abs(hAxis) > axisDeadZone || Mathf.Abs(vAxis) > axisDeadZone;

        bool keyIntent =
            Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);

        bool jumpIntent = Input.GetKey(KeyCode.Space);

        bool hasIntent = axisIntent || keyIntent || jumpIntent;
        bool actuallyMoved = movement > minMovementThreshold;

        if (hasIntent && actuallyMoved)
        {
            energy -= movement * drainPerUnit;  
            energy = Mathf.Clamp(energy, 0f, maxEnergy);

            if (energyBar != null)
                energyBar.value = energy / maxEnergy;

            if (energy <= 0f)
                GameOver();
        }

        lastPos = currentPos;
    }

    void GameOver()
    {
        Debug.Log("Energy depleted! Game Over.");
        // TODO: restart level / show UI
        SceneManager.LoadScene("RetryScene");
    }
}
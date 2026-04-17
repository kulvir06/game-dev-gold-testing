using TMPro;
using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp;
    [SerializeField] private char letter = 'A';
    [SerializeField] private int checkpointIndex;

    private void Awake()
    {
        tmp = GetComponentInChildren<TMP_Text>(true);
    }

    public void SetLetter(char c)
    {
        letter = char.ToUpper(c);

        if (tmp == null)
            tmp = GetComponentInChildren<TMP_Text>(true);

        if (tmp != null)
            tmp.text = letter.ToString();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"PICKUP: '{letter}'  int={(int)letter}  obj={gameObject.name}");

        // Only the player should collect
        if (!other.CompareTag("Player")) return;

        // Add to inventory (UI updates)
        if (LetterInventory.Instance != null)
            LetterInventory.Instance.AddLetter(letter, checkpointIndex);

        // Remove letter tile from scene
        Destroy(gameObject);
    }
}
using TMPro;
using UnityEngine;

public class LetterDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private char letter = 'A';
    [SerializeField] private int checkpointIndex;   // NEW

    void Start()
    {
        letterText.text = letter.ToString();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
    if (other.CompareTag("Player"))
    {
        Debug.Log("Inventory instance: " + CollectedLettersInventory.Instance);

        if (CollectedLettersInventory.Instance != null)
        {
            CollectedLettersInventory.Instance.AddLetter(letter, checkpointIndex);
        }
        else
        {
            Debug.LogError("CollectedLettersInventory.Instance is NULL!");
        }

        Destroy(gameObject);
    }
    }
}
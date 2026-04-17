using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterInventory : MonoBehaviour
{
    public static LetterInventory Instance;

    private string targetWord = "FIRE";

    [SerializeField] private GameObject letterSlotPrefab;
    [SerializeField] private Transform lettersContainer;

    private System.Collections.Generic.List<char> collectedLetters = new System.Collections.Generic.List<char>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            return;

        Instance = this;
    }

    public void AddLetter(char c, int checkpointIndex)
    {
    char upper = char.ToUpper(c);
    collectedLetters.Add(upper);

    GameObject slot = Instantiate(letterSlotPrefab, lettersContainer);

    TMP_Text text = slot.GetComponentInChildren<TMP_Text>();
    Image background = slot.GetComponent<Image>();

    text.text = upper.ToString();

    int correctIndex = targetWord.IndexOf(upper);

    if (correctIndex == checkpointIndex)
        background.color = Color.green;
    else
        background.color = Color.yellow;
    }

    public string GetCollectedLetters()
    {
    return new string(collectedLetters.ToArray());
    }
}
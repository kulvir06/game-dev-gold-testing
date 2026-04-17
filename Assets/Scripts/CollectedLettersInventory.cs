using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CollectedLettersInventory : MonoBehaviour
{
    public static CollectedLettersInventory Instance;

    private string targetWord = "FIRE";

    [SerializeField] private GameObject letterSlotPrefab;
    [SerializeField] private Transform lettersContainer;

    private List<char> collectedLetters = new List<char>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddLetter(char c, int checkpointIndex)
    {
        GameObject slot = Instantiate(letterSlotPrefab, lettersContainer);

        TMP_Text text = slot.GetComponentInChildren<TMP_Text>();
        Image background = slot.GetComponent<Image>();

        char upperLetter = char.ToUpper(c);
        text.text = upperLetter.ToString();

        int correctIndex = targetWord.IndexOf(upperLetter);

        if (correctIndex == checkpointIndex)
            background.color = Color.green;
        else
            background.color = Color.yellow;

        collectedLetters.Add(c);

        if (collectedLetters.Count >= targetWord.Length)
        {
            string finalString = new string(collectedLetters.ToArray());

            // Store word globally
            if (WordData.Instance == null)
            {
                GameObject go = new GameObject("WordData");
                go.AddComponent<WordData>();
            }

            WordData.Instance.collectedWord = finalString;

            // Load puzzle scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("WordFormationScene");
        }
    }

    public void ClearLetters()
    {
        collectedLetters.Clear();
        RefreshUI();
    }

    void RefreshUI()
    {
        foreach (Transform child in lettersContainer)
            Destroy(child.gameObject);

        foreach (char c in collectedLetters)
        {
            GameObject slot = Instantiate(letterSlotPrefab, lettersContainer);
            TMP_Text text = slot.GetComponentInChildren<TMP_Text>();
            text.text = c.ToString();
        }
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class WordGameManager : MonoBehaviour
{
    public static WordGameManager Instance;

    public Transform slotsParent;
    public GameObject slotPrefab;
    public TMP_Text timerText;

    public string targetWord = "FIRE";
    public float timeRemaining = 10f;
    private List<WordSlot> slots = new List<WordSlot>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        string word = WordData.Instance.collectedWord;

        foreach (char c in word)
        {
            GameObject go = Instantiate(slotPrefab, slotsParent);
            WordSlot slot = go.GetComponent<WordSlot>();
            slot.Setup(c);
            slots.Add(slot);
        }

        CheckWord();
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        while (timeRemaining > 0)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
            timeRemaining -= Time.deltaTime;
            yield return null;

            if (IsCorrect())
            {
                SceneManager.LoadScene("VictoryScene");
                yield break;
            }
        }

        SceneManager.LoadScene("RetryScene");
    }

    public void CheckWord()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].GetLetter().ToString() == targetWord[i].ToString())
                slots[i].SetGreen();
            else
                slots[i].SetYellow();
        }
    }

    bool IsCorrect()
    {
        string current = "";
        foreach (var s in slots)
            current += s.GetLetter();

        return current == targetWord;
    }
}
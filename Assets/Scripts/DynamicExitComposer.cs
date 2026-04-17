using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class DynamicExitComposer : MonoBehaviour
{

    public int slotCount = 5;


    public Button slotButtonPrefab;


    public Transform slotsParent;


    public Button letterButtonPrefab;


    public Transform poolParent;


    public Button checkButton;
    public Button clearButton;

    [Tooltip("Assign a TMP_Text UI element to display the result message.")]
    public TMP_Text resultText;

    public float poolDimAlpha = 0.45f;

    public GameObject resultOverlay;  
    public TMP_Text resultLabel;       

    // runtime structures
    private List<Button> slotButtons = new List<Button>();
    private List<TMP_Text> slotTexts = new List<TMP_Text>();
    private PoolEntry[] slotAssigned; // length = slotCount

    private List<PoolEntry> poolEntries = new List<PoolEntry>();
    private int nextPoolId = 0;

    private class PoolEntry
    {
        public int id;
        public char letter;
        public Button button;
        public bool active;
    }

    void Awake()
    {
        if (checkButton != null) checkButton.onClick.AddListener(OnCheckPressed);
        if (clearButton != null) clearButton.onClick.AddListener(OnClearPressed);
    }

    void Start()
    {
        CreateSlots(slotCount);
        PopulatePool();
    }

    public void CreateSlots(int count)
    {
        if (count < 1) count = 1;
        slotCount = count;

        foreach (var child in slotButtons)
            if (child != null) Destroy(child.gameObject);

        slotButtons.Clear();
        slotTexts.Clear();

        slotAssigned = new PoolEntry[slotCount];

        if (slotButtonPrefab == null || slotsParent == null)
        {
            Debug.LogError("[DynamicExitComposer] slotButtonPrefab or slotsParent not assigned.");
            return;
        }

        for (int i = 0; i < slotCount; i++)
        {
            var b = Instantiate(slotButtonPrefab, slotsParent);
            b.gameObject.SetActive(true);
            int idx = i;
            b.onClick.AddListener(() => OnSlotClicked(idx));

            // TMP child text
            var t = b.GetComponentInChildren<TMP_Text>();
            if (t == null) Debug.LogWarning("[DynamicExitComposer] slot prefab missing TMP child.");

            slotButtons.Add(b);
            slotTexts.Add(t);
            slotAssigned[i] = null;
            SetSlotEmptyDisplay(i);
        }
    }

    private void PopulatePool()
    {
        if (poolParent != null)
        {
            for (int i = poolParent.childCount - 1; i >= 0; i--)
                Destroy(poolParent.GetChild(i).gameObject);
        }
        poolEntries.Clear();
        nextPoolId = 0;

        if (letterButtonPrefab == null || poolParent == null)
        {
            Debug.LogError("[DynamicExitComposer] letterButtonPrefab or poolParent not assigned.");
            return;
        }

        string snapshot = "";
        if (GameState.Instance != null)
            snapshot = GameState.Instance.collectedLettersSnapshot ?? "";

        // If snapshot is empty, nothing to spawn
        if (string.IsNullOrWhiteSpace(snapshot))
            return;

        // If snapshot looks like a comma-separated list, split it into tokens.
        // Otherwise treat snapshot as a contiguous string of characters.
        List<string> tokens;
        if (snapshot.Contains(","))
        {
            tokens = new List<string>(snapshot.Split(','));
        }
        else
        {
            tokens = new List<string>();
            foreach (char c in snapshot)
                tokens.Add(c.ToString());
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            string token = tokens[i].Trim();
            if (string.IsNullOrEmpty(token))
                continue;

            // take first character of token
            char c = token[0];

            // skip non-letter characters (space, underscore, punctuation, null, etc.)
            if (!char.IsLetter(c))
                continue;

            var btn = Instantiate(letterButtonPrefab, poolParent);
            btn.gameObject.SetActive(true);

            var tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = c.ToString();

            var entry = new PoolEntry { id = nextPoolId++, letter = c, button = btn, active = true };
            poolEntries.Add(entry);

            btn.onClick.AddListener(() => OnPoolButtonClicked(entry));
        }
    }

    private void OnPoolButtonClicked(PoolEntry entry)
    {
        if (entry == null || !entry.active) return;

        // first empty slot
        int slotIndex = -1;
        for (int i = 0; i < slotAssigned.Length; i++)
            if (slotAssigned[i] == null) { slotIndex = i; break; }

        if (slotIndex == -1)
        {
            Debug.Log("[DynamicExitComposer] No empty slot available.");
            return;
        }

        slotAssigned[slotIndex] = entry;
        entry.active = false;

        if (entry.button != null)
        {
            entry.button.interactable = false;
            DimButtonText(entry.button, true);
        }

        SetSlotLetterDisplay(slotIndex, entry.letter);
    }

    private void OnSlotClicked(int idx)
    {
        if (idx < 0 || idx >= slotAssigned.Length) return;
        var entry = slotAssigned[idx];
        if (entry == null) return;

        // return to pool
        entry.active = true;
        if (entry.button != null)
        {
            entry.button.interactable = true;
            DimButtonText(entry.button, false);
        }

        slotAssigned[idx] = null;
        SetSlotEmptyDisplay(idx);
    }

    private void DimButtonText(Button button, bool dim)
    {
        var tmp = button.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = dim ? poolDimAlpha : 1f;
            tmp.color = c;
        }
    }

    private void SetSlotEmptyDisplay(int idx)
    {
        if (idx < 0 || idx >= slotTexts.Count) return;
        if (slotTexts[idx] != null) slotTexts[idx].text = "_";
    }

    private void SetSlotLetterDisplay(int idx, char letter)
    {
        if (idx < 0 || idx >= slotTexts.Count) return;
        if (slotTexts[idx] != null) slotTexts[idx].text = letter.ToString();
    }

    private void OnCheckPressed()
    {
        char[] arr = new char[slotAssigned.Length];
        for (int i = 0; i < slotAssigned.Length; i++)
        {
            var e = slotAssigned[i];
            if (e == null)
            {
                if (resultText != null) resultText.text = "Fill all the slots first!";
                Debug.Log("[DynamicExitComposer] Word incomplete — slot " + i + " empty.");
                return;
            }
            arr[i] = e.letter;
        }

        string word = new string(arr).ToLower();
        Debug.Log("[DynamicExitComposer] Composed word: " + word.ToUpper());
        StartCoroutine(CheckWordRoutine(word));
    }

    private IEnumerator CheckWordRoutine(string word)
{
    if (checkButton != null) checkButton.interactable = false;
    if (resultText != null) resultText.text = "Checking...";

    string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        yield return request.SendWebRequest();

        bool isValid = request.result == UnityWebRequest.Result.Success;

        // Show the overlay
        if (resultOverlay != null) resultOverlay.SetActive(true);

        if (resultLabel != null)
            resultLabel.text = isValid
                ? "Congratulations!\nNew level unlocked!"
                : "Loser!\nThat's not a real word.";

        // Keep the small inline text too if you want
        if (resultText != null) resultText.text = "";
    }

    if (checkButton != null) checkButton.interactable = true;
}

    private void OnClearPressed()
    {
        for (int i = 0; i < slotAssigned.Length; i++)
        {
            var e = slotAssigned[i];
            if (e != null)
            {
                e.active = true;
                if (e.button != null)
                {
                    e.button.interactable = true;
                    DimButtonText(e.button, false);
                }
                slotAssigned[i] = null;
            }
            SetSlotEmptyDisplay(i);
        }

        if (resultText != null) resultText.text = "";
        if (resultOverlay != null) resultOverlay.SetActive(false);
        if (resultLabel != null) resultLabel.text = "";
    }

    public void SetSlotCount(int newCount)
    {
        CreateSlots(newCount);
        PopulatePool();
    }

    public void SetCollectedSnapshotAndRefresh(string snapshot)
    {
        if (GameState.Instance != null)
            GameState.Instance.collectedLettersSnapshot = snapshot;

        PopulatePool();
        OnClearPressed();
    }
}
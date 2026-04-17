using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ExitSceneController : MonoBehaviour
{
    public Button goBackButton;
    [Tooltip("If no previous scene recorded, load this.")]
    public string fallbackScene = "MainMenu";
    public TMP_Text collectedLettersText;

    void Start()
    {
        if (goBackButton != null) goBackButton.onClick.AddListener(OnGoBackPressed);
        if (collectedLettersText != null && GameState.Instance != null)
        {
            collectedLettersText.text = GameState.Instance.collectedLettersSnapshot ?? "";
        }
    }

    void OnGoBackPressed()
    {
        string prev = GameState.Instance != null ? GameState.Instance.previousSceneName : null;
        if (string.IsNullOrEmpty(prev))
        {
            Debug.LogWarning("No previous scene recorded. Loading fallback: " + fallbackScene);
            SceneManager.LoadScene(fallbackScene);
            return;
        }
        StartCoroutine(LoadPrevScene(prev));
    }

    IEnumerator LoadPrevScene(string sceneName)
    {
        if (goBackButton != null) goBackButton.interactable = false;
        var async = SceneManager.LoadSceneAsync(sceneName);
        while (!async.isDone) yield return null;
        yield return null; // one extra frame so the scene's Start() runs
    }
}
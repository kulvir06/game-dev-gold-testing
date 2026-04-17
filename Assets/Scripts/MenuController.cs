using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    public string firstLevelSceneName;

    public TMP_Text subtitleText;

    public void OnPlayPressed()
    {
        if (string.IsNullOrEmpty(firstLevelSceneName))
        {
            Debug.LogWarning("MenuController: firstLevelSceneName not set in Inspector.");
            return;
        }

        SceneManager.LoadScene(firstLevelSceneName);
    }
}
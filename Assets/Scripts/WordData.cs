using UnityEngine;

public class WordData : MonoBehaviour
{
    public static WordData Instance;

    public string collectedWord;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
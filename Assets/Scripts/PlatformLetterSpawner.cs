using UnityEngine;

public class PlatformLetterSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;

    // 👇 THIS was missing
    public GameObject letterPrefab;

    public void SpawnLetters(char[] letters)
    {
        if (letterPrefab == null)
        {
            Debug.LogError($"{name}: LetterPrefab is NULL!");
            return;
        }

        int n = Mathf.Min(spawnPoints.Length, letters.Length);

        for (int i = 0; i < n; i++)
        {
            if (spawnPoints[i] == null)
                continue;

            GameObject obj = Instantiate(letterPrefab, spawnPoints[i].position, Quaternion.identity);

            LetterPickup pickup = obj.GetComponent<LetterPickup>();
            if (pickup != null)
                pickup.SetLetter(letters[i]);
            else
                Debug.LogError("LetterPickup missing on prefab!");
        }
    }
}
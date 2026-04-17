using UnityEngine;

public class LetterManager : MonoBehaviour
{
    public GameObject letterPrefab;
    public PlatformLetterSpawner[] platforms;

    public string lettersToSpawn = "AEIRSTNO";

    void Start()
{
    var letters = lettersToSpawn.ToUpper().ToCharArray();
    int index = 0;

    foreach (var p in platforms)
    {
        foreach (var sp in p.spawnPoints)
        {
            if (index >= letters.Length)
                return;

            GameObject obj = Instantiate(letterPrefab, sp.position, Quaternion.identity);

            LetterPickup pickup = obj.GetComponent<LetterPickup>();
            pickup.SetLetter(letters[index]);

            Debug.Log($"Assigned {letters[index]} to {obj.name}");

            index++;
        }
    }
}
}

using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    public GameObject ballPrefab;
    public float spawnRangeX = 2f;
    public float spawnY = 5f;
    public void SpawnBall()
    {
        if (ballPrefab == null)
        {
            Debug.LogError("No ballPrefab assigned in inspector!");
            return;
        }

        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
        Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Spawned ball at: " + spawnPos);
    }
}

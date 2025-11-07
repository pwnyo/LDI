using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInfo : MonoBehaviour
{
    public LevelLoader.SpawnPoint[] spawnPoints;
    
    public LevelLoader.SpawnPoint FindSpawnPoint(string name)
    {
        foreach (LevelLoader.SpawnPoint s in spawnPoints)
        {
            if (s.sceneName == name)
            {
                return s;
            }
        }
        return spawnPoints[0];
    }

    private void OnDrawGizmos()
    {
        if (spawnPoints == null)
            return;
        Gizmos.color = Color.white;
        foreach (LevelLoader.SpawnPoint s in spawnPoints)
        {
            Gizmos.DrawWireSphere(s.location, 0.15f);
        }

    }
}

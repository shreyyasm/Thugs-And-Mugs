using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhiraj
{
   

    public class NPCSpawnManager : MonoBehaviour
    {
        [System.Serializable]
        public class CustomSpawnPoint
        {
            public Transform point;
            public WaypointBank waybank;
        }

        [SerializeField] private SeatManager seatManager;
        [SerializeField] private CustomSpawnPoint[] spawnPoints;

        [SerializeField] private AManager[] npcPrefabs;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnDelay = 5f;
        [SerializeField] private int randomSpawnAfterCount = 3;

        [SerializeField] private int totalSpawnedCount = 0;
        [SerializeField] private bool canSpawn = true;

        private void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (canSpawn)
                {
                    TrySpawnNPC();
                }
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private void TrySpawnNPC()
        {
            if (GameManager.Instance.isFightStarted) return;
            bool spawned = false;

            Seat emptySeat = seatManager.GetEmptySeat();

            if (emptySeat != null)
            {
                SpawnAtSeat(emptySeat);
                spawned = true;
            }
            else if (totalSpawnedCount >= randomSpawnAfterCount && Random.value < 0.5f) // 50% chance
            {
                SpawnAtRandomPoint();
                spawned = true;
            }

            if (spawned)
            {
                totalSpawnedCount++;
                canSpawn = false;
                StartCoroutine(EnableSpawnDelay());
            }
            return;
        }

        private void SpawnAtSeat(Seat seat)
        {
            CustomSpawnPoint spawnPoint = GetRandomSpawnPoint();
            AManager npc = Instantiate(GetRandomNPCPrefab(), spawnPoint.point.position, spawnPoint.point.rotation);
            npc.waypointBank = spawnPoint.waybank;
            npc.npcSpawnManager = this;
        }

        private void SpawnAtRandomPoint()
        {
            CustomSpawnPoint spawnPoint = GetRandomSpawnPoint();
            AManager npc = Instantiate(GetRandomNPCPrefab(), spawnPoint.point.position, spawnPoint.point.rotation);
            npc.waypointBank = spawnPoint.waybank;
            npc.canStartFight = true;
            Debug.Log($"Random NPC {npc.name} Has spwanned");
        }

        private CustomSpawnPoint GetRandomSpawnPoint()
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        private AManager GetRandomNPCPrefab()
        {
            int level = GameManager.Instance.level;
            List<AManager> possibleNPCs = new List<AManager>();
            List<float> weights = new List<float>();

            // Setup weights and NPC types based on level
            if (level >= 0 && level <= 3)
            {
                possibleNPCs.Add(npcPrefabs[0]); weights.Add(1f); // 100% Type A
            }
            else if (level > 3 && level <= 5)
            {
                possibleNPCs.Add(npcPrefabs[0]); weights.Add(0.6f); // 60% A
                possibleNPCs.Add(npcPrefabs[1]); weights.Add(0.4f); // 40% B
            }
            else if (level > 5 && level <= 7)
            {
                possibleNPCs.Add(npcPrefabs[0]); weights.Add(0.4f); // 40% A
                possibleNPCs.Add(npcPrefabs[1]); weights.Add(0.3f); // 30% B
                possibleNPCs.Add(npcPrefabs[2]); weights.Add(0.3f); // 30% C
            }
            else if (level > 7 && level <= 10)
            {
                possibleNPCs.Add(npcPrefabs[0]); weights.Add(0.3f); // 30% A
                possibleNPCs.Add(npcPrefabs[1]); weights.Add(0.3f); // 30% B
                possibleNPCs.Add(npcPrefabs[2]); weights.Add(0.2f); // 20% C
                possibleNPCs.Add(npcPrefabs[3]); weights.Add(0.2f); // 20% D
            }
            else // level > 10
            {
                possibleNPCs.Add(npcPrefabs[0]); weights.Add(0.25f); // 25% A
                possibleNPCs.Add(npcPrefabs[1]); weights.Add(0.25f); // 25% B
                possibleNPCs.Add(npcPrefabs[2]); weights.Add(0.2f);  // 20% C
                possibleNPCs.Add(npcPrefabs[3]); weights.Add(0.2f);  // 20% D
                possibleNPCs.Add(npcPrefabs[4]); weights.Add(0.1f);  // 10% E
            }

            // Pick based on weights
            float total = 0f;
            foreach (float weight in weights) total += weight;

            float rand = Random.value * total;
            float cumulative = 0f;

            for (int i = 0; i < weights.Count; i++)
            {
                cumulative += weights[i];
                if (rand < cumulative)
                {
                    return possibleNPCs[i];
                }
            }

            // Fallback
            return possibleNPCs[0];
        }

        private IEnumerator EnableSpawnDelay()
        {
            yield return new WaitForSeconds(spawnDelay);
            canSpawn = true;
        }


    }
}

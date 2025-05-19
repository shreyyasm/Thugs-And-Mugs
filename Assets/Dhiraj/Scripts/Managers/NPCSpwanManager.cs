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
            return npcPrefabs[Random.Range(0, npcPrefabs.Length)];
        }

        private IEnumerator EnableSpawnDelay()
        {
            yield return new WaitForSeconds(spawnDelay);
            canSpawn = true;
        }
    }
}

using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Starter.Shooter
{
    public class AirSPAWN : NetworkBehaviour
    {
        public Airplane AirplanePrefab;
        public int MaxAirplanes = 30;
        public float SpawnRadius = 100f;
        public float SpawnHeightMin = 30f;
        public float SpawnHeightMax = 100f;
        public float SpawnInterval = 10f;

        private List<Airplane> _airplanes = new List<Airplane>();
        private float spawnTimer = 0f;

        public Transform PointA;
        public Transform PointB;

        public override void Spawned()
        {
            if (!HasStateAuthority) return;

            if (PointA == null || PointB == null)
            {
                Debug.LogError("PointA hoặc PointB chưa được gán!");
                return;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            spawnTimer += Runner.DeltaTime;

            if (spawnTimer >= SpawnInterval && _airplanes.Count < MaxAirplanes)
            {
                spawnTimer = 0f;
                SpawnNewAirplane();
            }

            for (int i = _airplanes.Count - 1; i >= 0; i--)
            {
                if (_airplanes[i] == null)
                {
                    _airplanes.RemoveAt(i);
                }
            }
        }

        private void SpawnNewAirplane()
        {
            Vector2 circlePos = Random.insideUnitCircle.normalized * SpawnRadius;
            Vector3 spawnPos = new Vector3(circlePos.x, Random.Range(SpawnHeightMin, SpawnHeightMax), circlePos.y);

            var airplane = Runner.Spawn(AirplanePrefab, spawnPos, Quaternion.identity, onBeforeSpawned: (runner, obj) =>
            {
                obj.GetComponent<Airplane>().IsActive = true;
            });

            _airplanes.Add(airplane);
            airplane.Respawn(spawnPos, PointA.position, PointB.position);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }
}

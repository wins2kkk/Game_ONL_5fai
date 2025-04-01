using Fusion;
using System;
using UnityEngine;

namespace Starter.Shooter
{
    public class Airplane : NetworkBehaviour
    {
        [Header("Movement Setup")]
        public Transform pointA; // Điểm A
        public Transform pointB; // Điểm B
        public float speed = 10f;

        [Header("References")]
        public NetworkTransform NetworkTransform; // Component để đồng bộ vị trí và xoay

        // Các biến networked để đồng bộ qua mạng
        [Networked] private Vector3 PointA { get; set; }
        [Networked] private Vector3 PointB { get; set; }
        [Networked] private Vector3 Target { get; set; }
        [Networked] private int Direction { get; set; } = 1; // 1: đi tới B, -1: quay lại A
        public bool IsActive { get; internal set; }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                if (pointA != null && pointB != null)
                {
                    PointA = pointA.position;
                    PointB = pointB.position;
                    Target = PointB;
                    Direction = 1;
                    IsActive = true;  // Kích hoạt máy bay ngay khi spawn

                    NetworkTransform.Teleport(PointA, Quaternion.LookRotation(PointB - PointA)); // Sửa lại
                }
                else
                {
                    Debug.LogError("Chưa gán điểm A hoặc điểm B!");
                }
            }
        }


        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false || !IsActive)
                return; // Nếu không có quyền hoặc máy bay không hoạt động thì không làm gì cả

            if (pointA == null || pointB == null)
            {
                Debug.LogError("Chưa gán điểm A hoặc điểm B!");
                return;
            }

            // Di chuyển máy bay về target
            Vector3 newPosition = Vector3.MoveTowards(transform.position, Target, speed * Runner.DeltaTime);

            // Khi tới target, đổi hướng
            if (Vector3.Distance(newPosition, Target) < 0.1f)
            {
                Direction *= -1;
                Target = Direction == 1 ? PointB : PointA;
            }

            // Cập nhật vị trí và hướng
            transform.position = newPosition;
            transform.LookAt(Target);
        }


        internal void Respawn(Vector3 startPosition, Vector3 newPointA, Vector3 newPointB)
        {
            transform.position = startPosition;
            pointA = new GameObject("PointA").transform; // Tạo điểm A mới nếu cần
            pointB = new GameObject("PointB").transform; // Tạo điểm B mới nếu cần
            pointA.position = newPointA;
            pointB.position = newPointB;

            PointA = newPointA;
            PointB = newPointB;
            Target = PointB;
            Direction = 1;
            IsActive = true;

            NetworkTransform.Teleport(startPosition, Quaternion.LookRotation(PointB - PointA));

            Debug.Log($"Máy bay đã respawn tại {startPosition} với điểm A: {newPointA}, điểm B: {newPointB}");
        }



    }
}
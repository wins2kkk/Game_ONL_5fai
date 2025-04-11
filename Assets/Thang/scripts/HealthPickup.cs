using UnityEngine;
using Fusion;
using Starter.Shooter;

public class HealthPickup : NetworkBehaviour
{
    public int healAmount = 1; // Lượng máu hồi
    public GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[HealthPickup] {other.name} chạm vào {gameObject.name}");

        // Kiểm tra nếu Player chạm vào item máu
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null && playerHealth.IsAlive)
            {
                Debug.Log($"[HealthPickup] {other.name} đang hồi {healAmount} máu");

                // Gọi RPC để cập nhật máu và xóa vật phẩm
                RPC_PickupHealth(playerHealth, healAmount);
            }
            else
            {
                Debug.Log("[HealthPickup] Player không thể hồi máu (đã chết hoặc thiếu Health component).");
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_PickupHealth(Health playerHealth, int amount)
    {
        if (playerHealth != null && playerHealth.IsAlive)
        {
            playerHealth.Heal(amount);
        }

        RPC_DestroyPickup();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DestroyPickup()
    {
        Debug.Log("[HealthPickup] RPC_DestroyPickup được gọi, đang xóa vật phẩm...");

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object); // Xóa vật phẩm trên tất cả client
            Debug.Log("[HealthPickup] Đã despawn item máu!");
        }
    }
}

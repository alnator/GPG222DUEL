using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);

    public override void OnNetworkSpawn()
    {
        Health.OnValueChanged += (oldValue, newValue) =>
            Debug.Log($"{name} health: {oldValue} -> {newValue}");
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;
        Health.Value -= amount;
    }
}
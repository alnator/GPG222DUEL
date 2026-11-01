using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    private Renderer rend;

    public override void OnNetworkSpawn()
    {
        rend = GetComponentInChildren<Renderer>();
        Health.OnValueChanged += OnHealthChanged;
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;
        Health.Value -= amount;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        rend.material.color = Color.red;
        Invoke(nameof(ResetColor), 0.15f);
    }

    private void ResetColor()
    {
        rend.material.color = Color.white;
    }
}
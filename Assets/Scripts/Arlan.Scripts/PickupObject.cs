using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PickupObject : NetworkBehaviour
{
    [SerializeField] private GameObject dummyPrefab;
    private readonly NetworkVariable<bool> isHeld = new NetworkVariable<bool>();
    public bool IsHeld => isHeld.Value;
    private Renderer[] localRenderers;
    private bool[] previousRendererStates;
    private Rigidbody rb;
    private Collider objectCollider;
    private Transform holdPoint;
    [SerializeField]private int maxThrows;
    private int amountThrows;
    // AYUSH DAMAGE START
    [SerializeField] private int damage = 10;
    private PlayerHealth thrower;
    private bool canDamage;
    // AYUSH DAMAGE END

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
    }

    public override void OnNetworkSpawn()
    {
        isHeld.OnValueChanged += OnHeldChanged;
        ApplyHeldState(isHeld.Value);
    }

    public override void OnNetworkDespawn()
    {
        isHeld.OnValueChanged -= OnHeldChanged;
        RestoreLocalVisual();
    }

    public GameObject CreateLocalDummy(Transform point)
    {
        // The prefab must contain visuals, with no gameplay/network scripts.
        if (dummyPrefab == null) return null;
        GameObject dummy = Instantiate(dummyPrefab, point, false);
        dummy.transform.localPosition = Vector3.zero;
        dummy.transform.localRotation = Quaternion.identity;

        // Hide only the real item's renderers on this machine; leave physics alone.
        localRenderers = GetComponentsInChildren<Renderer>(true);
        previousRendererStates = new bool[localRenderers.Length];
        for (int i = 0; i < localRenderers.Length; i++)
        {
            previousRendererStates[i] = localRenderers[i].enabled;
            localRenderers[i].enabled = false;
        }
        return dummy;
    }

    public void RestoreLocalVisual()
    {
        if (localRenderers == null) return;
        for (int i = 0; i < localRenderers.Length; i++)
            if (localRenderers[i] != null)
                localRenderers[i].enabled = previousRendererStates[i];
        localRenderers = null;
        previousRendererStates = null;
    }

    private void OnHeldChanged(bool previous, bool current)
    {
        ApplyHeldState(current);
    }

    private void ApplyHeldState(bool held)
    {
        // Clients receive positions; physics runs on the server.
        rb.isKinematic = !IsServer || held;
        rb.useGravity = !held;
        objectCollider.enabled = !held;
    }

    public bool TryPickup(Transform point)
    {
        if (!IsServer || !IsSpawned || isHeld.Value || point == null) return false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        holdPoint = point;
        isHeld.Value = true;
        rb.position = point.position;
        rb.rotation = point.rotation;
        return true;
    }

    private void FixedUpdate()
    {
        if (!IsServer || !IsSpawned || !isHeld.Value) return;
        rb.MovePosition(holdPoint.position);
        rb.MoveRotation(holdPoint.rotation);
        
    }

    public void Throw(Vector3 impulse)
    {
        if (!IsServer || !IsSpawned || !isHeld.Value) return;
        // AYUSH DAMAGE START
        thrower = holdPoint != null ? holdPoint.GetComponentInParent<PlayerHealth>() : null;
        canDamage = true;
        // AYUSH DAMAGE END
        holdPoint = null;
        isHeld.Value = false;
        rb.AddForce(impulse, ForceMode.Impulse);
    }

    // AYUSH DAMAGE START
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || !canDamage) return;

        PlayerHealth target = collision.collider.GetComponentInParent<PlayerHealth>();
        if (target != null && target == thrower) return; // ignore the thrower

        canDamage = false; // only the first thing hit counts
        if (target != null)
        {
            target.TakeDamage(damage);
            Debug.Log($"Hit {target.name}, health is now {target.Health.Value}");
        }
    }
    // AYUSH DAMAGE END


    private void DestroyObject()
    {// REWORK, ON 3RD immedeatelly destroys object, have to rework with adding health.cs and collidertrigger
    
        if(maxThrows <= amountThrows)
        {
            NetworkObject.Despawn();
        }
    }
    public void ThrowsCount()
    {
        amountThrows++;
        DestroyObject();
    }
}

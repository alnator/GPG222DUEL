using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private LayerMask interactionMask = ~0;

    // Used only on the server. Every player can hold one item.
    private PickupObject heldObject;

    // Local visuals only; these fields are not synchronized.
    private PickupObject predictedObject;
    private GameObject localDummy;
    private bool waitingForPickup;
    private bool waitingForThrow;

    private void Update()
    {
        if (!IsSpawned || !IsOwner || !IsClient) return;
        if (predictedObject != null && !predictedObject.IsSpawned)
            ClearLocalDummy();
        else if (predictedObject == null && localDummy != null)
            ClearLocalDummy();
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (playerCamera == null || holdPoint == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            BeginLocalPickup();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            predictedObject != null && !waitingForPickup && !waitingForThrow)
        {
            waitingForThrow = true;
            ThrowRpc(playerCamera.transform.forward);
        }
    }

    private void BeginLocalPickup()
    {
        if (predictedObject != null || waitingForPickup || waitingForThrow) return;
        Vector3 direction = playerCamera.transform.forward;
        if (!Physics.Raycast(playerCamera.transform.position, direction,
                out RaycastHit hit, pickupDistance, interactionMask,
                QueryTriggerInteraction.Ignore)) return;

        PickupObject item = hit.collider.GetComponentInParent<PickupObject>();
        if (item == null || !item.IsSpawned || item.IsHeld) return;

        predictedObject = item;
        waitingForPickup = true;
        // This happens before the request, without waiting for the server, to avoid latency problems.
        localDummy = item.CreateLocalDummy(holdPoint);
        PickupRpc(direction, item.NetworkObjectId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void PickupRpc(Vector3 direction, ulong requestedObjectId)
    {
        PickupResultRpc(TryPickupOnServer(direction, requestedObjectId));
    }

    private bool TryPickupOnServer(Vector3 direction, ulong requestedObjectId)
    {
        if (heldObject != null || playerCamera == null || holdPoint == null) return false;
        if (!IsValidDirection(direction)) return false;
        if (!Physics.Raycast(playerCamera.transform.position, direction.normalized,
                out RaycastHit hit, pickupDistance, interactionMask,
                QueryTriggerInteraction.Ignore)) return false;

        PickupObject item = hit.collider.GetComponentInParent<PickupObject>();
        if (item == null || item.NetworkObjectId != requestedObjectId) return false;
        if (!item.TryPickup(holdPoint)) return false;
        heldObject = item;
        return true;
    }

    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
    private void PickupResultRpc(bool accepted)
    {
        waitingForPickup = false;
        if (!accepted) ClearLocalDummy();
        // If accepted, keep the dummy while carrying to avoid  network lag.
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void ThrowRpc(Vector3 direction)
    {
        if (heldObject == null || !IsValidDirection(direction))
        {
            ThrowResultRpc(false);
            return;
        }
        heldObject.Throw(direction.normalized * throwForce);
        heldObject.ThrowsCount();
        heldObject = null;
        ThrowResultRpc(true);
        
    }

    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
    private void ThrowResultRpc(bool thrown)
    {
        waitingForThrow = false;
        if (thrown) ClearLocalDummy();
    }

    private void ClearLocalDummy()
    {
        if (localDummy != null)
        {
            localDummy.SetActive(false);
            Destroy(localDummy);
        }
        if (predictedObject != null) predictedObject.RestoreLocalVisual();
        localDummy = null;
        predictedObject = null;
        waitingForPickup = false;
        waitingForThrow = false;
    }

    private bool IsValidDirection(Vector3 direction)
    {
        float lengthSquared = direction.sqrMagnitude;
        return lengthSquared > 0.9f && lengthSquared < 1.1f;
    }
}

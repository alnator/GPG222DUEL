using Unity.Netcode.Components;

// Movement is simulated by the owning client, then replicated to everyone else.
public class OwnerNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative() => false;
}

using Unity.Netcode;
using UnityEngine;

public class ObjectSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int count = 5;
    [SerializeField] private Vector2 areaSize = new Vector2(20, 20);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                1f,
                Random.Range(-areaSize.y / 2, areaSize.y / 2));

            GameObject obj = Instantiate(objectPrefab, transform.position + offset, Quaternion.identity);
            obj.GetComponent<NetworkObject>().Spawn();
        }
    }
}
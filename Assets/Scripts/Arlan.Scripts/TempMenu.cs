using Unity.Netcode;


public class TempMenu : NetworkBehaviour
{
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
}
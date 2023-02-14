using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance {get; private set;}
    void Start()
    {
        instance = this;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("DungeonCell"))
        {
            DFSAlgorithm.instance.UpdateVisibility(other.GetComponent<RoomBehaviour>().id);
        }
    }
}

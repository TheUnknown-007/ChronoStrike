using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(!other.transform.CompareTag("Player")) return;
        PlayerManager.instance.TriggerBoss();
    }
}
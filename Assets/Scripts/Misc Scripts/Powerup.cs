using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] int type;
    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            switch(type)
            {
                case 0:
                    PlayerManager.instance.AddAmour();
                    break;
                case 1:
                    PlayerManager.instance.AddHealth();
                    break;
                case 2:
                    PlayerManager.instance.Enhance();
                    break;
            }
            Destroy(gameObject);
        }
    }
}

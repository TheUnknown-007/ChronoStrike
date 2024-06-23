using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] int type;

    void Update()
    {
        if(type == 3 && PlayerManager.instance.currentWeaponIndex == 8) Destroy(gameObject);
    }
    
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
                case 3:
                    PlayerManager.instance.GiveWeapon(7, 15);
                    break;
                case 4:
                    PlayerManager.instance.GiveWeapon(8, 10);
                    break;
            }
            Destroy(gameObject);
        }
    }
}

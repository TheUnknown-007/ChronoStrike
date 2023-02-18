using UnityEngine;

public class PointBoss : MonoBehaviour
{
    void Update()
    {
        var lookPos = Enemy.bossPosition - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = rotation;
    }
}

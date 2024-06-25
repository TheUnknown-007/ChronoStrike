using DitzeGames.Effects;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    int Typee;
    float BulletSpeed;
    float BulletDamage;
    float CollateralDamage;
    float CollateralRadius;
    float LineLength;
    LineRenderer line;
    GameObject ImpactParticles;
    bool Shake;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, CollateralRadius);
    }

    public void Instantiate(int type, bool shake, float bulletSpeed, float bulletDamage, float collateralDamage, float collateralRadius, float lineLength, Gradient bulletColor, GameObject impactParticles)
    {
        Typee = type;
        BulletSpeed = bulletSpeed;
        BulletDamage = bulletDamage;
        CollateralDamage = collateralDamage;
        CollateralRadius = collateralRadius;
        ImpactParticles = impactParticles;
        LineLength = lineLength;
        line = GetComponent<LineRenderer>();
        line.colorGradient = bulletColor;
        Shake = shake;
        Destroy(gameObject, 3);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, BulletSpeed*Time.deltaTime);
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position - transform.forward * LineLength);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("IgnoreBullet")) return;
        if(other.CompareTag("DungeonCell") || other.gameObject.layer == 8 || other.gameObject.layer == 2) return;
        if(Typee == 0)
        {
            if(other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("BossEnemy"))
            {
                other.gameObject.GetComponent<Enemy>().AddDamage(BulletDamage);
                Destroy(Instantiate(ImpactParticles, transform.position, transform.rotation), 2);
            }
            else if(other.gameObject.CompareTag("Player")) return;
        }
        else if(Typee == 1)
        {
            if(other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("BossEnemy")) return;
        }

        Destroy(Instantiate(ImpactParticles, transform.position, transform.rotation), 2);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, CollateralRadius);
        foreach (var hitCollider in hitColliders)
        {
            if(Typee == 0)
            {
                if(!hitCollider.CompareTag("Enemy")) continue;
                hitCollider.GetComponent<Enemy>().AddDamage(CollateralDamage);
            }
            else
            {
                if(!hitCollider.CompareTag("Player")) continue;
            }
        }
        StopAllCoroutines();
        if(Shake) CameraEffects.ShakeOnce();
        Destroy(gameObject);
    }
}

using UnityEngine;

public class LaserWeapon : MonoBehaviour
{
    [SerializeField] ScriptableWeapon weaponData;
    [SerializeField] float distance;
    [SerializeField] LayerMask layerMask;
    [SerializeField] LineRenderer laser;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Transform bulletPoint;
    [SerializeField] GameObject muzzleFlash;
    GameObject impact;
    bool firing;
    
    RaycastHit hit;

    void Update()
    {
        if(firing) Shoot();

        if(Input.GetButtonDown("Fire1")) StartShoot();

        if(Input.GetButtonUp("Fire1")) StopShoot();
    }

    void StartShoot()
    {
        if(Physics.Raycast(bulletPoint.position, transform.forward, out hit, distance, layerMask))
        {
            firing = true;
            muzzleFlash.SetActive(true);
            audioSource.loop = true;
            audioSource.Play();
            laser.SetPosition(0, bulletPoint.position);
            laser.SetPosition(1, hit.point);
            impact = Instantiate(weaponData.impactParticles, hit.point, Quaternion.Euler(hit.normal));
        }
    }

    void Shoot()
    {
        if(Physics.Raycast(bulletPoint.position, transform.forward, out hit, distance, layerMask))
        {
            laser.SetPosition(0, bulletPoint.position);
            laser.SetPosition(1, hit.point);
            impact.transform.position = hit.point;
            if(hit.transform.CompareTag("Enemy") || hit.transform.CompareTag("BossEnemy"))
                hit.transform.gameObject.GetComponent<Enemy>().AddDamage(weaponData.bulletDamage*Time.deltaTime);
        }
    }

    void StopShoot()
    {
        muzzleFlash.SetActive(false);
        audioSource.loop = false;
        audioSource.Stop();
        laser.SetPosition(0, Vector3.zero);
        laser.SetPosition(1, Vector3.zero);
        impact.GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(impact, 3);
        firing = false;
    }
}

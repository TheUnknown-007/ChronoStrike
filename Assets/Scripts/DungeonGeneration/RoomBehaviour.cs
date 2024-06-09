using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField] bool DoubleFloored;
    [SerializeField] GameObject[] walls; // 0 top, 1 bottom, 2 left, 3 right
    [SerializeField] GameObject[] doors; // 0 top, 1 bottom, 2 left, 3 right
    [SerializeField] Transform bossSpawnPoint;
    [Space, SerializeField] Transform[] enemySpawns;
    [SerializeField] Transform[] sentrySpawns;
    [Space, SerializeField] Transform[] floor2EnemySpawns;
    [SerializeField] Transform[] floor2SentrySpawns;
    [Space, SerializeField] Transform[] PowerupSpawns;
    [SerializeField] GameObject[] Powerups;
    [SerializeField] ScriptableWeapon[] EnemyWeapons;
    [SerializeField] ScriptableWeapon[] BossWeapons;
    [SerializeField] ScriptableWeapon SentryWeapon;
    [SerializeField] LayerMask DroneVisibility;
    [SerializeField] Vector2 healthRange;
    [SerializeField] Vector2 fireDelayRange;
    [HideInInspector] public bool[] currentStatus {get; private set;}
    [HideInInspector] public int id {get; private set;}


    List<Enemy> dronesSpawned = new List<Enemy>();
    [HideInInspector] public List<Transform> droneMovePts = new List<Transform>();
    [HideInInspector] public List<bool> dronePointUsed = new List<bool>();
    public Dictionary<float, int> unsortedDroneMoveScores = new Dictionary<float, int>();
    public SortedList<float, int> sortedDroneMoveScores;


    List<GameObject> AllChilds(GameObject root)
    {
        List<GameObject> result = new List<GameObject>();
        if (root.transform.childCount > 0)
            foreach (Transform VARIABLE in root.transform)
                Searcher(result,VARIABLE.gameObject);
        return result;
    }
 
    private void Searcher(List<GameObject> list,GameObject root)
    {
        list.Add(root);
        if (root.transform.childCount > 0)
            foreach (Transform VARIABLE in root.transform)
                Searcher(list,VARIABLE.gameObject);
    }

    public void UpdateRoom(int ID, bool[] Status, bool boss, GameObject sentryPrefab, GameObject enemyPrefab, GameObject dronePrefab, GameObject bossPrefab, Slider healthSlider)
    {
        foreach(GameObject t in AllChilds(gameObject))
        {
            if(t.CompareTag("DroneMovePoint")) 
            {
                droneMovePts.Add(t.transform);
                dronePointUsed.Add(false);
            }
        }

        for(int i = 0; i < Status.Length; i++)
        {
            doors[i].SetActive(Status[i]);
            walls[i].SetActive(!Status[i]);
        }
        currentStatus = Status;
        id = ID;
        
        if(id != 0)
        {
            if(boss) Spawn(true, null, null, null, bossPrefab, healthSlider);
            else Spawn(false, sentryPrefab, enemyPrefab, dronePrefab, null, healthSlider);
        }
    }

    void Spawn(bool boss, GameObject sentryPrefab, GameObject enemyPrefab, GameObject dronePrefab, GameObject bossPrefab, Slider healthSlider)
    {
        if(boss)
        {
            int weaponSet = Random.Range(0, 2);
            if(weaponSet == 1)
                Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {BossWeapons[1], BossWeapons[1]}, 500, 3, healthSlider, this, 0);
            else
                Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, transform).GetComponent<Enemy>().Init(BossWeapons, 500, 1.5f, healthSlider, this, 0);
            return;
        }

        // ===============================================================
        // ====================== Sentry Spawn ===========================
        // ===============================================================
        int index;
        if(Random.Range(0, 3) != 0)
        {
            index = Random.Range(0, sentrySpawns.Length-1);
            Instantiate(sentryPrefab, sentrySpawns[index].position, sentrySpawns[index].rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {SentryWeapon}, 30, 3, null, this, 0);
            if(Random.Range(0, 4) == 0)
            {
                index = Random.Range(0, sentrySpawns.Length-1);
                Instantiate(sentryPrefab, sentrySpawns[index].position, sentrySpawns[index].rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {SentryWeapon}, 30, 3, null, this, 0);
            }
        }
        
        if(DoubleFloored)
        {
            index = Random.Range(0, floor2SentrySpawns.Length-1);
            Instantiate(sentryPrefab, floor2SentrySpawns[index].position, floor2SentrySpawns[index].rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {SentryWeapon}, 30, 3, null, this, 0);
        }


        // ===============================================================
        // ======================= Enemy Spawn ===========================
        // ===============================================================
        bool[] usedPoints = new bool[enemySpawns.Length];
        int count = Random.Range(1, Mathf.FloorToInt(enemySpawns.Length/4));
        int x=0;
        while(x<count)
        {
            if(CheckAllSpawnUsed(usedPoints)) break;
            index = Random.Range(0, enemySpawns.Length-1);
            if(usedPoints[index]) continue;
            usedPoints[index] = true;

            float health = Random.Range(healthRange.x, healthRange.y);
            
            float fireDelay;
            int index2 = Random.Range(0, EnemyWeapons.Length-1);
            if(Random.Range(0, 5) == 0) index2 = EnemyWeapons.Length-1;

            if(index2 == 0 || index2 == 3) fireDelay = 0.35f;
            else if(index2 == 4) fireDelay = 3;
            else fireDelay = Random.Range(fireDelayRange.x, fireDelayRange.y);

            Instantiate(enemyPrefab, enemySpawns[index].position, enemySpawns[index].rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {EnemyWeapons[index2]}, health, fireDelay, null, this, 0);
            x+=1;
        }

        if(DoubleFloored){
            usedPoints = new bool[floor2EnemySpawns.Length];
            count = Random.Range(1, Mathf.FloorToInt(floor2EnemySpawns.Length/2));
            x=0;
            while(x<count)
            {
                if(CheckAllSpawnUsed(usedPoints)) break;
                index = Random.Range(0, floor2EnemySpawns.Length-1);
                if(usedPoints[index]) continue;
                usedPoints[index] = true;

                float health = Random.Range(healthRange.x, healthRange.y);
                
                float fireDelay;
                int index2 = Random.Range(0, EnemyWeapons.Length-1);
                if(Random.Range(0, 5) == 0) index2 = EnemyWeapons.Length-1;

                if(index2 == 0 || index2 == 3) fireDelay = 0.35f;
                else if(index2 == 4) fireDelay = 3;
                else fireDelay = Random.Range(fireDelayRange.x, fireDelayRange.y);

                Instantiate(enemyPrefab, floor2EnemySpawns[index].position, floor2EnemySpawns[index].rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {EnemyWeapons[index2]}, health, fireDelay, null, this, 0);
                x+=1;
            }
        }

        // ===============================================================
        // ======================= Drone Spawn ===========================
        // ===============================================================
        if(Random.Range(0,3) == 0)
        {
            index = Random.Range(0, EnemyWeapons.Length);

            int index2 = Random.Range(0, droneMovePts.Count);
            if(Random.Range(0, 5) == 0) index2 = EnemyWeapons.Length-1;

            float fireDelay;
            if(index == 0 || index == 3) fireDelay = 0.35f;
            else if(index == 4) fireDelay = 3;
            else fireDelay = Random.Range(fireDelayRange.x, fireDelayRange.y);

            Enemy drone = Instantiate(dronePrefab, droneMovePts[index2].position, droneMovePts[index2].rotation, transform).GetComponent<Enemy>();
            drone.Init(new ScriptableWeapon[] {EnemyWeapons[index]}, 30, fireDelay, null, this, index2);
            dronesSpawned.Add(drone);
        }

        if(DoubleFloored && Random.Range(0,3) == 0)
        {
            index = Random.Range(0, EnemyWeapons.Length);

            int index2 = Random.Range(0, droneMovePts.Count);
            if(Random.Range(0, 5) == 0) index2 = EnemyWeapons.Length-1;

            float fireDelay;
            if(index == 0 || index == 3) fireDelay = 0.35f;
            else if(index == 4) fireDelay = 3;
            else fireDelay = Random.Range(fireDelayRange.x, fireDelayRange.y);

            Enemy drone = Instantiate(dronePrefab, droneMovePts[index2].position, droneMovePts[index2].rotation, transform).GetComponent<Enemy>();
            drone.Init(new ScriptableWeapon[] {EnemyWeapons[index]}, 30, fireDelay, null, this, index2);
            dronesSpawned.Add(drone);
        }

        // ===============================================================
        // ===================== Powerup Spawn ===========================
        // ===============================================================
        if(Random.Range(0, 5) != 0)
        {
            x = 0;
            count = Random.Range(1, Powerups.Length-1);
            usedPoints = new bool[PowerupSpawns.Length];
            while(x < count)
            {
                if(CheckAllSpawnUsed(usedPoints)) break;
                do index = Random.Range(0, PowerupSpawns.Length); while(usedPoints[index]);

                usedPoints[index] = true;
                Instantiate(Powerups[x], PowerupSpawns[index].position, PowerupSpawns[index].rotation, transform);
                x+=1;
            }

            if(Random.Range(0, 5) == 0 && !CheckAllSpawnUsed(usedPoints))
            {
                do index = Random.Range(0, PowerupSpawns.Length); while(usedPoints[index]);
                usedPoints[index] = true;
                Instantiate(Powerups[Random.Range(Powerups.Length-2, Powerups.Length)], PowerupSpawns[index].position, PowerupSpawns[index].rotation, transform);
                x+=1;
            }
        }
        CheckDroneScores();
    }

    bool CheckAllSpawnUsed(bool[] usedPoints)
    {
        foreach(bool point in usedPoints)
            if(!point) return false;
        return true;
    }

    public void CheckDroneScores()
    {
        for(int i = 0; i < droneMovePts.Count; i++)
        {
            float score = Vector3.Distance(PlayerManager.instance.transform.position, droneMovePts[i].position);
            score *= Physics.Raycast(droneMovePts[i].position, PlayerManager.instance.transform.position - droneMovePts[i].position) ? 1 : 4;
            unsortedDroneMoveScores[score] = i;
            droneMovePts[i].gameObject.GetComponent<DebugDronePoint>().pointScore = score;
        }
        sortedDroneMoveScores = new SortedList<float, int>(unsortedDroneMoveScores);
    }


    public void RefreshRoom(bool active)
    {
        CheckDroneScores();
        foreach(Enemy drone in dronesSpawned)
        {
            if(!drone) continue;
            if(active) drone.EnableBehavior();
            else drone.StopAllCoroutines();
        }
    }
}

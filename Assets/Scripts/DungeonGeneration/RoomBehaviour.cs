using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField] GameObject[] walls; // 0 top, 1 bottom, 2 left, 3 right
    [SerializeField] GameObject[] doors; // 0 top, 1 bottom, 2 left, 3 right
    [SerializeField] Transform bossSpawnPoint;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform[] sentrySpawnPoints;
    [SerializeField] Transform[] lootSpawnPoints;
    [SerializeField] GameObject[] possibleLootSpawns;
    [SerializeField] ScriptableWeapon[] possibleEnemyWeapons;
    [SerializeField] ScriptableWeapon[] bossWeapons;
    [SerializeField] ScriptableWeapon sentryWeapon;
    [SerializeField] LayerMask droneVisibility;
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
                Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {bossWeapons[1], bossWeapons[1]}, 500, 3, healthSlider, this, 0);
            else
                Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, transform).GetComponent<Enemy>().Init(bossWeapons, 500, 1.5f, healthSlider, this, 0);
            return;
        }

        bool[] usedPoints = new bool[spawnPoints.Length];
        int count = Random.Range(1, Mathf.FloorToInt(spawnPoints.Length/4));

        int index = Random.Range(0, sentrySpawnPoints.Length-1);
        Instantiate(sentryPrefab, sentrySpawnPoints[index].position, sentrySpawnPoints[index].rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {sentryWeapon}, 30, 3, null, this, 0);
        
        int x=0;
        while(x<count)
        {
            if(CheckAllSpawnUsed(usedPoints)) break;
            index = Random.Range(0, spawnPoints.Length-1);
            if(usedPoints[index]) continue;
            usedPoints[index] = true;
            float fireDelay = Random.Range(fireDelayRange.x, fireDelayRange.y);
            float health = Random.Range(healthRange.x, healthRange.y);
            int index2 = Random.Range(0, possibleEnemyWeapons.Length-1);
            Instantiate(enemyPrefab, spawnPoints[index].position, spawnPoints[index].rotation, transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {possibleEnemyWeapons[index2]}, health, index2 == 0 ? 0.35f : fireDelay, null, this, 0);
            x+=1;
        }

        if(Random.Range(0,3) != 0)
        {
            index = Random.Range(0, possibleEnemyWeapons.Length);
            float fireDelay = Random.Range(fireDelayRange.x, fireDelayRange.y);
            int index2 = Random.Range(0, droneMovePts.Count);
            Enemy drone = Instantiate(dronePrefab, droneMovePts[index2].position, droneMovePts[index2].rotation, transform).GetComponent<Enemy>();
            drone.Init(new ScriptableWeapon[] {possibleEnemyWeapons[index]}, 30, index == 0 ? 0.35f : fireDelay, null, this, index2);
            dronesSpawned.Add(drone);
        }

        if(Random.Range(0, 5) != 0)
        {
            x = 0;
            count = Random.Range(1, possibleLootSpawns.Length+1);
            usedPoints = new bool[lootSpawnPoints.Length];
            while(x < count)
            {
                do index = Random.Range(0, lootSpawnPoints.Length); while(usedPoints[index]);
                usedPoints[index] = true;
                Instantiate(possibleLootSpawns[x], lootSpawnPoints[index].position, lootSpawnPoints[index].rotation, transform);
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

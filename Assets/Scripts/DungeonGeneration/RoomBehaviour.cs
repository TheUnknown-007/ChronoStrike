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
    [SerializeField] Vector2 healthRange;
    [SerializeField] Vector2 fireDelayRange;
    [HideInInspector] public bool[] currentStatus {get; private set;}
    [HideInInspector] public int id {get; private set;}

    public void UpdateRoom(int ID, bool[] Status, bool boss, GameObject sentryPrefab, GameObject enemyPrefab, GameObject bossPrefab, Slider healthSlider)
    {
        for(int i = 0; i < Status.Length; i++)
        {
            doors[i].SetActive(Status[i]);
            walls[i].SetActive(!Status[i]);
        }
        currentStatus = Status;
        id = ID;
        
        if(id != 0)
        {
            if(boss) Spawn(true, null, null, bossPrefab, healthSlider);
            else Spawn(false, sentryPrefab, enemyPrefab, null, healthSlider);
        }
    }

    void Spawn(bool boss, GameObject sentryPrefab, GameObject enemyPrefab, GameObject bossPrefab, Slider healthSlider)
    {
        if(boss)
        {
            int weaponSet = Random.Range(0, 2);
            if(weaponSet == 1)
                Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, this.transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {bossWeapons[1], bossWeapons[1]}, 500, 3, healthSlider);
            else
                Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation, this.transform).GetComponent<Enemy>().Init(bossWeapons, 500, 1.5f, healthSlider);
            return;
        }

        bool[] usedPoints = new bool[spawnPoints.Length];
        int count = Random.Range(1, Mathf.FloorToInt(spawnPoints.Length/3));

        int index = Random.Range(0, sentrySpawnPoints.Length-1);
        Instantiate(sentryPrefab, sentrySpawnPoints[index].position, sentrySpawnPoints[index].rotation, this.transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {sentryWeapon}, 30, 3, null);
        
        int x=0;
        while(x<count)
        {
            if(CheckAllSpawnUsed(usedPoints)) break;
            index = Random.Range(1, spawnPoints.Length-1);
            if(usedPoints[index]) continue;
            usedPoints[index] = true;
            float fireDelay = Random.Range(fireDelayRange.x, fireDelayRange.y);
            float health = Random.Range(healthRange.x, healthRange.y);
            int index2 = Random.Range(0, possibleEnemyWeapons.Length-1);
            Instantiate(enemyPrefab, spawnPoints[index].position, spawnPoints[index].rotation, this.transform).GetComponent<Enemy>().Init(new ScriptableWeapon[] {possibleEnemyWeapons[index2]}, health, fireDelay, null);
            x+=1;
        }

        count = Random.Range(0, 4);
        if(count > 1)
        {
            count = Mathf.Max(count, 2);
            usedPoints = new bool[lootSpawnPoints.Length];
            x=0;
            while(x<count+1 && x<possibleLootSpawns.Length)
            {
                index = Random.Range(0, lootSpawnPoints.Length-1);
                if(usedPoints[index]) continue;
                usedPoints[index] = true;
                Instantiate(possibleLootSpawns[x], lootSpawnPoints[index].position, lootSpawnPoints[index].rotation, this.transform);
                x+=1;
            }
        }
        
    }

    bool CheckAllSpawnUsed(bool[] usedPoints)
    {
        foreach(bool point in usedPoints)
            if(!point) return false;
        return true;
    }
}

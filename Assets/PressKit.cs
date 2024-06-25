using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PressKit : MonoBehaviour
{
    public int seed = -1;
    public RoomBehaviour[] rooms;
    public RoomBehaviour bossRoom;
    public GameObject enemyPrefab;
    public GameObject sentryPrefab;
    public GameObject dronePrefab;
    public GameObject bossPrefab;
    public Slider dummy;

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        if(seed == -1) seed = Random.Range(int.MinValue, int.MaxValue);
        Debug.Log(seed);
        bool[] status  = new bool[] { true, true, true, true, false };
        bool[] status2 = new bool[] { false, false, false, true, true };
        int i = 0;
        foreach(RoomBehaviour room in rooms)
        {
            room.UpdateRoom(seed+i, 1, status, false, sentryPrefab, enemyPrefab, dronePrefab, null, null);
            i++;
        }
        bossRoom.UpdateRoom(seed, 1, status2, true, null, null, null, bossPrefab, dummy);
    }
}

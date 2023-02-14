using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField] GameObject[] walls; // 0 top, 1 bottom, 2 left, 3 right
    [SerializeField] GameObject[] doors; // 0 top, 1 bottom, 2 left, 3 right
    [HideInInspector] public bool[] currentStatus {get; private set;}
    [HideInInspector] public int id {get; private set;}

    public void UpdateRoom(int ID, bool[] Status)
    {
        for(int i = 0; i < Status.Length; i++)
        {
            doors[i].SetActive(!Status[i]);
            walls[i].SetActive(Status[i]);
        }
        currentStatus = Status;
        id = ID;
    }
}

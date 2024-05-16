using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

[Serializable]
public struct RoomType{
    public GameObject[] variants;
}

public class DFSAlgorithm : MonoBehaviour
{
    public static DFSAlgorithm instance {get; private set;}
    [SerializeField] Slider healthSlider;
    [SerializeField] RoomType[] room;
    [SerializeField] GameObject bossPrefab;
    [SerializeField] GameObject sentryPrefab;
    [SerializeField] GameObject enemyPrefab; 
    [SerializeField] Vector2 sizeRange;
    [SerializeField] Vector2 offset;
    [SerializeField] int startPos = 0;
    int roomType;
    int size;
    List<Cell> board;
    Dictionary<int, GameObject> instantiatedRooms;

    void Start()
    {
        instance = this;
        size = Mathf.FloorToInt(UnityEngine.Random.Range(sizeRange.x, sizeRange.y));
        roomType = UnityEngine.Random.Range(0, room.Length);
        MazeGenerator();
    }

    public void UpdateVisibility(int currentCell) // for optimization, non visible rooms are disabled
    {
        for(int i = 0; i<size; i++)
            for(int j = 0; j<size; j++)
            {
                int cellToCheck = Mathf.FloorToInt(i+j*size);
                if(cellToCheck == currentCell || !instantiatedRooms.ContainsKey(cellToCheck)) continue;

                if( (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[0] && (cellToCheck - size >= 0) && (cellToCheck - size == currentCell)) ||
                    (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[1] && (cellToCheck + size < board.Count) && (cellToCheck + size == currentCell)) ||
                    (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[3] && ((cellToCheck+1) % size != 0) && (cellToCheck + 1 == currentCell)) ||
                    (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[2] && (cellToCheck % size != 0) && (cellToCheck - 1 == currentCell))
                )
                    instantiatedRooms[cellToCheck].SetActive(true);
                else
                    instantiatedRooms[cellToCheck].SetActive(false);
            }
    }

    void GenerateDungeon() // spawns the rooms
    {
        for(int i = 0; i<size; i++)
            for(int j = 0; j<size; j++)
            {
                Cell currentCell = board[Mathf.FloorToInt(i + j * size)];
                if(currentCell.visited)
                {
                    int cellID = Mathf.FloorToInt(i + j * size);
                    GameObject newRoom = Instantiate(room[roomType].variants[UnityEngine.Random.Range(0, room[roomType].variants.Length)], new Vector3(i*offset.x, 0, -j*offset.y), Quaternion.identity, transform);
                    RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                    if(cellID == board.Count-1) script.UpdateRoom(cellID, board[cellID].status, true, null, null, bossPrefab, healthSlider);
                    else script.UpdateRoom(cellID, board[cellID].status, false, sentryPrefab, enemyPrefab, null, healthSlider);
                    newRoom.name += " " + i + ":" + j;
                    if(cellID != 0) newRoom.SetActive(false);
                    instantiatedRooms[cellID] = newRoom;
                }
            }

    }

    void MazeGenerator() // as the name suggests
    {
        instantiatedRooms = new Dictionary<int, GameObject>();
        board = new List<Cell>();
        for(int i = 0; i<size; i++)
            for(int j = 0; j<size; j++)
                board.Add(new Cell());

        int currentCell = startPos;
        Stack<int> path = new Stack<int>();
        int k = 0;

        while((k<1000))
        {
            k++;
            board[currentCell].visited = true;
            if(currentCell == board.Count-1) break;
            List<int> neighbours = CheckNeighbours(currentCell);
            if(neighbours.Count == 0)
            {
                if(path.Count == 0) break;
                else currentCell = path.Pop();
            }
            else
            {
                path.Push(currentCell);
                int newCell = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
                if(newCell > currentCell)  // down or right
                {
                    if(newCell-1 == currentCell) // right
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else // down
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }

                }
                else // up or left
                {
                    if(newCell+1 == currentCell) // left
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else // down
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }   
                }
            }
        }
        GenerateDungeon();
    }

    List<int> CheckNeighbours(int cell)
    {
        List<int> neighbour = new List<int>();
        
        // Check Up Neighbour
        if(cell - size >= 0 && !board[Mathf.FloorToInt(cell-size)].visited)
            neighbour.Add(Mathf.FloorToInt(cell-size));
        // Check Bottom Neighbour
        if(cell + size < board.Count && !board[Mathf.FloorToInt(cell+size)].visited)
            neighbour.Add(Mathf.FloorToInt(cell+size));
        // Check Right Neighbour
        if((cell+1) % size != 0 && !board[Mathf.FloorToInt(cell+1)].visited)
            neighbour.Add(Mathf.FloorToInt(cell+1));
        // Check Left Neighbour
        if(cell % size != 0 && !board[Mathf.FloorToInt(cell-1)].visited)
            neighbour.Add(Mathf.FloorToInt(cell-1));

        return neighbour;
    }

    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }
}

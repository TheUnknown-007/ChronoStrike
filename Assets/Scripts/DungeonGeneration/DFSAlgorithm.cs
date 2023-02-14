using System.Collections.Generic;
using UnityEngine;
using System;

public class DFSAlgorithm : MonoBehaviour
{
    public static DFSAlgorithm instance {get; private set;}
    [SerializeField] GameObject[] room;
    [SerializeField] Vector2 size;
    [SerializeField] Vector2 offset;
    [SerializeField] int startPos = 0;
    int color;
    List<Cell> board;
    Dictionary<int, GameObject> instantiatedRooms;

    void Start()
    {
        instance = this;
        color = UnityEngine.Random.Range(0, room.Length);
        MazeGenerator();
    }

    public void UpdateVisibility(int currentCell)
    {
        for(int i = 0; i<size.x; i++)
            for(int j = 0; j<size.y; j++)
            {
                int cellToCheck = Mathf.FloorToInt(i+j*size.x);
                if(cellToCheck == currentCell || !instantiatedRooms.ContainsKey(cellToCheck)) continue;

                if(
                    (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[0] && (cellToCheck - size.x >= 0) && (cellToCheck - size.x == currentCell)) ||
                    (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[1] && (cellToCheck + size.x < board.Count) && (cellToCheck + size.x == currentCell)) ||
                    (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[3] && ((cellToCheck+1) % size.x != 0) && (cellToCheck + 1 == currentCell)) ||
                    (instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().currentStatus[2] && (cellToCheck % size.x != 0) && (cellToCheck - 1 == currentCell))
                )
                    instantiatedRooms[cellToCheck].SetActive(true);
                else
                    instantiatedRooms[cellToCheck].SetActive(false);
                

                
                // print((cellToCheck - size.x >= 0) + " + " + (cellToCheck - size.x == currentCell));
                // print((cellToCheck + size.x < board.Count) + " + " + (cellToCheck + size.x == currentCell));
                // print(((cellToCheck+1) % size.x != 0) + " + " + (cellToCheck + 1 == currentCell));
                // print((cellToCheck % size.x != 0) + " + " + (cellToCheck - 1 == currentCell));
            }
    }

    void GenerateDungeon()
    {
        for(int i = 0; i<size.x; i++)
            for(int j = 0; j<size.y; j++)
            {
                Cell currentCell = board[Mathf.FloorToInt(i + j * size.x)];
                if(currentCell.visited)
                {
                    int cellID = Mathf.FloorToInt(i + j * size.x);
                    GameObject newRoom = Instantiate(room[color], new Vector3(i*offset.x, 0, -j*offset.y), Quaternion.identity, transform);
                    RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                    script.UpdateRoom(cellID, board[cellID].status);
                    newRoom.name += " " + i + ":" + j;
                    if(cellID != 0) newRoom.SetActive(false);
                    instantiatedRooms[cellID] = newRoom;
                }
            }

    }

    void MazeGenerator()
    {
        instantiatedRooms = new Dictionary<int, GameObject>();
        board = new List<Cell>();
        for(int i = 0; i<size.x; i++)
            for(int j = 0; j<size.y; j++)
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
        if(cell - size.x >= 0 && !board[Mathf.FloorToInt(cell-size.x)].visited)
            neighbour.Add(Mathf.FloorToInt(cell-size.x));
        // Check Bottom Neighbour
        if(cell + size.x < board.Count && !board[Mathf.FloorToInt(cell+size.x)].visited)
            neighbour.Add(Mathf.FloorToInt(cell+size.x));
        // Check Right Neighbour
        if((cell+1) % size.x != 0 && !board[Mathf.FloorToInt(cell+1)].visited)
            neighbour.Add(Mathf.FloorToInt(cell+1));
        // Check Left Neighbour
        if(cell % size.x != 0 && !board[Mathf.FloorToInt(cell-1)].visited)
            neighbour.Add(Mathf.FloorToInt(cell-1));

        return neighbour;
    }

    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }
}

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public struct RoomType{
    public GameObject[] variants;
    public GameObject[] largeVariants;
}

public class DFSAlgorithm : MonoBehaviour
{
    [SerializeField] int seed;
    public static DFSAlgorithm instance {get; private set;}
    [SerializeField] Slider healthSlider;
    [SerializeField] RoomType[] room;
    [SerializeField] GameObject bossPrefab;
    [SerializeField] GameObject sentryPrefab;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject dronePrefab;
    [SerializeField] Vector2 sizeRange;
    [SerializeField] Vector2 roomCountBounds;
    [SerializeField] Vector2 largeOffset;
    [SerializeField] Vector2 largeRoomProbability;
    [SerializeField] int startPos = 0;
    [SerializeField] PlayerState settings;
    int roomType;
    int size;
    bool bossSpawned;

    List<Cell> board;
    Dictionary<int, GameObject> instantiatedRooms;

    void Start()
    {
        instance = this;
        size = UnityEngine.Random.Range((int)sizeRange.x, (int)sizeRange.y+1);
        roomType = UnityEngine.Random.Range(0, room.Length);
        MazeGenerator();
        UpdateVisibility();
    }

    public void UpdateVisibility(int currentCell = 0) // for optimization, non visible rooms are disabled
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
                {
                    instantiatedRooms[cellToCheck].SetActive(true);
                    instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().RefreshRoom(true);
                }
                else
                {
                    instantiatedRooms[cellToCheck].SetActive(false);
                    instantiatedRooms[cellToCheck].GetComponent<RoomBehaviour>().RefreshRoom(false);
                }
            }
    }

    System.Collections.IEnumerator DebugGenerate(List<Cell> b, int[] indices)
    {
        Transform t = new GameObject("Debug").transform;
        foreach(int ix in indices)
        {
            RoomBehaviour script;
            if(b[ix].status[4]) script = Instantiate(room[roomType].largeVariants[3], new Vector3(board[ix].y*largeOffset.x, 0, -board[ix].x*largeOffset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
            else script = Instantiate(room[roomType].variants[1], new Vector3(board[ix].y*largeOffset.x, 0, -board[ix].x*largeOffset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
            script.UpdateRoom(ix, b[ix].status, false, sentryPrefab, enemyPrefab, dronePrefab, null, healthSlider);
            script.transform.SetParent(t);
            yield return new WaitForSeconds(1f);
        }

        for(int i = 0; i<size; i++)
        {
            for(int j = 0; j<size; j++)
            {
                int cellID = i + j * size;
                if(indices.Contains(cellID) || !board[cellID].visited) continue;

                RoomBehaviour script;
                if(b[cellID].status[4]) script = Instantiate(room[roomType].largeVariants[2], new Vector3(board[cellID].y*largeOffset.x, 0, -board[cellID].x*largeOffset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                else script = Instantiate(room[roomType].variants[4], new Vector3(board[cellID].y*largeOffset.x, 0, -board[cellID].x*largeOffset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                script.UpdateRoom(cellID, b[cellID].status, false, sentryPrefab, enemyPrefab, dronePrefab, null, healthSlider);
                script.transform.SetParent(t);
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void GenerateDungeon() // spawns the rooms
    {
        for(int i = 0; i<size; i++)
        {
            for(int j = 0; j<size; j++)
            {
                Cell currentCell = board[Mathf.FloorToInt(i + j * size)];
                if(currentCell.visited)
                {
                    int cellID = Mathf.FloorToInt(i + j * size);
                    
                    GameObject newRoom;
                    if(cellID != 0 && UnityEngine.Random.Range((int)largeRoomProbability.x, (int)largeRoomProbability.y) == (int)largeRoomProbability.x)
                        newRoom = Instantiate(room[roomType].largeVariants[UnityEngine.Random.Range(0, room[roomType].largeVariants.Length)], new Vector3(i*largeOffset.x, 0, -j*largeOffset.y), Quaternion.identity, transform);
                    else
                        newRoom = Instantiate(room[roomType].variants[UnityEngine.Random.Range(0, room[roomType].variants.Length)], new Vector3(i*largeOffset.x, 0, -j*largeOffset.y), Quaternion.identity, transform);

                    RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                    if(board[cellID].status[4]) script.UpdateRoom(cellID, board[cellID].status, true, null, null, null, bossPrefab, healthSlider);
                    else script.UpdateRoom(cellID, board[cellID].status, false, sentryPrefab, enemyPrefab, dronePrefab, null, healthSlider);
                    newRoom.name += " " + i + ":" + j;
                    instantiatedRooms[cellID] = newRoom;

                }
            }
        }
        if(!settings.reflectionEnabled)
            foreach(ReflectionProbe probe in GetComponentsInChildren<ReflectionProbe>())
                Destroy(probe.gameObject);
    }

    void MazeGenerator() // as the name suggests
    {
        instantiatedRooms = new Dictionary<int, GameObject>();
        board = new List<Cell>();
        for(int i = 0; i<size; i++)
        {
            for(int j = 0; j<size; j++)
            {
                Cell c = new Cell();
                c.x = i;
                c.y = j;
                board.Add(c);
            }
        }

        int currentCell = startPos;
        Stack<int> path = new Stack<int>();
        int k = 0;

        while(k<1000)
        {
            k++;
            board[currentCell].visited = true;
            if((currentCell == board.Count-1 && path.Count >= roomCountBounds.x) || path.Count >= roomCountBounds.y) 
            {
                bossSpawned = true;
                board[currentCell].status[4] = true;
                path.Push(currentCell);
                break;
            }

            
            List<int> neighbours = CheckNeighbours(currentCell);
            if(neighbours.Count == 0)
            {
                if(path.Count == 0) 
                {
                    path.Push(currentCell);
                    break;
                }
                else 
                {
                    currentCell = path.Pop();
                }
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
        if(!bossSpawned)
        {
            Debug.LogError("Boss wasn't spawned, retrying. Bad Seed");
            Debug.Log(Random.seed);
            settings.DebugSeed = true;
            SceneManager.LoadScene(1);
        }

        GenerateDungeon();
    }

    List<int> CheckNeighbours(int cell)
    {
        List<int> neighbour = new List<int>();

        // Check Up Neighbour
        if(cell - size >= 0 && !board[cell-size].visited)
            neighbour.Add(cell-size);
        // Check Bottom Neighbour
        if(cell + size < board.Count && !board[cell+size].visited)
            neighbour.Add(cell+size);
        // Check Right Neighbour
        if((cell+1) % size != 0 && !board[cell+1].visited)
            neighbour.Add(cell+1);
        // Check Left Neighbour
        if(cell % size != 0 && !board[cell-1].visited)
            neighbour.Add(cell-1);


        // // Check Up Neighbour
        // if(cell - size >= 0 && !board[cell-size].visited)
        //     neighbour.Add(cell-size);
        // // Check Bottom Neighbour
        // if(cell + size < board.Count && !board[cell+size].visited)
        //     neighbour.Add(cell+size);
        // // Check Right Neighbour
        // if((cell+1) % size != 0 && cell+1 < board.Count && !board[cell+1].visited)
        //     neighbour.Add(cell+1);
        // // Check Left Neighbour
        // if((cell-1) % size != 0 && cell-1 >= 0  && !board[cell-1].visited)
        //     neighbour.Add(cell-1);

        return neighbour;
    }

    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[5];
        public int x;
        public int y;
    }
}

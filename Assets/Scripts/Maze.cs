using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Maze : MonoBehaviour
{
    public class Cell
    {
        public int cellIndex;
        public int xSize, ySize;
        public bool visited;
        public GameObject up;
        public GameObject right;
        public GameObject left;
        public GameObject down;
        public GameObject ground;
        public List<Cell> neighbours = new List<Cell>();
        public Cell previous = null;
        public Cell bfsPrevious = null;
        public bool bfsVisited;

        public int x,y;

        public float f = 0;
        public float g = 0;
        public float h = 0;

        public void addNeighbours(Cell[] cell)
        {
            if(x < xSize-1)
            {
                if (cell[cellIndex + 1].left == null)
                {
                    neighbours.Add(cell[cellIndex + 1]); // right cell
                }

            }
            if(x > 0)
            {
                if (cell[cellIndex - 1].right == null)
                {
                    neighbours.Add(cell[cellIndex - 1]); //left cell
                }
            }
            if(y < ySize-1)
            {
                if (cell[cellIndex + xSize].down == null)
                {
                    neighbours.Add(cell[cellIndex + xSize]); // up cell
                }
            }
            if(y > 0)
            {
                if (cell[cellIndex - xSize].up == null)
                {
                    neighbours.Add(cell[cellIndex - xSize]); //down cell
                }
            }
        }

    }

    public class CustomFixedUpdate
    {
        private float m_FixedDeltaTime;
        private float m_ReferenceTime = 0;
        private float m_FixedTime = 0;
        private float m_MaxAllowedTimestep = 0.3f;
        private System.Action m_FixedUpdate;
        private System.Diagnostics.Stopwatch m_Timeout = new System.Diagnostics.Stopwatch();

        public CustomFixedUpdate(float aFixedDeltaTime, System.Action aFixecUpdateCallback)
        {
            m_FixedDeltaTime = aFixedDeltaTime;
            m_FixedUpdate = aFixecUpdateCallback;
        }

        public bool Update(float aDeltaTime)
        {
            m_Timeout.Reset();
            m_Timeout.Start();

            m_ReferenceTime += aDeltaTime;
            while (m_FixedTime < m_ReferenceTime)
            {
                m_FixedTime += m_FixedDeltaTime;
                if (m_FixedUpdate != null)
                    m_FixedUpdate();
                if ((m_Timeout.ElapsedMilliseconds / 1000.0f) > m_MaxAllowedTimestep)
                    return false;
            }
            return true;
        }

        public float FixedDeltaTime
        {
            get { return m_FixedDeltaTime; }
            set { m_FixedDeltaTime = value; }
        }
        public float MaxAllowedTimestep
        {
            get { return m_MaxAllowedTimestep; }
            set { m_MaxAllowedTimestep = value; }
        }
        public float ReferenceTime
        {
            get { return m_ReferenceTime; }
        }
        public float FixedTime
        {
            get { return m_FixedTime; }
        }
    }

    private List<Cell> openSet = new List<Cell>();
    private List<Cell> closedSet = new List<Cell>();
    private List<Cell> pathAstar = new List<Cell>();
    private List<Cell> pathBfs = new List<Cell>();
    private Cell start;
    private Cell end;

    private Cell node;
    private List<Cell> neighbours = new List<Cell>();
    private Queue<Cell> queue = new Queue<Cell>();
    private List<Cell> path2 = new List<Cell>();

    public GameObject wall;
    public GameObject ground;
    public GameObject player;
    public GameObject award;
    public float wallLength = 1.0f;
    private int xSize;
    private int ySize;
    private Vector3 initialPos;
    private GameObject wallHolder;
    private GameObject groundHolder;

    private Cell[] cells;
    private int currentCell = 0;
    private int totalCells;
    private int visitedCells = 0;
    private bool startedBuilding = false;
    private int currentNeighbour = 0;
    private List<int> lastCells;
    private int backingUp = 0;
    private int wallToBreak = 0;
    private Vector3 scaleChange;
    private Vector3 positionChange;
    private Vector3 lastX;
    public Camera cam;
    private bool finishAStar;
    private bool finishBfs;

    private CustomFixedUpdate aStarAlgorithm;

    private System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
    private System.Diagnostics.Stopwatch e = new System.Diagnostics.Stopwatch();

    public Text aStar;
    public Text bfs;
    


    void Start()
    {
        xSize = Utils.x;
        ySize = Utils.y;
        CreateWalls();
        aStarAlgorithm = new CustomFixedUpdate(0.16f, pathFinder2);
    }

    void CreateWalls()
    {
        wallHolder = new GameObject();
        wallHolder.name = "Maze";

        initialPos = new Vector3(-(xSize / 2) + wallLength / 2, 0.0f, (-ySize / 2) + wallLength / 2); 
        Vector3 myPos = initialPos;
        GameObject tempWall;

        //For X Axis
        for(int i = 0; i < ySize; i++)
        {
            for (int j = 0; j <= xSize; j++)
            {
                myPos = new Vector3(initialPos.x + (j*wallLength)-wallLength/2, 0.0f, initialPos.z + (i * wallLength) - wallLength / 2);
                tempWall = Instantiate(wall, myPos, Quaternion.identity) as GameObject;
                tempWall.transform.parent = wallHolder.transform;
                tempWall.tag = "Wall";
                tempWall.GetComponent<MeshRenderer>().material.color = Color.white;
                tempWall.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Color");
            }
            if (i == ySize - 1)
            {
                lastX = myPos;
            }
        }

        //For Y Axis

        for (int i = 0; i <= ySize; i++)
        {
            for (int j = 0; j < xSize; j++)
            {
                myPos = new Vector3(initialPos.x + (j * wallLength), 0.0f, initialPos.z + (i * wallLength) - wallLength);
                tempWall = Instantiate(wall, myPos, Quaternion.Euler(0.0f,90.0f,0.0f)) as GameObject;
                tempWall.transform.parent = wallHolder.transform;
                tempWall.tag = "Wall";
                tempWall.GetComponent<MeshRenderer>().material.color = Color.white;
                tempWall.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Color");
            }
        }
        CreateCells();
    }   
    void CreateCells()
    {
        groundHolder = new GameObject();
        groundHolder.name = "Ground";
        int x = 0;
        int y = 0;
        lastCells = new List<int>();
        lastCells.Clear();
        totalCells = xSize * ySize;
        GameObject[] allWalls;
        int children = wallHolder.transform.childCount;
        allWalls = new GameObject[children];
        cells = new Cell[xSize*ySize];
        int eastWestProcess = 0;
        int childProcess = 0;
        int termCount = 0;
        int cellIndex = 0;
        //
        for (int i = 0; i < children; i++)
        {
            allWalls[i] = wallHolder.transform.GetChild(i).gameObject;
        }
        //
        for (int cellprocess = 0; cellprocess < cells.Length; cellprocess++)
        {
            if (termCount == xSize)
            {
                eastWestProcess++;
                termCount = 0;
            }
            cells[cellprocess] = new Cell();
            cells[cellprocess].cellIndex = cellIndex;
            cells[cellprocess].xSize = xSize;
            cells[cellprocess].ySize = ySize;
            cells[cellprocess].left = allWalls[eastWestProcess];
            Vector3 cellPosition = new Vector3(cells[cellprocess].left.transform.position.x, cells[cellprocess].left.transform.position.y, cells[cellprocess].left.transform.position.z);
            cells[cellprocess].down = allWalls[childProcess+(xSize+1)*(ySize)];
            cells[cellprocess].ground = CreateGround(cellPosition.x,cellPosition.y,cellPosition.z);
            eastWestProcess++;
            termCount++;
            childProcess++;

            cells[cellprocess].right = allWalls[eastWestProcess];
            cells[cellprocess].up = allWalls[(childProcess + (xSize + 1) * ySize)+xSize-1];
            cells[cellprocess].x = x;
            cells[cellprocess].y = y;
            cellIndex++;
            x++;
            if (x == xSize)
            {
                y++;
                x = 0;
            }

        }

        start = cells[0];
        end = cells[(xSize * ySize) - 1];
        cells[0].ground.GetComponent<MeshRenderer>().material.color = Color.green;
        cells[(xSize*ySize)-1].ground.GetComponent<MeshRenderer>().material.color = Color.red;
        CreateMaze();
    }

    void CreateMaze()
    {
        while (visitedCells < totalCells)
        {
            if (startedBuilding)
            {
                GiveNeighbour();
                if(cells[currentNeighbour].visited == false && cells[currentCell].visited == true)
                {
                    BreakWall();
                    cells[currentNeighbour].visited = true;
                    visitedCells++;
                    lastCells.Add(currentCell);
                    currentCell = currentNeighbour;
                    if(lastCells.Count > 0)
                    {
                        backingUp = lastCells.Count - 1;
                    }
                }
            }
            else
            {
                currentCell = Random.Range(0, totalCells);
                cells[currentCell].visited = true;
                visitedCells++;
                startedBuilding = true;
            }
        }

        Debug.Log("Finished");
    }

    void BreakWall()
    {
        switch (wallToBreak)
        {
            case 1:
                Destroy(cells[currentCell].up);
                break;
            case 2:
                Destroy(cells[currentCell].left);
                break;
            case 3:
                Destroy(cells[currentCell].right);
                break;
            case 4:
                Destroy(cells[currentCell].down);
                break;
        }
    }

    void GiveNeighbour()
    {
        
        int length = 0;
        int[] neighbours = new int[4];
        int[] connectingWall = new int[4];
        int check = 0;

        check = ((currentCell + 1) / xSize);
        check -= 1;
        check *= xSize;
        check += xSize;

        //right
        if (currentCell + 1 < totalCells && (currentCell + 1) != check)
        {
            if (cells[currentCell + 1].visited == false)
            {
                neighbours[length] = currentCell + 1;
                connectingWall[length] = 3;
                length++;
            }
        }

        //left
        if (currentCell - 1 >= 0 && currentCell != check)
        {
            if (cells[currentCell - 1].visited == false)
            {
                neighbours[length] = currentCell - 1;
                connectingWall[length] = 2;
                length++;
            }
        }
        //up
        if (currentCell + xSize < totalCells)
        {
            if (cells[currentCell +xSize].visited == false)
            {
                neighbours[length] = currentCell +xSize;
                connectingWall[length] = 1;
                length++;
            }
        }
        //down
        if (currentCell - xSize >=0)
        {
            if (cells[currentCell - xSize].visited == false)
            {
                neighbours[length] = currentCell - xSize;
                connectingWall[length] = 4;
                length++;
            }
        }

        if(length != 0)
        {
            int theChosenOne = Random.Range(0, length);
            currentNeighbour = neighbours[theChosenOne];
            wallToBreak = connectingWall[theChosenOne];
        }
        else
        {
            if(backingUp > 0)
            {
                currentCell = lastCells[backingUp];
                backingUp--;
            }
        }
        

    }
    
    GameObject CreateGround(float x,float y ,float z)
    {
        ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        scaleChange = new Vector3(1, 0.1f ,1);
        positionChange = new Vector3(x+0.5f, y-0.55f, z);
        ground.transform.localScale = scaleChange;
        ground.transform.position = positionChange;
        ground.name = "Ground";
        ground.tag = "Ground";
        ground.transform.parent = groundHolder.transform;
        
        ground.GetComponent<MeshRenderer>().material.color = Color.black;

        return ground;
    }

    void pathFinder2()
    {
        if (openSet.Count > 0 && finishAStar == false)
        {
            e.Start();
            int winner = 0;
            for (int i = 0; i < openSet.Count; i++)
            {

                if (openSet[i].f < openSet[winner].f)
                {
                    winner = i;
                }
            }

            Cell current = openSet[winner];

            if (current == end)
            {
                e.Stop();
                aStar.text = e.ElapsedMilliseconds.ToString() + "ms";
                Cell temp = current;
                pathAstar.Add(temp);
                while (temp.previous != null)
                {
                    pathAstar.Add(temp.previous);
                    temp = temp.previous;
                }

                Debug.Log("DONE!");
                finishAStar = true;
                return;
            }

            openSet.Remove(current);
            closedSet.Add(current);



            List<Cell> neighbours = new List<Cell>();
            neighbours = current.neighbours;
            for (int i = 0; i < neighbours.Count; i++)
            {
                Cell neighbour = current.neighbours[i];

                if (!closedSet.Contains(neighbour))
                {
                    float tempG = current.g + 1;

                    if (openSet.Contains(neighbour))
                    {
                        if (tempG < neighbour.g)
                        {
                            neighbour.g = tempG;
                        }
                    }
                    else
                    {
                        neighbour.g = tempG;
                        openSet.Add(neighbour);
                    }

                    neighbour.h = heuristic(neighbour, end);

                    neighbour.f = neighbour.g + neighbour.h;
                    neighbour.previous = current;
                }
            }
            current.ground.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        else
        {
            for (int i = 0; i < pathAstar.Count; i++)
            {
                pathAstar[i].ground.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            start.ground.GetComponent<MeshRenderer>().material.color = Color.green;
            end.ground.GetComponent<MeshRenderer>().material.color = Color.red;
            pathAstar.Clear();
        }
    }

    float heuristic(Cell a,Cell b)
    {
        float d = Mathf.Abs(a.x-b.x) + Mathf.Abs(a.y-b.y);

        return d;
    }

    void Update()
    {
        aStarAlgorithm.Update(Time.deltaTime);


        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            cam.fieldOfView--;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            cam.fieldOfView++;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i].addNeighbours(cells);
                cells[i].ground.GetComponent<MeshRenderer>().material.color = Color.black;
            }
            openSet.Add(start);
        }
        if (Input.GetKey(KeyCode.B))
        {
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i].addNeighbours(cells);
                cells[i].ground.GetComponent<MeshRenderer>().material.color = Color.black;
            }
            queue.Enqueue(start);
            start.bfsVisited = true;
        }
    }

    private void FixedUpdate()
    {
        if (queue.Count != 0 && finishBfs == false)
        {
            s.Start();
            node = queue.Dequeue();
            neighbours = node.neighbours;

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (neighbours[i].bfsVisited == false)
                {
                    queue.Enqueue(neighbours[i]);
                    neighbours[i].bfsVisited = true;
                    neighbours[i].bfsPrevious = node;
                    neighbours[i].ground.GetComponent<MeshRenderer>().material.color = Color.yellow;
                }
            }

            if (node == end)
            {
                Cell temp = node;
                pathBfs.Add(temp);
                while (temp.bfsPrevious != null)
                {
                    pathBfs.Add(temp.bfsPrevious);
                    temp = temp.bfsPrevious;
                }
                Debug.Log("DONE!");
                finishBfs = true;
                s.Stop();
                bfs.text = s.ElapsedMilliseconds.ToString() + "ms";
                return;
            }
        }
        else
        {
            for (int i = 0; i < pathBfs.Count; i++)
            {
                pathBfs[i].ground.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            start.ground.GetComponent<MeshRenderer>().material.color = Color.green;
            end.ground.GetComponent<MeshRenderer>().material.color = Color.red;
            pathBfs.Clear();
        }
    }

    public void backButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}

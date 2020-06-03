using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class bridges : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMBombModule module;
    public KMAudio Audio;

    public GameObject[] islandsX0, islandsX1, islandsX2, islandsX3, islandsX4, islandsX5, islandsX6;
    public GameObject[][] islandListObj;

    public GameObject[] singlesX0, singlesX1, singlesX2, singlesX3, singlesX4, singlesX5, singlesX6;
    public GameObject[][] verticalSingles;

    public GameObject[] doublesX0, doublesX1, doublesX2, doublesX3, doublesX4, doublesX5, doublesX6;
    public GameObject[][] verticalDoubles;

    public GameObject[] singlesY0, singlesY1, singlesY2, singlesY3, singlesY4, singlesY5, singlesY6, singlesY7, singlesY8;
    public GameObject[][] horizontalSingles;

    public GameObject[] doublesY0, doublesY1, doublesY2, doublesY3, doublesY4, doublesY5, doublesY6, doublesY7, doublesY8;
    public GameObject[][] horizontalDoubles;

    private Island[,] islandGrid;
    private Edge[,] correctEdgeGrid;
    private Edge[,] inputtedEdgeGrid;
    private List<Island> islandList;

    public KMSelectable[] selX0, selX1, selX2, selX3, selX4, selX5, selX6;

    public Material solvedMat, unsolvedMat, selectedMat, overMat;

    private Island currentlySelected = null;

    private bool _lightsOn, selected = false;

    //Logging
    static int _moduleIdCounter = 1;
    int _moduleId;
    private bool moduleSolved;

    void Awake () {
        for (int i = 0; i < 9; i++) {
            var j = i;
            selX0[i].OnInteract += delegate {
                handleIslandPress(0,j);
                return false;
            };
        }
        for (int i = 0; i < 9; i++)
        {
            var j = i;
            selX1[i].OnInteract += delegate {
                handleIslandPress(1, j);
                return false;
            };
        }
        for (int i = 0; i < 9; i++)
        {
            var j = i;
            selX2[i].OnInteract += delegate {
                handleIslandPress(2, j);
                return false;
            };
        }
        for (int i = 0; i < 9; i++)
        {
            var j = i;
            selX3[i].OnInteract += delegate {
                handleIslandPress(3, j);
                return false;
            };
        }
        for (int i = 0; i < 9; i++)
        {
            var j = i;
            selX4[i].OnInteract += delegate {
                handleIslandPress(4, j);
                return false;
            };
        }
        for (int i = 0; i < 9; i++)
        {
            var j = i;
            selX5[i].OnInteract += delegate {
                handleIslandPress(5, j);
                return false;
            };
        }
        for (int i = 0; i < 9; i++)
        {
            var j = i;
            selX6[i].OnInteract += delegate {
                handleIslandPress(6, j);
                return false;
            };
        }

    }

    // Use this for initialization
    void Start () {
        _moduleId = _moduleIdCounter++;
        module.OnActivate += Activate;
    }

    void Activate()
    {
        Init();
        _lightsOn = true;
    }

    void Init() {
        islandListObj = new[] {
            islandsX0, 
            islandsX1, 
            islandsX2, 
            islandsX3, 
            islandsX4, 
            islandsX5, 
            islandsX6
        };

        verticalSingles = new[] {
            singlesX0,
            singlesX1,
            singlesX2,
            singlesX3,
            singlesX4,
            singlesX5,
            singlesX6
        };

        horizontalSingles = new[] {
            singlesY0,
            singlesY1,
            singlesY2,
            singlesY3,
            singlesY4,
            singlesY5,
            singlesY6,
            singlesY7,
            singlesY8
        };

        verticalDoubles = new[] {
            doublesX0,
            doublesX1,
            doublesX2,
            doublesX3,
            doublesX4,
            doublesX5,
            doublesX6
        };

        horizontalDoubles = new[] {
            doublesY0,
            doublesY1,
            doublesY2,
            doublesY3,
            doublesY4,
            doublesY5,
            doublesY6,
            doublesY7,
            doublesY8
        };

        foreach (GameObject[] gList in islandListObj) {
            foreach (GameObject g in gList) {
                g.SetActive(false);
            }
        }
        Debug.LogFormat("[Bridges #{0}] Initiating puzzle generation.", _moduleId);
        IslandsInit();
        drawEdges();
        setupIslands();
        Debug.LogFormat("[Bridges #{0}] Initiating extra bridge generation.", _moduleId);
        addExtraBridges();
        Debug.LogFormat("[Bridges #{0}] Initiating double bridge generation.", _moduleId);
        addDoubleBridges();
        displayIslands();
    }

    void setupIslands() {
        int cap = 325;
        int iterations = 0;
        Debug.LogFormat("[Bridges #{0}] Initiating island generation.", _moduleId);
        int x = Random.Range(0, 7);
        int y = Random.Range(0, 9);
        Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, x, y);
        addIsland(new Island(x, y));
        for (int i = 0; getIslandList().Count < 16; i++) {
            iterations++;
            if (i >= cap) {
                break;
            }
            //Debug.LogFormat("[Bridges #{0}] _______Adding Island. Iteration: {1}_______", _moduleId, i+1);
            Island starter = getRandomIslandFromList();
            //Debug.LogFormat("[Bridges #{0}] Selected starting island location: {1}, {2}", _moduleId, starter.getX(), starter.getY());
            int dir = Random.Range(0, 4); //0=Up, 1=Right, 2=Down, 3=Left
            switch (dir) {
                case 0: //Up
                    if (starter.getY() <= 1) {
                        goto case 2;
                    }
                    //Debug.LogFormat("[Bridges #{0}] Direction to move from starting island: UP", _moduleId);
                    int counter1 = 1;
                    int tempY1 = starter.getY() - 1;
                    while (tempY1 != 0 && getIslandFromGrid(starter.getX(), tempY1) == null && getCorrectEdge(starter.getX(), tempY1) == Edge.None) {
                        counter1++;
                        tempY1--;
                    }
                    //Debug.LogFormat("Counter: {0}, tempY: {1}", counter1, tempY1);
                    if (counter1 <= 1) {
                        //Debug.LogFormat("Counter: {0}; Can't go this way.", counter1);
                    } else {
                        int bottom1 = tempY1 == 0 && getIslandFromGrid(starter.getX(), tempY1) == null && getCorrectEdge(starter.getX(), tempY1) == Edge.None ? tempY1 : (getIslandFromGrid(starter.getX(), tempY1) != null ? tempY1 + 2 : (getCorrectEdge(starter.getX(), tempY1) != Edge.None ? tempY1 + 1 : tempY1 + 2));
                        int top1 = starter.getY() - 1;
                        if (bottom1 >= top1) {
                            //Debug.LogFormat("Lower bound ({0}) greater than upper bound ({1}), can't place island", bottom1, top1);
                        } else {
                            for (int j = bottom1; j <= top1; j++) {
                                int newY = Random.Range(bottom1, top1);
                                if (getIslandFromGrid(starter.getX() == 0 ? 0 : starter.getX() - 1, newY) != null || getIslandFromGrid(starter.getX() == 6 ? 6 : starter.getX() + 1, newY) != null) {
                                    continue;
                                }

                                //Debug.LogFormat("Island would be placed with a y value between {0} and {1}", bottom1, top1 - 1);
                                Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, starter.getX(), newY);
                                addIsland(new Island(starter.getX(), newY));
                                correctlyConnectIslands(starter, getIslandFromGrid(starter.getX(), newY));
                                //Debug.LogFormat("[Bridges #{0}] Put a vertical connection between the island at {1}, {2} and {3}, {4}.", _moduleId, starter.getX(), starter.getY(), starter.getX(), newY);
                                break;
                            }                            
                        }
                    }
                    
                    break;
                case 1: //Right
                    if (starter.getX() >= 5) {
                        goto case 3;
                    }
                    //Debug.LogFormat("[Bridges #{0}] Direction to move from starting island: RIGHT", _moduleId);
                    int counter2 = 1;
                    int tempX2 = starter.getX() + 1;
                    while (tempX2 != 6 && getIslandFromGrid(tempX2, starter.getY()) == null && getCorrectEdge(tempX2, starter.getY()) == Edge.None) {
                        tempX2++;
                        counter2++;
                    }
                    //Debug.LogFormat("Counter: {0}, tempX: {1}", counter2, tempX2);
                    if (counter2 <= 1)
                    {
                        //Debug.LogFormat("Counter: {0}; Can't go this way.", counter2);
                    } else {
                        int bottom2 = starter.getX() + 2;
                        int top2 = tempX2 == 6 && getIslandFromGrid(6, starter.getY()) == null && getCorrectEdge(6, starter.getY()) == Edge.None ? tempX2 + 1 : (getIslandFromGrid(tempX2, starter.getY()) != null ? tempX2 - 2 : tempX2 - 1);
                        if (bottom2 >= top2) {
                            //Debug.LogFormat("Lower bound ({0}) greater than upper bound ({1}), can't place island", bottom2, top2);
                        } else {
                            for (int j = bottom2; j <= top2; j++) {
                                int newX = Random.Range(bottom2, top2);
                                if (getIslandFromGrid(newX, starter.getY() == 0 ? 0 : starter.getY() - 1) != null || getIslandFromGrid(newX, starter.getY() == 8 ? 8 : starter.getY() + 1) != null) {
                                    continue;
                                }
                                //Debug.LogFormat("Island would be placed with an x value between {0} and {1}", bottom2, top2 - 1);
                                Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, newX, starter.getY());
                                addIsland(new Island(newX, starter.getY()));
                                correctlyConnectIslands(starter, getIslandFromGrid(newX, starter.getY()));
                                //Debug.LogFormat("[Bridges #{0}] Put a horizontal connection between the island at {1}, {2} and {3}, {4}.", _moduleId, starter.getX(), starter.getY(), newX, starter.getY());
                                break;
                            }
                        }
                    }
                    break;
                case 2: //Down
                    if (starter.getY() >= 7) {
                        goto case 0;
                    }
                    //Debug.LogFormat("[Bridges #{0}] Direction to move from starting island: DOWN", _moduleId);
                    int counter3 = 1;
                    int tempY3 = starter.getY() + 1;
                    while (tempY3 != 8 && getIslandFromGrid(starter.getX(), tempY3) == null && getCorrectEdge(starter.getX(), tempY3) == Edge.None)
                    {
                        counter3++;
                        tempY3++;
                    }
                    //Debug.LogFormat("Counter: {0}, tempY: {1}", counter3, tempY3);
                    if (counter3 <= 1)
                    {
                        //Debug.LogFormat("Counter: {0}; Can't go this way.", counter3);
                    }
                    else {
                        int bottom3 = starter.getY() + 2;
                        int top3 = tempY3 == 8 && getIslandFromGrid(starter.getX(), tempY3) == null && getCorrectEdge(starter.getX(), tempY3) == Edge.None ? tempY3 + 1 : (getIslandFromGrid(starter.getX(), tempY3) != null ? tempY3 - 1 : tempY3);
                        if (bottom3 >= top3) {
                           // Debug.LogFormat("Lower bound ({0}) greater than upper bound ({1}), can't place island", bottom3, top3);
                        } else {
                            for (int j = bottom3; j <= top3; j++)
                            {
                                int newY = Random.Range(bottom3, top3);
                                if (getIslandFromGrid(starter.getX() == 0 ? 0 : starter.getX() - 1, newY) != null || getIslandFromGrid(starter.getX() == 6 ? 6 : starter.getX() + 1, newY) != null)
                                {
                                    continue;
                                }

                                //Debug.LogFormat("Island would be placed with a y value between {0} and {1}", bottom3, top3 - 1);
                                Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, starter.getX(), newY);
                                addIsland(new Island(starter.getX(), newY));
                                correctlyConnectIslands(starter, getIslandFromGrid(starter.getX(), newY));
                                //Debug.LogFormat("[Bridges #{0}] Put a vertical connection between the island at {1}, {2} and {3}, {4}.", _moduleId, starter.getX(), starter.getY(), starter.getX(), newY);
                                break;
                            }
                        }
                    }
                    break;
                case 3: //Left
                    if (starter.getX() <= 1) {
                        goto case 1;
                    }
                    //Debug.LogFormat("[Bridges #{0}] Direction to move from starting island: LEFT", _moduleId);
                    int counter4 = 1;
                    int tempX4 = starter.getX() - 1;
                    while (tempX4 != 0 && getIslandFromGrid(tempX4, starter.getY()) == null && getCorrectEdge(tempX4, starter.getY()) == Edge.None)
                    {
                        tempX4--;
                        counter4++;
                    }
                    //Debug.LogFormat("Counter: {0}, tempX: {1}", counter4, tempX4);
                    if (counter4 <= 1)
                    {
                        //Debug.LogFormat("Counter: {0}; Can't go this way.", counter4);
                    }
                    else
                    {
                        int top4 = starter.getX() - 1;
                        int bottom4 = tempX4 == 0 && getIslandFromGrid(0, starter.getY()) == null && getCorrectEdge(0, starter.getY()) == Edge.None ? tempX4 : (getIslandFromGrid(tempX4, starter.getY()) != null ? tempX4 + 2 : tempX4 + 1);
                        if (bottom4 >= top4)
                        {
                            //Debug.LogFormat("Lower bound ({0}) greater than upper bound ({1}), can't place island", bottom4, top4);
                        }
                        else
                        {
                            for (int j = bottom4; j <= top4; j++)
                            {
                                int newX = Random.Range(bottom4, top4);
                                if (getIslandFromGrid(newX, starter.getY() == 0 ? 0 : starter.getY() - 1) != null || getIslandFromGrid(newX, starter.getY() == 8 ? 8 : starter.getY() + 1) != null)
                                {
                                    continue;
                                }
                                //Debug.LogFormat("Island would be placed with an x value between {0} and {1}", bottom4, top4 - 1);
                                Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, newX, starter.getY());
                                addIsland(new Island(newX, starter.getY()));
                                correctlyConnectIslands(starter, getIslandFromGrid(newX, starter.getY()));
                                //Debug.LogFormat("[Bridges #{0}] Put a horizontal connection between the island at {1}, {2} and {3}, {4}.", _moduleId, starter.getX(), starter.getY(), newX, starter.getY());
                                break;
                            }
                        }
                    }
                    break;
            }
        }
        Debug.LogFormat("[Bridges #{0}] Completed island placement after {1} iterations (Capped at about {2}).", _moduleId, iterations, cap + 1);
    }

    void addExtraBridges() {
        int extraBridges = 0;
        for (int i = 0; i < 30; i++)
        {
            Island first = getRandomIslandFromList();
            Island second;
            int x = first.getX();
            int y = first.getY();
            int dir = Random.Range(0, 4); //0=Up, 1=Right, 2=Down, 3=Left
            switch (dir)
            {
                case 0:
                    if (first.getY() <= 1 || getCorrectEdge(x, y - 1) != Edge.None)
                    {
                        break;
                    }

                    int newY = y;
                    do
                    {
                        newY--;
                    } while (getCorrectEdge(x, newY) == Edge.None && getIslandFromGrid(x, newY) == null && newY != 0);

                    second = getIslandFromGrid(x, newY);
                    if (second == null)
                    {
                        break;
                    }

                    correctlyConnectIslands(first, second);
                    extraBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put an extra vertical connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, x, newY);

                    break;
                case 1:
                    if (first.getX() >= 5 || getCorrectEdge(x + 1, y) != Edge.None)
                    {
                        break;
                    }

                    int newX = x;
                    do
                    {
                        newX++;
                    } while (getCorrectEdge(newX, y) == Edge.None && getIslandFromGrid(newX, y) == null && newX != 6);

                    second = getIslandFromGrid(newX, y);
                    if (second == null)
                    {
                        break;
                    }

                    correctlyConnectIslands(first, second);
                    extraBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put an extra horizontal connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, newX, y);

                    break;
                case 2:
                    if (first.getY() >=7  || getCorrectEdge(x, y + 1) != Edge.None)
                    {
                        break;
                    }

                    int newY2 = y;
                    do
                    {
                        newY2++;
                    } while (getCorrectEdge(x, newY2) == Edge.None && getIslandFromGrid(x, newY2) == null && newY2 != 8);

                    second = getIslandFromGrid(x, newY2);
                    if (second == null)
                    {
                        break;
                    }

                    correctlyConnectIslands(first, second);
                    extraBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put an extra vertical connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, x, newY2);

                    break;
                case 3:
                    if (first.getX() <= 1 || getCorrectEdge(x - 1, y) != Edge.None)
                    {
                        break;
                    }

                    int newX2 = x;
                    do
                    {
                        newX2--;
                    } while (getCorrectEdge(newX2, y) == Edge.None && getIslandFromGrid(newX2, y) == null && newX2 != 0);

                    second = getIslandFromGrid(newX2, y);
                    if (second == null)
                    {
                        break;
                    }
                    correctlyConnectIslands(first, second);
                    extraBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put an extra horizontal connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, newX2, y);

                    break;
            }
        }
        Debug.LogFormat("[Bridges #{0}] Added {1} extra connection(s).", _moduleId, extraBridges);
    }

    void addDoubleBridges() {
        int doubleBridges = 0;
        for (int i = 0; i < 16; i++) {
            Island first = getRandomIslandFromList();
            Island second;
            int x = first.getX();
            int y = first.getY();
            int dir = Random.Range(0, 4); //0=Up, 1=Right, 2=Down, 3=Left
            switch (dir) {
                case 0:
                    if (first.getY() <= 1 || getCorrectEdge(x, y - 1) != Edge.Vertical) {
                        break;
                    }

                    int newY = y;
                    do {
                        newY--;
                    } while (getCorrectEdge(x, newY) == Edge.Vertical);

                    second = getIslandFromGrid(x, newY);
                    if (second == null)
                    {
                        break;
                    }

                    correctlyDoubleConnectIslands(first, second);
                    doubleBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put a double vertical connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, x, newY);

                    break;
                case 1:
                    if (first.getX() >= 5 || getCorrectEdge(x + 1, y) != Edge.Horizontal)
                    {
                        break;
                    }

                    int newX = x;
                    do
                    {
                        newX++;
                    } while (getCorrectEdge(newX, y) == Edge.Horizontal);

                    second = getIslandFromGrid(newX, y);
                    if (second == null)
                    {
                        break;
                    }

                    correctlyDoubleConnectIslands(first, second);
                    doubleBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put a double horizontal connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, newX, y);

                    break;
                case 2:
                    if (first.getY() >= 7 || getCorrectEdge(x, y + 1) != Edge.Vertical)
                    {
                        break;
                    }

                    int newY2 = y;
                    do
                    {
                        newY2++;
                    } while (getCorrectEdge(x, newY2) == Edge.Vertical);

                    second = getIslandFromGrid(x, newY2);
                    if (second == null)
                    {
                        break;
                    }

                    correctlyDoubleConnectIslands(first, second);
                    doubleBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put a double vertical connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, x, newY2);

                    break;
                case 3:
                    if (first.getX() <= 1 || getCorrectEdge(x - 1, y) != Edge.Horizontal)
                    {
                        break;
                    }

                    int newX2 = x;
                    do
                    {
                        newX2--;
                    } while (getCorrectEdge(newX2, y) == Edge.Horizontal);

                    second = getIslandFromGrid(newX2, y);
                    if (second == null) {
                        break;
                    }
                    correctlyDoubleConnectIslands(first, second);
                    doubleBridges++;
                    //Debug.LogFormat("[Bridges #{0}] Put a double horizontal connection between the island at {1}, {2} and {3}, {4}.", _moduleId, x, y, newX2, y);

                    break;
            }
        }
        Debug.LogFormat("[Bridges #{0}] Made {1} connection(s) doubled.", _moduleId, doubleBridges);
    }

    void displayIslands() {
        foreach (Island i in getIslandList()) {
            int x = i.getX();
            int y = i.getY();
            islandListObj[x][y].GetComponentInChildren<TextMesh>().text = i.getNeededConnections().ToString();
            islandListObj[x][y].SetActive(true);
            islandListObj[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
        }
    }

    void handleIslandPress(int x, int y) {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, selX3[4].transform);
        if (!_lightsOn || moduleSolved) return;

        Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} pressed.", _moduleId, x, y);
        Island clicked = getIslandFromGrid(x, y);
        //Debug.LogFormat("[Bridges #{0}] Made it past getting the clicked island from the grid.", _moduleId);
        if (clicked == null) {
            Debug.LogFormat("[Bridges #{0}] For some reason that island doesn't exist. Not sure how you clicked it. Auto solving. Please contact AAces#2652 on discord with this log file so we can fix this.", _moduleId);
            module.HandlePass();
            moduleSolved = true;
            return;
        }

        if (!selected) {
            currentlySelected = clicked;
            //Debug.LogFormat("[Bridges #{0}] That was the first.", _moduleId);
            selected = true;
            islandListObj[x][y].GetComponent<MeshRenderer>().material = selectedMat;            
        } else {
            if (currentlySelected.getX() == x && currentlySelected.getY() == y) {
                //Debug.LogFormat("[Bridges #{0}] Clicked the same, made it past the .Equals check", _moduleId);
                selected = false;
                Debug.LogFormat("[Bridges #{0}] Unselecting island at {1}, {2}.", _moduleId, x, y);
                if (clicked.getCurrentConnections() == clicked.getNeededConnections())
                {
                    islandListObj[x][y].GetComponent<MeshRenderer>().material = solvedMat;
                }
                else if (clicked.getCurrentConnections() > clicked.getNeededConnections()) {
                    islandListObj[x][y].GetComponent<MeshRenderer>().material = overMat;
                } else
                {
                    islandListObj[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
                }
                
                return;
            } else {
                //Debug.LogFormat("[Bridges #{0}] Didn't click the same, made it past the .Equals check", _moduleId);
                String error = playerConnect(currentlySelected, clicked);
                //Debug.LogFormat("[Bridges #{0}] That was the second.", _moduleId);
                if (error.Equals("")) {
                    selected = false;
                    drawEdges();
                    checkSolution();
                } else {
                    module.HandleStrike();
                    selected = false;
                    foreach (Island i in getIslandList()) {
                        if (i.getCurrentConnections() == i.getNeededConnections()) {
                            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = solvedMat;
                        } else if (i.getCurrentConnections() > i.getNeededConnections()) {
                            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = overMat;
                        } else {
                            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = unsolvedMat;
                        }
                    }

                    Debug.LogFormat("[Bridges #{0}] Strike! {1} Please contact AAces#2652 on discord with this log file if you feel that this is an error.", _moduleId, error);
                }
            }
        }
    }

    void drawEdges() {
        foreach (var g in verticalSingles) {
            foreach (var h in g) {
                h.SetActive(false);
            }
        }
        foreach (var g in verticalDoubles)
        {
            foreach (var h in g)
            {
                h.SetActive(false);
            }
        }
        foreach (var g in horizontalSingles)
        {
            foreach (var h in g)
            {
                h.SetActive(false);
            }
        }
        foreach (var g in horizontalDoubles)
        {
            foreach (var h in g)
            {
                h.SetActive(false);
            }
        }

        for (var x = 0; x < 7; x++) {
            for (var y = 0; y < 9; y++) {
                switch (getInputtedEdge(x, y)) {
                    case Edge.None:
                        break;
                    case Edge.Vertical:
                        verticalSingles[x][y].SetActive(true);
                        break;
                    case Edge.DoubleVertical:
                        verticalDoubles[x][y].SetActive(true);
                        break;
                    case Edge.Horizontal: //Horizontal need x and y flipped because of how the arrays work
                        horizontalSingles[y][x].SetActive(true);
                        break;
                    case Edge.DoubleHorizontal:
                        horizontalDoubles[y][x].SetActive(true);
                        break;
                }
            }
        }
    }

    void checkSolution() {
        bool solved = true;
        foreach (Island i in getIslandList()) {
            if (i.getCurrentConnections() == i.getNeededConnections()) {
                islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = solvedMat;
            } else if (i.getCurrentConnections() > i.getNeededConnections())
            {
                islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = overMat;
                solved = false;
            } else {
                islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = unsolvedMat;
                solved = false;
            }
        }

        if (solved) {
            module.HandlePass();
            moduleSolved = true;
            Debug.LogFormat("[Calendar #{0}] Module Solved!", _moduleId);
        }
    }

    public  void IslandsInit()
    {
        islandGrid = new Island[,]{
            { null, null, null, null, null, null, null, null, null },
            { null, null, null, null, null, null, null, null, null },
            { null, null, null, null, null, null, null, null, null },
            { null, null, null, null, null, null, null, null, null },
            { null, null, null, null, null, null, null, null, null },
            { null, null, null, null, null, null, null, null, null },
            { null, null, null, null, null, null, null, null, null }
        };

        correctEdgeGrid = new[,]{
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
        };

        inputtedEdgeGrid = new[,]{
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
        };

        islandList = new List<Island>();
    }

    void addIsland(Island island)
    {
        islandGrid[island.getX(), island.getY()] = island;
        islandList.Add(island);
    }

    Island getIslandFromGrid(int x, int y)
    {
        return islandGrid[x, y];
    }

    Edge getCorrectEdge(int x, int y)
    {
        return correctEdgeGrid[x, y];
    }

    void setCorrectEdge(int x, int y, Edge edge)
    {
        correctEdgeGrid[x, y] = edge;
    }

    Edge getInputtedEdge(int x, int y)
    {
        return inputtedEdgeGrid[x, y];
    }

    void setInputtedEdge(int x, int y, Edge edge)
    {
        inputtedEdgeGrid[x, y] = edge;
    }

    Island getRandomIslandFromList()
    {
        return islandList[Random.Range(0, islandList.Count)];
    }

    List<Island> getIslandList()
    {
        return islandList;
    }

    void correctlyConnectIslands(Island is1, Island is2)
    {
        int x1 = is1.getX();
        int x2 = is2.getX();
        int y1 = is1.getY();
        int y2 = is2.getY();

        if (x1 == x2 && y1 != y2)
        {
            for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
            {
                setCorrectEdge(x1, y, Edge.Vertical);

            }
        }
        else if (y1 == y2)
        {
            for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
            {
                setCorrectEdge(x, y1, Edge.Horizontal);

            }
        }
        else
        {
            return;
        }

        is1.addNeededConnection();
        is2.addNeededConnection();

    }

    void correctlyDoubleConnectIslands(Island is1, Island is2)
    {
        int x1 = is1.getX();
        int x2 = is2.getX();
        int y1 = is1.getY();
        int y2 = is2.getY();

        if (x1 == x2 && y1 != y2)
        {
            for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
            {
                setCorrectEdge(x1, y, Edge.DoubleVertical);
            }
        }
        else if (y1 == y2)
        {
            for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
            {
                setCorrectEdge(x, y1, Edge.DoubleHorizontal);
            }
        }
        else
        {
            return;
        }

        is1.addNeededConnection();
        is2.addNeededConnection();

    }

    String playerConnect(Island is1, Island is2)
    { //true=strike
        int x1 = is1.getX();
        int x2 = is2.getX();
        int y1 = is1.getY();
        int y2 = is2.getY();

        if (!(x1 == x2 || y1 == y2))
        {
            return "You can not connect these two islands, they do not share an x or y coordinate!";
        }

        if (x1 == x2)
        {
            for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
            {
                if (getIslandFromGrid(x1, y) != null)
                {
                    return "You can not connect these two islands, there is an island in the way at " + x1 + ", " + y + "!";
                }

                if (getInputtedEdge(x1, y) == Edge.Horizontal)
                {
                    return "You can not connect these two islands, you already have a bridge at " + x1 + ", " + y + "!";
                }

                if (getInputtedEdge(x1, y) == Edge.DoubleHorizontal)
                {
                    return "You can not connect these two islands, you already have a double bridge at " + x1 + ", " + y + "!";
                }
            }

            if (getInputtedEdge(x1, (y1 < y2 ? y1 : y2) + 1) == Edge.None)
            {
                for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
                {
                    setInputtedEdge(x1, y, Edge.Vertical);
                }
                is1.addCurrentConnections();
                is2.addCurrentConnections();
            }
            else if (getInputtedEdge(x1, (y1 < y2 ? y1 : y2) + 1) == Edge.Vertical)
            {
                for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
                {
                    setInputtedEdge(x1, y, Edge.DoubleVertical);
                }
                is1.addCurrentConnections();
                is2.addCurrentConnections();
            }
            else if (getInputtedEdge(x1, (y1 < y2 ? y1 : y2) + 1) == Edge.DoubleVertical)
            {
                for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
                {
                    setInputtedEdge(x1, y, Edge.None);
                }
                is1.subtractTwoCurrentConnections();
                is2.subtractTwoCurrentConnections();
            }

            return "";

        }
        else if (y1 == y2)
        {
            for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
            {
                if (getIslandFromGrid(x, y1) != null)
                {
                    return "You can not connect these two islands, there is an island in the way at " + x + ", " + y1 + "!";
                }

                if (getInputtedEdge(x, y1) == Edge.Vertical)
                {
                    return "You can not connect these two islands, you already have a bridge at " + x + ", " + y1 + "!";
                }

                if (getInputtedEdge(x, y1) == Edge.DoubleVertical)
                {
                    return "You can not connect these two islands, you already have a double bridge at " + x + ", " + y1 + "!";
                }
            }
            if (getInputtedEdge((x1 < x2 ? x1 : x2) + 1, y1) == Edge.None)
            {
                for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
                {
                    setInputtedEdge(x, y1, Edge.Horizontal);
                }
                is1.addCurrentConnections();
                is2.addCurrentConnections();
            }
            else if (getInputtedEdge((x1 < x2 ? x1 : x2) + 1, y1) == Edge.Horizontal)
            {
                for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
                {
                    setInputtedEdge(x, y1, Edge.DoubleHorizontal);
                }

                is1.addCurrentConnections();
                is2.addCurrentConnections();
            }
            else if (getInputtedEdge((x1 < x2 ? x1 : x2) + 1, y1) == Edge.DoubleHorizontal)
            {
                for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
                {
                    setInputtedEdge(x, y1, Edge.None);
                }

                is1.subtractTwoCurrentConnections();
                is2.subtractTwoCurrentConnections();
            }

            return "";
        }

        return "";
    }

}

class Island {
    private int x, y, neededConnections, currentConnections;

    public Island(int x, int y) {
        this.x = x;
        this.y = y;
        this.neededConnections = 0;
        this.currentConnections = 0;
    }

    public int getX() {
        return this.x;
    }

    public int getY() {
        return this.y;
    }

    public int getNeededConnections() {
        return this.neededConnections;
    }

    public void addNeededConnection() {
        //Debug.Log("Added needed connection to the island at " + this.x + " " + this.y);
        this.neededConnections++;
    }

    public int getCurrentConnections() {
        return this.currentConnections;
    }

    public void addCurrentConnections() {
        this.currentConnections++;
    }

    public void subtractTwoCurrentConnections() {
        this.currentConnections-=2;
    }

    
}

enum Edge {
    Vertical,
    Horizontal,
    None,
    DoubleVertical,
    DoubleHorizontal
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class bridges : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMBombModule module;
    public KMAudio Audio;

    public GameObject allIslands, allBridges;

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
    private Edge[,] inputtedEdgeGrid, solutionEdgeGrid, correctEdgeGrid;
    private List<Island> islandList;

    public KMSelectable[] selX0, selX1, selX2, selX3, selX4, selX5, selX6;
    public KMSelectable submit;

    public Material solvedMat, unsolvedMat, selectedMat, overMat, LEDGreen, LEDRed;
    public MeshRenderer indicator;
    public Sprite[] images;

    private Island currentlySelected = null;

    private bool _lightsOn, selected = false;
    private int shift = 0;

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

        submit.OnInteract += delegate {
            handleSubmitPress();
            return false;
        };

    }

    // Use this for initialization
    void Start () {
        _moduleId = _moduleIdCounter++;
        module.OnActivate += Activate;
        Init();
    }

    void Activate()
    {
        
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
        generatePuzzle(true);
        log();
        displayIslands();
        allBridges.SetActive(true);
        allIslands.SetActive(true);
    }

    private int generationCount = 0;
    void generatePuzzle(bool checkU) {
        generationCount++;
        Debug.LogFormat("[Bridges #{0}] Initiating puzzle generation: attempt #{1}.", _moduleId, generationCount);
        IslandsInit();
        drawEdges();
        setupIslands();
        addExtraBridges();
        addDoubleBridges();
        updateNeighbors();
        if (checkU) {
            checkUniqueness();
        }
    }

    private int solutionAttemptCounter = 0, solutionTotalAttemptCounter = 0, regenerationCounter = 0;

    private String message = "";
    void checkUniqueness() {
        foreach (Island i in getIslandList()) {
            i.resetSolutionAttempt();
        }
        solutionEdgeGrid = new[,]{
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
            { Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None, Edge.None },
        };
        for (int i = 0; i < 5; i++) {
            //Debug.Log("Iteration " + (i+1));
            foreach (Island island in getIslandList()) {
                if (island.isSolved()) {
                    //Debug.LogFormat("[Bridges #{0}] Island {1}, {2} solved, skipping.", _moduleId, island.getX(), island.getY());
                    continue;
                }

                int remainingConnections = island.getNeededConnections() - island.getSolutionConnections();
                //Debug.LogFormat("[Bridges #{0}] Island {1}, {2}: Needed Connections: {3}. Remaining Connections: {4}. Neighbors: {5}. Connected Neighbors: {6}.", _moduleId, island.getX(), island.getY(), island.getNeededConnections(), remainingConnections, island.getNeighbors().Count, island.getConnectedNeighbors().Count);

                switch (island.getNeededConnections()) {
                    case 1:
                        if (island.getNeighbors().Count == 1 && remainingConnections == 1) {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            break;
                        }

                        foreach (Island n in island.getNeighbors()) {
                            if (n.getNeededConnections() == 1 || n.isSolved()) {
                                n.addBannedNeighbor(island);
                                island.addBannedNeighbor(n);
                            }
                        }

                        break;
                    case 2:
                        foreach (Island n in island.getNeighbors())
                        {
                            if (n.isSolved())
                            {
                                n.addBannedNeighbor(island);
                                island.addBannedNeighbor(n);
                            }
                        }
                        if (island.getNeighbors().Count == 1 && remainingConnections == 2)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            break;
                        }
                        /*if (island.getConnectedNeighbors().Count == 1 && island.getNeighbors().Count >= 1) { //This breaks it for some reason? But I kinda need it
                            if (island.getConnectedNeighbors()[0].getNeededConnections() == 1) {
                                    foreach (Island n in island.getNeighbors()) {
                                        if (n.getNeededConnections() == 1) {
                                            n.addBannedNeighbor(island);
                                            island.addBannedNeighbor(n);
                                        }
                                    }
                            }
                        }*/
                        if (island.getNeighbors().Count == 2 && island.getConnectedNeighbors().Count == 0) {
                            if (island.getNeighbors()[0].getNeededConnections() == 2) {
                                solutionCheckConnection(island, island.getNeighbors()[1]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                                break;
                            } else if (island.getNeighbors()[1].getNeededConnections() == 2) {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                                break;
                            }
                        }
                        if (island.getNeighbors().Count == 1 && remainingConnections == 1) {
                            if (island.getConnectedNeighbors().Count == 0) {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                                break;
                            } else if (island.getConnectedNeighbors().Count == 1) {
                                if (island.getConnectedNeighbors()[0].isSolved())
                                {
                                    solutionCheckConnection(island, island.getNeighbors()[0]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                                    break;
                                }
                                else if (island.getNeighbors()[0].isSolved())
                                {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());

                                    break;
                                }
                            }

                            break;
                        }
                        if (island.getConnectedNeighbors().Count == 1 && island.getNeighbors().Count == 0 && remainingConnections == 1) {
                            solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());

                            break;
                        }
                        break;
                    case 3:
                        foreach (Island n in island.getNeighbors())
                        {
                            if (n.isSolved())
                            {
                                n.addBannedNeighbor(island);
                                island.addBannedNeighbor(n);
                            }
                        }
                        if (island.getNeighbors().Count == 2 && remainingConnections == 3)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());

                            break;
                        }
                        if (island.getNeighbors().Count == 1 && island.getConnectedNeighbors().Count == 0 && remainingConnections == 2)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            break;
                        }
                        if (island.getNeighbors().Count == 1 && remainingConnections == 1)
                        {
                            if (island.getConnectedNeighbors().Count == 1) {
                                if (!island.getNeighbors()[0].isSolved()) {
                                    solutionCheckConnection(island, island.getNeighbors()[0]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                                    break;
                                }
                            }

                            if (island.getConnectedNeighbors().Count == 2) {
                                if (island.getConnectedNeighbors()[0].isSolved() && island.getConnectedNeighbors()[1].isSolved()) {
                                    if (!island.getNeighbors()[0].isSolved())
                                    {
                                        solutionCheckConnection(island, island.getNeighbors()[0]);
                                        //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                                        break;
                                    }
                                }
                            }
                        }
                        if (island.getNeighbors().Count == 1 && remainingConnections == 2 && island.getConnectedNeighbors().Count == 1)
                        {

                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                            break;
                            
                        }

                        if (island.getNeighbors().Count == 0 && island.getConnectedNeighbors().Count == 2) {
                            if(island.getConnectedNeighbors()[0].isSolved()) {
                                solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());

                                break;
                            }
                            if (island.getConnectedNeighbors()[1].isSolved()) {
                                solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());

                                break;
                            }

                        }

                        if (island.getNeighbors().Count == 2 && island.getConnectedNeighbors().Count == 1 && remainingConnections == 1) {
                            if (island.getNeighbors()[0].isSolved())
                            {
                                solutionCheckConnection(island, island.getNeighbors()[1]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());

                                break;
                            }
                            if (island.getNeighbors()[1].isSolved())
                            {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                                break;
                            }
                        }
                        break;
                    case 4:
                        foreach (Island n in island.getNeighbors())
                        {
                            if (n.isSolved())
                            {
                                n.addBannedNeighbor(island);
                                island.addBannedNeighbor(n);
                            }
                        }
                        if (island.getNeighbors().Count == 2 && remainingConnections == 4)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            break;
                        }
                        if (island.getNeighbors().Count == 1 && island.getConnectedNeighbors().Count == 1) {
                            if (remainingConnections == 3) {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                                break;
                            } else if (remainingConnections == 2) {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                                break;
                            }
                        }
                        if (island.getNeighbors().Count == 1 && remainingConnections == 2 && island.getConnectedNeighbors().Count == 0)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());

                            break;
                        }
                        if (island.getNeighbors().Count == 1 && remainingConnections == 2 && island.getConnectedNeighbors().Count == 2)
                        {
                            if (island.getConnectedNeighbors()[0].isSolved() && island.getConnectedNeighbors()[1].isSolved()) {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                                break;
                            }
                        }

                        if (island.getNeighbors().Count == 2 && remainingConnections == 3 && island.getConnectedNeighbors().Count == 1) {
                            if (island.getConnectedNeighbors()[0].isSolved()) {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                solutionCheckConnection(island, island.getNeighbors()[1]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());

                                break;
                            }
                        }

                        if (island.getNeighbors().Count == 0 && island.getConnectedNeighbors().Count == 2) {
                            int r = 0;
                            while (island.getSolutionConnections() != 4)
                            {
                                r++;
                                if (r >= 10)
                                {
                                    //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} took too long to connect, skipping.", _moduleId, island.getX(), island.getY());
                                    break;
                                }
                                solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());

                                if (island.getSolutionConnections() == 4) break;
                                solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());

                                if (island.getSolutionConnections() == 4) break;
                            }

                            break;
                        }

                        if (island.getNeighbors().Count == 1 && island.getConnectedNeighbors().Count == 2) {
                            if (island.getConnectedNeighbors()[0].isSolved() && island.getConnectedNeighbors()[1].isSolved()) {
                                solutionCheckConnection(island, island.getNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                                break;
                            }

                            if (island.getNeighbors()[0].isSolved()) {
                                if (island.getConnectedNeighbors()[0].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());
                                } else if (island.getConnectedNeighbors()[1].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());
                                }
                            }
                        }

                        if (island.getConnectedNeighbors().Count == 3) {
                            List<Island> l = island.getConnectedNeighbors();
                            if (l[0].isSolved() && l[1].isSolved())
                            {
                                solutionCheckConnection(island, island.getConnectedNeighbors()[2]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[2].getX(), island.getConnectedNeighbors()[2].getY());
                                break;
                            }

                            if (l[0].isSolved() && l[2].isSolved())
                            {
                                solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());
                                break;
                            }

                            if (l[1].isSolved() && l[2].isSolved())
                            {
                                solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());
                                break;
                            }
                        }

                        break;
                    case 5:
                        foreach (Island n in island.getNeighbors())
                        {
                            if (n.isSolved())
                            {
                                n.addBannedNeighbor(island);
                                island.addBannedNeighbor(n);
                            }
                        }
                        if (island.getNeighbors().Count == 3 && remainingConnections == 5)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            solutionCheckConnection(island, island.getNeighbors()[2]);
                            //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} connected to the islands at {3}, {4} and {5}, {6} and {7}, {8}.", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY(), island.getNeighbors()[2].getX(), island.getNeighbors()[2].getY());
                            break;
                        }
                        if (island.getNeighbors().Count == 2 && island.getConnectedNeighbors().Count == 1 && (remainingConnections == 4 || remainingConnections == 3))
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} connected to the islands at {3}, {4} and {5}, {6}.", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            break;
                        }
                        if (island.getNeighbors().Count == 1 && island.getConnectedNeighbors().Count == 2 && (remainingConnections == 2 || remainingConnections == 3 || remainingConnections == 1))
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} connected to the island at {3}, {4}.", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            break;
                        }

                        if (island.getNeighbors().Count == 0 && island.getConnectedNeighbors().Count == 3) {
                            List<Island> l = island.getConnectedNeighbors();
                            if (remainingConnections == 1) {
                                if (l[0].isSolved() && l[1].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[2]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[2].getX(), island.getConnectedNeighbors()[2].getY());
                                    break;
                                }

                                if (l[0].isSolved() && l[2].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());
                                    break;
                                }

                                if (l[1].isSolved() && l[2].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());
                                    break;
                                }
                            } else if (remainingConnections == 2) {
                                if (l[0].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[2]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[2].getX(), island.getConnectedNeighbors()[2].getY());
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());

                                    break;
                                }

                                if (l[1].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[2]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[2].getX(), island.getConnectedNeighbors()[2].getY());

                                    break;
                                }

                                if (l[2].isSolved()) {
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());
                                    solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                    //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());

                                    break;
                                }
                            }
                        }
                        break;
                    case 6:
                        foreach (Island n in island.getNeighbors())
                        {
                            if (n.isSolved())
                            {
                                n.addBannedNeighbor(island);
                                island.addBannedNeighbor(n);
                            }
                        }
                        if (island.getNeighbors().Count == 3 && remainingConnections == 6)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            solutionCheckConnection(island, island.getNeighbors()[2]);
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            solutionCheckConnection(island, island.getNeighbors()[2]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[2].getX(), island.getNeighbors()[2].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[2].getX(), island.getNeighbors()[2].getY());
                            break;
                        }
                        if (island.getNeighbors().Count == 1 && island.getConnectedNeighbors().Count == 2)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                            break;
                        }
                        if (island.getNeighbors().Count == 2 && island.getConnectedNeighbors().Count == 1)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());

                            break;
                        }

                        if (island.getConnectedNeighbors().Count == 3 && island.getNeighbors().Count == 0) {
                            int r = 0;
                            while (island.getSolutionConnections() != 6) {
                                r++;
                                if (r >= 10)
                                {
                                    //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} took too long to connect, skipping.", _moduleId, island.getX(), island.getY());
                                    break;
                                }
                                solutionCheckConnection(island, island.getConnectedNeighbors()[0]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[0].getX(), island.getConnectedNeighbors()[0].getY());

                                if (island.getSolutionConnections() == 6) break;
                                solutionCheckConnection(island, island.getConnectedNeighbors()[1]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[1].getX(), island.getConnectedNeighbors()[1].getY());

                                if (island.getSolutionConnections() == 6) break;
                                solutionCheckConnection(island, island.getConnectedNeighbors()[2]);
                                //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getConnectedNeighbors()[2].getX(), island.getConnectedNeighbors()[2].getY());

                                if (island.getSolutionConnections() == 6) break;
                            }
                            break;
                        }

                        break;
                    case 7:
                        if (island.getNeighbors().Count == 4)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            solutionCheckConnection(island, island.getNeighbors()[2]);
                            solutionCheckConnection(island, island.getNeighbors()[3]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[2].getX(), island.getNeighbors()[2].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[3].getX(), island.getNeighbors()[3].getY());

                            break;
                        }
                        if (island.getNeighbors().Count == 3)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            solutionCheckConnection(island, island.getNeighbors()[2]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[2].getX(), island.getNeighbors()[2].getY());

                            break;
                        }
                        if (island.getNeighbors().Count == 2)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            solutionCheckConnection(island, island.getNeighbors()[1]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());

                            break;
                        }
                        if (island.getNeighbors().Count == 1)
                        {
                            solutionCheckConnection(island, island.getNeighbors()[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                            break;
                        }
                        break;
                    case 8:
                        Debug.LogFormat("[Bridges #{0}] Debug: Attempting to uniqueness-check an island with 8 connections.", _moduleId);
                        List<Island> allNeighbors = new List<Island>();
                        if (island.getNeighbors().Count != 0)
                        {
                            foreach (Island l in island.getNeighbors())
                            {
                                allNeighbors.Add(l);
                            }
                        }
                        if (island.getConnectedNeighbors().Count != 0)
                        {
                            foreach (Island l in island.getConnectedNeighbors())
                            {
                                allNeighbors.Add(l);
                            }
                        }
                        if(allNeighbors.Count != 4)
                        {
                            Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} didn't have 4 neighbors? This is an issue.", _moduleId, island.getX(), island.getY());
                            break;
                        }
                        int ro = 0;
                        while (island.getSolutionConnections() != 8)
                        {
                            ro++;
                            if (ro >= 16)
                            {
                                //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} took too long to connect, skipping.", _moduleId, island.getX(), island.getY());
                                break;
                            }
                            solutionCheckConnection(island, allNeighbors[0]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[0].getX(), island.getNeighbors()[0].getY());

                            if (island.getSolutionConnections() == 8) break;
                            solutionCheckConnection(island, allNeighbors[1]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[1].getX(), island.getNeighbors()[1].getY());

                            if (island.getSolutionConnections() == 8) break;
                            solutionCheckConnection(island, allNeighbors[2]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[2].getX(), island.getNeighbors()[2].getY());

                            if (island.getSolutionConnections() == 8) break;
                            solutionCheckConnection(island, allNeighbors[3]);
                            //Debug.LogFormat("[Bridges #{0}] Connecting {1}, {2} and {3}, {4}", _moduleId, island.getX(), island.getY(), island.getNeighbors()[3].getX(), island.getNeighbors()[3].getY());

                            if (island.getSolutionConnections() == 8) break;
                        }
                        break;
                }
                updateNeighbors();
                //For Debug:
                //drawSolutionEdges()
            }


        }

        bool solved = true;
        foreach (Island i in getIslandList()) {
            if (i.getSolutionConnections() != i.getNeededConnections()) {
                solved = false;
                break;
            }
        }

        bool cont = true;

        if (solved) {
            solved = checkSolutionIfSingleGroup();
            cont = solved;
        }

        List<Island> unsolved = new List<Island>();

        if (solved) {
            message = "After "+regenerationCounter+" regenerations and "+solutionTotalAttemptCounter+" total solution removal attempts, this puzzle should only have one solution. If you find that it doesn't, contact AAces#2652 on Discord with this log file (and maybe a screenshot of the module highlighting where multiple solutions arise if you can).";
            indicator.material = LEDGreen;
        } else {
            if (!cont) {
                if (regenerationCounter < 3)
                {
                    solutionAttemptCounter = 0;
                    regenerationCounter++;
                    generatePuzzle(true);
                }
                else
                {
                    generatePuzzle(false);
                    message = "After " + regenerationCounter + " regenerations and " + solutionTotalAttemptCounter + " total solution removal attempts, couldn't guarantee a unique solution, sorry.";
                    indicator.material = LEDRed;
                }

                return;
            }
            if (solutionAttemptCounter < 2) {
                foreach (Island i in getIslandList()) {
                    if (!i.isSolved() && i.getNeededConnections() != 1) {
                        unsolved.Add(i);
                    }
                }
                updateNeighbors();
                if (unsolved.Count > 2) {
                    Island a, b;
                    int c = 0;
                    do {
                        c++;
                        a = unsolved[Random.Range(0, unsolved.Count)];
                        b = unsolved[Random.Range(0, unsolved.Count)];
                    } while (!a.getConnectedNeighbors().Contains(b) && c<20);

                    while(b == a) {
                        b = unsolved[Random.Range(0, unsolved.Count)];
                    }
                    a.removeNeededConnection();
                    b.removeNeededConnection();

                    //Debug.LogFormat("Removed the/a connection between/from {0}, {1} and {2}, {3}.", a.getX(), a.getY(), b.getX(), b.getY());

                    solutionAttemptCounter++;
                    solutionTotalAttemptCounter++;
                    checkUniqueness();
                } else if (unsolved.Count == 2) {
                    Island a=unsolved[0], b=unsolved[1];
                    a.removeNeededConnection();
                    b.removeNeededConnection();
                    //Debug.LogFormat("Removed a connection from both {0}, {1} and {2}, {3}.", a.getX(), a.getY(), b.getX(), b.getY());

                    solutionAttemptCounter++;
                    solutionTotalAttemptCounter++;
                    checkUniqueness();
                } else if (unsolved.Count == 1) {
                    unsolved[0].removeNeededConnection();
                    //Debug.LogFormat("Removed a connection from both {0}, {1} and {2}, {3}.", unsolved[0].getX(), unsolved[0].getY());
                    solutionAttemptCounter++;
                    solutionTotalAttemptCounter++;
                    checkUniqueness();
                } else if(regenerationCounter < 3)
                {
                    solutionAttemptCounter = 0;
                    regenerationCounter++;
                    generatePuzzle(true);
                } else {
                    generatePuzzle(false);
                    message = "After " + regenerationCounter + " regenerations and " + solutionTotalAttemptCounter + " total solution removal attempts, couldn't guarantee a unique solution, sorry.";
                    indicator.material = LEDRed;
                }
            } else if(regenerationCounter < 3) {
                solutionAttemptCounter = 0;
                regenerationCounter++;
                generatePuzzle(true);
            } else {
                generatePuzzle(false);
                message = "After " + regenerationCounter + " regenerations and " + solutionTotalAttemptCounter + " total solution removal attempts, couldn't guarantee a unique solution, sorry.";
                indicator.material = LEDRed;
            }
        }
    }

    void updateNeighbors() {
        foreach (Island i in getIslandList()) {
            i.clearNeighborList();
            i.clearConnectedNeighborList();
        }
        foreach (Island first in getIslandList()) {
            Island second;
            int x = first.getX();
            int y = first.getY();
            for (int i = 0; i < 4; i++) {
                switch (i)
                {
                    case 0:
                        if (first.getY() <= 1 || getSolutionEdge(x, y - 1) != Edge.None)
                        {
                            break;
                        }

                        int newY = y;
                        do
                        {
                            newY--;
                        } while (getSolutionEdge(x, newY) == Edge.None && getIslandFromGrid(x, newY) == null && newY != 0);

                        second = getIslandFromGrid(x, newY);
                        if (second == null)
                        {
                            break;
                        }

                        if(!areNeighbors(first, second) && !first.getBannedNeighbors().Contains(second)){addNeighbors(first, second);}
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, x, newY);

                        break;
                    case 1:
                        if (first.getX() >= 5 || getSolutionEdge(x + 1, y) != Edge.None)
                        {
                            break;
                        }

                        int newX = x;
                        do
                        {
                            newX++;
                        } while (getSolutionEdge(newX, y) == Edge.None && getIslandFromGrid(newX, y) == null && newX != 6);

                        second = getIslandFromGrid(newX, y);
                        if (second == null)
                        {
                            break;
                        }

                        if (!areNeighbors(first, second) && !first.getBannedNeighbors().Contains(second)) { addNeighbors(first, second); }
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, newX, y);

                        break;
                    case 2:
                        if (first.getY() >= 7 || getSolutionEdge(x, y + 1) != Edge.None)
                        {
                            break;
                        }

                        int newY2 = y;
                        do
                        {
                            newY2++;
                        } while (getSolutionEdge(x, newY2) == Edge.None && getIslandFromGrid(x, newY2) == null && newY2 != 8);

                        second = getIslandFromGrid(x, newY2);
                        if (second == null)
                        {
                            break;
                        }

                        if (!areNeighbors(first, second) && !first.getBannedNeighbors().Contains(second)) { addNeighbors(first, second); }
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, x, newY2);

                        break;
                    case 3:
                        if (first.getX() <= 1 || getSolutionEdge(x - 1, y) != Edge.None)
                        {
                            break;
                        }

                        int newX2 = x;
                        do
                        {
                            newX2--;
                        } while (getSolutionEdge(newX2, y) == Edge.None && getIslandFromGrid(newX2, y) == null && newX2 != 0);

                        second = getIslandFromGrid(newX2, y);
                        if (second == null)
                        {
                            break;
                        }
                        if (!areNeighbors(first, second) && !first.getBannedNeighbors().Contains(second)) { addNeighbors(first, second); }
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, newX2, y);

                        break;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        if (first.getY() <= 1 || (getSolutionEdge(x, y - 1) != Edge.Vertical && getSolutionEdge(x, y - 1) != Edge.DoubleVertical))
                        {
                            break;
                        }

                        int newY = y;
                        do
                        {
                            newY--;
                        } while (getIslandFromGrid(x, newY) == null && newY != 0);

                        second = getIslandFromGrid(x, newY);
                        if (second == null)
                        {
                            break;
                        }

                        if (!areConnectedNeighbors(first, second)) { addConnectedNeighbors(first, second); }
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, x, newY);

                        break;
                    case 1:
                        if (first.getX() >= 5 || (getSolutionEdge(x + 1, y) != Edge.Horizontal && getSolutionEdge(x + 1, y) != Edge.DoubleHorizontal))
                        {
                            break;
                        }

                        int newX = x;
                        do
                        {
                            newX++;
                        } while (getIslandFromGrid(newX, y) == null && newX != 6);

                        second = getIslandFromGrid(newX, y);
                        if (second == null)
                        {
                            break;
                        }

                        if (!areConnectedNeighbors(first, second)) { addConnectedNeighbors(first, second); }
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, newX, y);

                        break;
                    case 2:
                        if (first.getY() >= 7 || (getSolutionEdge(x, y + 1) != Edge.Vertical && getSolutionEdge(x, y + 1) != Edge.DoubleVertical))
                        {
                            break;
                        }

                        int newY2 = y;
                        do
                        {
                            newY2++;
                        } while (getIslandFromGrid(x, newY2) == null && newY2 != 8);

                        second = getIslandFromGrid(x, newY2);
                        if (second == null)
                        {
                            break;
                        }

                        if (!areConnectedNeighbors(first, second)) { addConnectedNeighbors(first, second); }
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, x, newY2);

                        break;
                    case 3:
                        if (first.getX() <= 1 || (getSolutionEdge(x - 1, y) != Edge.Horizontal && getSolutionEdge(x - 1, y) != Edge.DoubleHorizontal))
                        {
                            break;
                        }

                        int newX2 = x;
                        do
                        {
                            newX2--;
                        } while (getIslandFromGrid(newX2, y) == null && newX2 != 0);

                        second = getIslandFromGrid(newX2, y);
                        if (second == null)
                        {
                            break;
                        }
                        if (!areConnectedNeighbors(first, second)) { addConnectedNeighbors(first, second); }
                        //Debug.LogFormat("[Bridges #{0}] Marked the islands at {1}, {2} and {3}, {4} as neighbors.", _moduleId, x, y, newX2, y);

                        break;
                }
            }

            //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} has {3} neighbor(s).", _moduleId, x, y, first.getNeighbors().Count);
        }
    }

    private int iterations = 0;
    private int cap = 325;
    void setupIslands() {
        iterations = 0;
        int x = Random.Range(0, 7);
        int y = Random.Range(0, 9);

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
                                //Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, starter.getX(), newY);
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
                                //Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, newX, starter.getY());
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
                                //Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, starter.getX(), newY);
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
                                //Debug.LogFormat("[Bridges #{0}] Island placed at ({1}, {2}).", _moduleId, newX, starter.getY());
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
        
    }

    void log() {
        Debug.LogFormat("[Bridges #{0}] Completed island placement. Placed {1} islands after {2} iterations (Capped at about {3}).", _moduleId, getIslandList().Count, iterations, cap + 1);
        String islandsString = "";
        foreach (Island i in getIslandList())
        {
            islandsString += "(" + i.getX() + ", " + i.getY() + ", " + i.getNeededConnections() + "), ";
        }

        islandsString = islandsString.Substring(0, islandsString.Length - 2);
        Debug.LogFormat("[Bridges #{0}] Islands placed at (x, y, connections): {1}.", _moduleId, islandsString);
        Debug.LogFormat("[Bridges #{0}] Added {1} extra connection(s).", _moduleId, extraBridges);
        Debug.LogFormat("[Bridges #{0}] Made {1} connection(s) doubled.", _moduleId, doubleBridges);
        Debug.LogFormat("[Bridges #{0}] {1}", _moduleId, message);
    }

    private int extraBridges = 0;
    void addExtraBridges() {
        extraBridges = 0;
        for (int i = 0; i < 50; i++)
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
        
    }

    private int doubleBridges = 0;
    void addDoubleBridges() {
        doubleBridges = 0;
        for (int i = 0; i < 15; i++) {
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
        
    }

    void displayIslands() {
        shift = (Bomb.GetBatteryCount() + getIslandList().Count) % 8;
        foreach (Island i in getIslandList()) {
            int x = i.getX();
            int y = i.getY();
            islandListObj[x][y].GetComponentInChildren<TextMesh>().text = i.getNeededConnections().ToString();
            islandListObj[x][y].SetActive(true);
            islandListObj[x][y].transform.GetChild(0).gameObject.SetActive(false);
            islandListObj[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
            islandListObj[x][y].GetComponentInChildren<SpriteRenderer>().sprite = images[(i.getNeededConnections() + shift - 1)%8];
        }
        Debug.LogFormat("[Bridges #{0}] Shifting symbol table to the left by {1}.", _moduleId, shift);
    }

    void handleIslandPress(int x, int y) {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, selX3[4].transform);
        if (!_lightsOn || moduleSolved) return;

        //Debug.LogFormat("[Bridges #{0}] Island at {1}, {2} pressed.", _moduleId, x, y);
        Island clicked = getIslandFromGrid(x, y);
        //Debug.LogFormat("[Bridges #{0}] Made it past getting the clicked island from the grid.", _moduleId);
        if (clicked == null) {
            Debug.LogFormat("[Bridges #{0}] For some reason that island doesn't exist. Not sure how you clicked it. Auto solving. Please contact AAces#2652 on discord at your earliest convenience with this log file so we can fix this.", _moduleId);
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
                //Debug.LogFormat("[Bridges #{0}] Unselecting island at {1}, {2}.", _moduleId, x, y);
                islandListObj[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;


                return;
            } else {
                //Debug.LogFormat("[Bridges #{0}] Didn't click the same, made it past the .Equals check", _moduleId);
                String error = playerConnect(currentlySelected, clicked);
                //Debug.LogFormat("[Bridges #{0}] That was the second.", _moduleId);
                foreach (Island i in getIslandList())
                {
                    islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = unsolvedMat;
                }
                if (error.Equals("")) {
                    selected = false;
                    drawEdges();

                } else {
                    module.HandleStrike();
                    selected = false;
                    

                    Debug.LogFormat("[Bridges #{0}] Strike! {1} Please contact AAces#2652 on discord with this log file if you feel that this is an error.", _moduleId, error);
                }
            }
        }
    }

    void handleSubmitPress() {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
        if (!_lightsOn || moduleSolved) return;
        submit.AddInteractionPunch();
        checkSolution();
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

    /*void drawSolutionEdges() { 
        foreach (var g in verticalSingles)
        {
            foreach (var h in g)
            {
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
        for (var x = 0; x < 7; x++)
        {
            for (var y = 0; y < 9; y++)
            {
                switch (getSolutionEdge(x, y))
                {
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
    }*/

    void checkSolution() {
        bool solved = true;
        foreach (Island i in getIslandList()) {
            if (i.getCurrentConnections() != i.getNeededConnections()) {
                solved = false;
            }
        }

        bool s = true;
        if (solved) {
            solved = checkIfSingleGroup();
            s = solved;
        }

        if (solved) {
            module.HandlePass();
            StartCoroutine(solvedAnim());
            moduleSolved = true;
            Debug.LogFormat("[Bridges #{0}] Module Solved!", _moduleId);
        } else {
            if (s) {
                module.HandleStrike();
                StartCoroutine(strikeAnim());
                Debug.LogFormat("[Bridges #{0}] Strike! Not all islands have the correct number of connections! If you feel that this is an error, please contact AAces#2652 on Discord.", _moduleId);
            }
        }
    }

    private int finalCounter = 0;

    bool checkIfSingleGroup() {
        Island i = getRandomIslandFromList();
        foreach (Island h in getIslandList())
        {
            h.unMark();
        }
        i.mark();
        finalCounter = 0;
        checkDirection(0, i);
        checkDirection(1, i);
        checkDirection(2, i);
        checkDirection(3, i);
        if (finalCounter != getIslandList().Count - 1) {
            Debug.LogFormat("[Bridges #{0}] Solution not valid, not all islands are connected. Island at ({1}, {2}) can only reach {3} out of the {4} other islands.", _moduleId, i.getX(), i.getY(), finalCounter, getIslandList().Count - 1);
            Debug.LogFormat("[Bridges #{0}] Single island check took {1} iterations.", _moduleId, debugCounter);
            StartCoroutine(warningFlash());
            foreach (Island h in getIslandList()) {
                h.unMark();
            }
            return false;
        }
        Debug.LogFormat("[Bridges #{0}] Island at ({1}, {2}) reached {3}/{4} other islands.", _moduleId, i.getX(), i.getY(), finalCounter, getIslandList().Count - 1);
        Debug.LogFormat("[Bridges #{0}] Single island check took {1} iterations.", _moduleId, debugCounter);
        return true;
    }

    private int debugCounter = 0;
    void checkDirection(int dir, Island i) {
        debugCounter++;
        
        int x = i.getX();
        int y = i.getY();
        switch (dir) {
            case 0:
                if (y <= 1) return;
                if (getInputtedEdge(x, y - 1) == Edge.Vertical || getInputtedEdge(x, y - 1) == Edge.DoubleVertical) {
                    do
                    {
                        y--;
                    } while (getInputtedEdge(x, y) == Edge.Vertical || getInputtedEdge(x, y) == Edge.DoubleVertical);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked()) {
                        return;
                    }
                    g.mark();
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    finalCounter++;
                    checkDirection(0, g);
                    checkDirection(1, g);
                    checkDirection(3, g);
                    return;
                } else {
                    return;
                }
            case 1:
                if (x >= 5) return;
                if (getInputtedEdge(x + 1, y) == Edge.Horizontal || getInputtedEdge(x + 1, y) == Edge.DoubleHorizontal)
                {
                    do
                    {
                        x++;
                    } while (getInputtedEdge(x, y) == Edge.Horizontal || getInputtedEdge(x, y) == Edge.DoubleHorizontal);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked())
                    {
                        return;
                    }
                    g.mark();
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    finalCounter++;
                    checkDirection(0, g);
                    checkDirection(1, g);
                    checkDirection(2, g);
                    return;
                } else {
                    return;
                }
            case 2:
                if (y >= 7) return;
                if (getInputtedEdge(x, y + 1) == Edge.Vertical || getInputtedEdge(x, y + 1) == Edge.DoubleVertical)
                {
                    do
                    {
                        y++;
                    } while (getInputtedEdge(x, y) == Edge.Vertical || getInputtedEdge(x, y) == Edge.DoubleVertical);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked())
                    {
                        return;
                    }
                    g.mark();
                    finalCounter++;
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    checkDirection(1, g);
                    checkDirection(2, g);
                    checkDirection(3, g);
                    return;
                }
                else
                {
                    return;
                }
            case 3:
                if (x <= 1) return;
                if (getInputtedEdge(x - 1, y) == Edge.Horizontal || getInputtedEdge(x - 1, y) == Edge.DoubleHorizontal)
                {
                    do
                    {
                        x--;
                    } while (getInputtedEdge(x, y) == Edge.Horizontal || getInputtedEdge(x, y) == Edge.DoubleHorizontal);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked())
                    {
                        return;
                    }
                    g.mark();
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    finalCounter++;
                    checkDirection(0, g);
                    checkDirection(2, g);
                    checkDirection(3, g);
                    return;
                }
                else
                {
                    return;
                }
        }
    }

    private int finalSolutionCounter = 0;

    bool checkSolutionIfSingleGroup()
    {
        Island i = getRandomIslandFromList();
        foreach (Island h in getIslandList())
        {
            h.unMark();
        }
        i.mark();
        finalSolutionCounter = 0;
        checkSolutionDirection(0, i);
        checkSolutionDirection(1, i);
        checkSolutionDirection(2, i);
        checkSolutionDirection(3, i);
        if (finalSolutionCounter != getIslandList().Count - 1)
        {

            foreach (Island h in getIslandList())
            {
                h.unMark();
            }
            return false;
        }

        return true;
    }

    void checkSolutionDirection(int dir, Island i)
    {

        int x = i.getX();
        int y = i.getY();
        switch (dir)
        {
            case 0:
                if (y <= 1) return;
                if (getSolutionEdge(x, y - 1) == Edge.Vertical || getSolutionEdge(x, y - 1) == Edge.DoubleVertical)
                {
                    do
                    {
                        y--;
                    } while (getSolutionEdge(x, y) == Edge.Vertical || getSolutionEdge(x, y) == Edge.DoubleVertical);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked())
                    {
                        return;
                    }
                    g.mark();
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    finalSolutionCounter++;
                    checkSolutionDirection(0, g);
                    checkSolutionDirection(1, g);
                    checkSolutionDirection(3, g);
                    return;
                }
                else
                {
                    return;
                }
            case 1:
                if (x >= 5) return;
                if (getSolutionEdge(x + 1, y) == Edge.Horizontal || getSolutionEdge(x + 1, y) == Edge.DoubleHorizontal)
                {
                    do
                    {
                        x++;
                    } while (getSolutionEdge(x, y) == Edge.Horizontal || getSolutionEdge(x, y) == Edge.DoubleHorizontal);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked())
                    {
                        return;
                    }
                    g.mark();
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    finalSolutionCounter++;
                    checkSolutionDirection(0, g);
                    checkSolutionDirection(1, g);
                    checkSolutionDirection(2, g);
                    return;
                }
                else
                {
                    return;
                }
            case 2:
                if (y >= 7) return;
                if (getSolutionEdge(x, y + 1) == Edge.Vertical || getSolutionEdge(x, y + 1) == Edge.DoubleVertical)
                {
                    do
                    {
                        y++;
                    } while (getSolutionEdge(x, y) == Edge.Vertical || getSolutionEdge(x, y) == Edge.DoubleVertical);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked())
                    {
                        return;
                    }
                    g.mark();
                    finalSolutionCounter++;
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    checkSolutionDirection(1, g);
                    checkSolutionDirection(2, g);
                    checkSolutionDirection(3, g);
                    return;
                }
                else
                {
                    return;
                }
            case 3:
                if (x <= 1) return;
                if (getSolutionEdge(x - 1, y) == Edge.Horizontal || getSolutionEdge(x - 1, y) == Edge.DoubleHorizontal)
                {
                    do
                    {
                        x--;
                    } while (getSolutionEdge(x, y) == Edge.Horizontal || getSolutionEdge(x, y) == Edge.DoubleHorizontal);

                    Island g = getIslandFromGrid(x, y);
                    if (g.isMarked())
                    {
                        return;
                    }
                    g.mark();
                    //Debug.LogFormat("Marked {0}, {1}", x, y);
                    finalSolutionCounter++;
                    checkSolutionDirection(0, g);
                    checkSolutionDirection(2, g);
                    checkSolutionDirection(3, g);
                    return;
                }
                else
                {
                    return;
                }
        }
    }

    private IEnumerator warningFlash() {
        yield return new WaitForSeconds(0.25f);
        foreach (Island i in getIslandList()) {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = selectedMat;
        }
        yield return new WaitForSeconds(0.25f);
        foreach (Island i in getIslandList())
        {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = unsolvedMat;
        }
        yield return new WaitForSeconds(0.25f);
        foreach (Island i in getIslandList())
        {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = selectedMat;
        }
        yield return new WaitForSeconds(0.25f);
        foreach (Island i in getIslandList())
        {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = unsolvedMat;
        }

    }

    private IEnumerator islandFlash(int x, int y) {
        yield return new WaitForSeconds(0.1f);
        islandListObj[x][y].GetComponent<MeshRenderer>().material = overMat;
        yield return new WaitForSeconds(0.25f);
        islandListObj[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
        yield return new WaitForSeconds(0.25f);
        islandListObj[x][y].GetComponent<MeshRenderer>().material = overMat;
        yield return new WaitForSeconds(0.25f);
        islandListObj[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
    }

    private IEnumerator edgeFlash(int x, int y) {
        yield return new WaitForSeconds(0.1f);
        switch (getInputtedEdge(x, y)) {
            case Edge.Horizontal:
                horizontalSingles[y][x].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                horizontalSingles[y][x].GetComponent<MeshRenderer>().material = unsolvedMat;
                yield return new WaitForSeconds(0.25f);
                horizontalSingles[y][x].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                horizontalSingles[y][x].GetComponent<MeshRenderer>().material = unsolvedMat;
                break;
            case Edge.DoubleHorizontal:
                horizontalDoubles[y][x].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                horizontalDoubles[y][x].GetComponent<MeshRenderer>().material = unsolvedMat;
                yield return new WaitForSeconds(0.25f);
                horizontalDoubles[y][x].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                horizontalDoubles[y][x].GetComponent<MeshRenderer>().material = unsolvedMat;
                break;
            case Edge.Vertical:
                verticalSingles[x][y].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                verticalSingles[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
                yield return new WaitForSeconds(0.25f);
                verticalSingles[x][y].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                verticalSingles[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
                break;
            case Edge.DoubleVertical:
                verticalDoubles[x][y].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                verticalDoubles[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
                yield return new WaitForSeconds(0.25f);
                verticalDoubles[x][y].GetComponent<MeshRenderer>().material = overMat;
                yield return new WaitForSeconds(0.25f);
                verticalDoubles[x][y].GetComponent<MeshRenderer>().material = unsolvedMat;
                break;
        }
    }

    private IEnumerator solvedAnim() {
        yield return new WaitForSeconds(0.15f);
        foreach (Island i in getIslandList()) {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = solvedMat;
            yield return new WaitForSeconds(0.075f);
        }
        foreach (Island i in getIslandList())
        {
            islandListObj[i.getX()][i.getY()].transform.GetChild(0).gameObject.SetActive(true);
            islandListObj[i.getX()][i.getY()].transform.GetChild(2).gameObject.SetActive(false);
            yield return new WaitForSeconds(0.075f);
        }
    }

    private IEnumerator strikeAnim() {
        yield return new WaitForSeconds(0.25f);
        List<Island> list = new List<Island>();
        foreach (Island i in getIslandList()) {
            if (i.getNeededConnections() != i.getCurrentConnections()) {
                list.Add(i);
            }
        }

        foreach (Island i in list) {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = overMat;
        }
        yield return new WaitForSeconds(0.25f);
        foreach (Island i in list)
        {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = unsolvedMat;
        }
        yield return new WaitForSeconds(0.25f);
        foreach (Island i in list)
        {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = overMat;
        }
        yield return new WaitForSeconds(0.25f);
        foreach (Island i in list)
        {
            islandListObj[i.getX()][i.getY()].GetComponent<MeshRenderer>().material = unsolvedMat;
        }
    }

    public void IslandsInit()
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

        solutionEdgeGrid = new[,]{
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

    Edge getSolutionEdge(int x, int y)
    {
        return solutionEdgeGrid[x, y];
    }

    void setSolutionEdge(int x, int y, Edge edge)
    {
        solutionEdgeGrid[x, y] = edge;
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

    void solutionCheckConnection(Island is1, Island is2) {
        int x1 = is1.getX();
        int x2 = is2.getX();
        int y1 = is1.getY();
        int y2 = is2.getY();

        if (!(x1 == x2 || y1 == y2))
        {
            return;
        }

        if (x1 == x2)
        {
            for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
            {
                if (getIslandFromGrid(x1, y) != null)
                {
                    return;
                }

                if (getSolutionEdge(x1, y) == Edge.Horizontal)
                {
                    return;
                }

                if (getSolutionEdge(x1, y) == Edge.DoubleHorizontal)
                {
                    return;
                }
            }

            if (getSolutionEdge(x1, (y1 < y2 ? y1 : y2) + 1) == Edge.None)
            {
                for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
                {
                    setSolutionEdge(x1, y, Edge.Vertical);
                }
                is1.addSolutionConnections();
                is2.addSolutionConnections();
            }
            else if (getSolutionEdge(x1, (y1 < y2 ? y1 : y2) + 1) == Edge.Vertical)
            {
                for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
                {
                    setSolutionEdge(x1, y, Edge.DoubleVertical);
                }
                is1.addSolutionConnections();
                is2.addSolutionConnections();
            }
            else if (getSolutionEdge(x1, (y1 < y2 ? y1 : y2) + 1) == Edge.DoubleVertical)
            {
                for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
                {
                    setSolutionEdge(x1, y, Edge.None);
                }
                is1.subtractTwoSolutionConnections();
                is2.subtractTwoSolutionConnections();
            }

            return;

        }
        else if (y1 == y2)
        {
            for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
            {
                if (getIslandFromGrid(x, y1) != null)
                {
                    return;
                }

                if (getSolutionEdge(x, y1) == Edge.Vertical)
                {
                    return;
                }

                if (getSolutionEdge(x, y1) == Edge.DoubleVertical)
                {
                    return;
                }
            }
            if (getSolutionEdge((x1 < x2 ? x1 : x2) + 1, y1) == Edge.None)
            {
                for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
                {
                    setSolutionEdge(x, y1, Edge.Horizontal);
                }
                is1.addSolutionConnections();
                is2.addSolutionConnections();
            }
            else if (getSolutionEdge((x1 < x2 ? x1 : x2) + 1, y1) == Edge.Horizontal)
            {
                for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
                {
                    setSolutionEdge(x, y1, Edge.DoubleHorizontal);
                }

                is1.addSolutionConnections();
                is2.addSolutionConnections();
            }
            else if (getSolutionEdge((x1 < x2 ? x1 : x2) + 1, y1) == Edge.DoubleHorizontal)
            {
                for (int x = (x1 < x2 ? x1 : x2) + 1; x < (x1 > x2 ? x1 : x2); x++)
                {
                    setSolutionEdge(x, y1, Edge.None);
                }

                is1.subtractTwoSolutionConnections();
                is2.subtractTwoSolutionConnections();
            }

            return;
        }

        return;
    }

    String playerConnect(Island is1, Island is2)
    { //true=strike
        int x1 = is1.getX();
        int x2 = is2.getX();
        int y1 = is1.getY();
        int y2 = is2.getY();

        if (!(x1 == x2 || y1 == y2))
        {
            StartCoroutine(islandFlash(x2, y2));
            StartCoroutine(islandFlash(x1, y1));
            return "You can not connect these two islands, they do not share an x or y coordinate!";
        }

        if (x1 == x2)
        {
            for (int y = (y1 < y2 ? y1 : y2) + 1; y < (y1 > y2 ? y1 : y2); y++)
            {
                if (getIslandFromGrid(x1, y) != null) {
                    StartCoroutine(islandFlash(x1, y));
                    return "You can not connect these two islands, there is an island in the way at " + x1 + ", " + y + "!";
                }

                if (getInputtedEdge(x1, y) == Edge.Horizontal)
                {
                    StartCoroutine(edgeFlash(x1, y));
                    return "You can not connect these two islands, you already have a bridge at " + x1 + ", " + y + "!";
                }

                if (getInputtedEdge(x1, y) == Edge.DoubleHorizontal)
                {
                    StartCoroutine(edgeFlash(x1, y));
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
                    StartCoroutine(islandFlash(x, y1));
                    return "You can not connect these two islands, there is an island in the way at " + x + ", " + y1 + "!";
                }

                if (getInputtedEdge(x, y1) == Edge.Vertical)
                {
                    StartCoroutine(edgeFlash(x, y1));
                    return "You can not connect these two islands, you already have a bridge at " + x + ", " + y1 + "!";
                }

                if (getInputtedEdge(x, y1) == Edge.DoubleVertical)
                {
                    StartCoroutine(edgeFlash(x, y1));
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

    bool areNeighbors(Island i, Island h) {
        return i.getNeighbors().Contains(h);
    }

    bool areConnectedNeighbors(Island i, Island h)
    {
        return i.getConnectedNeighbors().Contains(h);
    }

    void addNeighbors(Island i, Island h) {
        i.addNeighbor(h);
        h.addNeighbor(i);
    }

    void addConnectedNeighbors(Island i, Island h)
    {
        i.addConnectedNeighbor(h);
        h.addConnectedNeighbor(i);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use !{0} press b1 b4 g7 e7 to press the islands at the respective coordinates. Use !{0} submit to submit. Letters refer to columns and numbers refer to rows. The top left corner is a1 and the bottom right corner is g9.";
#pragma warning restore 414
    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<KMSelectable> Buttons = new List<KMSelectable> { };
        if (command.Equals("SUBMIT")) {
            Buttons.Add(submit);
            return Buttons.ToArray();
        }

        if (!command.StartsWith("PRESS")) return null;
        var buttons = new Dictionary<string, KMSelectable>();
        for (int i = 1; i < 10; i++) {
            buttons.Add("A" + i, selX0[i-1]);
        }
        for (int i = 1; i < 10; i++)
        {
            buttons.Add("B" + i, selX1[i - 1]);
        }
        for (int i = 1; i < 10; i++)
        {
            buttons.Add("C" + i, selX2[i - 1]);
        }
        for (int i = 1; i < 10; i++)
        {
            buttons.Add("D" + i, selX3[i - 1]);
        }
        for (int i = 1; i < 10; i++)
        {
            buttons.Add("E" + i, selX4[i - 1]);
        }
        for (int i = 1; i < 10; i++)
        {
            buttons.Add("F" + i, selX5[i - 1]);
        }
        for (int i = 1; i < 10; i++)
        {
            buttons.Add("G" + i, selX6[i - 1]);
        }

        command = command.Substring(6);
        string[] parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (Regex.IsMatch(command, "([A-G][1-9])+"))
        {
            
            foreach (string part in parts)
            {
                if (Regex.IsMatch(part, "^([A-G][1-9])$"))
                {
                    /*Debug.Log(part);
                    Debug.Log("x=" + (part.ToCharArray()[0] - 65) + " y=" + (part.ToCharArray()[1] - '0' - 1));*/
                    if (getIslandFromGrid(char.ToUpper(part.ToCharArray()[0]) - 65, part.ToCharArray()[1] - '0' - 1) == null)
                    {
                        return null;
                    }
                    Buttons.Add(buttons[part]);
                } else
                {
                    return null;
                }
            }

            return Buttons.ToArray();
        }

        return null;
    }
}

class Island {
    private int x, y, neededConnections, currentConnections, solutionConnections;
    private bool marked;
    private List<Island> neighbors, bannedNeighbors, connectedNeighbors;

    public Island(int x, int y) {
        this.x = x;
        this.y = y;
        this.neededConnections = 0;
        this.currentConnections = 0;
        this.marked = false;
        this.neighbors = new List<Island>();
        this.bannedNeighbors = new List<Island>();
        this.connectedNeighbors = new List<Island>();
        this.solutionConnections = 0;
    }

    public void clearNeighborList() {
        this.neighbors.Clear();
    }
    public void clearConnectedNeighborList()
    {
        this.connectedNeighbors.Clear();
    }

    public void mark() {
        this.marked = true;
    }

    public void unMark() {
        this.marked = false;
    }

    public bool isMarked() {
        return this.marked;
    }

    public List<Island> getBannedNeighbors() {
        return this.bannedNeighbors;
    }
    public List<Island> getConnectedNeighbors()
    {
        return this.connectedNeighbors;
    }

    public void addBannedNeighbor(Island i) {
        bannedNeighbors.Add(i);
    }

    public void addConnectedNeighbor(Island i)
    {
        connectedNeighbors.Add(i);
    }

    public bool isSolved() {
        return getSolutionConnections() == getNeededConnections();
    }

    public int getX() {
        return this.x;
    }

    public int getY() {
        return this.y;
    }

    public List<Island> getNeighbors() {
        return neighbors;
    }

    public void addNeighbor(Island i) {
        neighbors.Add(i);
    }

    public int getNeededConnections() {
        return this.neededConnections;
    }

    public void addNeededConnection() {
        //Debug.Log("Added needed connection to the island at " + this.x + " " + this.y);
        this.neededConnections++;
    }

    public void removeNeededConnection() {
        this.neededConnections--;
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

    public int getSolutionConnections()
    {
        return this.solutionConnections;
    }

    public void addSolutionConnections()
    {
        this.solutionConnections++;
    }

    public void subtractTwoSolutionConnections()
    {
        this.solutionConnections -= 2;
    }

    public void resetSolutionAttempt() {
        clearNeighborList();
        clearConnectedNeighborList();
        this.bannedNeighbors.Clear();
        this.solutionConnections = 0;
    }
}

enum Edge {
    Vertical,
    Horizontal,
    None,
    DoubleVertical,
    DoubleHorizontal
}
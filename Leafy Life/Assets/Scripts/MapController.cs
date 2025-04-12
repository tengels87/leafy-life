using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {
    public enum MapType {
        TREEHOUSE,
        GARDEN
    }
    public Sprite gridBackground;

    [SerializeField]
    private MapType mapType;

    private Tile[,] grid;
    private List<Tile> tileList = new List<Tile>();
    private int nodeID = 1;

    private Transform spriteContainer;

    private System.Random rnd = new System.Random();

    void Start() {
        spriteContainer = new GameObject("sprite_container_" + rnd.Next(10000)).transform;
        spriteContainer.SetParent(this.transform);

        init();
    }

    void Update() {

    }

    public Tile getTile(Vector2Int pos) {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (isInBounds(new Vector2Int(x, y))) {
            return grid[x, y];
        } else {
            return null;
        }
    }

    public Tile getNearestWalkableTile(Vector2Int rootLocation) {
        List<Vector2Int> locations = new List<Vector2Int>();

        locations.Add(rootLocation + Vector2Int.up);
        locations.Add(rootLocation + Vector2Int.down);
        locations.Add(rootLocation + Vector2Int.left);
        locations.Add(rootLocation + Vector2Int.right);

        foreach (Vector2Int pos in locations) {
            if (isInBounds(pos)) {
                if (grid[pos.x, pos.y] != null) {
                    if (grid[pos.x, pos.y].isWalkable) {
                        return grid[pos.x, pos.y];
                    }
                }
            }
        }

        return null;
    }

    public List<Vector2Int> getBuildLocations(Structure.StructureType typeToBuild) {
        List<Vector2Int> locations = new List<Vector2Int>();

        foreach (Tile t in grid) {
            if (t != null) {
                if (t.attachedGameObject != null) {
                    // look for empty location around tile
                    Vector2Int currentPos = t.getPositionAsVector();
                    Vector2Int up = t.getPositionAsVector() + Vector2Int.up;
                    Vector2Int down = t.getPositionAsVector() + Vector2Int.down;
                    Vector2Int left = t.getPositionAsVector() + Vector2Int.left;
                    Vector2Int right = t.getPositionAsVector() + Vector2Int.right;

                    Structure s = t.attachedGameObject.GetComponent<Structure>();

                    //if (t.hasSlot) {
                        if (typeToBuild == Structure.StructureType.BED) {
                            if (s.structureType == Structure.StructureType.PLATFORM) {
                                locations.Add(currentPos);
                            }
                        } else if (typeToBuild == Structure.StructureType.AGRICULTURAL) {
                            if (s.structureType == Structure.StructureType.GRASSLAND) {
                                locations.Add(currentPos);
                            }
                        // plants, crops
                        } else if (typeToBuild == Structure.StructureType.PLANT) {
                            if (s.structureType == Structure.StructureType.AGRICULTURAL) {
                                locations.Add(currentPos);
                            }
                        }
                    //} else {

                        if (typeToBuild == Structure.StructureType.PLATFORM) {
                            if (s.structureType == Structure.StructureType.PLATFORM) {
                                if (isEmpty(left)) locations.Add(left);
                                if (isEmpty(right)) locations.Add(right);
                            } else if (s.structureType == Structure.StructureType.LADDER) {
                                if (isEmpty(up)) locations.Add(up);
                                if (isEmpty(down)) locations.Add(down);
                            }
                        } else if (typeToBuild == Structure.StructureType.LADDER) {
                            if (isEmpty(up)) locations.Add(up);
                            if (isEmpty(down)) locations.Add(down);
                        }
                    //}
                }
            }
        }

        return locations;
    }

    public List<Vector2Int> getBuildLocationsWithSlot() {
        List<Vector2Int> locations = new List<Vector2Int>();

        foreach (Tile t in grid) {
            if (t != null) {
                if (t.hasSlot) {
                    locations.Add( t.getPositionAsVector() );
                }
            }
        }

        return locations;
    }

    public void updateCameraFOV(int gridWidth, int gridHeight) {
        Camera cam = Camera.main;

        float aspect = (float)cam.pixelWidth / (float)cam.pixelHeight;

        int maxGridHeight = gridHeight;

        float zoom = (float)maxGridHeight / cam.orthographicSize;

        cam.orthographicSize = (float)maxGridHeight * 0.5f;

        cam.transform.position = new Vector3((float)(maxGridHeight) * aspect / zoom - 0.5f, (float)(maxGridHeight) / zoom - 0.5f, cam.transform.position.z);
    }

    public void init() {
        int gridWidth = 20;
        int gridHeight = 20;

        grid = new Tile[gridWidth, gridHeight];
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                grid[i, j] = null;
                //GameObject vis = createSpriteInstance(gridBackground, i, j);
                //vis.transform.position += new Vector3(0, 0, 1);    // set z position, so it is shifted to background
            }
        }

        if (mapType == MapType.TREEHOUSE) {
            buildTile(4, 4, WorldConstants.Instance.getStructureManager().prefab_platform);
            buildTile(4, 5, WorldConstants.Instance.getStructureManager().prefab_ladder);
            buildTile(4, 6, WorldConstants.Instance.getStructureManager().prefab_platform);
        } else if (mapType == MapType.GARDEN) {
            for (int i = 1; i < 10; i++) {
                for (int j = 1; j < 10; j++) {
                    buildTile(i, j, WorldConstants.Instance.getStructureManager().prefab_grass);
                }
            }
        }
    }

    public void buildTile(int x, int y, GameObject prefab) {
        GameObject buildable = (GameObject)Object.Instantiate(prefab);
        buildable.transform.position = new Vector3(x, y, 0);
        //buildable.transform.SetParent(spriteContainer);

        Structure structure = buildable.GetComponent<Structure>();

        foreach (Structure.GridFootprint footprint in structure.gridFootprint) {
            Tile t = createTile(x + footprint.gridX, y + footprint.gridY);
            t.attachedGameObject = prefab;
            t.canConnectAt = "0123";//structure.canConnectAt;
            if (footprint.isWalkable) {
                t.isWalkable = footprint.isWalkable;
            }
            t.hasSlot = footprint.hasSlot;

            spawnTile(t);
        }

        Crop crop = structure.GetComponentInChildren<Crop>();
        if (crop != null) {
            crop.init();
        }
    }

    public static Vector2 pixelPos2WorldPos(Vector3 pixelPos) {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(pixelPos);

        Vector3 mousePos = pixelPos;

        //Vector3 relSize = (mousePos / cam.pixelHeight);
        //Vector3 gridPos = relSize * (cam.orthographicSize * 2);

        Vector2Int targetPosInt = new Vector2Int((int)worldPos.x, (int)worldPos.y);
        print(targetPosInt);
        return worldPos;//new Vector2(gridPos.x, gridPos.y) - Vector2.one;
    }

    public bool isInBounds(Vector2Int _pos) {
        if (_pos.x < 0 || _pos.y < 0)
            return false;

        if (_pos.x >= grid.GetLength(0) || _pos.y >= grid.GetLength(1))
            return false;

        return true;
    }

    public bool isEmpty(Vector2Int _pos) {
        if (isInBounds(_pos)) {

            if (grid[_pos.x, _pos.y] != null) {
                return false;
            }

            return true;
        }

        return false;
    }

    public Node getNode(Vector2 pos) {
        Vector2Int posInt = new Vector2Int((int)pos.x, (int)pos.y);

        if (isInBounds(posInt) && grid[posInt.x, posInt.y] != null) {
            return grid[posInt.x, posInt.y].node;
        } else {
            return null;
        }
    }

    public bool checkPath(Vector2 target, Vector2 start, out List<Vector2> resultList) {
        resultList = new List<Vector2>();

        Node startNode = getNode(new Vector2(Mathf.RoundToInt(start.x), Mathf.RoundToInt(start.y)));
        Node targetNode = getNode(target);

        if (startNode == null || targetNode == null) {
            return false;
        }

        if (startNode.posX == targetNode.posX && startNode.posY == targetNode.posY) {
            return false;
        }

        AStar astar = new AStar();

        List<Node> resultPath = astar.findPath(startNode, targetNode);

        if (resultPath.Count > 0) {
            resultList = AStar.nodeList2posList(resultPath);

            return true;
        } else {
            return false;
        }
    }

    public GameObject createSpriteInstance(Sprite spr, float posX, float posY) {
        GameObject go = new GameObject("sprite");
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = spr;
        go.transform.localPosition = new Vector2(posX, posY);
        go.transform.SetParent(spriteContainer);

        return go;
    }

    public Tile createTile(int gridX, int gridY) {
        Tile t = new Tile(gridX, gridY, null);

        return t;
    }

    public void linkTile(Tile t) {
        int x = t.gridX;
        int y = t.gridY;

        // look at N, E, S, W for adjacent tiles and connect them
        // only connections based on tile.canConnectAt[] array
        Vector2Int[] possibleNeighborPositions = new Vector2Int[4] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        List<Vector2Int> validNeighborPositions = new List<Vector2Int>();//new Vector2Int[4] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (char chr in t.canConnectAt.ToCharArray()) {
            int ownConntectID = int.Parse(chr.ToString());
            validNeighborPositions.Add(possibleNeighborPositions[ownConntectID]);
        }

        // connect neighbors
        foreach (Vector2Int nb in validNeighborPositions) {
            int cx = nb.x + x;
            int cy = nb.y + y;

            if (this.isInBounds(new Vector2Int(cx, cy))) {//(cx >= 0 && cx < grid.GetLength(0) && cy >= 0 && cy < grid.GetLength(1)) {
                if (grid[cx, cy] != null) {

                    // only allow when both connected node AND connecting node allow it (in canConnectAt)
                    //  0 and 2 can connect , 1 and 3 can connect together
                    string scn = t.canConnectAt;            // source connections
                    string tcn = grid[cx, cy].canConnectAt; // target connections
                    if ((scn.Contains("0") && tcn.Contains("2") && cy > y) || (scn.Contains("2") && tcn.Contains("0") && cy < y)
                        || (scn.Contains("1") && tcn.Contains("3") && cx > x) || (scn.Contains("3") && tcn.Contains("1") && cx < x)) {
                        t.node.addAdjacentNode(grid[cx, cy].node);
                        grid[cx, cy].node.addAdjacentNode(t.node);
                    }
                }
            }
        }
    }

    public void spawnTile(Tile t) {
        if (t.isWalkable) {
            t.node = createNode(t.gridX, t.gridY);
            linkTile(t);
        }

        grid[t.gridX, t.gridY] = t;

        tileList.Add(t);
    }

    private Node createNode(int posX, int posY) {
        nodeID = nodeID + 1;

        return new Node(nodeID, posX, posY);
    }

    public class Tile {
        public Node node;
        public GameObject attachedGameObject;
        public string canConnectAt = "0123";

        public int gridX;
        public int gridY;
        public bool isWalkable;
        public bool hasSlot;

        public Tile(int gridX, int gridY, GameObject attachedGameObject) {
            this.gridX = gridX;
            this.gridY = gridY;

            this.attachedGameObject = attachedGameObject;
        }

        public Vector2Int getPositionAsVector() {
            return new Vector2Int(gridX, gridY);
        }
    }
}

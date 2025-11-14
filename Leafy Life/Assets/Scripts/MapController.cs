using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour {
    public enum MapType {
        GARDEN,
        TREEHOUSE,
        FOREST_STARTER
    }
    public Sprite gridBackground;

    [SerializeField]
    private MapType mapType;

    private bool isInitiated = false;
    private Vector2Int spawnPosition;

    private Tile[,] grid;
    private List<Tile> tileList = new List<Tile>();
    private int nodeID = 1;

    private System.Random rnd = new System.Random();
    private int mapSeed = 0;
    
    void Start() {
        init();
    }

    void Update() {
        
    }

    public int getSeed() {
        return mapSeed;
    }

    public MapType getMapType() {
        return mapType;
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

    public Tile getNearestWalkableTile(Vector2Int targetLocation, Vector2Int startLocation) {
        List<Vector2Int> testLocations = new List<Vector2Int>();
        
        Tile resultTile = null;
        int resultPathLength = int.MaxValue;

        testLocations.Add(targetLocation + Vector2Int.up);
        testLocations.Add(targetLocation + Vector2Int.down);
        testLocations.Add(targetLocation + Vector2Int.left);
        testLocations.Add(targetLocation + Vector2Int.right);

        foreach (Vector2Int loc in testLocations) {
            if (isInBounds(loc)) {
                if (grid[loc.x, loc.y] != null) {
                    if (grid[loc.x, loc.y].isWalkable) {

                        // find path to loc
                        MapController mapController = WorldConstants.Instance.getMapController();
                        if (mapController.checkPath(loc, startLocation, out List<Vector2> resultPath)) {
                            if (resultPath.Count < resultPathLength) {
                                resultPathLength = resultPath.Count;
                                resultTile = grid[loc.x, loc.y];
                            }
                        }
                    }
                }
            }
        }

        return resultTile;
    }

    public List<Vector2Int> getBuildLocations(Structure target) {
        List<Vector2Int> locations = new List<Vector2Int>();

        Structure.StructureType targetType = target.structureType;

        foreach (Tile t in grid) {
            if (t != null) {
                if (t.attachedGameObject != null) {

                    Structure s = t.attachedGameObject.GetComponent<Structure>();

                    Vector2Int currentPos = t.getPositionAsVector();
                    Vector2Int up = t.getPositionAsVector() + Vector2Int.up;
                    Vector2Int down = t.getPositionAsVector() + Vector2Int.down;
                    Vector2Int left = t.getPositionAsVector() + Vector2Int.left;
                    Vector2Int right = t.getPositionAsVector() + Vector2Int.right;

                    // check, if target attaches to s, (ifstructureType is included in target's attach ist)
                    if ((target.buildOnStructure & s.structureType) == s.structureType) {
                        if (target.buildType == Structure.BuildType.ATTACH_TO_SLOT) {
                            if (t.hasAvailableSlot()) {
                                locations.Add(currentPos);
                                continue;
                            }
                        } else if (target.buildType == Structure.BuildType.REPLACE_TILE) {
                            locations.Add(currentPos);
                            continue;
                        } else {
                            continue;
                        }
                    }




                    // look for empty location around tile

                    if (target.buildType == Structure.BuildType.REPLACE_TILE) {
                        if (s.structureType == Structure.StructureType.PLATFORM && target.attachesToPlatform) {
                            locations.Add(currentPos);
                        }

                        if (targetType == Structure.StructureType.LADDER) {
                            if (s.structureType == Structure.StructureType.PLATFORM) {
                                if (isEmpty(up)) locations.Add(up);
                                if (isEmpty(down)) locations.Add(down);
                            }
                        }
                    }
                }
            }
        }

        return locations;
    }

    public void init() {
        if (isInitiated) return;

        SaveSystem.GameData saveData = WorldConstants.Instance.getSaveSystem().getLoadedData();
        if (saveData != null) {
            mapSeed = saveData.seed;
            rnd = new System.Random(mapSeed);
        } else {
            rnd = new System.Random();
            mapSeed = rnd.Next();
        }

        int gridWidth = 60;
        int gridHeight = 60;

        grid = new Tile[gridWidth, gridHeight];
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                grid[i, j] = null;
            }
        }

        if (mapType == MapType.FOREST_STARTER) {

            Transform[] mapObjects = this.gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform t2 in mapObjects) {
                if (t2 == this.transform || t2.parent != this.transform) continue;

                buildTile(t2.gameObject, (int)t2.position.x, (int)t2.position.y);

                Object.Destroy(t2.gameObject);
            }

            spawnPosition = new Vector2Int(10, 22);

        } else if (mapType == MapType.TREEHOUSE) {

            Transform[] mapObjects = this.gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform t2 in mapObjects) {
                if (t2 == this.transform || t2.parent != this.transform) continue;

                buildTile(t2.gameObject, (int)t2.position.x, (int)t2.position.y);

                Object.Destroy(t2.gameObject);
            }

            spawnPosition = new Vector2Int(8, 0);

        } else if (mapType == MapType.GARDEN) {

            Transform[] mapObjects = this.gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform t2 in mapObjects) {
                if (t2 == this.transform || t2.parent != this.transform) continue;

                buildTile(t2.gameObject, (int)t2.position.x, (int)t2.position.y);

                Object.Destroy(t2.gameObject);
            }

            spawnPosition = new Vector2Int(20, 10);
        }

        
        // load saved user-built tiles
        LoadingManager loadingManager = WorldConstants.Instance.getLoadingManager();
        if (loadingManager != null) {
            foreach (TileData tileData in loadingManager.getUserBuildTiles()) {
                if (tileData.mapType == this.mapType) {     // only build tiles registered for this map
                    if (PrefabDefs.TryGet(tileData.tilePrefabID, out PrefabDef prefabDef))
                    buildTile(prefabDef.Prefab, tileData.x, tileData.y);
                }
            }
        }
        

        isInitiated = true;
    }

    public void buildTile(GameObject prefab, int x, int y) {
        GameObject buildable = (GameObject)Object.Instantiate(prefab);

        buildable.name = buildable.name + "_" + rnd.Next();
        buildable.transform.position = new Vector3(x, y, 0);
        buildable.transform.SetParent(this.gameObject.transform);

        Structure structure = buildable.GetComponent<Structure>();

        if (structure.attachesToPlatform || structure.buildType == Structure.BuildType.ATTACH_TO_SLOT) {

            // attach to slot on existing tile, but do not override tile in grid[,]
            // only block slot when attachesToSlot != 0
            if (structure.buildOnStructure != 0) {
                Vector2Int buildPosInt = new Vector2Int(x, y);
                Tile t = getTile(buildPosInt);
                if (t != null && t.hasAvailableSlot()) {
                    t.fillSlot(structure);
                }
            }
        } else if (structure.buildType == Structure.BuildType.REPLACE_TILE) {

            // no attaching, so simply place/override tiles in grid, using footprints         
            foreach (Structure.GridFootprint footprint in structure.gridFootprint) {

                Tile t = createTile(x + footprint.gridX, y + footprint.gridY);
                t.attachedGameObject = buildable;
                t.canConnectAt = footprint.canConnectAt;
                if (footprint.isWalkable) {
                    t.isWalkable = footprint.isWalkable;
                }
                if (footprint.hasSlot) {
                    t.initSlot();
                }

                placeTile(t);
            }
        }
    }

    public void buildTileFromPrefabId(string id, int x, int y) {
        if (PrefabDefs.TryGet(id, out PrefabDef prefabDef)) {
            buildTile(prefabDef.Prefab, x, y);
        }
    }

    public static Vector2 pixelPos2WorldPos(Vector3 pixelPos) {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(pixelPos);

        Vector3 mousePos = pixelPos;

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

    public Vector2Int getSpawnPosition(MapType comingFromMapType) {

        // find link tile to comingFromMap
        Structure[] allStructures = FindObjectsOfType<Structure>();
        foreach (Structure s in allStructures) {
            if (s.structureType == Structure.StructureType.MAPLINK
                && s.linkToMapType == comingFromMapType) {
                Vector2 mapLinkPos = s.gameObject.transform.position;
                Vector2Int mapLinkPosInt = new Vector2Int((int)mapLinkPos.x, (int)mapLinkPos.y);

                return mapLinkPosInt;
            }
        }

        return spawnPosition;
    }

    public void setSpawnPosition(Vector2Int pos) {
        spawnPosition = pos;
    }

    public GameObject createSpriteInstance(Sprite spr, float posX, float posY) {
        GameObject go = new GameObject("sprite");
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = spr;
        go.transform.localPosition = new Vector2(posX, posY);
        go.transform.SetParent(this.transform);

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

    public void placeTile(Tile t) {

        // overwrite old tile
        grid[t.gridX, t.gridY] = t;

        // update links
        if (t.isWalkable) {
            t.node = createNode(t.gridX, t.gridY);
            linkTile(t);
        }

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

        private Structure[] slots = null;  // array elements are filld when i.e. GridFootPrint fills this when placin a Structure

        public Tile(int gridX, int gridY, GameObject attachedGameObject) {
            this.gridX = gridX;
            this.gridY = gridY;

            this.attachedGameObject = attachedGameObject;
        }

        public Vector2Int getPositionAsVector() {
            return new Vector2Int(gridX, gridY);
        }

        public bool hasAvailableSlot() {
            if (slots != null && slots[slots.Length - 1] == null) {
                return true;
            } else {
                return false;
            }
        }

        public void initSlot() {
            slots = new Structure[1] { null };
        }

        public void fillSlot(Structure s) {
            if (hasAvailableSlot()) {
                slots[slots.Length - 1] = s;
            } else {
                if (slots == null) {
                    initSlot();
                }
            }
        }
    }

    [System.Serializable]
    public class TileData {
        public string tilePrefabID;
        public MapType mapType;
        public int x;
        public int y;

        public TileData(string tilePrefabID, MapType mapType, int x, int y) {
            this.tilePrefabID = tilePrefabID;
            this.mapType = mapType;
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj) {
            return obj is TileData data &&
               tilePrefabID == data.tilePrefabID &&
               mapType == data.mapType &&
               x == data.x &&
               y == data.y;
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(tilePrefabID, mapType, x.ToString(), y.ToString());
        }
    }
}

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

    private bool isInitiated = false;
    private Vector2Int spawnPosition;

    private Tile[,] grid;
    private List<Tile> tileList = new List<Tile>();
    private int nodeID = 1;

    private Transform spriteContainer;
    private List<TileData> userBuiltTilesList = new List<TileData>();

    private System.Random rnd = new System.Random();
    private int mapSeed = 0;
    
    void Start() {
        spriteContainer = new GameObject("sprite_container_" + rnd.Next(10000)).transform;
        spriteContainer.SetParent(this.transform);

        init();
    }

    void Update() {
        
    }

    public int getSeed() {
        return mapSeed;
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

                    // ignore, if we need a slot but it is already occupied
                    if ((target.attachesToSlot) && s.gridFootprint[0].slot != null) {
                        continue;
                    }


                    Vector2Int currentPos = t.getPositionAsVector();
                    Vector2Int up = t.getPositionAsVector() + Vector2Int.up;
                    Vector2Int down = t.getPositionAsVector() + Vector2Int.down;
                    Vector2Int left = t.getPositionAsVector() + Vector2Int.left;
                    Vector2Int right = t.getPositionAsVector() + Vector2Int.right;

                    // look for empty location around tile 
                    if (targetType == Structure.StructureType.SOIL) {
                        if (s.structureType == Structure.StructureType.GRASS) {
                            locations.Add(currentPos);
                        }
                        // plants, crops
                    } else if (targetType == Structure.StructureType.CROP) {
                        if (s.structureType == Structure.StructureType.SOIL) {
                            locations.Add(currentPos);
                        }
                    } else {
                        if (s.structureType == Structure.StructureType.PLATFORM && target.attachesToPlatform) {
                            locations.Add(currentPos);
                        }
                    }

                    if (targetType == Structure.StructureType.PLATFORM) {
                        if (s.structureType == Structure.StructureType.PLATFORM) {
                            if (isEmpty(left)) locations.Add(left);
                            if (isEmpty(right)) locations.Add(right);
                        } else if (s.structureType == Structure.StructureType.LADDER) {
                            if (isEmpty(up)) locations.Add(up);
                            if (isEmpty(down)) locations.Add(down);
                        }
                    } else if (targetType == Structure.StructureType.LADDER) {
                        if (isEmpty(up)) locations.Add(up);
                        if (isEmpty(down)) locations.Add(down);
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

        int gridWidth = 40;
        int gridHeight = 40;

        grid = new Tile[gridWidth, gridHeight];
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                grid[i, j] = null;
                //GameObject vis = createSpriteInstance(gridBackground, i, j);
                //vis.transform.position += new Vector3(0, 0, 1);    // set z position, so it is shifted to background
            }
        }

        if (mapType == MapType.TREEHOUSE) {

            // tilemap
            for (int i = 1; i < grid.GetLength(0); i++) {
                buildTile(i, 0, WorldConstants.Instance.getStructureManager().prefab_ground_invisible);
            }
            buildTile(8, 1, WorldConstants.Instance.getStructureManager().prefab_ladder);
            buildTile(8, 2, WorldConstants.Instance.getStructureManager().prefab_platform);

            spawnPosition = new Vector2Int(8, 0);
            buildTile(spawnPosition.x, spawnPosition.y, WorldConstants.Instance.getStructureManager().prefab_maplink_garden);

        } else if (mapType == MapType.GARDEN) {

            spawnPosition = new Vector2Int(20, 19);
            buildTile(spawnPosition.x, spawnPosition.y, WorldConstants.Instance.getStructureManager().prefab_maplink_treehouse);

            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    if (isEmpty(new Vector2Int(i, j))) {
                        float rand = (float)rnd.NextDouble();
                        if (i == 10 || i == 29 || j == 10 || j == 29) {
                            Instantiate(WorldConstants.Instance.getStructureManager().prefab_pinetree, new Vector2(i, j), Quaternion.identity, this.transform);
                        } else if ((i < 15 || i > 25) || (j < 15 || j > 25)) {
                            if (rand < 0.1f) {
                                Instantiate(WorldConstants.Instance.getStructureManager().prefab_pinetree, new Vector2(i, j), Quaternion.identity, this.transform);
                            } else if (rand < 0.5f) {
                                // wooden logs
                                Instantiate(WorldConstants.Instance.getStructureManager().prefab_log, new Vector2(i + rand, j + rand), Quaternion.identity, this.transform);
                                Instantiate(WorldConstants.Instance.getStructureManager().prefab_log, new Vector2(i + rand, j + rand), Quaternion.identity, this.transform);
                            }
                            buildTile(i, j, WorldConstants.Instance.getStructureManager().prefab_grass);
                        } else {
                            buildTile(i, j, WorldConstants.Instance.getStructureManager().prefab_grass);
                        }

                    }
                }
            }

            // collectable items
            Instantiate(WorldConstants.Instance.getStructureManager().prefab_carrot, new Vector2(22, 17), Quaternion.identity, this.transform);
            Instantiate(WorldConstants.Instance.getStructureManager().prefab_carrot, new Vector2(20, 16), Quaternion.identity, this.transform);

            // decals
            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    Tile tile = getTile(new Vector2Int(i, j));
                    if (tile != null && tile.isWalkable) {
                        float rand = (float)rnd.NextDouble();
                        if (rand < 0.2f) {
                            Instantiate(WorldConstants.Instance.getStructureManager().prefab_decal_grass1, new Vector2(i + rand*3, j + rand*3), Quaternion.identity, this.transform);
                        } else if (rand < 0.4f) {
                            Instantiate(WorldConstants.Instance.getStructureManager().prefab_decal_grass2, new Vector2(i + rand*3, j + rand*3), Quaternion.identity, this.transform);
                        }
                    }
                }
            }
        }


        // load saved user-built tiles
        if (saveData != null) {
            foreach (TileData tileData in saveData.builtTilesList) {
                if (tileData.mapType == this.mapType) {     // only build tiles registered for this map
                    GameObject foundPrefab = WorldConstants.Instance.getStructureManager().getPrefabByUID(tileData.tilePrefabID);
                    buildTile(tileData.x, tileData.y, foundPrefab, true);
                }
            }
        }


        isInitiated = true;
    }

    public void buildTile(int x, int y, GameObject prefab, bool userCreated = false) {
        GameObject buildable = (GameObject)Object.Instantiate(prefab);
        buildable.name = buildable.name + "_" + rnd.Next();
        buildable.transform.position = new Vector3(x, y, 0);
        buildable.transform.SetParent(this.gameObject.transform);

        Structure structure = buildable.GetComponent<Structure>();
        
        if (structure.attachesToPlatform || structure.attachesToSlot) {

            // attach to slot on existing tile, but do not override tile in grid[,]
            // only block slot when attachesToSlot==true
            if (structure.attachesToSlot == true) {
                Tile t = getTile(new Vector2Int(x, y));
                Structure structureOnTile = t.attachedGameObject.GetComponent<Structure>();
                if (t != null) {
                    structureOnTile.gridFootprint[0].slot = structure;
                }
            }
        } else {

            // no attaching, so simply place/override tile in grid            
            foreach (Structure.GridFootprint footprint in structure.gridFootprint) {

                Tile t = createTile(x + footprint.gridX, y + footprint.gridY);
                t.attachedGameObject = buildable;
                t.canConnectAt = footprint.canConnectAt;
                if (footprint.isWalkable) {
                    t.isWalkable = footprint.isWalkable;
                }

                spawnTile(t);
            }
        }

        if (userCreated) {
            int prefabUID = WorldConstants.Instance.getStructureManager().getPrefabUID(prefab.name);
            TileData tileData = new TileData(prefabUID, mapType, x, y);
            userBuiltTilesList.Add(tileData);
        }
    }

    public List<TileData> getUserBuildTiles() {
        return userBuiltTilesList;
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

    public Vector2Int getSpawnPosition() {
        return spawnPosition;
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

        public Tile(int gridX, int gridY, GameObject attachedGameObject) {
            this.gridX = gridX;
            this.gridY = gridY;

            this.attachedGameObject = attachedGameObject;
        }

        public Vector2Int getPositionAsVector() {
            return new Vector2Int(gridX, gridY);
        }
    }

    [System.Serializable]
    public class TileData {
        public TileData(int tilePrefabID, MapType mapType, int x, int y) {
            this.tilePrefabID = tilePrefabID;
            this.mapType = mapType;
            this.x = x;
            this.y = y;
        }

        public int tilePrefabID;
        public MapType mapType;
        public int x;
        public int y;
    }
}

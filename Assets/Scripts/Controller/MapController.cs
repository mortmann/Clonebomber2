using System;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum MapType { Normal, Graveyard, Future }
public enum TileType { Empty, Floor, Ice, Hole, Box, ExplodedBox, RandomBox, Spawn, Wall, ArrowUp, ArrowDown, ArrowLeft, ArrowRight}
public class MapController : MonoBehaviour {
    public TypeSprites[] typeSprites;
    public List<PowerUPSprites> powerUPtypeSprites;
    public static MapController Instance;

    public Tilemap floorMap;

    public Tilemap wallMap;

    public Tilemap boxMap;
    public Tilemap triggerMap;
    public Tilemap previewMap;

    public Tilemap iceMap;

    public MapType currentMapType = MapType.Normal;

    public Sprite HoleSprite;
    public Sprite IceSprite;

    public Sprite UpArrowSprite;
    public Sprite DownArrowSprite;
    public Sprite RightArrowSprite;
    public Sprite LeftArrowSprite;
    public Sprite SpawnSprite;
    public Sprite RandomBoxSprite;

    public PowerUP PowerUPPrefab;
    
    Dictionary<TileType, Tile> typeToTileBase;
    private Grid2D grid2d;

    public MapTile[,] Tiles { get; private set; }
    List<MapTile> ListOfTiles;
    MapTile[] Spawns;
    public static readonly int maxX = 17;
    public static readonly int maxY = 13;

    internal Vector2 ClampVector(Vector2 target) {
        return new Vector2(Mathf.Clamp(target.x, 0, maxX), Mathf.Clamp(target.y, 0, maxY));
    }

    public static string[] GetMapFile(string map) {
        return File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "Maps", map+".map"));
    }
    public static string[] GetMapTileFileStrings(string map) {
        return GetMapFile(map).Skip(2).ToArray();
    }

    void Awake() {
        Instance = this;
        typeToTileBase = new Dictionary<TileType, Tile>();
        grid2d = FindObjectOfType<Grid2D>();
        LoadTileBases();
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="GameScene") {
            LoadMap(PlayerController.Instance.NextMapString,true);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Delete)) {
            DestroyTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    public void LoadMap(string name, bool setSpawns) {
        floorMap.ClearAllTiles();
        wallMap.ClearAllTiles();
        boxMap.ClearAllTiles();
        triggerMap.ClearAllTiles();
        if(previewMap!=null)
            previewMap.ClearAllTiles();
        iceMap.ClearAllTiles();

        Tiles = new MapTile[maxX + 2, maxY + 2];
        for (int mx = 0; mx < Tiles.GetLength(0); mx++) {
            for (int my = 0; my < Tiles.GetLength(1); my++) {
                Tiles[mx, my] = new MapTile(TileType.Empty, mx, my);
            }
        }
        MapFileData map = LoadMapFile(name);
        ListOfTiles = new List<MapTile>();
        for (int x = 0; x < map.Tiles.GetLength(0); x++) {
            for (int y = 0; y < map.Tiles.GetLength(1); y++) {
                Tiles[x + 1, y + 1].Type = map.Tiles[x, y];
                ListOfTiles.Add(Tiles[x + 1, y + 1]);
            }
        }
        SetTileMaps();
        for (int x = 0; x < Tiles.GetLength(0); x++) {
            for (int y = 0; y < Tiles.GetLength(1); y++) {
                if (Tiles[x, y].Type == TileType.Box) {
                    ChangeNeigbhoursBoxCount(x, y, 1);
                }
            }
        }
        //string debugS = "";
        //for (int y = 0; y < Tiles.GetLength(1); y++) {
        //    for (int x = 0; x < Tiles.GetLength(0); x++) {
        //        debugS += Tiles[x, y].BoxCount;
        //    }
        //    debugS += "\n";
        //}
        //Debug.Log(debugS);
        Spawns = new MapTile[map.Spawns.Length];
        for (int i = 0; i < map.Spawns.Length; i++) {
            Spawns[i] = Tiles[map.Spawns[i].x + 1, map.Spawns[i].y + 1];
        }
        if (setSpawns)
            PlayerController.Instance.SetSpawnPosition(new List<MapTile>( Spawns ));
        if (grid2d != null)
            grid2d.RecalculateGrid();
    }

    public static MapFileData LoadMapFile(string fileName) {
        MapFileData data = new MapFileData();
        data.Tiles = new TileType[MapController.maxX, MapController.maxY];
        Dictionary<int, Vector3Int> dicSpawns = new Dictionary<int, Vector3Int>();
        string[] allLines = GetMapFile(fileName);
        data.Author = allLines[0];
        int.TryParse(allLines[1], out data.Number);
        int y = 0; //because outer layer be empty
        for (int i = allLines.Length - 1; i >= 2; i--) {
            string line = allLines[i];
            int x = 0; //because outer layer be empty
            foreach (char c in line) {
                if (x >= maxX || y >= maxY)
                    continue;
                data.Tiles[x, y] = MapTile.ConvertChar(c);
                if (data.Tiles[x, y] == TileType.Spawn) {
                    dicSpawns[int.Parse("" + c)] = new Vector3Int(x, y, 0);
                }
                x++;
            }
            y++;
        }
        data.Spawns = new Vector3Int[dicSpawns.Count];
        for (int i = 0; i < PlayerController.MaxPlayer; i++) {
            if (dicSpawns.ContainsKey(i) && dicSpawns[i] != null) {
                data.Spawns[i] = dicSpawns[i];
            }
        }
        return data;
    }

    internal TileType GetTileTypeAt(Vector3 pos) {
        if (pos.x < 0 || pos.y < 0 || pos.x >= Tiles.GetLength(0) || pos.y >= Tiles.GetLength(1)) {
            return TileType.Empty;
        }
        return Tiles[Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y)].Type;
    }
    internal MapTile GetTile(Vector3 pos) {
        if (pos.x < 0 || pos.y < 0 || pos.x >= Tiles.GetLength(0)  || pos.y >= Tiles.GetLength(1) ) {
            return null;
        }
        return Tiles[Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y)];
    }
    
    private void SetTileMaps() {
        for (int x = 0; x < Tiles.GetLength(0); x++) {
            for (int y = 0; y < Tiles.GetLength(1); y++) {
                if (Tiles[x, y].Type != TileType.Empty) {
                    floorMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[TileType.Floor]);
                }
                if (Tiles[x, y].Type == TileType.Empty) {
                    floorMap.SetTile(new Vector3Int(x, y, 0), null);
                }
                switch (Tiles[x, y].Type) {
                    case TileType.Box:
                        boxMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.RandomBox:
                        if (previewMap == null) {
                            if (UnityEngine.Random.Range(0, 3) > 0) { //66.6666%
                                boxMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[TileType.Box]);
                                Tiles[x, y].Type = TileType.Box;
                            }
                            else {
                                Tiles[x, y].Type = TileType.Floor;
                            }
                        } else {
                            previewMap?.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        }
                        break;
                    case TileType.Wall:
                        wallMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.Ice:
                        iceMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.Hole:
                        triggerMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.ArrowUp:
                        triggerMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.ArrowDown:
                        triggerMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.ArrowLeft:
                        triggerMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.ArrowRight:
                        triggerMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                    case TileType.Spawn:
                        if(previewMap!=null)
                            previewMap.SetTile(new Vector3Int(x, y, 0), typeToTileBase[Tiles[x, y].Type]);
                        break;
                }
            }
        }
    }

    public void LoadTileBases() {
        TypeSprites current = typeSprites[(int)currentMapType];
        Tile floorBase = ScriptableObject.CreateInstance<Tile>();
        floorBase.sprite = current.floor;
        floorBase.colliderType = Tile.ColliderType.None;
        typeToTileBase[TileType.Floor] = floorBase;

        Tile boxBase = ScriptableObject.CreateInstance<Tile>();
        boxBase.sprite = current.normalBox;
        boxBase.colliderType = Tile.ColliderType.Grid;
        typeToTileBase[TileType.Box] = boxBase;
        Tile eboxBase = ScriptableObject.CreateInstance<Tile>();
        eboxBase.sprite = current.explodedBox;
        eboxBase.colliderType = Tile.ColliderType.Grid;
        typeToTileBase[TileType.ExplodedBox] = eboxBase;

        Tile wallBase = ScriptableObject.CreateInstance<Tile>();
        wallBase.sprite = current.wallSprite;
        wallBase.colliderType = Tile.ColliderType.Grid;
        typeToTileBase[TileType.Wall] = wallBase;

        Tile holeBase = ScriptableObject.CreateInstance<Tile>();
        holeBase.sprite = HoleSprite;
        holeBase.colliderType = Tile.ColliderType.Sprite;
        typeToTileBase[TileType.Hole] = holeBase;

        Tile iceBase = ScriptableObject.CreateInstance<Tile>();
        iceBase.sprite = IceSprite;
        iceBase.colliderType = Tile.ColliderType.Grid;
        typeToTileBase[TileType.Ice] = iceBase;

        Tile upArrowBase = ScriptableObject.CreateInstance<Tile>();
        upArrowBase.sprite = UpArrowSprite;
        upArrowBase.colliderType = Tile.ColliderType.Sprite;
        typeToTileBase[TileType.ArrowUp] = upArrowBase;

        Tile downArrowBase = ScriptableObject.CreateInstance<Tile>();
        downArrowBase.sprite = DownArrowSprite;
        downArrowBase.colliderType = Tile.ColliderType.Sprite;
        typeToTileBase[TileType.ArrowDown] = downArrowBase;

        Tile rightArrowBase = ScriptableObject.CreateInstance<Tile>();
        rightArrowBase.sprite = RightArrowSprite;
        rightArrowBase.colliderType = Tile.ColliderType.Sprite;
        typeToTileBase[TileType.ArrowRight] = rightArrowBase;

        Tile leftArrowBase = ScriptableObject.CreateInstance<Tile>();
        leftArrowBase.sprite = LeftArrowSprite;
        leftArrowBase.colliderType = Tile.ColliderType.Sprite;
        typeToTileBase[TileType.ArrowLeft] = leftArrowBase;

        Tile spawnBase = ScriptableObject.CreateInstance<Tile>();
        spawnBase.sprite = SpawnSprite;
        spawnBase.colliderType = Tile.ColliderType.Sprite;
        typeToTileBase[TileType.Spawn] = spawnBase;

        Tile randomBoxBase = ScriptableObject.CreateInstance<Tile>();
        randomBoxBase.sprite = RandomBoxSprite;
        randomBoxBase.colliderType = Tile.ColliderType.Sprite;
        typeToTileBase[TileType.RandomBox] = randomBoxBase;
    }

    internal void ExplodeBox(Vector3 position) {
        TileType tt = GetTileTypeAt(position);
        if (tt != TileType.Box)
            return;
        Vector3Int vector3Int = new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0);
        boxMap.SetTile(vector3Int, typeToTileBase[TileType.ExplodedBox]);
    }

    internal void RemoveExplodeBox(Vector3 position) {
        TileType tt = GetTileTypeAt(position);
        if (tt != TileType.Box)
            return;
        DestroyTile(position);
    }

    private IEnumerator RemoveDelayedTile(Vector3Int vector3Int, Tilemap map, TileType type) {
        float time = 0.34f;
        while(time>0) {
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        RemoveTile(vector3Int, map, type);
        yield return null;
    }
    internal void DestroyTile(Vector3 vector3) {
        Vector3Int vector3Int = new Vector3Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), 0);
        switch (Tiles[vector3Int.x, vector3Int.y].Type) {
            case TileType.Box:
                RemoveTile(vector3Int, boxMap, TileType.Box);
                break;
            case TileType.Wall:
                RemoveTile(vector3Int, wallMap, TileType.Wall);
                break;
            default:
                RemoveTile(vector3Int, floorMap, Tiles[vector3Int.x, vector3Int.y].Type);
                break;
        }
    }
    public void RemoveTile(Vector3Int vector3Int, Tilemap map, TileType type) {
        map.SetTile(vector3Int, null);
        TileType tt = Tiles[vector3Int.x, vector3Int.y].Type;
        if (tt != type)
            return;
        switch (type) {            
            case TileType.Ice:
            case TileType.Hole:
            case TileType.ArrowUp:
            case TileType.ArrowDown:
            case TileType.ArrowLeft:
            case TileType.ArrowRight:
            case TileType.Floor:
                grid2d.UpdateTile(vector3Int.x, vector3Int.y, true);
                break;
            case TileType.Box:
                ChangeNeigbhoursBoxCount(vector3Int.x, vector3Int.y, -1);
                grid2d.UpdateTile(vector3Int.x, vector3Int.y, false);
                break;
            case TileType.ExplodedBox:
            case TileType.RandomBox:
            case TileType.Wall:
                grid2d.UpdateTile(vector3Int.x, vector3Int.y, false);
                break;
            
        }
        if (tt == TileType.Box||tt==TileType.ExplodedBox) {
            Tiles[Mathf.FloorToInt(vector3Int.x), Mathf.FloorToInt(vector3Int.y)].Type = TileType.Floor;
            OnBoxDestroy(vector3Int);
        }
        else if(tt == TileType.Wall) {
            Tiles[Mathf.FloorToInt(vector3Int.x), Mathf.FloorToInt(vector3Int.y)].Type = TileType.Floor;
        }
        else {
            floorMap.SetTile(vector3Int, null);
            wallMap.SetTile(vector3Int, null);
            boxMap.SetTile(vector3Int, null);
            iceMap.SetTile(vector3Int, null);
            triggerMap.SetTile(vector3Int, null);
            Tiles[Mathf.FloorToInt(vector3Int.x), Mathf.FloorToInt(vector3Int.y)].Type = TileType.Empty;
        }
    }

    private void ChangeNeigbhoursBoxCount(int x, int y, int change) {
        foreach(MapTile t in GetTile(new Vector3(x, y)).GetNeighbours()) {
            if (t == null)
                continue;
            t.BoxCount += change;
        }
    }

    private void OnBoxDestroy(Vector3Int vector3Int) {
        int random = UnityEngine.Random.Range(0, 8);
        PowerUPType put = PowerUPType.Blastradius;
        switch (random) {
            case 0:
            case 1:
            case 2:
                //Speed, Blastradius, Bomb
                put = (PowerUPType)random;
                break;
            case 3:
                //Push, Throw
                put = (PowerUPType)(random + UnityEngine.Random.Range(0, 2));
                break;
            case 4:
                //Diarrhea, Joint, Superspeed
                int negative = UnityEngine.Random.Range(0, 16);
                if (negative > 2) {
                    //no powereffect
                    return;
                }
                put = (PowerUPType)5 + negative;
                break;
            default:
                return;
        }
        PowerUP pu = Instantiate(PowerUPPrefab);
        pu.SetPowerType(put, powerUPtypeSprites.Find(x => x.type == put).Sprite);
        pu.transform.position = new Vector3(0.5f, 0.5f) + vector3Int;
        Tiles[vector3Int.x, vector3Int.y].Type = TileType.Floor;
    }

    internal Vector2 GetRandomTargetTile(Vector3 notThis,bool nonWall = false, bool nonBox = false) {
        List<MapTile> choose = new List<MapTile>(ListOfTiles);
        choose.RemoveAll(x => x.Type == TileType.Empty);
        choose.RemoveAll(x => x.GetCenter() == notThis);
        if (nonWall) {
            choose.RemoveAll(x => x.Type == TileType.Wall);
        }
        if (nonBox) {
            choose.RemoveAll(x => x.Type == TileType.Box);
        }
        if (choose.Count == 0)
            return notThis;
        MapTile mt = choose[UnityEngine.Random.Range(0, choose.Count)];
        return mt.GetCenter();
    }

    internal Vector2 GetRandomTargetTile(bool nonWall=false, bool nonBox=false) {
        List<MapTile> choose = new List<MapTile>(ListOfTiles);
        choose.RemoveAll(x => x.Type == TileType.Empty);
        if (nonWall) {
            choose.RemoveAll(x => x.Type == TileType.Wall);
        }
        if (nonBox) {
            choose.RemoveAll(x => x.Type == TileType.Box);
        }
        MapTile mt = choose[UnityEngine.Random.Range(0, choose.Count)];
        return mt.GetCenter();
    }

    internal void CreateAndFlyPowerUPs(Dictionary<PowerUPType, int> powerUPTypeToAmount, PlayerData pm) {
        foreach(PowerUPType up in powerUPTypeToAmount.Keys) {
            for (int i = 0; i < powerUPTypeToAmount[up]; i++) {
                CreateAndFlyPowerUP(up, pm);
                //PowerUP powerUP = Instantiate(PowerUPPrefab);
                //powerUP.SetPowerType(up, powerUPtypeSprites.Find(x => x.type == up).Sprite);
                //powerUP.transform.position = pm.gameObject.transform.position;
                //powerUP.FlyToTarget(GetRandomTargetTile(true, true));
            }
        }
    }
    public void CreateAndFlyPowerUP(PowerUPType type, PlayerData pm) {
        PowerUP powerUP = Instantiate(PowerUPPrefab);
        powerUP.SetPowerType(type, powerUPtypeSprites.Find(x => x.type == type).Sprite);
        powerUP.transform.position = pm.gameObject.transform.position;
        powerUP.FlyToTarget(GetRandomTargetTile(true, true));
    }
    public static List<string> GetAllMapNames() {
        List<string> AllMaps = new List<string> ();
        string path = Path.Combine(Application.streamingAssetsPath, "Maps");
        string[] files = Directory.GetFiles(path, "*.map");
        foreach (string file in files) {
            AllMaps.Add(Path.GetFileNameWithoutExtension(file));
        }
        return AllMaps;
    }
    [Serializable]
    public struct TypeSprites {
        public MapType type;
        public Sprite wallSprite;
        public Sprite normalBox;
        public Sprite explodedBox;
        public Sprite floor;
    }
    [Serializable]
    public struct PowerUPSprites {
        public PowerUPType type;
        public Sprite Sprite;
    }
    public class MapTile {
        public readonly int x;
        public readonly int y;
        public Action<MapTile, TileType, TileType> cbTileTypeChange;
        private TileType type = TileType.Empty;
        private Bomb _bomb;
        public HashSet<PowerUP> PowerUPs = new HashSet<PowerUP>();
        public Bomb Bomb {
            get { return _bomb; }
            set {
                _bomb = value;
                MapController.Instance.UpdateGrid(this);
            }
        }
        public bool HasBomb => Bomb != null;
        public MapTile(TileType type, int x, int y) {
            this.x = x;
            this.y = y;
            Type = type;
        }

        public Vector2 Vector => new Vector2(x, y);
        public Vector3 Vector3 => new Vector3(x, y);

        public int BoxCount { get; internal set; }
        public TileType Type { get => type; 
            set {
                cbTileTypeChange?.Invoke(this, type, value);
                type = value;
            }
        }
        public MapTile[] GetNeighbours() {
            MapTile[] tiles = new MapTile[4];
            tiles[0] = MapController.Instance.GetTile (new Vector3(x - 1, y));
            tiles[1] = MapController.Instance.GetTile(new Vector3(x + 1, y));
            tiles[2] = MapController.Instance.GetTile(new Vector3(x, y - 1));
            tiles[3] = MapController.Instance.GetTile(new Vector3(x, y + 1));
            return tiles;
        }
        internal Vector3 GetCenter() {
            return new Vector3(x + 0.5f, y + 0.5f);
        }
        public static TileType ConvertChar(char c) {
            switch (c) {
                case '-':
                    return TileType.Empty;
                case '*':
                    return TileType.Wall;
                case ' ':
                    return TileType.Floor;
                case '+':
                    return TileType.Box;
                case 'v':
                    return TileType.ArrowDown;
                case '^':
                    return TileType.ArrowUp;
                case '<':
                    return TileType.ArrowLeft;
                case '>':
                    return TileType.ArrowRight;
                case 'o':
                    return TileType.Hole;
                case 'S':
                    return TileType.Ice;
                case 'R':
                    return TileType.RandomBox;
                default:
                    if (Char.IsDigit(c)) {
                        return TileType.Spawn;
                    }
                    return TileType.Empty;
            }

        }
    }

    private void UpdateGrid(MapTile mapTile) {
        grid2d.UpdateTile(mapTile.x, mapTile.y, mapTile.HasBomb);
    }

    public class MapFileData {
        public string Author;
        public int Number;
        public TileType[,] Tiles;
        public Vector3Int[] Spawns;
    }
}

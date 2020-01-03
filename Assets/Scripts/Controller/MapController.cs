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

    MapTile[,] Tiles;
    List<MapTile> ListOfTiles;
    Vector2Int[] Spawns;
    int maxX = 18;
    int maxY = 14;

    internal Vector2 ClampVector(Vector2 target) {
        return new Vector2(Mathf.Clamp(target.x, 0, maxX), Mathf.Clamp(target.y, 0, maxY));
    }

    void Start() {
        Instance = this;
        typeToTileBase = new Dictionary<TileType, Tile>();
        Tiles = new MapTile[maxX+1, maxY+1];
        for (int x = 0; x < Tiles.GetLength(0); x++) {
            for (int y = 0; y < Tiles.GetLength(1); y++) {
                Tiles[x, y] = new MapTile(TileType.Empty, x, y);
            }
        }
        ListOfTiles = new List<MapTile>();
        LoadTileBases();
        LoadMap("Hole_Run.map");
    }

    void Update() {
    }

    public void LoadMap(string name) {
        floorMap.ClearAllTiles();
        wallMap.ClearAllTiles();
        boxMap.ClearAllTiles();
        triggerMap.ClearAllTiles();
        if(previewMap!=null)
            previewMap.ClearAllTiles();
        iceMap.ClearAllTiles();

        Dictionary<int, Vector2Int> dicSpawns = new Dictionary<int, Vector2Int>();
        string[] allLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "Maps", name));
        int y = 1; //because outer layer be empty
        for (int i = allLines.Length-1; i >= 2; i--) {
            string line = allLines[i];
            int x = 1; //because outer layer be empty
            foreach (char c in line) {
                if (c == '-') {
                    Tiles[x, y] = new MapTile(TileType.Empty,x,y);
                }
                else if (c == ' ') {
                    Tiles[x, y] = new MapTile(TileType.Floor, x, y);
                }
                else if (c == '*') {
                    Tiles[x, y] = new MapTile(TileType.Wall, x, y);
                }
                else if (c == '+') {
                    Tiles[x, y] = new MapTile(TileType.Box, x, y); 
                }
                else if (c == 'v') {
                    Tiles[x, y] = new MapTile(TileType.ArrowDown, x, y); 
                }
                else if (c == '^') {
                    Tiles[x, y] = new MapTile(TileType.ArrowUp, x, y); 
                }
                else if (c == '>') {
                    Tiles[x, y] = new MapTile(TileType.ArrowRight, x, y);
                }
                else if (c == '<') {
                    Tiles[x, y] = new MapTile(TileType.ArrowLeft, x, y);
                }
                else if (c == 'S') {
                    Tiles[x, y] = new MapTile(TileType.Ice, x, y); 
                }
                else if (c == 'o') {
                    Tiles[x, y] = new MapTile(TileType.Hole, x, y); 
                }
                else if (c == 'R') {
                    Tiles[x, y] = new MapTile(TileType.RandomBox, x, y); 
                }
                else if (Char.IsDigit(c)) {
                    Tiles[x, y] = new MapTile(TileType.Spawn, x, y); 
                    dicSpawns[int.Parse("" + c)] = (new Vector2Int(x, y));
                }
                ListOfTiles.Add(Tiles[x, y]);
                x++;
            }
            y++;
        }
        Spawns = new Vector2Int[dicSpawns.Count];
        foreach (int i in dicSpawns.Keys) {
            Spawns[i] = dicSpawns[i];
        }
        SetTileMaps();
    }

    internal TileType GetTileTypeAt(Vector3 pos) {
        if (pos.x < 0 || pos.y < 0 || pos.x > Tiles.GetLength(0) - 1 || pos.y > Tiles.GetLength(1) - 1) {
            return TileType.Empty;
        }
        return Tiles[Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y)].Type;
    }
    internal MapTile GetTile(Vector3 pos) {
        if (pos.x < 0 || pos.y < 0 || pos.x > Tiles.GetLength(0) - 1 || pos.y > Tiles.GetLength(1) - 1) {
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
                            if (UnityEngine.Random.Range(0, 2) == 0) {
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
        typeToTileBase[TileType.Hole] = wallBase;

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

    internal void DestroyBox(Vector3 position) {
        TileType tt = Tiles[Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y)].Type;
        if (tt != TileType.Box)
            return;
        Vector3Int vector3Int = new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0);
        boxMap.SetTile(vector3Int, typeToTileBase[TileType.ExplodedBox]);
        StartCoroutine(RemoveDelayedTile(vector3Int, boxMap, TileType.Box));
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
    public void RemoveTile(Vector3Int vector3Int, Tilemap map, TileType type) {
        map.SetTile(vector3Int, null);
        TileType tt = Tiles[vector3Int.x, vector3Int.y].Type;
        if (tt != type)
            return;
        if (tt == TileType.Box||tt==TileType.ExplodedBox) {
            Tiles[Mathf.FloorToInt(vector3Int.x), Mathf.FloorToInt(vector3Int.y)].Type = TileType.Floor;
            OnBoxDestroy(vector3Int);
        }
        else if(tt == TileType.Wall) {
            Tiles[Mathf.FloorToInt(vector3Int.x), Mathf.FloorToInt(vector3Int.y)].Type = TileType.Floor;
        }
        else {
            floorMap.SetTile(vector3Int, null);
            Tiles[Mathf.FloorToInt(vector3Int.x), Mathf.FloorToInt(vector3Int.y)].Type = TileType.Floor;
        }
    }

    private void OnBoxDestroy(Vector3Int vector3Int) {
        //if (UnityEngine.Random.Range(0, 100) > 33) {
        //    return;
        //}
        PowerUP pu = Instantiate(PowerUPPrefab);
        if(UnityEngine.Random.Range(0, 100)<25) {
            PowerUPType put = (PowerUPType)5 + UnityEngine.Random.Range(0, 3);
            pu.SetPowerType(put, powerUPtypeSprites.Find(x=>x.type==put).Sprite);
        } else {
            PowerUPType put = (PowerUPType)UnityEngine.Random.Range(0, 5);
            pu.SetPowerType(put, powerUPtypeSprites.Find(x => x.type == put).Sprite);
        }
        pu.transform.position = new Vector3(0.5f,0.5f) + vector3Int;
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
        return new Vector2(mt.x, mt.y);
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
        public TileType Type;
        public Bomb Bomb;
        public bool HasBomb => Bomb!=null;
        public MapTile(TileType type, int x, int y) {
            this.x = x;
            this.y = y;
            Type = type;
        }

        public Vector2 Vector => new Vector2(x, y);

        internal Vector3 GetCenter() {
            return new Vector3(x + 0.5f, y + 0.5f);
        }
    }
}

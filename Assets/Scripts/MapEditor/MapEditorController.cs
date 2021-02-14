using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.EventSystems;

public class MapEditorController : MonoBehaviour {
    public static MapEditorController Instance;

    public Sprite EmptyTileSprite;
    public Tilemap FloorMap;
    public Tilemap OtherMap;

    public Tilemap GridMap;
    public TileTypeSprite[] sprites;
    Dictionary<TileType, Sprite> tileTypeToSprite;
    TileSelector tileSelector;
    TileType CurrentTileType => tileSelector.selectedType;
    TileType[,] Tiles;
    public List<Vector3Int> Spawns;
    internal string SpawnCount =>""+ Spawns.Count;
    Stack<TileChange> changes;
    Stack<TileChange> revertedChanges;
    void Start() {
        Instance = this;
        Spawns = new List<Vector3Int>();
        changes = new Stack<TileChange>();
        revertedChanges = new Stack<TileChange>();
        Tile tileBase = ScriptableObject.CreateInstance<Tile>();
        tileSelector = FindObjectOfType<TileSelector>();
        Tiles = new TileType[MapController.maxX, MapController.maxY];
        tileTypeToSprite = new Dictionary<TileType, Sprite>();
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = EmptyTileSprite;
        foreach(TileTypeSprite tts in sprites) {
            tileTypeToSprite[tts.type] = tts.sprite;
        }
        for (int y = 0; y < MapController.maxY; y++) {
            for (int x = 0; x < MapController.maxX; x++) {
                GridMap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        
    }

    void Update() {
        if(Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.Z)) {
            if(changes.Count>0) {
                TileChange tc = changes.Pop();
                SetTile(tc.oldType, tc.position, false);
                revertedChanges.Push(tc);
            }
        }
        if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.Y)) {
            if (revertedChanges.Count > 0) {
                TileChange tc = revertedChanges.Pop();
                SetTile(tc.newType, tc.position, false);
                changes.Push(tc);
            }
        }
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }
        Vector3 mousePosition3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mousePositionInt = new Vector3Int(Mathf.FloorToInt(mousePosition3.x),
                                                        Mathf.Abs(Mathf.FloorToInt(mousePosition3.y)),
                                                        0
                                                    );
        if (mousePositionInt.x < 0 
            || mousePositionInt.y < 0 
            || mousePositionInt.x > Tiles.GetLength(0) - 1 
            || mousePositionInt.y > Tiles.GetLength(1) - 1) {
            return;
        }
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Mouse0)) {
            SetTile(CurrentTileType, mousePositionInt);
        }
        if (Input.GetMouseButton(1) || Input.GetKeyDown(KeyCode.Mouse1)) {
            SetTile(TileType.Empty, mousePositionInt);
        }
    }
    public void SetTile(TileType type, Vector3Int position, bool addChanges=true) {
        if (type == Tiles[position.x, position.y])
            return;
        switch (type) {
            case TileType.Empty:
                OtherMap.SetTile(position, null);
                FloorMap.SetTile(position, null);
                break;
            case TileType.Floor:
                FloorMap.SetTile(position, GetTile(type));
                OtherMap.SetTile(position, null);
                break;
            default:
                FloorMap.SetTile(position, GetTile(TileType.Floor));
                OtherMap.SetTile(position, GetTile(type));
                break;
        }
        if (Tiles[position.x, position.y] == TileType.Spawn)
            Spawns.Remove(position);
        if (type == TileType.Spawn) {
            if (Spawns.Count == PlayerController.MaxPlayer) {
                SetTile(TileType.Floor, Spawns[0]);
            }
            Spawns.Add(position);
        }
        if(addChanges) {
            changes.Push(new TileChange {
                oldType = Tiles[position.x, position.y],
                newType = type,
                position = position
            });
            revertedChanges.Clear();
        }
        Tiles[position.x, position.y] = type;
    }

    private TileBase GetTile(TileType type) {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = tileTypeToSprite[type];
        return tile;
    }

    public string[] GetMapString() {
        string[] mapStrings = new string[MapController.maxY];
        for (int y = 0; y < MapController.maxY; y++) {
            mapStrings[y] = "";
            for (int x = 0; x < MapController.maxX; x++) {
                switch (Tiles[x, y]) {
                    case TileType.Empty:
                        mapStrings[y] += "-";
                        break;
                    case TileType.Floor:
                        mapStrings[y] += " ";
                        break;
                    case TileType.Ice:
                        mapStrings[y] += "S";
                        break;
                    case TileType.Hole:
                        mapStrings[y] += "o";
                        break;
                    case TileType.Box:
                        mapStrings[y] += "+";
                        break;
                    case TileType.RandomBox:
                        mapStrings[y] += "R";
                        break;
                    case TileType.Spawn:
                        mapStrings[y] += ""+Spawns.IndexOf(new Vector3Int(x,y,0));
                        break;
                    case TileType.Wall:
                        mapStrings[y] += "*";
                        break;
                    case TileType.ArrowUp:
                        mapStrings[y] += "^";
                        break;
                    case TileType.ArrowDown:
                        mapStrings[y] += "v";
                        break;
                    case TileType.ArrowLeft:
                        mapStrings[y] += "<";
                        break;
                    case TileType.ArrowRight:
                        mapStrings[y] += ">";
                        break;
                }
            }
        }
        return mapStrings;
    }

    internal void ResetChanges() {
        changes.Clear();
        revertedChanges.Clear();
    }

    struct TileChange {
        public TileType oldType;
        public Vector3Int position;
        public TileType newType;
    }

    [Serializable]
    public struct TileTypeSprite {
        public TileType type;
        public Sprite sprite;
    }
}

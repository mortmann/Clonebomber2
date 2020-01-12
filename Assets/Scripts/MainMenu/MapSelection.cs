using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapSelection : MonoBehaviour {
    public static MapSelection Instance;
    public MapSelect MapPrefab;
    public GameObject MapContent;
    public MapController MapController;
    public Action SelectAll;
    List<string> AllMaps;
    List<string> CurrentlyInList;

    public List<string> SelectedMaps;

    public Action UnselectAll { get; internal set; }
    Dictionary<int, List<string>> playerNumToMaps;
    Dictionary<string, MapSelect> nameToMapSelect;

    void Start() {
        Instance = this;
        AllMaps = new List<string>();
        SelectedMaps = new List<string>();
        string path = Path.Combine(Application.streamingAssetsPath, "Maps");
        string[] files = Directory.GetFiles(path,"*.map");
        foreach(string file in files) {
            AllMaps.Add(Path.GetFileNameWithoutExtension(file));
        }
        CurrentlyInList = new List<string>(AllMaps);
        playerNumToMaps = new Dictionary<int, List<string>>();
        nameToMapSelect = new Dictionary<string, MapSelect>();
        foreach (string map in AllMaps) {
            MapSelect go = Instantiate(MapPrefab);
            go.mapName = map;
            go.OnSelect += OnMapClick;
            go.transform.SetParent(MapContent.transform);
            go.GetComponentInChildren<Text>().text = map;
            go.Select(false);
            nameToMapSelect[map] = go;
            int.TryParse(MapController.Instance.GetMapFile(map)[1], out int playerNum);
            if (playerNumToMaps.ContainsKey(playerNum)) {
                playerNumToMaps[playerNum].Add(map);
            } else {
                playerNumToMaps.Add(playerNum, new List<string> { map });
            }
        }
        SelectedMaps.AddRange(AllMaps);
    }

    internal void ShowMap(string name) {
        MapController.Instance?.LoadMap(name);
    }

    void OnMapClick(string name, bool select) {
        if(select) {
            if(SelectedMaps.Contains(name)==false)
                SelectedMaps.Add(name);
        }
        else {
            SelectedMaps.Remove(name);
        }
        PlayerController.Instance.CheckStartButton();
        MapController.Instance?.LoadMap(name);
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            SelectAll?.Invoke();
            SelectedMaps.Clear();
            SelectedMaps.AddRange(AllMaps);
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            UnselectAll?.Invoke();
            SelectedMaps.Clear();
        }
    }

    internal void UpdateList(int count) {
        foreach(int pn in playerNumToMaps.Keys) {
            if (pn >= count) {
                foreach(string s in playerNumToMaps[pn]) {
                    if (nameToMapSelect[s].selected)
                        SelectedMaps.Add(s);
                }
                CurrentlyInList.AddRange(playerNumToMaps[pn]);
                continue;
            }
            SelectedMaps.RemoveAll(x => playerNumToMaps[pn].Contains(x));
            CurrentlyInList.RemoveAll(x => playerNumToMaps[pn].Contains(x));
        }
        foreach (MapSelect ms in nameToMapSelect.Values)
            ms.gameObject.SetActive(false);
        foreach (string name in CurrentlyInList)
            nameToMapSelect[name].gameObject.SetActive(true);
        PlayerController.Instance.CheckStartButton();
        MapController.Instance?.LoadMap(SelectedMaps[0]);

    }
}

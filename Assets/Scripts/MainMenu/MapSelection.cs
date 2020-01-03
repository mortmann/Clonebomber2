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
    List<string> SelectedMaps;

    public Action UnselectAll { get; internal set; }

    void Start() {
        Instance = this;
        AllMaps = new List<string>();
        SelectedMaps = new List<string>();
        string path = Path.Combine(Application.streamingAssetsPath, "Maps");
        string[] files = Directory.GetFiles(path,"*.map");
        foreach(string file in files) {
            AllMaps.Add(Path.GetFileNameWithoutExtension(file));
        }
        foreach(string map in AllMaps) {
            MapSelect go = Instantiate(MapPrefab);
            go.mapName = map;
            go.OnSelect += OnMapClick;
            go.transform.SetParent(MapContent.transform);
            go.GetComponentInChildren<Text>().text = map;
            go.Select(false);
        }
        SelectedMaps.AddRange(AllMaps);
    }

    internal void ShowMap(string name) {
        name += ".map";
        MapController.Instance?.LoadMap(name);
    }

    void OnMapClick(string name, bool select) {
        name += ".map";
        if(select) {
            SelectedMaps.Add(name);
        }
        else
            SelectedMaps.Remove(name);

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
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapEditorMenu : PauseMenu {
    public GameObject SavePanel;
    public GameObject MainPanel;
    public GameObject LoadPanel;
    public InputField SaveGameName;
    public InputField AuthorName;
    public GameObject SaveGamePrefab;
    public Transform LoadSaveGameParent;
    public Transform SaveSaveGameParent;

    public GameObject OverwriteWarning;
    public GameObject FailedSaveWarning;
    public GameObject YesButton;

    Dictionary<string, GameObject> saveNameToGO;

    string currentLoadSaveGame;
    protected override void Start() {
        base.Start();
        saveNameToGO = new Dictionary<string, GameObject>();
    }

    protected override void Update() {
        if (SavePanel.activeSelf || LoadPanel.activeSelf) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                GoToMain();
            }
            return;
        }
        base.Update();
    }

    public void OpenSavePanel() {
        MainPanel.SetActive(false);
        SavePanel.SetActive(true);
        foreach (Transform transform in SaveSaveGameParent) {
            Destroy(transform.gameObject);
        }
        saveNameToGO.Clear();
        foreach (string file in MapController.GetAllMapNames()) {
            GameObject go = Instantiate(SaveGamePrefab);
            go.transform.SetParent(SaveSaveGameParent, false);
            go.GetComponentInChildren<Text>().text = file;
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            EventTrigger.Entry click = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerClick
            };
            string temp = file;
            click.callback.AddListener((data) => {
                if (((PointerEventData)data).pointerId == -1) {
                    SaveGameName.text = temp;
                }
            });
            EventTrigger.Entry scroll = new EventTrigger.Entry {
                eventID = EventTriggerType.Scroll
            };
            scroll.callback.AddListener((data) => {
                ScrollRect sr = go.GetComponentInParent<ScrollRect>();
                sr.verticalScrollbar.value += sr.scrollSensitivity * Time.unscaledDeltaTime * ((PointerEventData)data).scrollDelta.y;
            });
            trigger.triggers.Add(scroll);
            trigger.triggers.Add(click);

        }
    }
    public void OpenLoadPanel() {
        MainPanel.SetActive(false);
        LoadPanel.SetActive(true);
        foreach (Transform transform in LoadSaveGameParent) {
            Destroy(transform.gameObject);
        }
        saveNameToGO.Clear();
        foreach (string file in MapController.GetAllMapNames()) {
            GameObject go = Instantiate(SaveGamePrefab);
            go.transform.SetParent(LoadSaveGameParent, false);
            go.GetComponentInChildren<Text>().text = file;
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            EventTrigger.Entry click = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerClick
            };
            string temp = file;
            click.callback.AddListener((data) => {
                if (((PointerEventData)data).pointerId == -1) {
                    if(currentLoadSaveGame!=null)
                        saveNameToGO[currentLoadSaveGame].GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                    currentLoadSaveGame = temp;
                    saveNameToGO[currentLoadSaveGame].GetComponent<Image>().color = new Color32(200, 0, 0, 128);
                }
            });
            EventTrigger.Entry scroll = new EventTrigger.Entry {
                eventID = EventTriggerType.Scroll
            };
            scroll.callback.AddListener((data) => {
                ScrollRect sr = go.GetComponentInParent<ScrollRect>();
                sr.verticalScrollbar.value += sr.scrollSensitivity * Time.unscaledDeltaTime * ((PointerEventData)data).scrollDelta.y;
            });
            trigger.triggers.Add(scroll);
            trigger.triggers.Add(click);
            saveNameToGO[temp] = go;
        }
    }
    public void GoToMain() {
        MainPanel.SetActive(true);
        LoadPanel.SetActive(false);
        SavePanel.SetActive(false);
    }
    public void LoadFile() {
        if (string.IsNullOrEmpty(currentLoadSaveGame))
            return;
        MapController.MapFileData map = MapController.LoadMapFile(currentLoadSaveGame);
        for (int x = 0; x < MapController.maxX; x++) {
            for (int y = 0; y < MapController.maxY; y++) {
                MapEditorController.Instance.SetTile(map.Tiles[x, y], new Vector3Int(x, y, 0));
            }
        }
        for(int i = 0; i<map.Spawns.Length; i++) {
            MapEditorController.Instance.Spawns.Add(map.Spawns[i]);
        }
        MapEditorController.Instance.ResetChanges();
        MapEditorController.Instance.Spawns.Clear();
        foreach (Vector3Int spawn in map.Spawns) {
            if (spawn != null) {
                MapEditorController.Instance.Spawns.Add(spawn);
            }
        }
        ClosePauseMenu();
        Debug.Log("LOADED");
    }
    public void SaveFileButton() {
        if(File.Exists(Path.Combine(Application.streamingAssetsPath, "Maps", SaveGameName.text + ".map"))) {
            OverwriteWarning.SetActive(true);
            YesButton.GetComponent<Button>().onClick.AddListener(() => SaveFile());
            return;
        }
        if (SaveFile()) {
            ClosePauseMenu();
        } else {
            FailedSaveWarning.SetActive(true);
        }
    }

    public bool SaveFile() {
        if (string.IsNullOrEmpty(SaveGameName.text)) {
            return false;
        }
        if (string.IsNullOrEmpty(AuthorName.text)) {
            return false;
        }
        OverwriteWarning.SetActive(false);

        List<string> finalStrings = new List<string>();
        string[] map = MapEditorController.Instance.GetMapString();
        finalStrings.Add(AuthorName.text);
        finalStrings.Add(MapEditorController.Instance.SpawnCount);
        for (int i = map.Length; i >= 0; i--) {
            finalStrings.Add(map[i]);
        }
        try {
            File.WriteAllLines(Path.Combine(Application.streamingAssetsPath, "Maps", SaveGameName.text + ".map"), finalStrings);
        } catch {
            return false;
        }
        return true;
    }
    internal string[] GetMapFile(string map) {
        return File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "Maps", map + ".map"));
    }
}

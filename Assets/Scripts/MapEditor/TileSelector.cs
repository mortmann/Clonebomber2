using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileSelector : MonoBehaviour {

    public GameObject TilePrefab;
    public Transform Parent;
    public TileType selectedType;
    Dictionary<TileType, GameObject> tileTypeToGO;
    void Start() {
        tileTypeToGO = new Dictionary<TileType, GameObject>();
        foreach (TileType tt in typeof(TileType).GetEnumValues()) {
            if (tt == TileType.ExplodedBox)
                continue;
            GameObject go = Instantiate(TilePrefab);
            go.transform.SetParent(Parent, false);
            tileTypeToGO[tt] = go;
            go.GetComponentInChildren<Text>().text = tt+"";
            EventTrigger triggers = go.GetComponent<EventTrigger>();
            EventTrigger.Entry click = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerClick
            };
            TileType temp = tt;
            click.callback.AddListener((data) => {
                if (((PointerEventData)data).pointerId == -1) {
                    OnClick(temp);
                }
            });
            triggers.triggers.Add(click);
        }
        OnClick(selectedType);
    }

    private void OnClick(TileType type) {
        tileTypeToGO[selectedType].GetComponent<Image>().color = new Color32(0, 0, 0, 0);
        selectedType = type;
        tileTypeToGO[selectedType].GetComponent<Image>().color = new Color32(200, 0, 0, 128);
    }

}

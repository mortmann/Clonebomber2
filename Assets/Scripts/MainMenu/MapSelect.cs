﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapSelect : MonoBehaviour {
    public string mapName;
    public bool selected=true;
    public Color32 selectedColor;

    public Action<string, bool> OnSelect { get; internal set; }

    void Start() {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry enter = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerClick
        };
        enter.callback.AddListener((data) => {

        if (((PointerEventData)data).pointerId == -1) {
                OnClick();
            } else {
                OnShow();
            }
        });
        trigger.triggers.Add(enter);
        MapSelection.Instance.SelectAll += SelectAll;
        MapSelection.Instance.UnselectAll += UnselectAll;
        //Select(true);
    }

    private void OnShow() {
        MapSelection.Instance.ShowMap(mapName);
    }

    private void SelectAll() {
        Select(false);
    }
    private void UnselectAll() {
        Unselect();
    }
    private void OnClick() {
        if (selected) {
            Unselect();
        } else {
            Select(true);
        }
    }
    public void Select(bool trigger) {
        selected = true;
        if(trigger)
            OnSelect?.Invoke(mapName, true);
        GetComponent<Image>().color = selectedColor;
    }
    void Unselect() {
        selected = false;
        OnSelect?.Invoke(mapName, false);
        GetComponent<Image>().color = new Color32(0, 0, 0, 0);
    }
    void Update() {
        
    }
}
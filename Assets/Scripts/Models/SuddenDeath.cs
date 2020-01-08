﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuddenDeath : Flyable {

    public Vector2 target;
    public Vector2 startPosition = new Vector2(9.5f,14.75f);
    public AudioClip crunchClip;
    AudioSource audioSource;
    void Start() {
        transform.position = startPosition;
        target = MapController.Instance.GetRandomTargetTile();
        FlyToTarget(target);
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        if (transform.position == flyTarget) {
            target = MapController.Instance.GetRandomTargetTile();
            FlyToTarget(target);
            MapController.Instance.DestroyTile(transform.position);
            audioSource.PlayOneShot(crunchClip);
        }
    }
}

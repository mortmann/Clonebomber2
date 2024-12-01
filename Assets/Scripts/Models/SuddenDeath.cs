using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuddenDeath : Flyable {

    public Vector2 target;
    public Vector2 initialPosition = new Vector2(9.5f,14.75f);
    public AudioClip crunchClip;
    AudioSource audioSource;
    void Start() {
        transform.position = initialPosition;
        target = MapController.Instance.GetRandomTargetTile();
        FlyToTarget(target,false,true);
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

    protected override void IsDoneFalling() {
        throw new NotImplementedException();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PowerUPType { Speed, Blastradius, Bomb, Push, Throw, Diarrhea, Joint, Superspeed }
public class PowerUP : Flyable {

    public PowerUPType PowerType;
    public AudioClip fallingClip;

    private void Update() {
        
    }
    protected override void CheckTile() {
        MapController.MapTile tt = MapController.Instance.GetTile(transform.position);
        switch (tt.Type) {
            case TileType.Empty:
                if (gameObject.layer == LayerMask.NameToLayer("FLYING"))
                    return;
                isFlying = true;
                Debug.Log("Falling");
                gameObject.layer = LayerMask.NameToLayer("FLYING");
                GetComponent<AudioSource>().PlayOneShot(fallingClip);
                StartCoroutine(Falling());
                break;
            }
        }
                private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("OnCollisionEnter2D" + collision);
        if (collision.collider.GetComponent<Blastbeam>() != null) {
            Destroy(this.gameObject);
        }
        if (collision.collider.GetComponent<PlayerMove>() != null) {
            collision.collider.GetComponent<PlayerMove>().AddPowerUP(PowerType);
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Blastbeam>()!=null) {
            if (PowerType == PowerUPType.Diarrhea || PowerType == PowerUPType.Joint || PowerType == PowerUPType.Superspeed) {
                FlyToTarget(MapController.Instance.GetRandomTargetTile(true,true));
            }
            else {
                Destroy(this.gameObject);
            }
        }
        if (collision.GetComponent<PlayerMove>() != null) {
            collision.GetComponent<PlayerMove>().AddPowerUP(PowerType);
            Destroy(this.gameObject);
        }
    }

    internal void SetPowerType(PowerUPType powerUPType, Sprite sprite) {
        PowerType = powerUPType;
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PowerUPType { Speed, Blastradius, Bomb, Push, Throw, Diarrhea, Joint, Superspeed }
public class PowerUP : Flyable {

    public PowerUPType PowerType;

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("OnCollisionEnter2D" + collision);
        if (collision.collider.GetComponent<Blastbeam>() != null) {
            Destroy(this.gameObject);
        }
        if (collision.collider.GetComponent<PlayerData>() != null) {
            collision.collider.GetComponent<PlayerData>().AddPowerUP(PowerType);
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
        if (collision.GetComponent<PlayerData>() != null) {
            collision.GetComponent<PlayerData>().AddPowerUP(PowerType);
            Destroy(this.gameObject);
        }
    }

    internal void SetPowerType(PowerUPType powerUPType, Sprite sprite) {
        PowerType = powerUPType;
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}

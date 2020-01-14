using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTrigger : MonoBehaviour {
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == gameObject.transform.parent.gameObject.layer) {
            collision.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<PlayerMove>()) {
            Bomb b = GetComponentInParent<Bomb>();
            if(b.isThrown&& collision.gameObject.layer==LayerMask.NameToLayer("Player"))
                collision.gameObject.layer = gameObject.transform.parent.gameObject.layer;
        }
    }
}

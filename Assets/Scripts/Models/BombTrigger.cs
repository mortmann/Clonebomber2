using System;
using System.Collections.Generic;
using UnityEngine;
public class BombTrigger : MonoBehaviour {
    private void OnTriggerExit2D(Collider2D collision) {   
        Bomb b = GetComponentInParent<Bomb>();
        if (collision.gameObject.layer == b.insideLayer) {
            collision.gameObject.layer = LayerMask.NameToLayer("Player");
        }
        if (b.isFlying || b.isThrown)
            return;
        Collider2D[] c2ds = Physics2D.OverlapCircleAll(transform.position, b.HitBox.radius);
        if(Array.Exists(c2ds,x=>x.GetComponent<PlayerMove>()!=null) == false) {
            //noo player so change layer
            b.gameObject.layer = b.finalLayer;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<PlayerMove>()) {
            Bomb b = GetComponentInParent<Bomb>();
            if(b.isThrown&& collision.gameObject.layer==LayerMask.NameToLayer("Player")) {
                collision.gameObject.layer = b.insideLayer;
                b.gameObject.layer = b.startLayer;
            }
        }
    }
}

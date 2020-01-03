using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMapCollider : MonoBehaviour {
    public void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log(collision.name);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log(collision.collider.name);
    }
}

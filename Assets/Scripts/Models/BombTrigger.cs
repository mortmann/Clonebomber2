using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTrigger : MonoBehaviour {
    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.gameObject.layer == gameObject.transform.parent.gameObject.layer) {
            collision.gameObject.layer = 0;
        }
    }
}

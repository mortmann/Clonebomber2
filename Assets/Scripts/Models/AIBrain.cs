using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapController;

public class AIBrain : MonoBehaviour {
    AIMove move;
    PlayerData data;
    MapTile[,] tiles;
    Pathfinding2D pathfinding;
    float pathfindingCooldown = 0.25f;
    Vector3 Position => transform.position;
    Vector3 Target = new Vector3(12, 4);
    Vector3 nextPosition = Vector3.zero;
    void Start() {
        nextPosition = transform.position;
        move = GetComponent<AIMove>();
        data = GetComponent<PlayerData>();
        pathfinding = GetComponent<Pathfinding2D>();
        //tiles = MapController.Instance.Tiles;
    }

    void Update() {
        //if (data.IsDead)
        //return;
        if(pathfinding.HasValidPath) {
            if((nextPosition - transform.position).magnitude < 0.02f) {
                Debug.Log(nextPosition +  " " + transform.position);
                nextPosition = pathfinding.GetNext();
            }
        } else {
            pathfindingCooldown -= Time.deltaTime;
            if (pathfindingCooldown < 0) {
                pathfinding.FindPath(Position, Target);
                pathfindingCooldown = 0.25f;
            }
            return;
        }
        Vector3 dir = nextPosition - transform.position;
        move.SetDirection(dir);
    }
}

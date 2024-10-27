using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapController;
using System.Linq;

public class AIBrain : MonoBehaviour {
    AIMove move;
    PlayerData data;
    List<MapTile> canReach = new List<MapTile>();
    Pathfinding2D pathfinding;
    float pathfindingCooldown = 0.25f;
    Vector3 Position => transform.position;
    MapTile currentTile;
    Vector3? Target;
    Vector3? nextPosition = null;
    void Start() {
        move = GetComponent<AIMove>();
        data = GetComponent<PlayerData>();
        if(GetComponent<Pathfinding2D>()==null)
            pathfinding = gameObject.AddComponent<Pathfinding2D>();
        //tiles = MapController.Instance.Tiles;
    }

    private void CheckTileForReach(MapTile tile) {
        if (tile.Type == TileType.Empty)
            return;
        if(tile.Type == TileType.Wall || tile.Type == TileType.Box) {
            tile.cbTileTypeChange += OnMapTileTypeChange;
            return;
        }
        canReach.Add(tile);
        foreach(MapTile t in tile.GetNeighbours()) {
            if (t == null)
                continue;
            if (canReach.Contains(t))
                continue;
            CheckTileForReach(t);
        }
    }

    private void OnMapTileTypeChange(MapTile tile, TileType old, TileType newT) {
        if (newT != TileType.Wall || newT != TileType.Empty || newT != TileType.Box)
            return;
        canReach.Add(tile);
    }

    void Update() {
        if (data.IsDead || move.isFlying)
            return;
        if (canReach.Count == 0) {
            CheckTileForReach(MapController.Instance.GetTile(Position));
        }
        if(pathfinding.HasValidPath == false)
            DecideTarget();
        DoMovement();
        DecideAction();
    }

    private void DecideTarget() {
        List<MapTile> possible = canReach.ToList();
        foreach(Bomb b in BombController.Instance.AllBombs) {
            possible.RemoveAll(x => b.BlastBeamTiles().Contains(x));
        }
        if (possible.Count == 0)
            return;
        List<MapTile> ordered = possible.OrderByDescending(x => x.BoxCount).ThenBy(x => Vector3.Distance(x.GetCenter(), Position)).ToList();
        for (int i = 0; i < ordered.Count; i++) {
            if (possible.Union(Bomb.BlastBeamTiles(ordered[i].GetCenter(), move.Blastradius)).Distinct().Any() == false)
                continue;
            Target = ordered[i].GetCenter();
            break;
        }
        Debug.Log("Target " + Target);
    }

    private void DecideAction() {
        List<MapTile> possible = canReach.ToList();
        if (possible.Union(Bomb.BlastBeamTiles(currentTile.GetCenter(), move.Blastradius)).Distinct().Any() == false)
            return;
        if (Target == null && currentTile.BoxCount>0)
            move.doAction = true;
    }

    private void DoMovement() {
        currentTile = MapController.Instance.GetTile(Position);
        if ((transform.position - Target.Value).magnitude < 0.02f) {
            Target = null;
            move.SetDirection(Vector3.zero);
        }
        if (Target.HasValue == false)
            return;
        if (pathfinding.HasValidPath) {
            if (nextPosition == null) {
                nextPosition = pathfinding.GetNext();
                Debug.Log("nextPosition " + nextPosition);
            }
        }
        else if(nextPosition == null) {
            pathfindingCooldown -= Time.deltaTime;
            if (pathfindingCooldown < 0) {
                pathfinding.FindPath(Position, Target.Value);
                nextPosition = pathfinding.GetNext();
                pathfindingCooldown = 0.25f;
            }
            return;
        }
        if (nextPosition == null)
            return;
        if ((nextPosition.Value - Position).magnitude > 0.02f) {
            Vector3 dir = nextPosition.Value - transform.position;
            //Debug.Log(nextPosition + " " + dir);
            move.SetDirection(dir);
        } else {
            nextPosition = null;
        }
        //else {
        //    Target = null;
        //    move.SetDirection(Vector3.zero);
        //}
    }
    public float CalculateTileValue(MapTile tile) {
        return Vector3.Distance(tile.GetCenter(), Position) * tile.BoxCount * tile.PowerUPs.Count * (tile.HasBomb? 100000000 : 1);
    }
    private void OnDrawGizmos() {
        if (Target.HasValue == false)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawCube(Target.Value, Vector3.one * .6f);
        Gizmos.color = Color.yellow;
        if(pathfinding.HasValidPath)
            foreach (Node2D t in pathfinding.Path) {
                Gizmos.DrawCube(t.worldPosition, Vector3.one * .6f);
            }
    }
    
}

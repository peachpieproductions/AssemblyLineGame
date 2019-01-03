using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispensor : BaseEntity {

    public bool vacuum;

    public override void Start() {
        base.Start();
        
    }

    public override void UpdateEntity(bool updateNeighbors = false) {
        base.UpdateEntity(updateNeighbors);

        if (vacuum) {
            foreach(EntityNeighbor n in neighbors) {
                if (n.dir == -(Vector2)transform.right) {
                    n.entity = null;
                }
            }
        }
    }



}

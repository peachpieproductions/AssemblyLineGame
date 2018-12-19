using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : BaseEntity {

    public Vector3 pushDir;
    public BaseEntity facingEntity;

    public override void Start() {
        base.Start();
        
    }

    public void FixedUpdate() {

        for (var i = itemsInZone.Count-1; i >= 0; i--) {
            itemsInZone[i].position += (pushDir - itemsInZone[i].position) * Time.deltaTime;
        }

    }

    public override void UpdateEntity(bool updateNeighbors = false) {
        base.UpdateEntity(updateNeighbors);

        var facingIndex = gCon.entityGrid[currentCoord.x + (int)transform.right.x, currentCoord.y + (int)transform.right.y];
        if (facingIndex > 0) facingEntity = gCon.entities[facingIndex - 1];
        else facingEntity = null;
        if (facingEntity) {
            foreach(EntityNeighbor n in facingEntity.neighbors) {
                if (n.entity == this) n.entity = null;
            }
        }
        pushDir = transform.position + transform.right;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : BaseEntity {

    public bool inboundPackageZone;

    public override void Start() {
        base.Start();
        if (inboundPackageZone) GameController.inst.inboundZones.Add(this);
    }

    public override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        var p = collision.transform.GetComponent<Package>();
        if (p && !inboundPackageZone) {
            GameController.inst.outboundPackages.Add(p);
        }
    }

    private void OnDestroy() {
        if (inboundPackageZone) GameController.inst.inboundZones.Remove(this);
    }



}

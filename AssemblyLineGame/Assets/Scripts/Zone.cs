using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Zone : BaseEntity {

    public bool inboundPackageZone;
    public int inboundPackageZoneID;
    public string inboundZoneTag = "";

    public override void Start() {
        base.Start();
        if (inboundPackageZone) {
            GameController.inst.inboundZones.Add(this);
            inboundPackageZoneID = GameController.inst.inboundZones.Count;
            GetComponentInChildren<TextMeshPro>().text = "#" + inboundPackageZoneID;
            GameController.inst.UpdateInboundZonesSelector();
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        var p = collision.transform.GetComponent<Package>();
        if (p && !inboundPackageZone) {
            if (!GameController.inst.outboundPackages.Contains(p)) {
                GameController.inst.outboundPackages.Add(p);
                GameController.inst.outboundStockOverlay.BuildMenu();
            }
        }
    }

    private void OnDestroy() {
        if (inboundPackageZone) {
            GameController.inst.inboundZones.Remove(this);
            GameController.inst.UpdateInboundZonesSelector();
        }
    }



}

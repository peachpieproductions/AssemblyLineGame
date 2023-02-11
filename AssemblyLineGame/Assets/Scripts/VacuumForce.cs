using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuumForce : MonoBehaviour {


    public BaseEntity entity;
    public List<Item> itemsInZone = new List<Item>();
    public List<Package> packagesInZone = new List<Package>();

    private void Start() {
        entity = GetComponentInParent<BaseEntity>();
    }

    private void FixedUpdate() {
        for (var i = itemsInZone.Count - 1; i >= 0; i--) {
            if (itemsInZone[i].gameObject.activeSelf) {
                if (entity && entity.filters.Count > 0) {
                    if (!entity.filters.Contains(itemsInZone[i].data)) continue;
                }
                var diff = (transform.position - itemsInZone[i].transform.position);
                itemsInZone[i].transform.position += diff.normalized * Time.deltaTime * (3.5f - diff.magnitude);
            } else {
                itemsInZone.Remove(itemsInZone[i]);
            }
        }
        for (var i = packagesInZone.Count - 1; i >= 0; i--) {
            if (packagesInZone[i]) {
                if (entity && entity.filters.Count > 0) {
                    if (!entity.filters.Contains(packagesInZone[i].storage.data)) continue;
                }
                var diff = (transform.position - packagesInZone[i].transform.position);
                packagesInZone[i].transform.position += diff.normalized * Time.deltaTime * (3.5f - diff.magnitude);
            }
            else {
                packagesInZone.Remove(packagesInZone[i]);
            }
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision) {
        var item = collision.transform.GetComponent<Item>();
        if (item) itemsInZone.Add(item);
        else {
            var package = collision.transform.GetComponent<Package>();
            if (package) packagesInZone.Add(package);
        }

    }

    public virtual void OnTriggerExit2D(Collider2D collision) {
        var item = collision.transform.GetComponent<Item>();
        if (item) itemsInZone.Remove(item);
        else {
            var package = collision.transform.GetComponent<Package>();
            if (package) packagesInZone.Remove(package);
        }
    }


}

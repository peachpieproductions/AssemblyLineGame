using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuumForce : MonoBehaviour {


    public List<Transform> itemsInZone = new List<Transform>();

    private void FixedUpdate() {
        for (var i = itemsInZone.Count - 1; i >= 0; i--) {
            if (itemsInZone[i].gameObject.activeSelf) {
                var diff = (transform.position - itemsInZone[i].position);
                itemsInZone[i].position += diff.normalized * Time.deltaTime * (3.5f - diff.magnitude);
            } else {
                itemsInZone.Remove(itemsInZone[i]);
            }
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision) {
        itemsInZone.Add(collision.transform);
    }

    public virtual void OnTriggerExit2D(Collider2D collision) {
        itemsInZone.Remove(collision.transform);
    }


}

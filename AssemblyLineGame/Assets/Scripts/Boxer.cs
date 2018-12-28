using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxer : BaseEntity {

    public GameObject box;


    public override void Start() {
        base.Start();
        StartCoroutine(BoxItems());
    }

    public IEnumerator BoxItems() {

        while (true) {

            foreach (EntityNeighbor n in neighbors) {
                if (n.entity && !n.entity.ignoredByDispensors) {
                    foreach (StorageSlot s in storage) {
                        if (s.itemCount >= 10) {
                            var inst = Instantiate(box,(Vector2)transform.position + n.dir,Quaternion.identity).GetComponent<Package>();
                            inst.storage.data = s.data;
                            inst.spriteRenderer.sprite = s.data.sprite;
                            inst.storage.itemCount = 10;
                            inst.GetComponent<Rigidbody2D>().velocity = n.dir * 3 + new Vector2(Random.Range(-1f,1f), Random.Range(-1f, 1f));
                            s.itemCount -= 10;
                            if (s.itemCount == 0) s.data = null;
                        }
                    }
                }
            }
            
            yield return new WaitForSeconds(1f);
        }

    }


}

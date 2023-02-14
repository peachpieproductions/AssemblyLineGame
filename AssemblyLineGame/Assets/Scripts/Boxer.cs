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

            if (active) {
                for (var i = 1; i < 5; i++) {
                    var index = currentNeighbor + i;
                    if (index > 3) index -= 4;
                    if (neighbors[index].entity) {
                        if (!neighbors[index].entity.ignoredByDispensors || neighbors[index].entity is Zone) {
                            currentNeighbor = index;
                            break;
                        }
                    }
                }

                if (neighbors[currentNeighbor].entity) {
                    foreach (StorageSlot s in storage) {
                        if (s.itemCount >= 10) {
                            PlaySound(0);
                            var inst = Instantiate(box, (Vector2)transform.position + neighbors[currentNeighbor].dir, Quaternion.identity).GetComponent<Package>();
                            inst.storage.data = s.data;
                            inst.spriteRenderer.sprite = s.data.sprite;
                            inst.storage.itemCount = 10;
                            if (neighbors[currentNeighbor].entity is Zone) {
                                inst.GetComponent<Rigidbody2D>().velocity = neighbors[currentNeighbor].dir * 4 + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
                            }
                            s.itemCount -= 10;
                            if (s.itemCount == 0) s.data = null;
                            if (GameController.inst.selectedEntity == this) GameController.inst.entityMenu.BuildMenu();
                            //if (GameController.inst.contractsMenu.open) GameController.inst.CheckForCompletedContracts();
                        }
                    }
                }
            }
            
            yield return new WaitForSeconds(1f);
        }

    }


}

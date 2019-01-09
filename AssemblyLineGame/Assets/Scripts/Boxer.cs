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

            getNextNeighbor();

            if (neighbors[currentNeighbor].entity) {
                foreach (StorageSlot s in storage) {
                    if (s.itemCount >= 10) {
                        var inst = Instantiate(box,(Vector2)transform.position + neighbors[currentNeighbor].dir,Quaternion.identity).GetComponent<Package>();
                        inst.storage.data = s.data;
                        inst.spriteRenderer.sprite = s.data.sprite;
                        inst.storage.itemCount = 10;
                        if (neighbors[currentNeighbor].entity is Zone) {
                            inst.GetComponent<Rigidbody2D>().velocity = neighbors[currentNeighbor].dir * 4 + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                        }
                        s.itemCount -= 10;
                        if (s.itemCount == 0) s.data = null;
                        if (GameController.inst.selectedEntity == this) GameController.inst.entityMenu.BuildMenu();
                        if (GameController.inst.contractsMenu.open) GameController.inst.contractsMenu.BuildMenu();
                    }
                }
            }
            
            yield return new WaitForSeconds(1f);
        }

    }


}

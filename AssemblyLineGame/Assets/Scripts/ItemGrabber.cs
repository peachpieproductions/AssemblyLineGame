using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGrabber : BaseEntity {

    public Transform armPivot;
    public Transform transferingItem;
    public BaseEntity nextToStorage;

    public override void Start() {
        base.Start();
        StartCoroutine(TransferItems());
    }

    public override void UpdateEntity(bool updateNeighbors = false) {
        base.UpdateEntity(updateNeighbors);

        BaseEntity nextToEntity = null;
        var facingIndex = gCon.entityGrid[currentCoord.x + -(int)transform.right.x, currentCoord.y + -(int)transform.right.y];
        if (facingIndex > 0) nextToEntity = gCon.entities[facingIndex - 1];
        if (nextToEntity) {
            if (nextToEntity.storage.Count > 0) {
                nextToStorage = nextToEntity;
            }
        }
    }

    public IEnumerator TransferItems() {

        while (true) {

            if (transferingItem) {
                armPivot.localEulerAngles = Vector3.Slerp(armPivot.localEulerAngles, new Vector3(0, 0, 0), Time.deltaTime * 3);
                if (armPivot.localEulerAngles.z < 5) {
                    transferingItem.parent = null;
                    transferingItem.GetComponent<Rigidbody2D>().simulated = true;
                    transferingItem = null;
                }
            }
            else {
                armPivot.localEulerAngles = Vector3.Slerp(armPivot.localEulerAngles, new Vector3(0, 0, 180), Time.deltaTime * 3);
                if (armPivot.localEulerAngles.z > 175) {
                    if (nextToStorage) { //Grab items from storage slots
                        foreach(StorageSlot s in nextToStorage.storage) {
                            if (s.itemCount > 0) {
                                s.itemCount--;
                                var newItem = GameController.inst.SpawnItem(s.data);
                                transferingItem = newItem.transform;
                                break;
                            }
                        }
                    } else {
                        foreach (Transform t in itemsInZone) { //Grab Items in Zone
                            if (t.GetComponent<Rigidbody2D>()) {
                                transferingItem = t;
                                break;
                            }
                        }
                    }
                    if (transferingItem) {
                        transferingItem.parent = armPivot;
                        transferingItem.localPosition = new Vector3(1, 0);
                        transferingItem.GetComponent<Rigidbody2D>().simulated = false;
                    } else yield return new WaitForSeconds(.1f);
                }
            }
            yield return null;
        }

    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityNeighbor {
    public BaseEntity entity;
    public Vector2Int dir;
}

public class BaseEntity : MonoBehaviour {
   
    public EntityData data;
    public EntityNeighbor[] neighbors = new EntityNeighbor[4];
    public int currentNeighbor;
    public List<StorageSlot> storage = new List<StorageSlot>();
    public Vector2Int currentCoord;
    public List<Transform> itemsInZone = new List<Transform>();
    public bool storesItems;
    public bool ignorePackages;
    public bool dispense;
    public bool ignoredByDispensors;
    public bool noEntityMenu;
    public bool canFilter;
    public ItemData filter;
    public Vector2Int size = new Vector2Int(1, 1);
    public SpriteRenderer spr;

    protected GameController gCon;

    private void OnMouseUp() {
        Clicked();
    }

    private void Awake() {
        gCon = GameController.inst;
    }

    public virtual void Start() {
        SetNeighbors();
        UpdateEntity(true);
        if (dispense) StartCoroutine(DispenseCycle());
        if (storesItems) StartCoroutine(StoreItems());
    }

    public void Clicked() {
        if (!noEntityMenu && !GameController.inst.hoveringOverlay && !GameController.inst.buildMode) {
            GameController.inst.selectedEntity = this;
            GameController.inst.entityMenu.ToggleOpenClose(true);
        }
    }


    public void SetNeighbors() {
        neighbors[0].dir = new Vector2Int(1, 0);
        neighbors[1].dir = new Vector2Int(0, 1);
        neighbors[2].dir = new Vector2Int(-1, 0);
        neighbors[3].dir = new Vector2Int(0, -1);
    }

    public virtual void UpdateEntity(bool updateNeighbors = false) {

        currentCoord = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

        for (var i = 0; i < 4; i++) {
            var check = currentCoord + neighbors[i].dir;
            if (check.x > 0 && check.y > 0 && check.x < 50 && check.y < 50) {
                var index = gCon.entityGrid[check.x, check.y];
                bool ignore = false;
                if (index > 0) {
                    if (gCon.entities[index - 1] is Conveyor && gCon.entities[index - 1].GetComponent<Conveyor>().facingEntity == this) {
                        ignore = true; // if neighbor is conveyor feeding into this entity, ignore it.
                    }
                    if (gCon.entities[index - 1] is Dispensor && (this is Dispensor || this is Assembler)) ignore = true;
                    if (gCon.entities[index - 1] is Assembler && this is Assembler) ignore = true;
                    if (!ignore) neighbors[i].entity = gCon.entities[index - 1];
                } else neighbors[i].entity = null;
            }
        }

        if (updateNeighbors) UpdateNeighbors();
    }

    public void UpdateNeighbors() {
        foreach(EntityNeighbor n in neighbors) {
            if (n.entity) n.entity.UpdateEntity();
            else {
                var coord = currentCoord + n.dir;
                if (coord.x > 0 && coord.y > 0 && coord.x < 50 && coord.y < 50) {
                    int index = GameController.inst.entityGrid[coord.x, coord.y] - 1;
                    if (index > 0) GameController.inst.entities[index].UpdateEntity();
                }
            }
        }
    }

    public bool AddToStorage(ItemData data, int amount, StorageSlot fromSlot = null) {
        bool addedToInv = false;
        foreach (StorageSlot slot in storage) { //find existing slot
            if (slot.data == data) {
                slot.itemCount += amount;
                addedToInv = true;
                break;
            }
        }
        if (!addedToInv) {
            foreach (StorageSlot slot in storage) { //find new slot
                if (slot.itemCount == 0) {
                    slot.data = data;
                    slot.itemCount += amount;
                    addedToInv = true;
                    break;
                }
            }
        }
        if (GameController.inst.selectedEntity == this) {
            GameController.inst.entityMenu.BuildMenu();
        }
        if (addedToInv && fromSlot != null) {
            fromSlot.itemCount -= amount;
            if (fromSlot.itemCount == 0) fromSlot.data = null;
        }
        return addedToInv;
    }

    public virtual void OnTriggerEnter2D(Collider2D collision) {
        itemsInZone.Add(collision.transform);
        if (collision.CompareTag("Item")) {
            collision.GetComponent<Item>().inEntityZone.Add(this);
        }
    }

    public virtual void OnTriggerExit2D(Collider2D collision) {
        itemsInZone.Remove(collision.transform);
        if (collision.CompareTag("Item")) {
            collision.GetComponent<Item>().inEntityZone.Remove(this);
        }
    }

    public IEnumerator DispenseCycle() {
        yield return new WaitForSeconds(1f);
        var i = 0;  foreach(EntityNeighbor n in neighbors) {
            if (n.entity && n.entity.ignoredByDispensors) neighbors[i].entity = null;
            i++;
        }
        while (true) {

            foreach (StorageSlot slot in storage) {
                if (slot.data && slot.itemCount > 0) {
                    if (Dispense(slot.data)) {
                        slot.itemCount--;
                        if (slot.itemCount == 0) slot.data = null;

                        if (GameController.inst.selectedEntity == this) {
                            GameController.inst.entityMenu.BuildMenu();
                        }
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(.5f);
        }

    }

    public IEnumerator StoreItems() {

        while (storesItems) {

            for (var i = itemsInZone.Count - 1; i >= 0; i--) {
                var item = itemsInZone[i].GetComponent<Item>();
                if (item) {
                    if (filter == null || filter == item.data) {
                        bool stored = false;
                        for (var j = 0; j < storage.Count; j++) {
                            if (storage[j].data == item.data) {
                                storage[j].itemCount++;
                                gCon.DespawnItem(item);
                                stored = true;
                                break;
                            }
                        }
                        if (!stored) {
                            for (var j = 0; j < storage.Count; j++) {
                                if (storage[j].itemCount == 0) {
                                    storage[j].data = item.data;
                                    storage[j].itemCount++;
                                    gCon.DespawnItem(item);
                                    break;
                                }
                            }
                        }
                        if (GameController.inst.selectedEntity == this) {
                            GameController.inst.entityMenu.BuildMenu();
                        }
                    }
                } else {
                    if (!ignorePackages) {
                        var package = itemsInZone[i].GetComponent<Package>();
                        if (package) {
                            if (filter == null || filter == package.storage.data) {
                                package.UnpackageIntoStorage(this);
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(.25f);
        }

    }

    public void getNextNeighbor() {
        for (var i = 1; i < 5; i++) {
            var index = currentNeighbor + i;
            if (index > 3) index -= 4;
            if (neighbors[index].entity && !neighbors[index].entity.ignoredByDispensors) { currentNeighbor = index; break; }
        }
    }

    public bool Dispense(ItemData data) {

        getNextNeighbor();

        if (neighbors[currentNeighbor].entity) {
            var newItem = gCon.SpawnItem(data);
            newItem.transform.position = (Vector2)transform.position + (Vector2)neighbors[currentNeighbor].dir * .65f;
            return true;
        }
        return false;

    }

}

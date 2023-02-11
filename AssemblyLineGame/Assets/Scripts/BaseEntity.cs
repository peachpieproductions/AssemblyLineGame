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
    public bool active = true;
    public bool canBeToggledActive = true;
    public EntityNeighbor[] neighbors = new EntityNeighbor[4];
    public int currentNeighbor;
    public int currentDispensingSlot;
    public List<StorageSlot> storage = new List<StorageSlot>();
    public Vector2Int currentCoord;
    public List<Transform> itemsInZone = new List<Transform>();
    public bool storesItems;
    public bool ignorePackages;
    public bool dispense;
    public bool ignoredByDispensors;
    public bool noEntityMenu;
    public bool canFilter;
    public List<ItemData> filters;
    public SpriteRenderer spr;
    public int relevantItemSpriteCount;
    public List<SpriteRenderer> relevantItemSprites;

    protected GameController gCon;

    /*private void OnMouseUp() {
        Clicked();
    }*/

    private void OnMouseOver() {
        if (Input.GetMouseButtonUp(0)) {
            Clicked();
        }
        if (Input.GetMouseButtonUp(1)) {
            ToggleActive();
        }
    }

    private void Awake() {
        gCon = GameController.inst;
    }

    public virtual void Start() {
        SetNeighbors();
        UpdateEntity(true);
        if (dispense) StartCoroutine(DispenseCycle());
        if (storesItems) StartCoroutine(StoreItems());
        SetUpRelevantItemSprite();
    }

    public void Clicked() {
        if (!noEntityMenu && !GameController.inst.hoveringOverlay && !GameController.inst.buildMode) {
            GameController.inst.selectedEntity = this;
            GameController.inst.entityMenu.ToggleOpenClose(true);
        }
    }

    public virtual void ToggleActive() {
        if (canBeToggledActive) {
            active = !active;
            if (active) spr.color = Color.white;
            else spr.color = Color.grey;
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

            if (active) {
                if (storage[currentDispensingSlot].data && storage[currentDispensingSlot].itemCount > 0) {
                    var lastActiveNeighbor = currentNeighbor;
                    if (Dispense(storage[currentDispensingSlot].data)) {
                        storage[currentDispensingSlot].itemCount--;
                        if (storage[currentDispensingSlot].itemCount == 0) storage[currentDispensingSlot].data = null;
                        if (currentNeighbor <= lastActiveNeighbor) GetNextDispensingSlot();

                        if (GameController.inst.selectedEntity == this) {
                            GameController.inst.entityMenu.BuildMenu();
                        }
                    }
                }
                else GetNextDispensingSlot();
            }

            yield return new WaitForSeconds(.5f);
        }

    }

    public void GetNextNeighbor() {
        for (var i = 1; i < 5; i++) {
            var index = currentNeighbor + i;
            if (index > 3) { index -= 4; }
            if (neighbors[index].entity && !neighbors[index].entity.ignoredByDispensors && neighbors[index].entity.active) { currentNeighbor = index; break; }
        }
    }

    public void GetNextDispensingSlot() {
        for (int i = 1; i < storage.Count + 1; i++) {
            var index = currentDispensingSlot + i;
            if (index >= storage.Count) index -= storage.Count;
            if (storage[index].data && storage[index].itemCount > 0) { currentDispensingSlot = index; break; }
        }
    }

    public bool Dispense(ItemData data) {

        if (this is Assembler == false) GetNextNeighbor();

        if (neighbors[currentNeighbor].entity && neighbors[currentNeighbor].entity.active) {
            var newItem = gCon.SpawnItem(data);
            newItem.transform.position = (Vector2)transform.position + (Vector2)neighbors[currentNeighbor].dir * .65f;
            return true;
        }
        return false;

    }

    public IEnumerator StoreItems() {

        while (storesItems) {

            for (var i = itemsInZone.Count - 1; i >= 0; i--) {
                var item = itemsInZone[i].GetComponent<Item>();
                if (item) {
                    if (filters.Count == 0 || filters.Contains(item.data)) {
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
                            if (filters.Count == 0 || filters.Contains(package.storage.data)) {
                                package.UnpackageIntoStorage(this);
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(.25f);
        }

    }

    public void UpdateRelevantItemSprites() {
        for (int i = 0; i < relevantItemSprites.Count; i++) {
            if (this is Assembler) {
                var assembler = (Assembler)this;
                relevantItemSprites[0].gameObject.SetActive(assembler.assemblingItem != null);
                if (assembler.assemblingItem != null) relevantItemSprites[0].sprite = assembler.assemblingItem.sprite;
            } else {
                if (filters.Count > i) {
                    relevantItemSprites[i].gameObject.SetActive(true);
                    relevantItemSprites[i].sprite = filters[i].sprite;
                } else {
                    relevantItemSprites[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetUpRelevantItemSprite() {
        for (int i = 0; i < relevantItemSpriteCount; i++) {
            var newSpriteGO = new GameObject("relevantSprite");
            newSpriteGO.transform.parent = transform;
            newSpriteGO.transform.localPosition = new Vector3(0, 1 * i, -.01f);
            relevantItemSprites.Add(newSpriteGO.AddComponent<SpriteRenderer>());
            relevantItemSprites[i].color = new Color(1f, 1f, 1f, .8f);
            newSpriteGO.SetActive(false);
        }
    }

}

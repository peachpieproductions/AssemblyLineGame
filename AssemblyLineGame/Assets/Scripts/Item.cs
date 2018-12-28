using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorageSlot {
    public ItemData data;
    public int itemCount;
    [HideInInspector] public int differenceToApply;
}

[System.Serializable]
public class ItemDelivery {
    public ItemData data;
    public int itemCount;
    public float timeRemaining = 60f;
}

[System.Serializable]
public class ItemContract {
    public bool completed;
    public List<StorageSlot> itemsRequested = new List<StorageSlot>();
    public string clientName;
    public int paymentAmount;
    public bool recurring;
    public float hoursTimeRemaining;
}

public class Item : MonoBehaviour {

    public ItemData data;
    public SpriteRenderer spr;
    public List<BaseEntity> inEntityZone = new List<BaseEntity>();

    public void Set(ItemData data) {
        if (data) this.data = data;
        spr.sprite = data.sprite;
    }

    private void OnMouseDown() {
        if (GameController.inst.AddToInventory(data, 1)) {
            GameController.inst.DespawnItem(this);
        }
    }

}

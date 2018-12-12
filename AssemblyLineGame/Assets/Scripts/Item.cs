using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorageSlot {
    public ItemData data;
    public int itemCount;
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

    public void Set(ItemData data) {
        if (data) this.data = data;
        spr.sprite = data.sprite;
    }

}

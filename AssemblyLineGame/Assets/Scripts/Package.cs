using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour {

    public StorageSlot storage;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    bool updating = true;

    private void Start() {
        StartCoroutine(UpdatePackage());
    }

    private void OnMouseUp() {
        GameController.inst.selectedPackage = this;
        GameController.inst.PackageInfoPopup.ToggleOpenClose(true);
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonUp(1)) {
            UnpackageIntoInventory();
        }
    }

    public void UnpackageIntoInventory() {
        if (GameController.inst.AddToInventory(storage.data, storage.itemCount)) {
            if (GameController.inst.selectedPackage == this) {
                if (GameController.inst.selectedPackage == this) GameController.inst.PackageInfoPopup.ToggleOpenClose(false);
                GameController.inst.selectedPackage = null;
            }
            Destroy(gameObject);
        }
    }

    public void UnpackageIntoStorage(BaseEntity entity) {
        if (entity.AddToStorage(storage.data, storage.itemCount)) {
            if (GameController.inst.selectedPackage == this) {
                GameController.inst.selectedPackage = null;
                if (GameController.inst.selectedPackage == this) GameController.inst.PackageInfoPopup.ToggleOpenClose(false);
            }
            Destroy(gameObject);
        }
    }

    public IEnumerator UpdatePackage() {
        while (!rb.IsSleeping()) {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * .01f);
            yield return null;
        }
        updating = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!updating) StartCoroutine(UpdatePackage());
    }

    private void OnDestroy() {
        GameController.inst.outboundPackages.Remove(this);  
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour {

    public StorageSlot storage;

    private void Start() {
        StartCoroutine(UpdatePackage());
    }

    private void OnMouseUp() {
        GameController.inst.selectedPackage = this;
        GameController.inst.PackageInfoPopup.ToggleOpenClose(true);
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonUp(1)) {
            Unpackage();
        }
    }

    public void Unpackage() {
        if (GameController.inst.AddToInventory(storage.data, storage.itemCount)) {
            if (GameController.inst.selectedPackage == this) {
                GameController.inst.selectedPackage = null;
                GameController.inst.PackageInfoPopup.ToggleOpenClose(false);
            }
            Destroy(gameObject);
        }

    }

    public IEnumerator UpdatePackage() {

        while (true) {

            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * .01f);

            yield return new WaitForSeconds(.5f);
        }
    }

    private void OnDestroy() {
        GameController.inst.outboundPackages.Remove(this);  
    }



}

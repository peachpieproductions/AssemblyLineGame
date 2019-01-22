using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEntity : MonoBehaviour {

    public BaseEntity entity;

    private void OnMouseOver() {
        if (Input.GetMouseButtonUp(0)) {
            entity.Clicked();
        }
        if (Input.GetMouseButtonUp(1)) {
            entity.ToggleActive();
        }
    }

}

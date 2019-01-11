using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEntity : MonoBehaviour {

    public BaseEntity entity;

    private void OnMouseUp() {
        entity.Clicked();
    }
}

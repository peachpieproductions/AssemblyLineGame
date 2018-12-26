using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIButton : MonoBehaviour {

    [HideInInspector] public OverlayMenu overlayMenu;
    public string function;
    public bool selectedOnClick;
    [HideInInspector] public bool selected;
    public EntityData entityData;
    public ItemData itemData;
    public ResearchData researchData;
    public StorageSlot storageSlot;

    [Header("Refs")]
    public Button button;
    public Image image;
    public TextMeshProUGUI text;
    public Image[] miscImages;
    public TextMeshProUGUI miscText1;
    public TextMeshProUGUI miscText2;
    public TextMeshProUGUI miscText3;

    private void Awake() {
        overlayMenu = GetComponentInParent<OverlayMenu>();
    }

    public void Clicked() {
        if (selectedOnClick) {
            selected = true;
            overlayMenu.ResetButtons();
            SetButtonColor(overlayMenu.buttonSelectedColor);
        }

        if (function == "SelectEntityToBuild") {
            GameController.inst.currentBuildEntity = entityData;
            if (GameController.inst.currentBuildObject) Destroy(GameController.inst.currentBuildObject.gameObject);
        } 

        else if (function == "SelectRecipe") {
            GameController.inst.selectedRecipe = itemData;
        } 

        else if (function == "OpenRecipeList") {
            GameController.inst.selectingRecipe = true;
            GameController.inst.recipeListMenu.ToggleOpenClose(true);
        } 

        else if (function == "InventorySlot") {
            if (Input.GetKey(KeyCode.LeftShift) && GameController.inst.entityMenu.open) {
                GameController.inst.selectedEntity.AddToStorage(storageSlot.data, storageSlot.itemCount, storageSlot);
            }
            GameController.inst.RefreshOverlays();
        } 

        else if (function == "StorageSlot") {
            if (Input.GetKey(KeyCode.LeftShift)) {
                GameController.inst.AddToInventory(storageSlot.data, storageSlot.itemCount, storageSlot);
            }
            GameController.inst.RefreshOverlays();
        }

        else if (function == "ResearchListing") {
            GameController.inst.StartResearch(researchData);
        }

    }

    public void SetButtonColor(Color color) {
        var cols = button.colors;
        cols.normalColor = color;
        button.colors = cols;
    }

}

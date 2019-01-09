using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIButton : MonoBehaviour, IPointerClickHandler {

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

    public void Clicked(bool rightClick = false) {
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
            Assembler ass = GameController.inst.selectedEntity as Assembler;
            if (ass) ass.assemblingItem = itemData;
            GameController.inst.entityMenu.BuildMenu();
            GameController.inst.recipeListMenu.ToggleOpenClose(false);
        } 

        else if (function == "OpenRecipeList") {
            if (rightClick) {
                if (itemData) GameController.inst.OpenItemDataInfo(itemData);
            } else {
                GameController.inst.recipeListMenu.ToggleOpenClose(true);
            }
        } 

        else if (function == "InventorySlot") {
            if (rightClick) {
                if (GameController.inst.entityMenu.open) {
                    if (Input.GetKey(KeyCode.LeftShift)) {
                        GameController.inst.selectedEntity.AddToStorage(storageSlot.data, storageSlot.itemCount / 2, storageSlot);
                    } else {
                        GameController.inst.selectedEntity.AddToStorage(storageSlot.data, storageSlot.itemCount, storageSlot);
                    }
                }
            } else {
                if (storageSlot.itemCount > 0) GameController.inst.OpenItemDataInfo(storageSlot.data);
            }
            GameController.inst.RefreshOverlays();
        } 

        else if (function == "StorageSlot") {
            if (rightClick) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    GameController.inst.AddToInventory(storageSlot.data, storageSlot.itemCount / 2, storageSlot);
                } else {
                    GameController.inst.AddToInventory(storageSlot.data, storageSlot.itemCount, storageSlot);
                }
            } else {
                if (storageSlot.itemCount > 0) GameController.inst.OpenItemDataInfo(storageSlot.data);
            }
            
            GameController.inst.RefreshOverlays();
        }

        else if (function == "ResearchListing") {
            GameController.inst.StartResearch(researchData);
        }

        else if (function == "OpenItemInfo") {
            GameController.inst.OpenItemDataInfo(itemData);
        }

    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            Clicked(true);
        }
    }

    public void SetButtonColor(Color color) {
        var cols = button.colors;
        cols.normalColor = color;
        button.colors = cols;
    }

    public void OpenItemInfo() {
        if (itemData) GameController.inst.OpenItemDataInfo(itemData);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class OverlayMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public string menuName;
    public UIButton templateButton;
    public List<UIButton> buttons = new List<UIButton>();
    public Color buttonSelectedColor = Color.white;
    public Vector2 openTargetPosition;
    public Vector2 closedTargetPosition;
    public float lerpSpeed = 1;
    public bool open;
    public Transform follow;
    public bool followMouse;
    public bool blockScrolling;
    public bool closesImmediately;
    public UIButton miscButton1;
    public UIButton miscButton2;
    public TextMeshProUGUI miscText1;
    public TextMeshProUGUI miscText2;
    public Image miscImage1;

    RectTransform rect;

    private void Awake() {
        rect = GetComponent<RectTransform>();
        if (!open) gameObject.SetActive(false);
    }

    private void OnEnable() {
        BuildMenu();
    }

    private void Update() {

        if (open) {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, openTargetPosition, Time.deltaTime * 5 * lerpSpeed);
        } else {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, closedTargetPosition, Time.deltaTime * 5 * lerpSpeed);
            if (Vector2.Distance(rect.anchoredPosition, closedTargetPosition) < 10 || closesImmediately) gameObject.SetActive(false);
        }

        if (follow) {
            rect.position = Camera.main.WorldToScreenPoint(follow.position);
        }

    }

    public void BuildMenu() {

        for (var i = buttons.Count - 1; i >= 0; i--) {
            var b = buttons[i];
            buttons.Remove(buttons[i]);
            Destroy(b.gameObject);
        }

        var temp = templateButton;
        if (temp) temp.gameObject.SetActive(true);

        if (menuName == "Inventory") {
            for (var i = 0; i < 8; i++) {
                var newButton = Instantiate(temp, temp.transform.parent);
                buttons.Add(newButton);
                newButton.storageSlot = GameController.inst.inventory[i];
                if (GameController.inst.inventory[i].itemCount > 0) {
                    newButton.text.gameObject.SetActive(true);
                    newButton.text.text = GameController.inst.inventory[i].itemCount.ToString();
                    newButton.image.gameObject.SetActive(true);
                    newButton.image.sprite = GameController.inst.inventory[i].data.sprite;
                } else {
                    newButton.text.gameObject.SetActive(false);
                    newButton.image.gameObject.SetActive(false);
                }
            }
        }

        else if (menuName == "EntityMenu") {
            var selEntity = GameController.inst.selectedEntity;
            if (selEntity) {
                miscButton1.gameObject.SetActive(selEntity is Assembler);
                miscButton2.transform.parent.gameObject.SetActive(false);
                miscText1.text = selEntity.data.name;
                if (selEntity is Assembler) {
                    var assem = selEntity as Assembler;
                    miscButton1.image.enabled = assem.assemblingItem;
                    if (assem.assemblingItem) {
                        miscButton1.image.sprite = assem.assemblingItem.sprite;
                        miscButton2.transform.parent.gameObject.SetActive(true);
                        foreach(UIButton b in miscButton2.transform.parent.GetComponentsInChildren<UIButton>()) { if (b != miscButton2) Destroy(b.gameObject); }
                        int i = 0; foreach(ItemData.CraftingIngredient ing in assem.assemblingItem.recipe) {
                            var button = miscButton2;
                            UIButton b; if (i == 0) b = button; else b = Instantiate(button, button.transform.parent);
                            b.itemData = ing.ingredient;
                            b.image.sprite = ing.ingredient.sprite;
                            b.text.text = ing.ingredientCount.ToString();
                            i++;
                        }
                    }
                }
                for (var i = 0; i < selEntity.storage.Count; i++) {
                    var newButton = Instantiate(temp, temp.transform.parent);
                    buttons.Add(newButton);
                    newButton.storageSlot = selEntity.storage[i];
                    if (selEntity.storage[i].itemCount > 0) {
                        newButton.text.gameObject.SetActive(true);
                        newButton.text.text = selEntity.storage[i].itemCount.ToString();
                        newButton.image.gameObject.SetActive(true);
                        newButton.image.sprite = selEntity.storage[i].data.sprite;
                    } else {
                        newButton.text.gameObject.SetActive(false);
                        newButton.image.gameObject.SetActive(false);
                    }
                }
            }
        }

        else if (menuName == "PackageInfoPopup") {
            var selPackage = GameController.inst.selectedPackage;
            if (selPackage) {
                follow = selPackage.transform;
                miscText1.text = selPackage.storage.data.name;
                miscText2.text = selPackage.storage.itemCount.ToString();
                miscImage1.sprite = selPackage.storage.data.sprite;
            }
        }

        else if (menuName == "ItemInfoPopup") {
            var selItemData = GameController.inst.selectedItemData;
            if (selItemData) {
                miscText1.text = selItemData.name;
                //miscText2.text = ITEM DATA INFO
                miscImage1.sprite = selItemData.sprite;

                //crafting recipe
                miscButton1.gameObject.SetActive(false);
                foreach (UIButton b in miscButton1.transform.parent.GetComponentsInChildren<UIButton>()) { if (b != miscButton1) Destroy(b.gameObject); }
                if (selItemData.recipe.Length > 0) {
                    miscButton1.gameObject.SetActive(true);
                    int i = 0; foreach (ItemData.CraftingIngredient ing in selItemData.recipe) {
                        var button = miscButton1;
                        UIButton b; if (i == 0) b = button; else b = Instantiate(button, button.transform.parent);
                        b.itemData = ing.ingredient;
                        b.image.sprite = ing.ingredient.sprite;
                        b.text.text = ing.ingredientCount.ToString();
                        i++;
                    }
                }

                //used to craft list
                miscButton2.gameObject.SetActive(false);
                foreach (UIButton b in miscButton2.transform.parent.GetComponentsInChildren<UIButton>()) { if (b != miscButton2) Destroy(b.gameObject); }
                if (selItemData.usedToCraft.Count > 0) {
                    miscButton2.gameObject.SetActive(true);
                    int i = 0; foreach (ItemData d in selItemData.usedToCraft) {
                        var button = miscButton2;
                        UIButton b; if (i == 0) b = button; else b = Instantiate(button, button.transform.parent);
                        b.itemData = d;
                        b.image.sprite = d.sprite;
                        b.text.text = "";
                        i++;
                    }
                }
            }
        }

        else if (menuName == "RecipeListMenu") {
            foreach (ItemData item in GameController.inst.recipeList) {
                var newButton = Instantiate(temp, temp.transform.parent);
                buttons.Add(newButton);
                newButton.text.text = item.name;
                newButton.image.sprite = item.sprite;
                newButton.itemData = item;
            }
        }

        else if (menuName == "BuildModeMenu") {
            foreach (EntityData ent in GameController.inst.entityDatas) {
                var newButton = Instantiate(temp, temp.transform.parent);
                buttons.Add(newButton);
                newButton.text.text = ent.name;
                newButton.image.sprite = ent.sprite;
                newButton.entityData = ent;
            }
        }

        else if (menuName == "Marketplace") {
            foreach (ItemData item in GameController.inst.itemDatas) {
                if (item.recipe.Length == 0) {
                    var newButton = Instantiate(temp, temp.transform.parent);
                    buttons.Add(newButton);
                    newButton.text.text = item.name;
                    newButton.image.sprite = item.sprite;
                    newButton.itemData = item;
                }
            }
            GameController.inst.UpdateMarketplace();
        }

        else if (menuName == "Contracts") {

            foreach (ItemContract cont in GameController.inst.contractList) {
                var newButton = Instantiate(temp, temp.transform.parent);
                buttons.Add(newButton);
                newButton.text.text = cont.clientName;
                newButton.miscText2.text = "$" + cont.paymentAmount.ToString();
                newButton.miscText3.text = Mathf.CeilToInt(cont.hoursTimeRemaining).ToString() + " hrs";
                newButton.image.sprite = cont.itemsRequested[0].data.sprite;
                newButton.image.GetComponentInChildren<TextMeshProUGUI>().text = cont.itemsRequested[0].itemCount.ToString();
                if (cont.itemsRequested.Count > 1) {
                    newButton.miscImages[0].sprite = cont.itemsRequested[1].data.sprite;
                    newButton.miscImages[0].gameObject.SetActive(true);
                }
                else newButton.miscImages[0].gameObject.SetActive(false);
                if (cont.itemsRequested.Count > 2) {
                    newButton.miscImages[1].sprite = cont.itemsRequested[2].data.sprite;
                    newButton.miscImages[1].gameObject.SetActive(true);
                } 
                else newButton.miscImages[1].gameObject.SetActive(false);
                newButton.miscImages[2].transform.GetChild(0).gameObject.SetActive(cont.completed);
                if (cont.completed) newButton.SetButtonColor(new Color(.8f, 1, .8f));
                else newButton.SetButtonColor(Color.white);
            }
            
        }

        else if (menuName == "Research") {

            foreach (ResearchData res in GameController.inst.researchDatas) {
                var newButton = Instantiate(temp, temp.transform.parent);
                buttons.Add(newButton);
                newButton.text.text = res.name;
                newButton.researchData = res;

                if (res.beingResearched) {
                    newButton.miscImages[4].transform.localScale = new Vector3(GameController.inst.researchProgress / res.cost, 1, 1);
                    newButton.miscText2.text = Mathf.Floor((GameController.inst.researchProgress / res.cost) * 100) + "%";
                    newButton.miscText2.color = new Color(0, 0, .75f);
                } else if (!res.researched) newButton.miscText2.text = "$" + res.cost.ToString();
                else newButton.miscImages[4].transform.localScale = new Vector3(1, 1, 1);
                if (res.researched) {
                    newButton.image.gameObject.SetActive(true);
                    newButton.miscText2.enabled = false;
                }


                if (res.items.Length > 0) newButton.miscImages[0].sprite = res.items[0].sprite;
                else newButton.miscImages[0].enabled = false;
                if (res.items.Length > 1) newButton.miscImages[1].sprite = res.items[1].sprite;
                else newButton.miscImages[1].enabled = false;
                if (res.items.Length > 2) newButton.miscImages[2].sprite = res.items[2].sprite;
                else newButton.miscImages[2].enabled = false;
                if (res.items.Length > 3) newButton.miscImages[3].sprite = res.items[3].sprite;
                else newButton.miscImages[3].enabled = false;
            }
            
        }

        if (temp) temp.gameObject.SetActive(false);

    }

    public void CloseMenu() {

        GameController.inst.hoveringList.Remove(this);
        if (GameController.inst.hoveringList.Count == 0) GameController.inst.hoveringOverlay = false;

        if (menuName == "EntityMenu") {
            if (GameController.inst.recipeListMenu.gameObject.activeSelf) GameController.inst.recipeListMenu.ToggleOpenClose(false);
        }

        else if (menuName == "RecipeListMenu") {
            GameController.inst.selectingRecipe = false;
        }
    }

    public void ToggleOpenClose(bool open) {
        this.open = open;
        if (open) {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            else BuildMenu();
            rect.anchoredPosition = closedTargetPosition;
        } else {
            if (follow) gameObject.SetActive(false);
            CloseMenu();
        }
    }

    public void ResetButtons() {
        foreach(UIButton b in buttons) {
            b.SetButtonColor(templateButton.button.colors.normalColor);
        }
    }

    public void OnPointerEnter(PointerEventData ped) {
        GameController.inst.hoveringOverlay = true;
        GameController.inst.hoveringList.Add(this);
    }
    public void OnPointerExit(PointerEventData ped) {
        GameController.inst.hoveringList.Remove(this);
        if (GameController.inst.hoveringList.Count == 0) GameController.inst.hoveringOverlay = false;
    }

    public void SetAsLastSibling() {
        transform.SetAsLastSibling();
    }


}

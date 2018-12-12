using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler : BaseEntity {

    public ItemData assemblingItem;


    public override void Start() {
        base.Start();
        StartCoroutine(Work());
    }

    public IEnumerator Work() {

        while (true) {
            bool craftingSuccessful = false;
            foreach (EntityNeighbor n in neighbors) {
                if (assemblingItem && n.entity) {
                    bool canCraft = true;

                    if (assemblingItem.recipe.Length == 0) canCraft = false;
                    foreach(ItemData.CraftingIngredient ing in assemblingItem.recipe) {
                        for (var i = 0; i < storage.Count; i++) {
                            if (storage[i].data != ing.ingredient || storage[i].itemCount < ing.ingredientCount) {
                                if (i == storage.Count - 1) {
                                    canCraft = false;
                                    break;
                                }
                            } else {
                                break;
                            }
                        }
                    }

                    if (canCraft) {
                        craftingSuccessful = true;
                        storage[0].itemCount -= assemblingItem.recipe[0].ingredientCount;
                        if (storage[0].itemCount == 0) storage[0].data = null;
                        for (var i = 0; i < assemblingItem.craftingOutputCount; i++) {
                            Dispense(assemblingItem);
                            yield return new WaitForSeconds(.1f);
                        }
                        if (GameController.inst.entityMenu.open) GameController.inst.entityMenu.BuildMenu();
                    }
                }
            }
            if (craftingSuccessful) yield return new WaitForSeconds(1f);
            else yield return new WaitForSeconds(.2f); //Switch to "Wait Until" condition 
        }

    }

}

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

            if (active) {
                foreach (EntityNeighbor n in neighbors) {
                    if (assemblingItem && n.entity && n.entity.active) {
                        bool canCraft = true;

                        if (assemblingItem.recipe.Length == 0) canCraft = false;
                        foreach (ItemData.CraftingIngredient ing in assemblingItem.recipe) {
                            for (var i = 0; i < storage.Count; i++) {
                                if (storage[i].data != ing.ingredient || storage[i].itemCount < ing.ingredientCount) {
                                    if (i == storage.Count - 1) {
                                        canCraft = false;
                                        break;
                                    }
                                } else {
                                    storage[i].differenceToApply = -ing.ingredientCount;
                                    break;
                                }
                            }
                        }

                        if (canCraft) {
                            craftingSuccessful = true;
                            if (assemblingItem) {
                                foreach (StorageSlot s in storage) {
                                    s.itemCount += s.differenceToApply;
                                    s.differenceToApply = 0;
                                    if (s.itemCount == 0) s.data = null;
                                }

                                for (var i = 0; i < assemblingItem.craftingOutputCount; i++) {
                                    Dispense(assemblingItem);
                                    yield return new WaitForSeconds(.1f);
                                }
                                GetNextNeighbor();
                            }
                            if (GameController.inst.entityMenu.open) GameController.inst.entityMenu.BuildMenu();
                        } else {
                            foreach (StorageSlot s in storage) {
                                s.differenceToApply = 0;
                            }
                        }
                        if (craftingSuccessful) yield return new WaitForSeconds(1f);
                    }
                }
            }

            if (!craftingSuccessful) yield return new WaitForSeconds(.2f);

        }

    }

}

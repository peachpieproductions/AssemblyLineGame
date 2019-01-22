using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ResearchData : ScriptableObject {

    public bool beingResearched;
    public bool researched;
    public bool researchedOnStart;
    public int cost;
    public ItemData[] items;
    public EntityData[] entities;



    public void GenerateCost() {
        cost = 0;
        if (!researchedOnStart) {
            for (int i = 0; i < items.Length; i++) {
                if (i < 4 || items[i].isProduct) cost += items[i].basePrice * 10;
            }
        }
    }

    public void UnlockResearch() {
        foreach (ItemData i in items) {
            i.isUnlocked = true;
            if (i.recipe.Length > 0) GameController.inst.recipeList.Add(i);
        }
        researched = true;
        beingResearched = false;
        GameController.inst.researchesCompleted++;
    }
	
}

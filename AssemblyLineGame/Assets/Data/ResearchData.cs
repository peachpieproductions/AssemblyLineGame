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
        foreach(ItemData item in items) {
            cost += item.basePrice * 10;
        }
    }

    public void UnlockResearch() {
        foreach (ItemData i in items) {
            i.isUnlocked = true;
            GameController.inst.recipeList.Add(i);
        }
        researched = true;
        beingResearched = false;
    }
	
}

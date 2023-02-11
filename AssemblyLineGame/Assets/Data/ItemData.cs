using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject {

    [System.Serializable]
    public class CraftingIngredient {
        public ItemData ingredient;
        public int ingredientCount = 1;
    }

    public Sprite sprite;
    public int basePrice;
    public float priceVariant = 1;
    public float popularity = .5f;
    public bool isProduct;
    public int stepsToMake;
    public CraftingIngredient[] recipe;
    public int craftingOutputCount = 1;
    public ResearchData researchRequired;
    public bool isUnlocked = true;
    public List<ItemData> usedToCraft = new List<ItemData>();       

    public int getCurrentPrice() {
        return Mathf.Max(Mathf.RoundToInt(basePrice * priceVariant),1);
    }

    public void SetBasePrice() {
        if (recipe.Length > 0) {
            stepsToMake = 1;
            foreach(CraftingIngredient ing in recipe) {
                if (ing.ingredient.recipe.Length > 0) {
                    stepsToMake++;
                    foreach (var ingrIng in ing.ingredient.recipe) {
                        if (ingrIng.ingredient.recipe.Length > 0) stepsToMake++;
                    }
                }
                if (ing.ingredient.basePrice == 0) {
                    ing.ingredient.SetBasePrice();
                }
            }  
            foreach (CraftingIngredient ing in recipe) {
                basePrice += Mathf.Max(1,Mathf.RoundToInt((ing.ingredient.basePrice * ing.ingredientCount) / craftingOutputCount));
                basePrice = Mathf.RoundToInt(basePrice * 1.1f + (.1f * stepsToMake)); //steps to make multiplier
            }
            
        }
    }

}

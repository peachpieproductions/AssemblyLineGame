using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor {



    public override void OnInspectorGUI() {

        GameController gCon = (GameController)target;

        GUI.color = new Color(.8f, .8f, 1);
        if (GUILayout.Button("Get Data")) {
            gCon.entityDatas.Clear();
            gCon.itemDatas.Clear();
            gCon.researchDatas.Clear();

            //gCon.clientNames.Clear();
            //gCon.LoadClientNames();

            //Load Entity Datas
            var found = AssetDatabase.FindAssets("t:EntityData");
            foreach (string s in found) gCon.entityDatas.Add((EntityData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(EntityData)));

            //Load Item Datas
            found = AssetDatabase.FindAssets("t:ItemData");
            foreach (string s in found) gCon.itemDatas.Add((ItemData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(ItemData)));

            //Load Research Datas
            found = AssetDatabase.FindAssets("t:ResearchData");
            foreach (string s in found) gCon.researchDatas.Add((ResearchData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(ResearchData)));
        }

        GUI.color = new Color(.8f, 1, .8f);
        if (GUILayout.Button("Price Items")) {
            foreach(ItemData itemData in gCon.itemDatas) { //reset prices for crafted items
                if (itemData.recipe.Length > 0) {
                    itemData.basePrice = 0;
                }
            }
            foreach (ItemData itemData in gCon.itemDatas) {
                if (itemData.recipe.Length > 0 && itemData.basePrice == 0) {
                    itemData.SetBasePrice();
                }
            }
        }

        GUI.color = Color.white;
        DrawDefaultInspector();

        



    }

}

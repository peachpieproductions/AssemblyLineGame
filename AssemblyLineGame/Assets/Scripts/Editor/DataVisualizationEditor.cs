using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;


[CustomEditor(typeof(DataVisualization))]
public class DataVisualizationEditor : Editor {


    public override void OnInspectorGUI() {

        DataVisualization v = (DataVisualization)target;

        if (GUILayout.Button("Get Data")) {
            v.entityDatas.Clear();
            v.itemDatas.Clear();
            v.researchDatas.Clear();

            //Load Entity Datas
            var found = AssetDatabase.FindAssets("t:EntityData");
            foreach (string s in found) v.entityDatas.Add((EntityData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(EntityData)));

            //Load Item Datas
            found = AssetDatabase.FindAssets("t:ItemData");
            foreach (string s in found) v.itemDatas.Add((ItemData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(ItemData)));

            //Load Research Datas
            found = AssetDatabase.FindAssets("t:ResearchData");
            foreach (string s in found) v.researchDatas.Add((ResearchData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(ResearchData)));

            //Sort Research Listings
            v.researchDatas.Sort((d1, d2) => d1.cost.CompareTo(d2.cost));

        }

        DrawDefaultInspector();

        if (GUILayout.Button("Clear Nodes")) {

            v.ClearNodes();

        }

        if (GUILayout.Button("Show Item Data")) {

            v.ClearNodes();

            int yOffset = 0;
            for (var i = 0; i < v.itemDatas.Count; i++) {
                if (v.itemDatas[i].isProduct) {
                    var newNode = Instantiate(v.dataNodePrefab);
                    newNode.SetActive(true);
                    v.nodes.Add(newNode);
                    newNode.transform.position = new Vector3(0, -14 * yOffset);
                    newNode.GetComponentInChildren<TextMeshPro>().text = v.itemDatas[i].name + " ($" + v.itemDatas[i].basePrice + ")";
                    newNode.transform.Find("New Sprite").GetComponent<SpriteRenderer>().enabled = true;
                    newNode.transform.Find("New Sprite").GetComponent<SpriteRenderer>().sprite = v.itemDatas[i].sprite;

                    for (var j = 0; j < 4; j++) {
                        if (v.itemDatas[i].recipe.Length >= j + 1) {
                            newNode = Instantiate(v.dataNodePrefab);
                            newNode.SetActive(true);
                            v.nodes.Add(newNode);
                            newNode.transform.position = new Vector3(10, -14 * yOffset - (j - 1.5f) * 2);
                            newNode.GetComponentInChildren<TextMeshPro>().text = v.itemDatas[i].recipe[j].ingredient.name + " (" + v.itemDatas[i].recipe[j].ingredientCount + ") " + "$" + v.itemDatas[i].recipe[j].ingredient.basePrice;
                            newNode.transform.Find("New Sprite").GetComponent<SpriteRenderer>().enabled = true;
                            newNode.transform.Find("New Sprite").GetComponent<SpriteRenderer>().sprite = v.itemDatas[i].recipe[j].ingredient.sprite;
                        }
                    }
                    yOffset++;
                }
            }

        }

        if (GUILayout.Button("Show Research Data")) {

            v.ClearNodes();

            for(var i = 0; i < v.researchDatas.Count; i++) {
                var newNode = Instantiate(v.dataNodePrefab);
                newNode.SetActive(true);
                v.nodes.Add(newNode);
                newNode.transform.position = new Vector3(0, -14 * i);
                newNode.GetComponentInChildren<TextMeshPro>().text = v.researchDatas[i].name + " (" + v.researchDatas[i].cost + ")";
                
                for (var j = 0; j < 6; j++) {
                    if (v.researchDatas[i].items.Length >= j + 1) {
                        newNode = Instantiate(v.dataNodePrefab);
                        newNode.SetActive(true);
                        v.nodes.Add(newNode);
                        newNode.transform.position = new Vector3(10, -14 * i - (j-1.5f) * 2);
                        newNode.GetComponentInChildren<TextMeshPro>().text = v.researchDatas[i].items[j].name + " (" + v.researchDatas[i].items[j].basePrice + ")";
                        newNode.transform.Find("New Sprite").GetComponent<SpriteRenderer>().enabled = true;
                        newNode.transform.Find("New Sprite").GetComponent<SpriteRenderer>().sprite = v.researchDatas[i].items[j].sprite;
                    }
                }
            }

        }

    }

    



}

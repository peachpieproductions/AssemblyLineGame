using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataVisualization : MonoBehaviour {

    public GameObject dataNodePrefab;
    public List<GameObject> nodes = new List<GameObject>();
    public List<EntityData> entityDatas = new List<EntityData>();
    public List<ItemData> itemDatas = new List<ItemData>();
    public List<ResearchData> researchDatas = new List<ResearchData>();


    public void ClearNodes() {
        for (var i = nodes.Count - 1; i >= 0; i--) {
            DestroyImmediate(nodes[i]);
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ResearchData : ScriptableObject {

    public bool researched;
    public int cost;
    public ItemData[] items;
    public EntityData[] entities;
	
}

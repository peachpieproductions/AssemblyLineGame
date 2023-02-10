using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EntityData : ScriptableObject {

    public GameObject prefab;
    public Sprite sprite;
    public Sprite[] altSprites;
    public Sprite placementSprite;
    public int cost;
    public Vector2Int size = new Vector2Int(1, 1);
    public List<Vector2Int> emptyTiles = new List<Vector2Int>();
    public bool cantBeRotated;
    public float zPosValue;

    [TextArea(2,10)]
    public string info;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUnit : MonoBehaviour
{
    public Vector2Int LocalPosition { get; private set; }
    public bool IsAlternative { get; private set; }
    public Tile ContainedTile { get; set; }

    public bool IsEmpty => ContainedTile == null;

    
    public void Initialize(Vector2Int localPosition, bool isAlternative)
    {
        LocalPosition = localPosition;
        IsAlternative = isAlternative;
    }

    public void RemoveTile()
    {
        ContainedTile = null;
    }
}

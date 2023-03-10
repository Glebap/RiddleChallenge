using UnityEngine;


[CreateAssetMenu(fileName = "TileData", menuName = "Tile Data", order = 0)]
public class TileData : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string[] _keyWords;

    public string[] KeyWords => _keyWords;
    public Sprite Icon => _icon;
}

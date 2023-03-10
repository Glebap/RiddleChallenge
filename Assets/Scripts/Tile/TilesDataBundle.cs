using UnityEngine;


[CreateAssetMenu(fileName = "TilesDataBundle", menuName = "Tiles Data Bundle", order = 0)]
public class TilesDataBundle : ScriptableObject
{
	[SerializeField] private TileData[] _tilesData;

	public TileData[] TilesData => _tilesData;
}

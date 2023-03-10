using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] private TilesMerger _tilesMerger;
    [SerializeField] private Transform _tilesTransform;
    [SerializeField] private TilesDataBundle _tilesDataBundle;
    [SerializeField] private Tile _tilePrefab;


    public Tile SpawnTile()
    {
        var tile = Instantiate(_tilePrefab, _tilesTransform);
        var tileData = GetRandomTileData();
        
        tile.Initialize(tileData);
        tile.TilesCollided += _tilesMerger.OnTilesCollided;
        return tile;
    }

    private TileData GetRandomTileData()
    {
        var tilesData = _tilesDataBundle.TilesData;
        var tilesDataLength = tilesData.Length;

        if (tilesDataLength == 0)
            throw new IndexOutOfRangeException("Tiles data lenght is 0");

        var tileData = tilesData[Random.Range(0, tilesDataLength)];

        return tileData;
    }
}

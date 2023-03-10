using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    [SerializeField] private BoardView _boardView;
    [SerializeField] private TileSpawner _tileSpawner;
    [SerializeField] private TilesHandler _tilesHandler;
    [SerializeField] private Vector2Int _boardSize;
    [SerializeField] private bool _heapGeneration;

    private BoardUnit[,] _boardUnits;
    private List<BoardUnit> _availableBoardUnits;
    private List<BoardUnit> _usedBoardUnits;
    private List<BoardUnit> _suitableBoardUnits;
    private bool _startTilesSpawn = true;

    public int Height => _boardSize.x;
    public int Width => _boardSize.y;
    
    public BoardUnit[,] BoardUnits => _boardUnits;
    public List<BoardUnit> UsedBoardUnits => _usedBoardUnits;
    
    private void Start()
    {
        _usedBoardUnits = new List<BoardUnit>();
        _suitableBoardUnits = new List<BoardUnit>();
        _boardUnits = _boardView.Initialize(_boardSize);
        _availableBoardUnits = _boardUnits.Cast<BoardUnit>().ToList();
        
        GenerateTiles(10);
        _startTilesSpawn = false;
    }

    private void OnEnable()
    {
        _tilesHandler.TilesDestroyed += OnTilesDestroyed;
    }
    
    private void OnDisable()
    {
        _tilesHandler.TilesDestroyed -= OnTilesDestroyed;
    }

    public void OnDealButtonClicked()
    {
        GenerateTiles(5);
    }

    private void GenerateTiles(int quantity)
    {
        if (_availableBoardUnits.Count == 0) return;
        
        if (_heapGeneration)
            GenerateTilesInHeap(quantity);
        else
            GenerateTilesRandomly(quantity);
    }

    private void GenerateTilesInHeap(int quantity)
    {
        if (_usedBoardUnits.Count == 0)
        {
            var boardUnit = _boardUnits[(int)(Width * 0.5f), (int)(Height * 0.5f)];
                
            AddTile(boardUnit);
            GetSuitableBoardUnits();
            quantity--;
        }
        
        while (quantity-- > 0)
        {
            var suitableBoardUnitsCount = _suitableBoardUnits.Count;
            if(suitableBoardUnitsCount == 0) return;

            var boardUnit = _suitableBoardUnits[Random.Range(0, suitableBoardUnitsCount)];
            AddTile(boardUnit);
            RemoveFromSuitableBoardUnits(boardUnit);
        }
    }
    
    private void TryAddSuitableBoardUnits(BoardUnit boardUnit)
    {
        foreach (var nearBoardUnit in GetNeighbors(boardUnit))
        {
            if (!nearBoardUnit.IsEmpty || 
                _suitableBoardUnits.Contains(nearBoardUnit)) continue;
				
            _suitableBoardUnits.Add(nearBoardUnit);
        }
    }

    private void RemoveFromSuitableBoardUnits(BoardUnit boardUnit)
    {
        _suitableBoardUnits.Remove(boardUnit);
        TryAddSuitableBoardUnits(boardUnit);
    }

    private void GetSuitableBoardUnits()
    {
        foreach (var boardUnit in _usedBoardUnits)
            TryAddSuitableBoardUnits(boardUnit);

        if (_usedBoardUnits.Count == 0) 
            OnDealButtonClicked();
    }

    private void GenerateTilesRandomly(int quantity)
    {
        var availableBoardUnitsCount = _availableBoardUnits.Count;

        while (quantity-- > 0)
        {
            if(availableBoardUnitsCount == 0) return;
                
            var index = Random.Range(0, availableBoardUnitsCount);
            var boardUnit = _availableBoardUnits[index];
                
            AddTile(boardUnit);
            availableBoardUnitsCount--;
        }
    }
    
    private void AddTile(BoardUnit boardUnit)
    {
        var tile = _tileSpawner.SpawnTile();
        var position = new Vector3(boardUnit.LocalPosition.x, 0.0f, boardUnit.LocalPosition.y);
        var tileTransform = tile.transform;
        
        tileTransform.localPosition = position;
        if (!_startTilesSpawn)
        {
            var initialScale = tileTransform.localScale;
            tileTransform.localScale = Vector3.zero;
            tileTransform.DOScale(initialScale, 0.42f).SetEase(Ease.InOutSine);
        }
        boardUnit.ContainedTile = tile;
        _availableBoardUnits.Remove(boardUnit);
        _usedBoardUnits.Add(boardUnit);
    }
    
    private List<BoardUnit> GetNeighbors(BoardUnit boardUnit)
    {
        var neighbors = new List<BoardUnit>();
        var position = boardUnit.LocalPosition;
        
        if (position.x > 0)
            neighbors.Add(_boardUnits[position.x - 1, position.y]);
        if (position.y > 0)
            neighbors.Add(_boardUnits[position.x, position.y - 1]);
        if (position.x < Width - 1)
            neighbors.Add(_boardUnits[position.x + 1, position.y]);
        if (position.y < Height - 1)
            neighbors.Add(_boardUnits[position.x, position.y + 1]);
        
        return neighbors;
    }

    private void OnTilesDestroyed(BoardUnit first, BoardUnit second)
    {
        _usedBoardUnits.Remove(first);
        _usedBoardUnits.Remove(second);
        _availableBoardUnits.Add(first);
        _availableBoardUnits.Add(second);
        GetSuitableBoardUnits();
    }
}

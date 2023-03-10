using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class TilesMerger : MonoBehaviour
{
	private const float SelectDuration = 0.16f;
	
	private int _boardWidth, _boardHeight;
	private Board _board;
	private readonly Vector2Int[] _neighborsOffsets = {
		new (0, 1), new (0, -1),
		new (1, 0), new (-1, 0)
	};
	private Dictionary<Vector2Int, Vector2Int> cameFrom; 
	private Queue<Vector2Int> frontier;
	private List<Sequence> _moveSequences;

	public event Action<Vector3> TilesMerged;
	public event Action MergeCompleted;
	public event Action MergeFailed;

	private void Start()
	{
		_board = GetComponent<Board>();
		_boardWidth = _board.Width;
		_boardHeight = _board.Height;
	}
	
	public void TryMergeTiles(BoardUnit first, BoardUnit second)
	{
		if (!BoardUnitsTilesIsRelated(first, second) ||
		    !TryGetPath(first, second, out var path))
		{
			MergeFailed?.Invoke();
			return;
		}
		
		_moveSequences = new List<Sequence>();
	    
		MoveByPath(second, path);
		ReversePath();
		MoveByPath(first, path);
		
		void ReversePath()
		{
			path.Reverse();
			path.RemoveAt(0);
			path.Add(second.LocalPosition);
		}
	}
	
    private bool BoardUnitsTilesIsRelated(BoardUnit first, BoardUnit second)
    {
	    var firstTileKeyWords = first.ContainedTile.KeyWords;
	    var secondTileKeyWords = second.ContainedTile.KeyWords;

	    foreach (var key in firstTileKeyWords)
	    {
		    if (secondTileKeyWords.Contains(key)) return true;
	    }

	    return false;
    }

    private bool TryGetPath(BoardUnit first, BoardUnit second, out List<Vector2Int> path)
    {
	    path = new List<Vector2Int>();
	    var startPosition = first.LocalPosition;
	    var endPosition = second.LocalPosition;
	    
	    if (!HavePath(startPosition, endPosition)) return false;
	    
	    var currentPoint = endPosition;

	    while (currentPoint != startPosition)
	    {
		    currentPoint = cameFrom[currentPoint];
		    path.Add(currentPoint);
	    }

	    return true;
    }
    
    private bool HavePath(Vector2Int startPosition, Vector2Int endPosition)
    {
	    var boardUnits = _board.BoardUnits;
	    var pathFounded = false;
	    cameFrom = new Dictionary<Vector2Int, Vector2Int>();
	    frontier = new Queue<Vector2Int>();
	    frontier.Enqueue(startPosition);
	    
	    while (frontier.Count > 0)
	    {
		    var current = frontier.Dequeue();
		    if (current == endPosition)
		    {
			    pathFounded = true;
			    break;
		    }
		    
		    var isAlternative = boardUnits[current.x, current.y].IsAlternative;

		    var ints = GetNeighbors(current, isAlternative);
		    foreach (var next in ints)
		    {
			    if (cameFrom.ContainsKey(next)) continue;
			    if (next != endPosition && !boardUnits[next.x, next.y].IsEmpty) continue;

			    frontier.Enqueue(next);
			    cameFrom[next] = current;
		    }
	    }

	    return pathFounded;
    }

    private void MoveByPath(BoardUnit fromBoardUnit, List<Vector2Int> path)
    {
	    var moveSequence = DOTween.Sequence();
	    _moveSequences.Add(moveSequence);
	    var tileTransform = fromBoardUnit.ContainedTile.transform;

	    foreach (var pointPosition in path)
	    {
		    var destinationPoint = new Vector3(pointPosition.x, 0.0f, pointPosition.y);
		    moveSequence.Append(tileTransform.DOMove(destinationPoint, 0.32f).SetEase(Ease.Linear));
	    }

	    moveSequence.PrependInterval(SelectDuration).Play();
    }

    private List<Vector2Int> GetNeighbors(Vector2Int point, bool isAlternative)
    {
	    var neighbors = new List<Vector2Int>();
	    var neighborsOffsets = _neighborsOffsets.ToList();

	    if (!isAlternative) neighborsOffsets.Reverse();

	    foreach (var offset in neighborsOffsets)
	    {
		    var position = new Vector2Int(point.x + offset.x, point.y + offset.y);
		    if (position.x < 0 || position.x > _boardWidth - 1 || 
		        position.y < 0 || position.y > _boardHeight - 1) continue;
		    
		    neighbors.Add(new Vector2Int(position.x, position.y));
	    }
	    
	    return neighbors;
    }

    public void OnTilesCollided(Transform firstTile, Transform secondTile)
    {
	    foreach (var sequence in _moveSequences) sequence.Kill();
	    
	    var mergeSequence = DOTween.Sequence();
	    var goalPosition = (firstTile.position + secondTile.position) * 0.5f;

	    mergeSequence.Join(secondTile.DOMove(goalPosition, 0.2f))
					 .Join(secondTile.DOScale(Vector3.zero, 0.2f))
					 .Join(firstTile.DOMove(goalPosition, 0.2f))
					 .Join(firstTile.DOScale(Vector3.zero, 0.2f));

	    mergeSequence.Play().OnComplete(() =>
	    {
		    TilesMerged?.Invoke(goalPosition);
		    MergeCompleted?.Invoke();
	    });
    }

    public bool TryGetMergeableTiles(out Tuple<Tile, Tile> tiles)
    {
	    var checkedPairs = new Dictionary<BoardUnit, BoardUnit>();
	    
	    foreach (var firstBoardUnit in _board.UsedBoardUnits)
	    {
		    foreach (var secondBoardUnit in _board.UsedBoardUnits)
		    {
			    if (firstBoardUnit == secondBoardUnit) continue;
			    if (checkedPairs.ContainsKey(secondBoardUnit) && 
			        checkedPairs[secondBoardUnit] == firstBoardUnit) continue;
			    if (!BoardUnitsTilesIsRelated(firstBoardUnit, secondBoardUnit)) continue;
			    checkedPairs[firstBoardUnit] = secondBoardUnit;
			    if (!HavePath(firstBoardUnit.LocalPosition, secondBoardUnit.LocalPosition)) continue;

			    tiles = new Tuple<Tile, Tile>(firstBoardUnit.ContainedTile, secondBoardUnit.ContainedTile);
			    
			    return true;
		    }
	    }
	    
	    tiles = default;
	    return false;
    }
}

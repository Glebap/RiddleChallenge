using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


public class TilesHandler : MonoBehaviour
{
	private const float SelectDuration = 0.16f;
	
	[SerializeField] private Camera _camera;
	[SerializeField] private TilesMerger _tilesMerger;
	[SerializeField] private GameObject _touchBlock;

	private BoardUnit _firstBoardUnit, _secondBoardUnit;
	private Tile _firstTile => _firstBoardUnit == null ? null : _firstBoardUnit.ContainedTile;
	private Tile _secondTile => _secondBoardUnit == null ? null : _secondBoardUnit.ContainedTile;
	private bool _canTouch = true;
	
	private bool _hintMode;
	private Sequence _highlightSequence;
	private Tuple<Tile, Tile> _highlightedTiles;

	public event Action<BoardUnit, BoardUnit> TilesDestroyed;

	private void Update()
	{
		if (!_canTouch || Input.touchCount <= 0) return;
		
		var touch = Input.GetTouch(0);
		if (touch.phase == TouchPhase.Began) 
			HandleTouch(_camera.ScreenPointToRay(touch.position));
	}

    private void OnEnable()
    {
	    _tilesMerger.MergeCompleted += OnMergeCompleted;
	    _tilesMerger.MergeFailed += OnMergeFailed;
    }

    private ref BoardUnit UninitializedBoardUnit()
	{
		if (_firstBoardUnit == null)
			return ref _firstBoardUnit;
		return ref _secondBoardUnit;
	}
    
    private void SelectBoardUnit(BoardUnit boardUnit)
    {
	    if (_firstBoardUnit == boardUnit)
	    {
		    DeselectTile(boardUnit.ContainedTile);
		    _firstBoardUnit = null;
		    return;
	    }
	    
	    ref var uninitializedBoardUnitRef = ref UninitializedBoardUnit();
	    uninitializedBoardUnitRef = boardUnit;
	    
	    SelectTile(boardUnit.ContainedTile);
    }
    
    private void HandleTouch(Ray touchRay)
    {
	    if (!Physics.Raycast(touchRay, out var hit)) return;
	    var boardUnit = hit.transform.GetComponent<BoardUnit>();
	    if (boardUnit == null || boardUnit.IsEmpty) return;

	    if (_hintMode) ExitHintMode();
	    SelectBoardUnit(boardUnit);
	    if (_secondBoardUnit == null) return;

	    DisableTouch();
	    _tilesMerger.TryMergeTiles(_firstBoardUnit, _secondBoardUnit);
    }

    private void SelectTile(Tile tile)
    {
	    var tileTransform = tile.transform;
	    tileTransform.DOKill();
	    var scale = tileTransform.localScale * 0.86f;
	    
	    tileTransform.DOScale(scale, SelectDuration).SetEase(Ease.InOutSine);
    }
    
    private void DeselectTile(Tile tile)
    {
	    tile.transform.DOScale(Vector3.one, SelectDuration).SetEase(Ease.InOutSine);
    }

    private void ResetTiles()
    {
	    if (_firstTile != null) DeselectTile(_firstTile);
	    if (_secondTile != null) DeselectTile(_secondTile);
	    _firstBoardUnit = _secondBoardUnit = null;
	    
	    EnableTouch();
    }
    
    private Sequence DoShake(Transform tTransform)
    {
        const float offset = 0.05f;
        const float shakeDuration = 0.3f;
        var localPositionX = tTransform.localPosition.x;
	    
        return DOTween.Sequence()
            .Append(tTransform.DOLocalMoveX(localPositionX + offset, shakeDuration * 0.5f))
            .Append(tTransform.DOLocalMoveX(localPositionX - offset, shakeDuration))
            .Append(tTransform.DOLocalMoveX(localPositionX, shakeDuration * 0.5f));
    }
    
    private void OnMergeFailed()
    {
        DOTween.Sequence()
            .Join(DoShake(_firstTile.transform))
            .Join(DoShake(_secondTile.transform))
            .PrependInterval(SelectDuration)
            .Play().OnComplete(ResetTiles);
    }
    
    private void OnMergeCompleted()
    {
	    DestroyTileFromBoardUnit(_firstBoardUnit);
	    DestroyTileFromBoardUnit(_secondBoardUnit);
	    TilesDestroyed?.Invoke(_firstBoardUnit, _secondBoardUnit);
	    ResetTiles();
    }

    private void DestroyTileFromBoardUnit(BoardUnit boardUnit)
    {
	    var tile = boardUnit.ContainedTile;
	    tile.enabled = false;
	    Destroy(tile);
	    boardUnit.RemoveTile();
    }
    
    private void HighlightTiles()
    {
	    var targetScale = new Vector3(-0.3f, -0.3f, -0.3f);
	    
	    _highlightSequence = DOTween.Sequence()
		    .Join(_highlightedTiles.Item1.transform.DOPunchScale(targetScale, 0.7f, 1))
		    .Join(_highlightedTiles.Item2.transform.DOPunchScale(targetScale, 0.7f, 1));
	    
	    _highlightSequence.SetLoops(-1, LoopType.Yoyo).Play();
    }

    public void EnterHintMode()
    {
	    if (_hintMode) return;
	    if (!_tilesMerger.TryGetMergeableTiles(out var tiles)) return;
	    
	    _highlightedTiles = tiles;
	    _hintMode = true;
	    ResetTiles();
	    HighlightTiles();
    }

    private void ExitHintMode()
    {
	    _highlightSequence.Kill();
	    DeselectTile(_highlightedTiles.Item1);
	    DeselectTile(_highlightedTiles.Item2);
	    _hintMode = false;
    }

    private void EnableTouch()
    {
	    _touchBlock.SetActive(false);
	    _canTouch = true;
    }
    
    private void DisableTouch()
    {
	    _touchBlock.SetActive(true);
	    _canTouch = false;
    }
}

using System;
using UnityEngine;


public class Tile : MonoBehaviour
{
	[SerializeField] private SpriteRenderer _spriteRenderer;
	public string[] KeyWords { get; private set; }

	public event Action<Transform, Transform> TilesCollided;

	public bool Collided { get; set; }

	public void Initialize(TileData tileData)
	{
		KeyWords = tileData.KeyWords;
		_spriteRenderer.sprite = tileData.Icon;
		Collided = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent<Tile>(out var tile) || tile.Collided) return;
		
		TilesCollided?.Invoke(transform, tile.transform);
		Collided = true;
		tile.Collided = true;
	}
}

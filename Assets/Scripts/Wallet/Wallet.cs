using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] private TilesMerger _tilesMerger;

    private int _coins;
    public int Coins
    {
        get => _coins;
        private set
        {
            _coins = value;
            SaveCoins();
        }
    }

    public event Action<Vector3> CoinAdded;
    public event Action<int> CoinsChanged;

    private void OnEnable()
    {
        _tilesMerger.TilesMerged += OnTilesMerged;
    }
    
    private void OnDisable()
    {
        _tilesMerger.TilesMerged -= OnTilesMerged;
    }

    private void OnBlockDestroyed()
    {
        //TODO
    }

    private void Awake()
    {
        Coins = PlayerPrefs.HasKey("Coins") ? PlayerPrefs.GetInt("Coins") : 0;
        OnCoinsChanged();
    }
    
    public void SpendCoins(int amount)
    {
        Coins -= amount;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
    }

    private void OnTilesMerged(Vector3 position)
    {
        AddCoins(1);
        CoinAdded?.Invoke(position);
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("Coins", Coins);
    }

    private void OnCoinsChanged()
    {
        CoinsChanged?.Invoke(Coins);
    }


}

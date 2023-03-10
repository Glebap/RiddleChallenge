using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Wallet))]
public class WalletUIView : MonoBehaviour
{
    [SerializeField] private GameObject _coin;
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private Transform _coinsIndicator;
    [SerializeField] private Camera _camera;
    [SerializeField] private ParticleSystem _destroyParticle;
    
    private float distance;
    private Wallet _wallet;
    private int UiCoins;
    

    private void Awake()
    {
        _wallet = GetComponent<Wallet>();
        distance = FindObjectOfType<BoardView>().CameraDistance;
    }

    private void OnEnable()
    {
        _wallet.CoinAdded += OnCoinAdded;
        _wallet.CoinsChanged += OnCoinsChanged;
    }
    
    private void OnDisable()
    {
        _wallet.CoinAdded -= OnCoinAdded;
        _wallet.CoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int amount)
    {
        UpdateUIWallet(amount);
    }

    private void UpdateUIWallet(int coins)
    {
        _coinsText.text = $"{coins}";
        UiCoins = coins;
    }

    private void OnCoinAdded(Vector3 position)
    {
        var coin = Instantiate(_coin);
        var coinT = coin.transform;
        coinT.position = position;
        coinT.localScale = Vector3.zero;

        Instantiate(_destroyParticle, position, quaternion.identity);
        coinT.DOScale(1.0f, 0.4f);
        coinT.DOMoveY(0.5f, 0.4f).OnComplete(() =>
        {
            coinT.DOMove(GetCoinEndPoint(), 1f)
                .SetEase(Ease.InOutBack)
                .OnComplete(() =>
                {
                    Destroy(coin);
                    UpdateUIWallet(++UiCoins);
                });
        });
    }
    
    private Vector3 GetCoinEndPoint()
    {
        var startPosition = _camera.transform.position;
        var guideVector = (_coinsIndicator.position - startPosition).normalized;
        
        return startPosition + guideVector * distance * 1.2f;
    }
}

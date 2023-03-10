using UnityEngine;

[RequireComponent(typeof(Wallet))]
public class WalletManager : MonoBehaviour
{
    private Wallet _wallet;
    
    public static WalletManager instance { get; private set; }
    
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Purchase Manager can be only one");
        }
        instance = this;
    }

    private void Start()
    {
        _wallet = GetComponent<Wallet>();
    }

    public void Purchase(IPurchasable item)
    {
        var price = item.GetPrice();
        _wallet.SpendCoins(price);
    }
    
    public void ValidatePurchase(IPurchasable item)
    {
        var price = item.GetPrice();
        if (_wallet.Coins - price < 0)
        {
            item.OnPurchaseDenied();
        }
        else
        {
            item.OnPurchaseValidated();
        }
    }

    public void TryAddCoins(int amount)
    {
        if (amount >= 0)
        {
            _wallet.AddCoins(amount);
        }
        else
        {
            Debug.LogError("Cant add negative amount of coins");
        }
    }
}

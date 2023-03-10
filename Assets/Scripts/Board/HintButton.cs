using UnityEngine;

public class HintButton : MonoBehaviour, IPurchasable
{
	[SerializeField, Min(0)] private int _price;
	[SerializeField] private TilesHandler _tilesHandler;
	
	public int GetPrice()
	{
		return _price;
	}

	public void OnPurchaseValidated()
	{
		_tilesHandler.EnterHintMode();
		OnBoosterUsed();
	}

	public void OnPurchaseDenied()
	{
		//TODO
	}

	public void OnHintButtonClicked()
	{
		TryValidatePurchase();
	}

	private void TryValidatePurchase()
	{
		WalletManager.instance.ValidatePurchase(this);
	}

	private void OnBoosterUsed()
	{
		WalletManager.instance.Purchase(this);
	}
}

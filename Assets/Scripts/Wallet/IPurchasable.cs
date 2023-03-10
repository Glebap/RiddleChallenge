public interface IPurchasable
{
    public int GetPrice();

    public void OnPurchaseValidated();
    
    public void OnPurchaseDenied();
}

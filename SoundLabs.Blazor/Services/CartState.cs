using SoundLabs.Blazor.Models;

namespace SoundLabs.Blazor.Services;
public sealed class CartState
{
    public Dictionary<int, (Product Product, int Quantity)> Items { get; } = new();
    public int Count => Items.Values.Sum(x => x.Quantity);
    public decimal Total => Items.Values.Sum(x => x.Product.Price * x.Quantity);
    public event Action? Changed;
    public void Add(Product p) { if (Items.TryGetValue(p.Id, out var old)) Items[p.Id] = (p, old.Quantity + 1); else Items[p.Id] = (p, 1); Changed?.Invoke(); }
    public void SetQuantity(int id, int q) { if (!Items.ContainsKey(id)) return; if (q <= 0) Items.Remove(id); else Items[id] = (Items[id].Product, q); Changed?.Invoke(); }
    public void Clear() { Items.Clear(); Changed?.Invoke(); }
}

namespace Products.Domain;

public class Product
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public object? Data { get; set; }
}
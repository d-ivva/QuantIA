using System.Text.Json.Serialization;

namespace QuantIA.Models;

public class Category
{
    public int Id { get; set; }

    public int? UserId { get; set; } // TEMPORARIO, após testes e implementar Keycloak alterar para int 

    public string Name { get; set; } = null!;

    public string Color { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public User? User { get; set; } = null!;
}
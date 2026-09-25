namespace PruebaOben.Domain.Entities;

public class User
{
    public int id { get; set; }
    public string username { get; set; } = string.Empty;
    public string fullname { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string passwordHash { get; set;  } = string.Empty;
    public string rol { get; set; } = string.Empty;
    public bool active { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime? updatedAt { get; set; }
    public DateTime? deletedAt { get; set; }
}
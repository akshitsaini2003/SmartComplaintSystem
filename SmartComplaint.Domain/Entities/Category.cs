namespace SmartComplaint.Domain.Entities;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
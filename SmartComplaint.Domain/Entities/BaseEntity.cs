namespace SmartComplaint.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
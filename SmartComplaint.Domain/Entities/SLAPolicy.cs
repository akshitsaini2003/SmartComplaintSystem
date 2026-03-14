using SmartComplaint.Domain.Enums;

namespace SmartComplaint.Domain.Entities;

public class SLAPolicy
{
    public int SLAPolicyId { get; set; }
    public Priority Priority { get; set; }
    public int MaxResolutionHours { get; set; }
}
namespace SmartComplaint.Application.Interfaces;

public interface ISlaMonitorService
{
    Task CheckAndProcessBreachesAsync();
}
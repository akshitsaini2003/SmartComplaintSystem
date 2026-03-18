namespace SmartComplaint.Web.Models;

public class ComplaintListModel
{
    public int ComplaintId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class ComplaintDetailModel
{
    public int ComplaintId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class CreateComplaintModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Priority { get; set; }
}

public class UpdateComplaintModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class UpdateStatusModel
{
    public string Status { get; set; } = string.Empty;
}

public class CategoryModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AttachmentModel
{
    public int AttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
}

public class CommentModel
{
    public int CommentId { get; set; }
    public int ComplaintId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class ComplaintHistoryModel
{
    public int HistoryId { get; set; }
    public int ComplaintId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedDate { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
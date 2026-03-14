using AutoMapper;
using SmartComplaint.Application.DTOs;
using SmartComplaint.Domain.Entities;

namespace SmartComplaint.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Complaint → ComplaintResponseDto
        CreateMap<Complaint, ComplaintResponseDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.Name))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // Complaint → ComplaintListDto
        CreateMap<Complaint, ComplaintListDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
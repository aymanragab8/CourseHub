using AutoMapper;
using CourseHub.Models;
using CourseHub.ViewModels;

namespace CourseHub.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCategoryViewModel, Category>();
            CreateMap<Category, EditCategoryViewModel>();
            CreateMap<EditCategoryViewModel, Category>();

            CreateMap<CreateCourseViewModel, Course>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.InstructorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<Course, EditCourseViewModel>()
                .ForMember(dest => dest.ExistingImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore());

            CreateMap<EditCourseViewModel, Course>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.InstructorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Enrollments, opt => opt.Ignore());

            CreateMap<Course, CourseDetailsViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.FullName))
                .ForMember(dest => dest.IsEnrolled, opt => opt.Ignore())
                .ForMember(dest => dest.CanManage, opt => opt.Ignore());
        }
    }
}
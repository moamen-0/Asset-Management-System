using AutoMapper;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.PL.Models;

namespace AssetManagementSystem.PL.Mapping
{
	public class AssetProfile : Profile
	{
		public AssetProfile()
		{
		//	CreateMap<Asset, AssetViewModel>()
		//   .ForMember(dest => dest.Facility.Name, opt => opt.MapFrom(src => src.Facility.Name))
		//   .ForMember(dest => dest.Building.Name, opt => opt.MapFrom(src => src.Building.Name))
		//   .ForMember(dest => dest.Floor.Name, opt => opt.MapFrom(src => src.Floor.Name))
		//   .ForMember(dest => dest.Room.Name, opt => opt.MapFrom(src => src.Room.Name))
		//   .ForMember(dest => dest.Department.Name, opt => opt.MapFrom(src => src.Department.Name))
		//   .ForMember(dest => dest.InsertUser, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
		//   .ReverseMap(); // Enables mapping back from ViewModel to Entity
		}
	}
}
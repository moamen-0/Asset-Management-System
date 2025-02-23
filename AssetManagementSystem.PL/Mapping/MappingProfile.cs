using AutoMapper;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.PL.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<Asset, AssetViewModel>().ReverseMap();
		CreateMap<Facility, FacilityViewModel>().ReverseMap();
		CreateMap<Building, BuildingViewModel>().ReverseMap();
		CreateMap<Floor, FloorViewModel>().ReverseMap();
		CreateMap<Room, RoomViewModel>().ReverseMap();
		CreateMap<Department, DepartmentViewModel>().ReverseMap();
	}
}

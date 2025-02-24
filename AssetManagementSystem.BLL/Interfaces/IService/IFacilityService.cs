using AssetManagementSystem.DAL.Entities;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IFacilityService
	{
		 Task<IEnumerable<Facility>> GetAllAsync();
		 Task<Facility> GetByIdAsync(int id);
		 Task<IEnumerable<Department>> GetDepartmentsAsync(int facilityId);
		Task<IEnumerable<Building>> GetBuildingsAsync(int facilityId);

	}
}
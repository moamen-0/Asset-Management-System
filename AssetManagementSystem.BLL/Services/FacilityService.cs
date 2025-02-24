using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class FacilityService : IFacilityService
	{
		private readonly IFacilityRepository _facilityRepository;

		public FacilityService(IFacilityRepository facilityRepository)
		{
			_facilityRepository = facilityRepository;
		}

		public async Task<IEnumerable<Facility>> GetAllAsync()
		{
			return await _facilityRepository.GetAllAsync();
		}

		public async Task<Facility> GetByIdAsync(int id)
		{
			return await _facilityRepository.GetByIdAsync(id);
		}

		public async Task<IEnumerable<Department>> GetDepartmentsAsync(int facilityId)
		{
			var facility = await _facilityRepository.GetByIdAsync(facilityId);
			return facility?.Departments ?? Enumerable.Empty<Department>();
		}

		public async Task<IEnumerable<Building>> GetBuildingsAsync(int facilityId)
		{
			var facility = await _facilityRepository.GetByIdAsync(facilityId);
			return facility?.Buildings ?? Enumerable.Empty<Building>();
		}
	}
}

using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IDisbursementRepository
	{
		Task<List<DisbursementRequest>> GetAllAsync();
		Task<DisbursementRequest> GetByIdAsync(int id);
		Task<DisbursementRequest> AddAsync(DisbursementRequest request);
		Task UpdateAsync(DisbursementRequest request);
		Task DeleteAsync(int id);
		Task<string> GenerateRequestNumberAsync();
	}
}

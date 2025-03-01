using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IChangeLogRepository
	{
		Task<IEnumerable<ChangeLog>> GetAllAsync();
		Task<ChangeLog?> GetByIdAsync(int id);
		Task AddAsync(ChangeLog changeLog);
		Task UpdateAsync(ChangeLog changeLog);
		Task DeleteAsync(int id);
		Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(
			int start,
			int length,
			string searchValue,
			string sortColumn,
			string sortDirection);

		Task<ChangeLog> GetByEntityAsync(string entityName, string entityId);

	}
}

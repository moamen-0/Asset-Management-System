using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IChangeLogService
	{
		Task<IEnumerable<ChangeLog>> GetAllChangeLogsAsync();
		Task<ChangeLog?> GetChangeLogByIdAsync(int id);
		Task AddChangeLogAsync(ChangeLog changeLog);
		Task UpdateChangeLogAsync(ChangeLog changeLog);
		Task DeleteChangeLogAsync(int id);
		Task<(IEnumerable<ChangeLog> Logs, int TotalCount)> GetPaginatedAsync(
			int start,
			int length,
			string searchValue,
			string sortColumn,
			string sortDirection);
		Task<ChangeLog> GetChangeLogByEntityAsync(string entityName, string entityId);


	}
}

using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IRepository
{
	public interface IReturnDocumentRepository
	{
		Task<IEnumerable<ReturnDocument>> GetAllAsync();
		Task<ReturnDocument> GetByIdAsync(int id);
		Task<ReturnDocument> AddAsync(ReturnDocument document);
		Task UpdateAsync(ReturnDocument document);
		Task DeleteAsync(int id);
		Task<string> GenerateDocumentNumberAsync();
	}
}

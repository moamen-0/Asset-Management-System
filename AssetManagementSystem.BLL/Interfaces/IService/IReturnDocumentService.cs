using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IReturnDocumentService
	{
		Task<IEnumerable<ReturnDocument>> GetAllDocumentsAsync();
		Task<ReturnDocument> GetDocumentByIdAsync(int id);
		Task<Dictionary<int, ReturnDocument>> CreateReturnDocumentsAsync(List<string> assetTags, string returnReason);
		Task UpdateReturnDocumentAsync(ReturnDocument document);
		Task DeleteReturnDocumentAsync(int id);
		Task<byte[]> GeneratePdfAsync(int documentId);
		Task<Dictionary<int, List<string>>> GroupAssetsByDepartmentAsync(List<string> assetTags);
	}
}

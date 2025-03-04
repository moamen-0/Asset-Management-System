using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IDisbursementService
	{
		Task<List<DisbursementRequest>> GetAllRequestsAsync();
		Task<DisbursementRequest> GetRequestByIdAsync(int id);
		Task<DisbursementRequest> CreateRequestAsync(DisbursementRequest request, List<string> assetTags);
		Task UpdateRequestAsync(DisbursementRequest request, List<string> assetTags);
		Task DeleteRequestAsync(int id);
		Task<byte[]> GeneratePdfAsync(int requestId);
		Task<string> GetStoreKeeperForAssetsAsync(List<string> assetTags);

	}
}

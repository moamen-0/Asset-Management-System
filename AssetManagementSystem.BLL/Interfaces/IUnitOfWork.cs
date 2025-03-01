using AssetManagementSystem.BLL.Interfaces.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces
{
	public interface IUnitOfWork : IDisposable
	{
		public IAssetRepository AssetRepository { get; }
		public IAssetTransferRepository AssetTransferRepository { get; }
		public IDisposalRepository DisposalRepository { get; }
		public IDepartmentRepository DepartmentRepository { get; }
		public IFacilityRepository FacilityRepository { get; }
		public IChangeLogRepository ChangeLogRepository { get; }
		public IRoomRepository RoomRepository { get; }
		public IBuildingRepository buildingRepository { get; }
		public IFloorRepository floorRepository { get; }
		public IUserRepository user { get; }
		Task<int> SaveChangesAsync();
		public INotificationRepository NotificationRepository { get; }

	}
}

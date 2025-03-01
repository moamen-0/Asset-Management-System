using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly AssetManagementDbContext _context;
		private IAssetRepository _assetRepository;
		private IDisposalRepository _disposalRepository;
		private IDepartmentRepository _departmentRepository;
		private IFacilityRepository _facilityRepository;
		private IChangeLogRepository _changeLogRepository;
		private IRoomRepository _roomRepository;
		private IBuildingRepository _buildingRepository;
		public IFloorRepository _floorRepository;
		private IAssetTransferRepository _assetTransferRepository;
		private IUserRepository _userRepository;
		private INotificationRepository _notificationRepository;
		public UnitOfWork(AssetManagementDbContext context)
		{
			_context = context;
			_assetRepository = new AssetRepository(_context);
			_disposalRepository = new DisposalRepository(_context);
			_departmentRepository = new DepartmentRepository(_context);
			_facilityRepository = new FacilityRepository(_context);
			_changeLogRepository = new ChangeLogRepository(_context);
			_roomRepository = new RoomRepository(_context);
			_buildingRepository = new BuildingRepository(_context);
			_floorRepository = new FloorRepository(_context);
			_assetTransferRepository = new AssetTransferRepository(_context);
			_userRepository = new UserRepository(_context);
			_notificationRepository = new NotificationRepository(_context);
		}



		public IAssetRepository AssetRepository => _assetRepository;

		public IDisposalRepository DisposalRepository => _disposalRepository;

		public IDepartmentRepository DepartmentRepository => _departmentRepository;

		public IFacilityRepository FacilityRepository => _facilityRepository;

		public IChangeLogRepository ChangeLogRepository => _changeLogRepository;

		public IRoomRepository RoomRepository => _roomRepository;

		public IBuildingRepository buildingRepository => _buildingRepository;

		public IFloorRepository floorRepository => _floorRepository;

		public IAssetTransferRepository AssetTransferRepository => _assetTransferRepository;

		public IUserRepository user => _userRepository;

		public INotificationRepository NotificationRepository => _notificationRepository ??= new NotificationRepository(_context);	
		
		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}

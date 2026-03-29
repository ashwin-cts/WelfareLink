using Microsoft.EntityFrameworkCore.Storage;
using WelfareLink.Data;
using WelfareLink.Interfaces;

namespace WelfareLink.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WelfareLinkDbContext _context;
        private IDbContextTransaction? _transaction;

        // Expose context for password hash operations
        public WelfareLinkDbContext Context => _context;

        public UnitOfWork(WelfareLinkDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            AuditLogs = new AuditLogRepository(_context);
            Notifications = new NotificationRepository(_context);
        }

        public IUserRepository Users { get; private set; }
        public IAuditLogRepository AuditLogs { get; private set; }
        public INotificationRepository Notifications { get; private set; }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
   
}

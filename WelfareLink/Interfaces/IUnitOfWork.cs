using WelfareLink.Interfaces;
using WelfareLink. Models;
using System;


namespace WelfareLink.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IAuditLogRepository AuditLogs { get; }
        INotificationRepository Notifications { get; }

        Task<int> SaveAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

    }
}
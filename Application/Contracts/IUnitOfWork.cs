using System;
namespace Application.Contracts
{
    public interface IUnitOfWork : IProgramAbilitySupport, IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        Task SaveChangesAsync();

        void SaveChanges();
    }
}


using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IRepositoryService
    {
        void Add<T>(T Entity) where T : class;
        Task AddAsync<T>(T Entity) where T : class;
        void AddRange<T>(List<T> Entity) where T : class;
        Task AddRangeAsync<T>(List<T> Entity) where T : class;
        void BeginTransaction(IsolationLevel Isolation = IsolationLevel.ReadCommitted);
        Task BeginTransactionAsync(IsolationLevel Isolation = IsolationLevel.ReadCommitted);
        void CommitTransaction();
        Task CommitTransactionAsync();
        int ExecuteDelete<T>(Expression<Func<T, bool>> predicate) where T : class;
        Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
        Task<int> ExecuteStoreProcedure(string sql, params object[] parameter);
        int ExecuteUpdate<T>(Expression<Func<T, bool>> predicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> SetProperty) where T : class;
        Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> SetProperty) where T : class;
        IQueryable<T> Queryrable<T>() where T : class;
        void Remove<T>(T Entity) where T : class;
        void RemoveRange<T>(List<T> Entity) where T : class;
        int SaveChanges();
        Task<int> SaveChangesAsync();
        void Update<T>(T Entity) where T : class;
        void UpdateRange<T>(List<T> Entity) where T : class;
    }
}

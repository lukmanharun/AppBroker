using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Infrastructure.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly AppDbContext dbContext;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public RepositoryService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        /// <summary>
        /// Execute Store Procedure
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<int> ExecuteStoreProcedure(string sql, params object[] parameter)
        {
            return await dbContext.Database.ExecuteSqlRawAsync(sql, parameter);
        }
        /// <summary>
        /// Queryrable Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> Queryrable<T>() where T : class
        {
            return this.dbContext.Set<T>() as IQueryable<T>;
        }
        /// <summary>
        /// Add Entity Asyncronus
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public async Task AddAsync<T>(T Entity) where T : class
        {
            await this.dbContext.AddAsync<T>(Entity);
        }
        /// <summary>
        /// Add Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        public void Add<T>(T Entity) where T : class
        {
            this.dbContext.Add<T>(Entity);
        }
        /// <summary>
        /// Add Range Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        public void AddRange<T>(List<T> Entity) where T : class
        {
            this.dbContext.AddRange(Entity);
        }
        /// <summary>
        /// Add Range Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public async Task AddRangeAsync<T>(List<T> Entity) where T : class
        {
            await this.dbContext.AddRangeAsync(Entity);
        }
        /// <summary>
        /// delete entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        public void Remove<T>(T Entity) where T : class
        {
            this.dbContext.Remove<T>(Entity);
        }
        /// <summary>
        /// Delete range entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        public void RemoveRange<T>(List<T> Entity) where T : class
        {
            this.dbContext.RemoveRange(Entity);
        }
        /// <summary>
        /// Update entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        public void Update<T>(T Entity) where T : class
        {
            this.dbContext.Update<T>(Entity);
        }
        /// <summary>
        /// Update range entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        public void UpdateRange<T>(List<T> Entity) where T : class
        {
            this.dbContext.UpdateRange(Entity);
        }
        /// <summary>
        /// Save change entity
        /// </summary>
        /// <returns></returns>
        public int SaveChanges() 
        {
            return this.dbContext.SaveChanges();
        }
        /// <summary>
        /// Save Change async entity
        /// </summary>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync()
        {
            return await this.dbContext.SaveChangesAsync();
        }
        /// <summary>
        /// Begin transaction 
        /// </summary>
        /// <param name="Isolation"></param>
        public void BeginTransaction(System.Data.IsolationLevel Isolation = System.Data.IsolationLevel.ReadCommitted)
        {
            this.dbContext.Database.BeginTransaction(Isolation);
        }
        /// <summary>
        /// Begin transaction async
        /// </summary>
        /// <param name="Isolation"></param>
        public async Task BeginTransactionAsync(System.Data.IsolationLevel Isolation = System.Data.IsolationLevel.ReadCommitted)
        {
            await this.dbContext.Database.BeginTransactionAsync(Isolation);
        }
        /// <summary>
        /// Delete entity with filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int ExecuteDelete<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return this.dbContext.Set<T>().Where(predicate).ExecuteDelete();
        }
        /// <summary>
        /// Delete entity with filter async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<int> ExecuteDeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await dbContext.Set<T>().Where(predicate).ExecuteDeleteAsync();
        }
        /// <summary>
        /// Update enity with filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="SetProperty"></param>
        /// <returns></returns>
        public int ExecuteUpdate<T>(Expression<Func<T, bool>> predicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> SetProperty) where T : class
        {
           return this.dbContext.Set<T>().Where(predicate).ExecuteUpdate(SetProperty);
        }
        /// <summary>
        /// Update entity with filter async 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="SetProperty"></param>
        /// <returns></returns>
        public async Task<int> ExecuteUpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> SetProperty) where T : class
        {
            return await this.dbContext.Set<T>().Where(predicate).ExecuteUpdateAsync(SetProperty);
        }
        /// <summary>
        /// Commit transaction
        /// </summary>
        public void CommitTransaction()
        {
            this.dbContext.Database.CommitTransaction();
        }
        /// <summary>
        /// Commit transaction async
        /// </summary>
        /// <returns></returns>
        public async Task CommitTransactionAsync()
        {
            await this.dbContext.Database.CommitTransactionAsync();
        }
    }
}

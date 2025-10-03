namespace etickets_aspnet_api.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {

        #region ReadAll
        Task<List<T>> GetAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>[]? include = null, bool tracked = true);
        #endregion

        #region ReadOne
        Task<T> GetOneAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>[]? include = null, bool tracked = true);
        #endregion

        #region Create
        Task AddAsync(T entity);
        #endregion

        #region Create Range

        Task CreateRangeAsync(List<T> entity);
 
        #endregion

        #region Update
        Task Update(T entity);
        #endregion

        #region Delete
        Task DeleteAsync(T entity);
        #endregion

        #region Delete Range

        Task DeleteRangeAsync(List<T> entity);
  
        #endregion

        #region Save
        Task CommitAsync();
        #endregion

    }
}

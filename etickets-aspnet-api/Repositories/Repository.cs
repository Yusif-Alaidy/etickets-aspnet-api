namespace etickets_aspnet_api.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {

        #region Fields

        private CineBookContext _context;
        private DbSet<T> _db;

        #endregion

        #region Constructor

        public Repository(CineBookContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        #endregion

        #region ReadAll

        public async Task<List<T>> GetAsync(Expression<Func<T,bool>>? filter = null, Expression<Func<T, object>>[]? include = null, bool tracked = true)
        {
            var data = _db.AsQueryable();

            if (filter is not null) 
            {
                data = data.Where(filter);
            }
            if (include is not null) 
            {
                foreach (var item in include)
                {
                    data = data.Include(item);
                }
            }
            if (!tracked)
            {
                data = data.AsNoTracking();
            }

            return await data.ToListAsync();
        }


    //    public async Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
    //Expression<Func<T, object>>[]? includes = null, bool tracked = true)
    //    {
    //        var entities = _db.AsQueryable();

    //        if (expression is not null)
    //        {
    //            entities = entities.Where(expression);
    //        }

    //        if (includes is not null)
    //        {
    //            foreach (var item in includes)
    //            {
    //                entities = entities.Include(item);
    //            }
    //        }

    //        if (!tracked)
    //        {
    //            entities = entities.AsNoTracking();
    //        }

    //        return await entities.ToListAsync();
    //    }



        #endregion

        #region ReadOne
        public async Task<T> GetOneAsync(Expression<Func<T, bool>>? filter = null, Expression<Func<T, object>>[]? include = null, bool tracked = true)
        {

            return (await GetAsync(filter, include, tracked)).FirstOrDefault()!;
        }
        #endregion

        #region Create

        public async Task AddAsync(T entity)                                         
        {
            await _db.AddAsync(entity);
        }
    
        #endregion

        #region Create Range

        public async Task CreateRangeAsync(List<T> entity)
        {
            await _db.AddRangeAsync(entity);
        }

        #endregion

        #region Update

        public async Task Update(T entity)
        {
            _db.Update(entity);
        }

        #endregion

        #region Delete

        public async Task DeleteAsync(T entity)
        {
             _db.Remove(entity);
        }

        #endregion

        #region Delete Rang

        public async Task DeleteRangeAsync(List<T> entity)
        {
            _db.RemoveRange(entity);
        }

        #endregion

        #region Save

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        #endregion

    }
}

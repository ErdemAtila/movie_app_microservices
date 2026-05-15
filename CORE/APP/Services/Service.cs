using CORE.APP.Domain;
using Microsoft.EntityFrameworkCore;

namespace CORE.APP.Services
{
    public abstract class Service<TEntity> : ServiceBase, IDisposable where TEntity : Entity, new()
    {
        private readonly DbContext _db;

        // DbContext db = new DbContext();
        // Service service = new Service(db);
        protected Service(DbContext db) // constructor injection (dependency injection)
        {
            _db = db;
        }

        // select * from Products where, order by
        // read
        protected virtual IQueryable<TEntity> DbSet()
        {
            return _db.Set<TEntity>();
        }

        // insert
        protected async Task CreateAsync(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Add(entity);
            if (save)
                await SaveAsync(cancellationToken);
        }

        // update
        protected async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Update(entity);
            if (save)
                await SaveAsync(cancellationToken);
        }

        // delete
        protected async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Remove(entity);
            if (save)
                await SaveAsync(cancellationToken);
        }

        protected virtual async Task<int> SaveAsync(CancellationToken cancellationToken) => await _db.SaveChangesAsync(cancellationToken);

        public IQueryable<TRelationalEntity> DbSet<TRelationalEntity>() where TRelationalEntity : Entity, new() => _db.Set<TRelationalEntity>().AsNoTracking();

        protected void Delete<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : Entity, new() => _db.Set<TRelationalEntity>().RemoveRange(relationalEntities);

        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using F4ST.Common.Extensions;
using F4ST.Common.Mappers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace F4ST.Data.RavenDB
{
    public class RavenDbRepository : IRepository
    {
        private readonly IRavenDbConnection _dbConnection;
        private readonly IAsyncDocumentSession _session;

        public RavenDbRepository(DbConnectionModel config)
        {
            _dbConnection = new RavenDbConnection(config);
            _session = _dbConnection.Connection.OpenAsyncSession();
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public async Task SaveChanges()
        {
            await _session.SaveChangesAsync();
        }

        #region Get

        /// <inheritdoc />
        public Task<T> Get<T>(object id, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return _session.LoadAsync<T>((string) id, cancellationToken);
        }

        public async Task<IEnumerable<T>> Get<T>(IEnumerable<object> ids, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            var res = await _session.LoadAsync<T>((IEnumerable<string>) ids, cancellationToken);
            return res.Select(t => t.Value);
        }

        /// <inheritdoc />
        public async Task<T> Get<T, TIncType>(object id, Expression<Func<T, object>> field,
            Expression<Func<T, TIncType>> targetField, CancellationToken cancellationToken = default)
            where T : BaseEntity
            where TIncType : BaseEntity
        {
            var res = await _session
                .Include(field.Cast<T, object, string>())
                .LoadAsync<T>((string) id, cancellationToken);

            var inc = await _session.LoadAsync<TIncType>(res.GetPropertyValue<T, string>(field.Name),
                cancellationToken);

            res.SetPropertyValue(targetField, inc);

            return res;
        }

        /// <summary>
        /// Get document by id and include related documents
        /// </summary>
        /// <typeparam name="T">Type of document</typeparam>
        /// <typeparam name="TIncType">Type of list of included document</typeparam>
        /// <param name="id">Id of document</param>
        /// <param name="field">Field with have list of related document id</param>
        /// <param name="targetField">Target field for set related document</param>
        /// <param name="cancellationToken"></param>
        /// <returns>T</returns>
        public async Task<T> Get<T, TIncType>(string id, Expression<Func<T, IEnumerable<string>>> field,
            Expression<Func<T, IEnumerable<TIncType>>> targetField, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            var res = await _session
                .Include(field)
                .LoadAsync<T>(id, cancellationToken);

            var keys = res.GetPropertyValue(field);

            var inc = await _session.LoadAsync<TIncType>(keys, cancellationToken);

            var v = inc.Select(p => p.Value).ToList().AsEnumerable();

            res.SetPropertyValue(targetField, v);
            return res;
        }

        /// <inheritdoc />
        public async Task<T> Get<T, TIncType1, TIncType2>(string id, Expression<Func<T, object>> field1,
            Expression<Func<T, TIncType1>> targetField1, Expression<Func<T, object>> field2,
            Expression<Func<T, TIncType2>> targetField2, CancellationToken cancellationToken = default)
            where T : BaseEntity
            where TIncType1 : BaseEntity
            where TIncType2 : BaseEntity
        {
            var res = await _session
                .Include(field1.Cast<T, object, string>())
                .Include(field2.Cast<T, object, string>())
                .LoadAsync<T>(id, cancellationToken);

            var inc1 = await _session.LoadAsync<TIncType1>(res.GetPropertyValue<T, string>(field1.Name),
                cancellationToken);
            res.SetPropertyValue(targetField1, inc1);

            var inc2 = await _session.LoadAsync<TIncType2>(res.GetPropertyValue<T, string>(field2.PropertyName()),
                cancellationToken);
            res.SetPropertyValue(targetField2, inc2);

            return res;
        }

        #endregion

        #region Insert

        /// <inheritdoc />
        public Task Add<T>(T entity, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return _session.StoreAsync(entity, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Add<T>(IEnumerable<T> entities)
            where T : BaseEntity
        {
            await using var bulk = _dbConnection.Connection.BulkInsert();
            foreach (var entity in entities)
            {
                await bulk.StoreAsync(entity);
            }
        }

        #endregion

        #region Update

        /// <inheritdoc />
        public Task Update<T>(T entity, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            entity.ModifiedOn = DateTime.Now;
            var id = entity.GetPropertyValue<T, string>("Id");
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("Id not found");

            return _session.StoreAsync(entity, id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Update<T>(IEnumerable<T> entities)
            where T : BaseEntity
        {
            await using var bulk = _dbConnection.Connection.BulkInsert();
            foreach (var entity in entities)
            {
                entity.ModifiedOn = DateTime.Now;

                var id = entity.GetPropertyValue<T, string>("Id");
                if (string.IsNullOrWhiteSpace(id))
                    throw new Exception("Id not found");

                await bulk.StoreAsync(entity, id);
            }
        }

        /// <inheritdoc />
        public Task Update<T, TField>(T entity, Expression<Func<T, TField>> field, TField value,
            CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            entity.SetPropertyValue(field, value);
            return Update(entity, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Update<T, TField>(Expression<Func<T, bool>> filter,
            Expression<Func<T, TField>> field, TField value, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            var items = await _session.Query<T>().Where(filter, true).ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                item.SetPropertyValue(field, value);
            }

            await Update(items);
        }

        #endregion

        #region Delete

        /// <inheritdoc />
        public void Delete<T>(object id)
            where T : BaseEntity
        {
            _session.Delete(id);
        }

        /// <inheritdoc />
        public async Task Delete<T>(T entity, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            _session.Delete(entity);
        }

        /// <inheritdoc />
        public async Task Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            //await _dbConnection.Connection.Operations.SendAsync(new DeleteByQueryOperation<T>(filter));
            var items = await _session.Query<T>().Where(filter, true).ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                await Delete(item, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task DeleteAll<T>(CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            var items = await _session.Query<T>().ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                await Delete(item, cancellationToken);
            }
        }

        #endregion

        #region Find

        /// <inheritdoc />
        public async Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return await _session.Query<T>().Where(filter, true).ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order,
            int pageIndex, int size, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return Find(filter, order, pageIndex, size, false, cancellationToken);
        }

        public Task<IEnumerable<T>> Find<T, TIncType>(Expression<Func<T, bool>> filter,
            Expression<Func<T, object>> field, Expression<Func<T, TIncType>> targetField,
            Expression<Func<T, object>> order, int pageIndex,
            int size, CancellationToken cancellationToken = default) where T : BaseEntity where TIncType : BaseEntity
        {
            return Find(filter,field,targetField, order, pageIndex, size, false, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, int pageIndex, int size,
            CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return Find(filter, "Id", pageIndex, size, false, cancellationToken);
        }

        public Task<IEnumerable<T>> Find<T, TIncType>(Expression<Func<T, bool>> filter,
            Expression<Func<T, object>> field, Expression<Func<T, TIncType>> targetField, int pageIndex, int size,
            CancellationToken cancellationToken = default) where T : BaseEntity where TIncType : BaseEntity
        {
            return Find(filter, "Id", pageIndex, size, false, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter,
            Expression<Func<T, object>> order,
            int pageIndex, int size, bool isDescending, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            var orderField = order.Name ?? order.PropertyName();
            return Find(filter, orderField, pageIndex, size, isDescending, cancellationToken);
        }

        public Task<IEnumerable<T>> Find<T, TIncType>(Expression<Func<T, bool>> filter,
            Expression<Func<T, object>> field, Expression<Func<T, TIncType>> targetField,
            Expression<Func<T, object>> order, int pageIndex,
            int size, bool isDescending, CancellationToken cancellationToken = default)
            where T : BaseEntity 
            where TIncType : BaseEntity
        {
            var orderField = order.Name ?? order.PropertyName();
            return Find(filter,field,targetField, orderField, pageIndex, size, isDescending, cancellationToken);
        }

        private async Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter,
            string order, int pageIndex, int size, bool isDescending, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            var items = _session.Query<T>().Where(filter, true);
            var oItems = !isDescending
                ? items.OrderBy(order)
                : items.OrderByDescending(order);
            return await oItems
                .Skip(pageIndex * size)
                .Take(size)
                .ToListAsync(cancellationToken);
        }
        
        private async Task<IEnumerable<T>> Find<T, TIncType>(Expression<Func<T, bool>> filter,
            Expression<Func<T, object>> field, Expression<Func<T, TIncType>> targetField,
            string order, int pageIndex, int size, bool isDescending, CancellationToken cancellationToken = default)
            where T : BaseEntity
            where TIncType : BaseEntity
        {
            var items = _session
                .Query<T>().Where(filter, true)
                .Include(field);

            var oItems = !isDescending
                ? items.OrderBy(order)
                : items.OrderByDescending(order);


            var res= await oItems
                .Skip(pageIndex * size)
                .Take(size)
                .ToListAsync(cancellationToken);

            var incFieldValues = res.Select(field.Compile()).Cast<string>().ToList();

            var resInc = await _session.LoadAsync<TIncType>(incFieldValues, cancellationToken);

            foreach (var item in res)
            {
                var tValue = item.GetPropertyValue<T, string>(field.PropertyName());
                
                if(!resInc.ContainsKey(tValue))
                    continue;
                
                var obj = resInc.FirstOrDefault(c => c.Key == tValue).Value;
                item.SetPropertyValue(targetField, obj);
            }

            return res;
        }

        #endregion

        #region Util

        /// <inheritdoc />
        public async Task<long> Count<T>(CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return await _session.Query<T>().CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> Count<T>(Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return await _session.Query<T>().CountAsync(filter, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> Any<T>(CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return _session.Query<T>().AnyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> Any<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return _session.Query<T>().AnyAsync(filter, cancellationToken);
        }

        #endregion

        #region Max/Min

        /// <inheritdoc />
        public Task<T> Max<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> field,
            CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return _session.Query<T>()
                .Where(filter, true)
                .OrderByDescending(field)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> Max<T>(Expression<Func<T, object>> field, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return Max(f => true, field, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> Min<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> field,
            CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return _session.Query<T>()
                .Where(filter, true)
                .OrderBy(field)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> Min<T>(Expression<Func<T, object>> field, CancellationToken cancellationToken = default)
            where T : BaseEntity
        {
            return Min(f => true, field, cancellationToken);
        }

        #endregion
    }
}
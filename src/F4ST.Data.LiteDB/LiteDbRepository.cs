using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using F4ST.Common.Extensions;
using LiteDB;

namespace F4ST.Data.LiteDB
{
    public class LiteDbRepository : IRepository
    {
        private readonly LiteDatabase _dbConnection;

        public LiteDbRepository(DbConnectionModel config)
        {
            //ILiteDbConnection connection
            var connection = new LiteDbConnection(config);
            _dbConnection = connection.Connection;
        }


        public void Dispose()
        {
        }

        public async Task SaveChanges()
        {

        }

        private string ResolveTableName<T>()
        {
            var type = typeof(T);
            var tableName = type.Name;

            var tableAttr = type.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
            if (tableAttr != null)
            {
                tableName = tableAttr.Name;
            }

            return tableName;
        }

        private ILiteCollection<T> GetCollection<T>()
        {
            var col = _dbConnection.GetCollection<T>(ResolveTableName<T>());
            return col;
        }

        #region Get

        /// <inheritdoc />
        public async Task<T> Get<T>(object id, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            return col.FindOne(Query.EQ("Id", (BsonValue)id));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> Get<T>(IEnumerable<object> ids, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var res = new List<T>();
            foreach (var id in ids)
            {
                res.Add(await Get<T>(id, cancellationToken));
            }

            return res;
        }

        /// <inheritdoc />
        public async Task<T> Get<T, TIncType>(object id, Expression<Func<T, string>> field, Expression<Func<T, TIncType>> targetField,
            CancellationToken cancellationToken = default) where T : BaseEntity where TIncType : BaseEntity
        {
            var res = await Get<T>(id, cancellationToken);

            if (res == default)
                return null;

            var inc = await Get<TIncType>(res.GetPropertyValue(field), cancellationToken);

            res.SetPropertyValue(targetField, inc);

            return res;
        }

        /// <inheritdoc />
        public async Task<T> Get<T, TIncType1, TIncType2>(string id, Expression<Func<T, string>> field1, Expression<Func<T, TIncType1>> targetField1, Expression<Func<T, string>> field2,
            Expression<Func<T, TIncType2>> targetField2, CancellationToken cancellationToken = default) where T : BaseEntity where TIncType1 : BaseEntity where TIncType2 : BaseEntity
        {
            var res = await Get<T>(id, cancellationToken);

            var inc1 = await Get<TIncType1>(res.GetPropertyValue(field1), cancellationToken);
            res.SetPropertyValue(targetField1, inc1);

            var inc2 = await Get<TIncType2>(res.GetPropertyValue(field2), cancellationToken);
            res.SetPropertyValue(targetField2, inc2);

            return res;
        }

        #endregion

        #region Insert

        /// <inheritdoc />
        public async Task Add<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            col.Insert(entity);
        }

        /// <inheritdoc />
        public async Task Add<T>(IEnumerable<T> entities) where T : BaseEntity
        {
            var col = GetCollection<T>();
            col.InsertBulk(entities);
        }

        #endregion

        #region Update

        /// <inheritdoc />
        public async Task Update<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            entity.ModifiedOn = DateTime.Now;
            col.Update(entity);
        }

        /// <inheritdoc />
        public async Task Update<T>(IEnumerable<T> entities) where T : BaseEntity
        {
            var col = GetCollection<T>();
            var dbEntities = entities as T[] ?? entities.ToArray();
            foreach (var item in dbEntities)
            {
                item.ModifiedOn = DateTime.Now;
            }
            col.Update(dbEntities);
        }

        /// <inheritdoc />
        public Task Update<T, TField>(T entity, Expression<Func<T, TField>> field, TField value, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            entity.SetPropertyValue(field, value);
            return Update(entity, cancellationToken);
        }

        /// <inheritdoc />
        public Task Update<T, TField>(Expression<Func<T, bool>> filter, Expression<Func<T, TField>> field, TField value,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            var items = col.Find(filter).ToArray();
            foreach (var item in items)
            {
                item.SetPropertyValue(field, value);
            }

            return Update(items);
        }

        #endregion

        #region Delete

        /// <inheritdoc />
        public void Delete<T>(object id) where T : BaseEntity
        {
            var col = GetCollection<T>();
            //todo: must check
            Debugger.Break();
            col.DeleteMany(Query.EQ("Id", (BsonValue)id));
        }

        /// <inheritdoc />
        public async Task Delete<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            //todo: must check
            Debugger.Break();
            var id = entity.GetPropertyValue<T, string>("Id");
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("Id not found");
            col.DeleteMany(Query.EQ("Id", id));
        }

        /// <inheritdoc />
        public async Task Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            col.DeleteMany(filter);
        }

        /// <inheritdoc />
        public async Task DeleteAll<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            col.DeleteMany(d => true);
        }

        #endregion

        #region Find

        /// <inheritdoc />
        public async Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            return col.Find(filter);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return Find(filter, order, pageIndex, size, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, int pageIndex, int size, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            return col.Find(filter, pageIndex * size, size);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size, bool isDescending,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            var items = !isDescending
                ? col.FindAll().OrderBy(order.Compile())
                : col.FindAll().OrderByDescending(order.Compile());

            return items
                .Where(filter.Compile())
                .Skip(pageIndex * size)
                .Take(size);

            //return col.Find(filter, pageIndex * size, size);
        }

        #endregion

        #region Util

        /// <inheritdoc />
        public async Task<long> Count<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            return col.LongCount();
        }

        /// <inheritdoc />
        public async Task<long> Count<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            var col = GetCollection<T>();
            return col.LongCount(filter);
        }

        /// <inheritdoc />
        public async Task<bool> Any<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return await Count<T>(cancellationToken) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> Any<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return await Count(filter, cancellationToken) > 0;
        }

        #endregion

        #region Max/Min

        /// <inheritdoc />
        public Task<T> Max<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> field, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Max<T>(Expression<Func<T, object>> field, CancellationToken token = default) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Min<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> field, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Min<T>(Expression<Func<T, object>> field, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

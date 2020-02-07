using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using F4ST.Common.Extensions;

namespace F4ST.Data.Dapper
{
    public class DapperRepository : IRepository
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction = null;

        private int Timeout { get; set; } = 30;

        public DapperRepository(IDapperConnection connection)
        {
            _connection = connection.Connection;
            _connection.Open();
            OpenTransaction();
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                RollbackTransaction();
            }

            if (_connection?.State != ConnectionState.Closed) 
                _connection?.Close();
            _connection?.Dispose();
        }

        public async Task SaveChanges()
        {
            CommitTransaction();
        }

        #region Transaction

        private void OpenTransaction()
        {
            _transaction = _connection.BeginTransaction();
        }

        private void CommitTransaction()
        {
            if (_transaction == null)
                return;

            _transaction?.Commit();
            _transaction = null;

            OpenTransaction();
        }

        private void RollbackTransaction()
        {
            if (_transaction == null)
                return;
            _transaction?.Rollback();
            _transaction = null;
        }

        #endregion


        #region Get

        /// <inheritdoc />
        public Task<T> Get<T>(object id, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            return _connection.GetAsync<T>(id, _transaction, Timeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> Get<T>(IEnumerable<object> ids, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            //todo: must be change for better performance
            var res = new List<T>();
            foreach (var id in ids)
            {
                res.Add(await Get<T>(id, cancellationToken));
            }

            return res;
        }

        public async Task<T> Get<T, TIncType>(object id, Expression<Func<T, string>> field,
            Expression<Func<T, TIncType>> targetField, CancellationToken cancellationToken = default)
            where T : DbEntity
            where TIncType : DbEntity
        {
            var res = await Get<T>(id, cancellationToken);

            if (res == default)
                return null;

            var inc = await Get<TIncType>(res.GetPropertyValue(field), cancellationToken);

            res.SetPropertyValue(targetField, inc);

            return res;
        }

        /// <inheritdoc />
        public async Task<T> Get<T, TIncType1, TIncType2>(string id, Expression<Func<T, string>> field1,
            Expression<Func<T, TIncType1>> targetField1, Expression<Func<T, string>> field2,
            Expression<Func<T, TIncType2>> targetField2, CancellationToken cancellationToken = default)
            where T : DbEntity
            where TIncType1 : DbEntity
            where TIncType2 : DbEntity
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
        public Task Add<T>(T entity, CancellationToken cancellationToken = default) where T : DbEntity
        {
            return _connection.InsertAsync(entity, _transaction, Timeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Add<T>(IEnumerable<T> entities)
            where T : DbEntity
        {
            var addList = entities
                .Select(entity => Add(entity));
            Task.WaitAll(addList.ToArray());
        }

        #endregion

        #region Update

        /// <inheritdoc />
        public Task Update<T>(T entity, CancellationToken cancellationToken = default) where T : DbEntity
        {
            entity.ModifiedOn = DateTime.Now;
            return _connection.UpdateAsync(entity, _transaction, Timeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Update<T>(IEnumerable<T> entities)
            where T : DbEntity
        {
            var updateList = entities
                .Select(entity => Update(entity));
            Task.WaitAll(updateList.ToArray());
        }

        /// <inheritdoc />
        public Task Update<T, TField>(T entity, Expression<Func<T, TField>> field, TField value,
            CancellationToken cancellationToken = default) where T : DbEntity
        {
            entity.SetPropertyValue(field, value);
            return Update(entity, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Update<T, TField>(Expression<Func<T, bool>> filter, Expression<Func<T, TField>> field, TField value,
            CancellationToken cancellationToken = default) where T : DbEntity
        {
            //todo: must check for work
            var tran = new ExprToQueryTranslator();
            var where = tran.Translate(filter);
            Debugger.Break();

            var items = await _connection.GetListAsync<T>(where, null, _transaction, Timeout, cancellationToken);
            var dbItems = items as T[] ?? items.ToArray();
            foreach (var item in dbItems)
            {
                item.SetPropertyValue(field, value);
            }

            await Update(dbItems);
        }

        #endregion

        #region Delete

        /// <inheritdoc />
        public void Delete<T>(object id)
            where T : DbEntity
        {
            _connection.Delete<T>(id, _transaction, Timeout);
        }

        /// <inheritdoc />
        public Task Delete<T>(T entity, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            return _connection.DeleteAsync(entity, _transaction, Timeout, cancellationToken);
        }

        /// <inheritdoc />
        public Task Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            //todo: must check for work
            var tran = new ExprToQueryTranslator();
            var where = tran.Translate(filter);
            Debugger.Break();

            return _connection.DeleteListAsync<T>(where, null, _transaction, Timeout, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteAll<T>(CancellationToken cancellationToken = default) where T : DbEntity
        {
            return _connection.DeleteListAsync<T>(new{}, _transaction, Timeout, cancellationToken);
        }

        #endregion

        #region Find

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            //todo: must check for work
            var tran = new ExprToQueryTranslator();
            var where = tran.Translate(filter);
            Debugger.Break();

            return _connection.GetListAsync<T>(where, null, _transaction, Timeout, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter,
            Expression<Func<T, object>> order, int pageIndex, int size, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            return Find(filter, order, pageIndex, size, false, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, int pageIndex, int size, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            return Find(filter, f => f.Id, pageIndex, size, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex,
            int size, bool isDescending, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            //todo: must check for work
            var tran = new ExprToQueryTranslator();
            var where = tran.Translate(filter);
            Debugger.Break();

            var orderBy = $"{order.Name} ";
            orderBy += !isDescending ? "asc" : "desc";

            return _connection.GetListPagedAsync<T>(pageIndex, size, where, orderBy,null, _transaction, Timeout, cancellationToken);
        }

        #endregion

        #region Util

        /// <inheritdoc />
        public async Task<long> Count<T>(CancellationToken cancellationToken = default) where T : DbEntity
        {
            return await _connection.RecordCountAsync<T>(cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<long> Count<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            //todo: must check for work
            var tran = new ExprToQueryTranslator();
            var where = tran.Translate(filter);
            Debugger.Break();

            return await _connection.RecordCountAsync<T>(where,null,_transaction,Timeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> Any<T>(CancellationToken cancellationToken = default) where T : DbEntity
        {
            return await Count<T>(cancellationToken) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> Any<T>(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            return await Count(filter, cancellationToken) > 0;
        }

        #endregion

        #region Max/Min

        /// <inheritdoc />
        public Task<T> Max<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> field,
            CancellationToken cancellationToken = default) where T : DbEntity
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Max<T>(Expression<Func<T, object>> field, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Min<T>(Expression<Func<T, bool>> filter, Expression<Func<T, object>> field,
            CancellationToken cancellationToken = default) where T : DbEntity
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Min<T>(Expression<Func<T, object>> field, CancellationToken cancellationToken = default)
            where T : DbEntity
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
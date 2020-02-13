using System;

namespace F4ST.Data
{
    public interface IDbProvider
    {
        /// <summary>
        /// Provider key stored in AppSetting.json => DbConnection
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Get type of connection model, <see cref="DbConnectionModel"/>
        /// </summary>
        Type GetConnectionModel { get; }

        /// <summary>
        /// Get type of connection model, <see cref="IRepository"/>
        /// </summary>
        Type GetRepository { get; }

    }
}
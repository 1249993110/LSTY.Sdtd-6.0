using IceCoffee.DbCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSTY.Sdtd.Data
{
    public class DefaultDbConnectionInfo : DbConnectionInfo
    {
        public DefaultDbConnectionInfo(string connectionName, string connectionString)
        {
            ConnectionName = connectionName;
            ConnectionString = connectionString;
            DatabaseType = DatabaseType.SQLite;
        }
    }
}

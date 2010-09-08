using System;
using System.Data.OleDb;
using Carbon.Sql.Adapter.ContextProvider;

namespace Carbon.Integration.Tests.Sql
{
    public class MSAccessDataContextProvider : ISqlContextProvider
    {
        public string ConnectionString
        {
            get; set;
        }

        public string QueryText
        {
            get; set;
        }

        public MSAccessDataContextProvider()
        {
            QueryText = "select employee.* from employee;";
        }

        public OleDbConnection GetConnection()
        {
            var connection = new OleDbConnection(this.ConnectionString);
            return connection;
        }
    }
}
using System.Data.OleDb;

namespace Carbon.Sql.Adapter.ContextProvider
{
    public interface ISqlContextProvider
    {
        /// <summary>
        /// (Read-Write). The connection information used to communicate with the data store.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// (Read-Write). The text used by the <seealso cref="SqlChannel">SQL channel</seealso> to query the 
        /// data store for information. This is typically a SELECT statement.
        /// </summary>
        string QueryText { get; set; }

        /// <summary>
        /// This will retreive a connection to the data store via the connection information for communication.
        /// </summary>
        /// <returns></returns>
        OleDbConnection GetConnection();
    }
}
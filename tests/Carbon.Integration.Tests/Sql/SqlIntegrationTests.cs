using System;
using System.Data;
using Carbon.Core;
using Carbon.Core.Adapter.Factory;
using Carbon.Integration.Configuration;
using Castle.Windsor;
using Xunit;

namespace Carbon.Integration.Tests.Sql
{
    // Note: in .NET 3.5+, the ole db runtime wants to enlist in the ambient transaction by default, 
    // this is no good if you are using MSAccess as it does not support ambient transactions being 
    // passed in or participating distributed transactions (dist. txns are not supported by design). 
    // To get around this use the following connection string 
    // 
    // Provider=Microsoft.Jet.OLEDB.4.0;OLE DB Services=-4;Data Source={path to your database}
    //
    public class SqlIntegrationTests
    {
        private WindsorContainer container;
        private string _uri = string.Empty;

        public SqlIntegrationTests()
        {
            container = new WindsorContainer(@"sql/sql.config.xml");
            container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _uri = @"sql://ms_access_data_context"; // here is the sql uri to access the data context in the config:

        }

        [Fact]
        public void can_send_sql_select_command_to_data_store_and_return_back_a_dataset_with_results()
        {
            var factory = container.Resolve<IAdapterFactory>();
            var adapter = factory.BuildInputAdapterFromUri(_uri);

            Assert.NotNull(adapter);

            var envelope = adapter.Receive();
            Assert.NotEqual(typeof(NullEnvelope), envelope.GetType());

            Assert.Equal(typeof(DataSet), envelope.Body.GetPayload<DataSet>().GetType());
        }

        [Fact(Skip = "The insert and select are not working together for this test")]
        public void can_send_sql_insert_command_to_data_store_and_increment_the_rows_in_the_table()
        {
            var factory = container.Resolve<IAdapterFactory>();
            var inputAdapter = factory.BuildInputAdapterFromUri(_uri);

            var before = inputAdapter.Receive();

            var commandText = "insert into employee (firstname, lastname, age) values (\"test\", \"test\", 1)";
            var outputChannelAdapter = factory.BuildOutputAdapterFromUri(_uri);
            outputChannelAdapter.Send(new Envelope(commandText));

            var after = inputAdapter.Receive();

            // sequence size for inserts, updates always reflect the number of records inserted or updated...
            Assert.Equal(1, after.Header.SequenceSize);
            
        }
    }
}
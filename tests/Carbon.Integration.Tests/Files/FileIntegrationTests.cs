using System;
using System.IO;
using System.Text;
using System.Threading;
using Carbon.Core.Builder;
using Carbon.Core.Components;
using Carbon.Core.Pipeline.Component;
using Carbon.Core.Pipeline.Receive;
using Carbon.Core.Pipeline.Send;
using Carbon.Integration.Configuration;
using Carbon.Integration.Dsl.Surface;
using Carbon.Integration.Dsl.Surface.Ports;
using Castle.Windsor;
using Xunit;

namespace Carbon.Integration.Tests.Files
{
    public class FileIntegrationTests
    {
        private WindsorContainer container;
        static string _source_directory = string.Empty;
        static string _target_directory = string.Empty;
        private static string _error_directory = string.Empty;

        public FileIntegrationTests()
        {
            container = new WindsorContainer(@"files/file.config.xml");
            container.Kernel.AddFacility(CarbonIntegrationFacility.FACILITY_ID, new CarbonIntegrationFacility());

            _source_directory = @"c:\trash\incoming";
            _target_directory = @"c:\trash\outgoing";
            _error_directory = @"c:\trash\errors";

            this.CleanDirectory(_source_directory);
            this.CleanDirectory(_target_directory);
            this.CleanDirectory(_error_directory);

        }

        [Fact]
        public void can_read_a_file_from_the_source_directory_and_transfer_it_to_the_target_directory_via_integration_surface_definition()
        {
            // create some files in the source directory:
            var numberOfFiles = 5;
            this.PopulateDirectory(numberOfFiles, _source_directory);

            using (var context = container.Resolve<IIntegrationContext>())
            {
                // load our integration surface for processing messsages:
                context.LoadSurface(typeof (TestFileMoverSurface));

                context.Start();

                // see the verbal configuration of the surface (can do this only after the context is started):
                var surface = context.GetComponent<TestFileMoverSurface>();
                Console.WriteLine(surface.Verbalize());

                var wait = new ManualResetEvent(false);
                wait.WaitOne(TimeSpan.FromSeconds(numberOfFiles * 2)); // give the adapters enough time to process...
                wait.Set();

                Assert.Equal(0, Directory.GetFiles(_source_directory).Length);
                Assert.Equal(numberOfFiles, Directory.GetFiles(_target_directory).Length);
            }

        }

        private void PopulateDirectory(int numberOfItems, string directory)
        {
            for(var index = 0; index < numberOfItems; index++)
            {
                var contents = Guid.NewGuid().ToString();
                var fileName = Path.Combine(directory, string.Concat(contents, ".txt"));

                using(var filestream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    var bytes = ASCIIEncoding.ASCII.GetBytes(contents);
                    filestream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private void CleanDirectory(string directory)
        {
            var files = Directory.GetFiles(directory);

            foreach(var file in files)
                new FileInfo(file).Delete();
        }

        
    }

    public class TestFileMoverSurface : AbstractIntegrationComponentSurface
    {
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public string ErrorDirectory { get; set; }

        public TestFileMoverSurface(IObjectBuilder builder)
            : base(builder)
        {
            this.Name = "Test Integration Surface for Moving Files";
            this.IsAvailable = true;

            /*
            // set these via the configuration file and let the surface pick these up via container for real usage:
            this.SourceDirectory = @"c:\trash\incoming"; 
            this.TargetDirectory = @"c:\trash\outgoing"; 
            this.ErrorDirectory = @"c:\trash\errors"; 
            */
        }

        public override void BuildReceivePorts()
        {
            // create ports for accepting messages:
            var uri = string.Format(@"file://{0}", SourceDirectory);
            CreateReceivePort(this.GetFileReceivePipeline(), "in", uri, 4, 1);
        }

        public override void BuildCollaborations()
        {
            // need component to actually handle the message when it is retrieved for processing
            // let's use the pass through component if no custom logic is to be applied:
            AddComponent<PassThroughComponentFor<string>>("in", "out");
        }

        public override void BuildSendPorts()
        {
            // create ports for sending messages:
            var uri = string.Format(@"file://{0}", TargetDirectory);
            CreateSendPort(this.GetFileSendPipeline(), "out", uri, 1, 1);
        }

        public override void BuildErrorPort()
        {
            // create an error handling port where all messages will be sent:
            var uri = string.Format(@"file://{0}", ErrorDirectory);
            var config = new ErrorOutputPortConfiguration("error", uri);
            CreateErrorPort(null, config);
        }

        private AbstractReceivePipeline GetFileReceivePipeline()
        {
            // custom receive post-activities:
            var pipeline = new ReceivePipeline() { Name = "File Mover Receive Pipeline" };

            // change the byte[] representation of the message to a string
            // and make sure that no duplicate messages are received...
            pipeline.RegisterComponents(
                ObjectBuilder.Resolve<BytesToStringPipelineComponent>(),
                ObjectBuilder.Resolve<NonIdempotentPipelineComponent>()
                );

            return pipeline;
        }

        private AbstractSendPipeline GetFileSendPipeline()
        {
            return null; // not going to do any custom pre-send activities..
        }
    }
    
}
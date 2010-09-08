using System;
using System.Configuration;
using Carbon.Core.Builder;
using log4net;

namespace Carbon.Core.Adapter.Impl.Log
{
    /// <summary>
    /// This is the output channel adapter that is used to send message contents to a log location using log4Net. 
    /// 
    /// Uri scheme for this adapter:
    /// log://{logger name}/?level={debug, info, error, warn, fatal}
    /// </summary>
    public class LogOutputAdapter :AbstractOutputChannelAdapter
    {
        private ILog m_logger = null;
        private string m_log_level = "DEBUG";

        public LogOutputAdapter(IObjectBuilder builder) : base(builder)
        {
        }

        public override void DoStartActivities()
        {
            var loggerName = string.Empty;

            var location = ConfigurationManager.AppSettings["log4net"];

            try
            {
                if (string.IsNullOrEmpty(location.Trim()))
                    throw new Exception("'log4net' app setting key value not defined in configuration file.");
            }
            catch (Exception exception)
            {
                throw;
            }

            try
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(location));
            }
            catch (System.Threading.ThreadInterruptedException tex)
            {
                // ignore:
            }
            catch (Exception exception)
            {
                var msg = "An error has occurred while configuring log4net. Reason:" + exception.Message;
                throw new Exception(msg, exception);
            }

            try
            {
                var logUri = new Uri(this.Uri);
                loggerName = logUri.Host;
            }
            catch (Exception exception)
            {

            }

            var nameValuePairs = Utils.CreateNameValuePairsFromUri(this.Uri);

            m_logger = LogManager.GetLogger(typeof(LogOutputAdapter));

            if (!string.IsNullOrEmpty(loggerName))
            {
                try
                {
                    m_logger = LogManager.GetLogger(loggerName);
                }
                catch (Exception exception)
                {

                }
            }

            if (nameValuePairs.Count > 0)
                if (nameValuePairs.Get("level") != string.Empty)
                    m_log_level = nameValuePairs.Get("level");
        }

        public override void DoStopActivities()
        {
            m_logger = null;
        }

        public override void DoSend(IEnvelope envelope)
        {
            this.SendMessage(envelope);
        }

        private void SendMessage(IEnvelope message)
        {
            if (m_log_level.Trim().ToUpper() == "DEBUG")
                if (m_logger.IsDebugEnabled)
                    m_logger.Debug(message.Body.GetPayload<string>());

            if (m_log_level.Trim().ToUpper() == "INFO")
                if (m_logger.IsInfoEnabled)
                    m_logger.Info(message.Body.GetPayload<string>());

            if (m_log_level.Trim().ToUpper() == "WARN")
                if (m_logger.IsWarnEnabled)
                    m_logger.Warn(message.Body.GetPayload<string>());

            if (m_log_level.Trim().ToUpper() == "ERROR")
                if (m_logger.IsErrorEnabled)
                    m_logger.Error(message.Body.GetPayload<string>());

            if (m_log_level.Trim().ToUpper() == "FATAL")
                if (m_logger.IsFatalEnabled)
                    m_logger.Fatal(message.Body.GetPayload<string>());

        }
    }
}
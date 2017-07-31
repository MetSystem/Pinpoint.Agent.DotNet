namespace Pinpoint.Agent
{
    using Common;
    using Configuration;
    using Meta;
    using Network;
    using System;
    using System.Net;

    internal static class Environment
    {
        private const string agentVersion = "1.7.0-SNAPSHOT";

        public static void Init()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            var agentConfig = LoadAgentConfig();

            var pinpointConfig = LoadPinpointConfig();

            Logger.Init(agentConfig.ApplicationName);

            var tcpClient = new DefaultPinpointTcpClient(pinpointConfig.CollectorIp, pinpointConfig.TcpListenPort);

            #region register all components

            container.Register<AgentConfig>(agentConfig);

            container.Register<PinpointConfig>(pinpointConfig);

            container.Register<DefaultPinpointTcpClient>(tcpClient);

            container.Register<PinpointUdpClient>(
                new PinpointUdpClient(pinpointConfig.CollectorIp, pinpointConfig.UpdSpanListenPort));

            container.Register<DefaultApiMetaDataService>(
                 new DefaultApiMetaDataService(agentConfig.AgentId, agentConfig.AgentStartTime, tcpClient));

            container.Register<DefaultSqlMetaDataService>(
                 new DefaultSqlMetaDataService(agentConfig.AgentId, agentConfig.AgentStartTime, tcpClient));

            #endregion
        }

        private static AgentConfig LoadAgentConfig()
        {
            return new AgentConfig()
            {
                HostName = Dns.GetHostName(),
                AgentId = System.Environment.GetEnvironmentVariable("PINPOINT_AGENT_ID"),
                ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName(),
                AgentStartTime = TimeUtils.GetCurrentTimestamp(),
                AgentVersion = agentVersion
            };
        }

        private static PinpointConfig LoadPinpointConfig()
        {
            var pinpointHome = System.Environment.GetEnvironmentVariable("PINPOINT_HOME");
            var configs = ConfigManager.Load(pinpointHome.TrimEnd('\\') + "\\pinpoint.config");
            var pinpointConfig = new PinpointConfig();
            var val = String.Empty;
            configs.TryGetValue("profiler.collector.ip", out val);
            pinpointConfig.CollectorIp = val;
            configs.TryGetValue("profiler.collector.span.port", out val);
            pinpointConfig.UpdSpanListenPort = int.Parse(val);
            configs.TryGetValue("profiler.collector.stat.port", out val);
            pinpointConfig.UdpStatListenPort = int.Parse(val);
            configs.TryGetValue("profiler.collector.tcp.port", out val);
            pinpointConfig.TcpListenPort = int.Parse(val);

            return pinpointConfig;
        }
    }
}

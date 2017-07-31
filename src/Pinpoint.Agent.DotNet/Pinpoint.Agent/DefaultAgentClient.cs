namespace Pinpoint.Agent
{
    using Configuration;
    using Meta;
    using Thrift.Dto;
    using Thrift.IO;
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using TinyIoC;
    using Network;
    using Packet;
    using Pinpoint;

    public class DefaultAgentClient
    {
        private DefaultPinpointTcpClient tcpClient;

        private PinpointUdpClient spanUdpClient;

        private DefaultApiMetaDataService dataService;

        private DefaultSqlMetaDataService sqlDataService;

        private Thread sendAgentInfoThread;

        private Thread sendAgentStatInfoThread;

        private static Object locker = new Object();

        private static DefaultAgentClient instance = null;

        private static bool isStart = false;

        public bool IsStart
        {
            get
            {
                return isStart;
            }
        }

        private DefaultAgentClient(DefaultPinpointTcpClient tcpClient, PinpointUdpClient spanUdpClient,
            DefaultApiMetaDataService dataService, DefaultSqlMetaDataService sqlDataService)
        {
            this.tcpClient = tcpClient;
            this.spanUdpClient = spanUdpClient;
            this.dataService = dataService;
            this.sqlDataService = sqlDataService;
        }

        public static DefaultAgentClient GetInstance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        Environment.Init();

                        var tcpClient = TinyIoCContainer.Current.Resolve<DefaultPinpointTcpClient>();
                        var spanUdpClient = TinyIoCContainer.Current.Resolve<PinpointUdpClient>();
                        var dataService = TinyIoCContainer.Current.Resolve<DefaultApiMetaDataService>();
                        var sqlDataService = TinyIoCContainer.Current.Resolve<DefaultSqlMetaDataService>();

                        instance = new DefaultAgentClient(tcpClient, spanUdpClient,
                            dataService, sqlDataService);
                    }
                }
            }

            return instance;
        }

        public void Start()
        {
            lock (locker)
            {
                if (isStart)
                {
                    return;
                }

                new Thread(StartAgent).Start();
                isStart = true;

                Logger.Current.Info("Pinpoint Agent Started");
            }
        }

        public static void SendSpanData(TSpan span)
        {
            DefaultAgentClient.GetInstance().spanUdpClient.Send(span);
        }

        public static int CacheApi(MethodDescriptor methodDescriptor)
        {
            methodDescriptor.ApiId = methodDescriptor.ApiId = IdGenerator.SequenceId();
            return GetInstance().dataService.CacheApi(methodDescriptor);
        }
        public static int CacheSql(string sql)
        {
            var parseResult = GetInstance().sqlDataService.ParseSql(sql);
            parseResult.Id = IdGenerator.SequenceId();
            return GetInstance().sqlDataService.CacheSql(parseResult);
        }

        private void StartAgent()
        {
            try
            {
                HandShake();

                sendAgentInfoThread = new Thread(SendAgentInfo);
                sendAgentInfoThread.Start();

                sendAgentStatInfoThread = new Thread(SendAgentStatInfo);
                sendAgentStatInfoThread.Start();

            }
            catch (Exception ex)
            {
                Logger.Current.Error(ex.ToString());
            }
        }

        private void SendAgentInfo()
        {
            var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();
            while (true)
            {
                var agentInfo = new TAgentInfo
                {
                    AgentId = agentConfig.AgentId,
                    Hostname = agentConfig.HostName,
                    ApplicationName = agentConfig.ApplicationName,
                    AgentVersion = agentConfig.AgentVersion,
                    VmVersion = "1.8.0_121",
                    ServiceType = 1010,
                    StartTimestamp = agentConfig.AgentStartTime,
                    JvmInfo = new TJvmInfo()
                    {
                        Version = 0,
                        VmVersion = "1.8.0_121",
                        GcType = TJvmGcType.PARALLEL
                    }
                };

                try
                {
                    using (var serializer = new HeaderTBaseSerializer())
                    {
                        var payload = serializer.serialize(agentInfo);
                        var request = new RequestPacket(IdGenerator.SequenceId(), payload);
                        tcpClient.Send(request.ToBuffer());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Error(ex.ToString());
                }

                Thread.Sleep(5 * 60 * 1000);
            }
        }

        private void SendAgentStatInfo()
        {
            var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();
            var pinpointConfig = TinyIoCContainer.Current.Resolve<PinpointConfig>();
            while (true)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(pinpointConfig.CollectorIp), pinpointConfig.UdpStatListenPort);
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                #region assemble agent stat batch entity

                var agentStatBatch = new TAgentStatBatch();
                agentStatBatch.AgentId = agentConfig.AgentId;
                agentStatBatch.StartTimestamp = agentConfig.AgentStartTime;
                agentStatBatch.AgentStats = new List<TAgentStat>();

                #endregion

                #region assemble agent stat entity

                var agentStat = new TAgentStat();
                agentStat.AgentId = agentConfig.AgentId;
                agentStat.StartTimestamp = agentConfig.AgentStartTime;
                agentStat.Timestamp = TimeUtils.GetCurrentTimestamp();
                agentStat.CollectInterval = 5000;
                agentStat.Gc = new TJvmGc()
                {
                    Type = TJvmGcType.PARALLEL,
                    JvmMemoryHeapUsed = 73842768,
                    JvmMemoryHeapMax = 436207616,
                    JvmMemoryNonHeapUsed = 196555576,
                    JvmMemoryNonHeapMax = -1,
                    JvmGcOldCount = 5,
                    JvmGcOldTime = 945,
                    JvmGcDetailed = new TJvmGcDetailed()
                    {
                        JvmGcNewCount = 110,
                        JvmGcNewTime = 1666,
                        JvmPoolCodeCacheUsed = 0.22167689005533855,
                        JvmPoolNewGenUsed = 0.025880894190828566,
                        JvmPoolOldGenUsed = 0.20353155869704026,
                        JvmPoolSurvivorSpaceUsed = 0.4635740007672991,
                        JvmPoolMetaspaceUsed = 0.9706939329583961
                    }
                };
                agentStat.CpuLoad = new TCpuLoad()
                {
                    JvmCpuLoad = 0.002008032128514056,
                    SystemCpuLoad = AgentStat.GetCpuLoad()
                };
                agentStat.Transaction = new TTransaction()
                {
                    SampledNewCount = 0,
                    SampledContinuationCount = 0,
                    UnsampledContinuationCount = 0,
                    UnsampledNewCount = 0
                };
                agentStat.ActiveTrace = new TActiveTrace()
                {
                    Histogram = new TActiveTraceHistogram()
                    {
                        Version = 0,
                        HistogramSchemaType = 2,
                        ActiveTraceCount = new List<int>() { 0, 0, 0, 0 }
                    }
                };
                agentStat.DataSourceList = new TDataSourceList()
                {
                    DataSourceList = new List<TDataSource>()
                {
                    new TDataSource()
                    {
                        Id = 1,
                        DatabaseName = "test",
                        ServiceTypeCode = 6050,
                        Url = "jdbc:mysql://192.168.1.1:3306/test",
                        MaxConnectionSize = 8
                    }
                }
                };

                #endregion

                for (var i = 0; i < 6; i++)
                {
                    agentStat.Timestamp -= 5000;
                    agentStatBatch.AgentStats.Add(agentStat);
                }

                try
                {
                    using (var serializer = new HeaderTBaseSerializer())
                    {
                        var data = serializer.serialize(agentStatBatch);
                        server.SendTo(data, ip);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Error(ex.ToString());
                }

                Thread.Sleep(5 * 60 * 1000);
            }
        }

        private void HandShake()
        {
            var agentConfig = TinyIoCContainer.Current.Resolve<AgentConfig>();
            var handshakeData = new Dictionary<string, object>();
            handshakeData.Add("serviceType", 1010);
            handshakeData.Add("socketId", 1);
            handshakeData.Add("hostName", agentConfig.HostName);
            handshakeData.Add("agentId", agentConfig.AgentId);
            handshakeData.Add("supportCommandList", new List<int> { 730, 740, 750, 710 });
            handshakeData.Add("ip", "192.168.56.1");
            handshakeData.Add("pid", 6496);
            handshakeData.Add("supportServer", true);
            handshakeData.Add("version", agentConfig.AgentVersion);
            handshakeData.Add("applicationName", agentConfig.ApplicationName);
            handshakeData.Add("startTimestamp", agentConfig.AgentStartTime);
            var payload = new ControlMessageEncoder().EncodeMap(handshakeData);
            var helloPacket = new ControlHandshakePacket(IdGenerator.SequenceId(), payload);
            tcpClient.Send(helloPacket.ToBuffer());
        }
    }
}

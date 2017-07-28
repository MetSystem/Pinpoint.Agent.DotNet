namespace Pinpoint.Agent.Network
{
    using DotNet.Configuration;
    using global::Thrift.Protocol;
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Thrift.IO;
    using TinyIoC;

    public class PinpointUdpClient
    {
        private ConcurrentQueue<TBase> cachedQueue = null;
        private Timer flushMsgTimer = null;
        private ManualResetEvent flushMsgThreadSignal = null;
        private IPEndPoint ip = null;
        private Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public PinpointUdpClient()
        {
            var pinpointConfig = TinyIoCContainer.Current.Resolve<PinpointConfig>();
            ip = new IPEndPoint(IPAddress.Parse(pinpointConfig.CollectorIp), pinpointConfig.UpdSpanListenPort);

            cachedQueue = new ConcurrentQueue<TBase>();
            flushMsgTimer = new Timer(FlushMsg, null, 1000, 1000);
            flushMsgThreadSignal = new ManualResetEvent(true);
        }

        public void Send(TBase @base)
        {
            if (@base != null)
            {
                cachedQueue.Enqueue(@base);
            }
        }

        public void FlushMsg(object state)
        {
            if (!flushMsgThreadSignal.WaitOne(5))
            {
                return;
            }

            flushMsgThreadSignal.Reset();

            try
            {
                TBase msg = null;
                using (var serializer = new HeaderTBaseSerializer())
                {
                    while (cachedQueue.TryDequeue(out msg))
                    {
                        var data = serializer.serialize(msg);
                        server.SendTo(data, ip);
                    }
                }
            }
            catch(Exception ex)
            {
                Common.Logger.Current.Error(ex.ToString());
            }

            flushMsgThreadSignal.Set();
        }
    }
}

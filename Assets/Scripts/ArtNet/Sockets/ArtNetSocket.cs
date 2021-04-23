using ArtNet.IO;
using ArtNet.Packets;
using System;
using System.Net;
using System.Net.Sockets;

namespace ArtNet.Sockets
{
    public class ArtNetSocket : Socket
    {
        public const int Port = 6454;

        public event UnhandledExceptionEventHandler UnhandledException;
        public event EventHandler<NewPacketEventArgs<ArtNetPacket>> NewPacket;


        public ArtNetSocket()
            : base(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp)
        {            
        }

        #region Information

        private bool portOpen = false;

        public bool PortOpen
        {
            get { return portOpen; }
            set { portOpen = value; }
        }

        public IPAddress LocalIP { get; protected set; }

        public IPAddress LocalSubnetMask { get; protected set; }

        private static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public IPAddress BroadcastAddress
        {
            get
            {
                if (LocalSubnetMask == null || LocalIP == null)
                    return IPAddress.Broadcast;
                return GetBroadcastAddress(LocalIP, LocalSubnetMask);
            }
        }

        private DateTime? lastPacket = null;

        public DateTime? LastPacket
        {
            get { return lastPacket; }
            protected set { lastPacket = value; }
        }

        #endregion




        public void Open(IPAddress localIp, IPAddress localSubnetMask)
        {
            LocalIP = localIp;
            LocalSubnetMask = localSubnetMask;

            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Bind(new IPEndPoint(LocalIP, Port));
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            PortOpen = true;
            
            StartRecieve();
        }

        public void StartRecieve()
        {
            try
            {
                EndPoint localPort = new IPEndPoint(IPAddress.Any, Port);
                ArtNetRecieveData recieveState = new ArtNetRecieveData();
                BeginReceiveFrom(recieveState.buffer, 0, recieveState.bufferSize, SocketFlags.None, ref localPort, new AsyncCallback(OnRecieve), recieveState);
            }
            catch (Exception ex)
            {
                OnUnhandledException(new ApplicationException("An error ocurred while trying to start recieving ArtNet.", ex));
            }
        }

        private void OnRecieve(IAsyncResult state)
        {
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            if (PortOpen)
            {
                try
                {
                    ArtNetRecieveData recieveState = (ArtNetRecieveData)(state.AsyncState);

                    if (recieveState != null)
                    {
                        recieveState.DataLength = EndReceiveFrom(state, ref remoteEndPoint);

                        //Protect against UDP loopback where we recieve our own packets.
                        if (LocalEndPoint != remoteEndPoint && recieveState.Valid)
                        {
                            LastPacket = DateTime.Now;

                            ProcessPacket((IPEndPoint)remoteEndPoint, ArtNetPacket.Create(recieveState));
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnUnhandledException(ex);
                }
                finally
                {
                    //Attempt to recieve another packet.
                    StartRecieve();
                }
            }
        }

        private void ProcessPacket(IPEndPoint source, ArtNetPacket packet)
        {
            if (packet != null)
            {
                if (NewPacket != null)
                    NewPacket(this, new NewPacketEventArgs<ArtNetPacket>(source, packet));
            }
        }

        protected void OnUnhandledException(Exception ex)
        {
            if (UnhandledException != null) UnhandledException(this, new UnhandledExceptionEventArgs((object)ex, false));
        }

        #region Sending

        public void Send(ArtNetPacket packet)
        {
            Send(packet, new IPEndPoint(BroadcastAddress, Port));
        }

        public void Send(ArtNetPacket packet, IPEndPoint remote)
        {
            SendTo(packet.ToArray(), remote);
        }
      
        #endregion

        protected override void Dispose(bool disposing)
        {
            PortOpen = false;

            base.Dispose(disposing);
        }
    }
}

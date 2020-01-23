using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ResurrectionRP_Server.Utils.TopServer
{
    public class Receptor
    {
        private Action<string, string, string, string, string> _callback;

        public Receptor(Action<string, string, string, string, string> callback, int port)
        {
            this._callback = callback;
            UdpClient udpClient = new UdpClient(port);
            udpClient.BeginReceive(new AsyncCallback(this.OnReceptSocket), udpClient);
            AltV.Net.Alt.Server.LogColored($"[VotePlugin] Plugin de vote actif sur le port { port}");
        }

        private void OnReceptSocket(IAsyncResult result)
        {
            UdpClient udpClient = result.AsyncState as UdpClient;
            IPEndPoint ipendPoint = new IPEndPoint(0L, 0);
            byte[] bytes = udpClient.EndReceive(result, ref ipendPoint);
            string[] array = Encoding.UTF8.GetString(bytes).Split(new char[] { ',' });

            if (array.Length >= 5)
            {
                string arg = array[0];
                string arg2 = array[1];
                string arg3 = array[2];
                string arg4 = array[3];
                string arg5 = array[4];
                this._callback(arg, arg2, arg3, arg4, arg5);
            }
            else if (array.Length == 2 && array[1] == "test")
            {
                AltV.Net.Alt.Server.LogInfo("[VotePlugin] Test : Le plugin de vote est bien relié à Top-Serveurs !");
            }
            udpClient.BeginReceive(new AsyncCallback(this.OnReceptSocket), udpClient);
        }
    }
}

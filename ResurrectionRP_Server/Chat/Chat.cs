using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    static class Chat
    {
        public delegate void CmdCallback(IPlayer player, string[] args = null);

        static Dictionary<string, CmdCallback> _cmdHandlers = new Dictionary<string, CmdCallback>();

        #region Client events
        public static void Initialize()
        {
            Alt.OnClient("chatmessage", OnChatMessage);
            
        }
        #endregion

        #region Private static methods
        private static void InvokeCmd(IPlayer player, string cmd, string[] args)
        {
            if (!_cmdHandlers.ContainsKey(cmd))
                Send(player, $"{{FF0000}} Unknown command /{cmd}");
            else
                _cmdHandlers[cmd](player, args);
        }

        private static void OnChatMessage(IPlayer player, object[] args)
        {
            string msg = (string)args[0];

            if (msg.Length > 0 && msg[0] == '/')
            {
                msg = msg.Trim().Substring(1);

                if (msg.Length > 0)
                {
                    Alt.Log($"[Chat:cmd] {player.Name}: /{msg}");
                    string[] arguments = msg.Split(' ');
                    int cmdEnd = msg.IndexOf(' ');

                    if (cmdEnd == -1)
                        cmdEnd = msg.Length;

                    string cmd = msg.Substring(0, cmdEnd);
                    msg = msg.Substring(cmdEnd).TrimStart();

                    if (msg == string.Empty)
                        InvokeCmd(player, cmd, null);
                    else
                    {
                        arguments = msg.Split(' ');
                        InvokeCmd(player, cmd, arguments);
                    }
                }
            }
            else
            {
                msg = msg.Trim();

                if (msg.Length == 0)
                    return;

                Alt.Log($"[Chat:msg] {player.Name}: {msg}");
            }
        }
        #endregion

        #region Public static methods
        public static void Broadcast(string msg)
        {
            Alt.EmitAllClients("chatmessage", msg);
        }

        public static bool RegisterCmd(string cmd, CmdCallback callback)
        {
            if (_cmdHandlers.ContainsKey(cmd))
            {
                Alt.Log($"[Error]Failed to register command /{cmd}, already registered");
                return false;
            }

            _cmdHandlers.Add(cmd, callback);
            return true;
        }

        public static void Send(IPlayer player, string msg)
        {
            player.Emit("chatmessage", msg);
        }
        #endregion

        #region Debug messages
        public static void Debug(string msg)
        {
            Broadcast($"{{FF00FF}}[Debug] {msg}");
        }

        public static void Error(string msg)
        {
            Broadcast($"{{FF0000}}[Error] {msg}");
        }

        public static void Info(string msg)
        {
            Broadcast($"{{FFAB0F}}[Info] {msg}");
        }

        public static void Success(string msg)
        {
            Broadcast($"{{00FF00}}[Success] {msg}");
        }

        public static void Warning(string msg)
        {
            Broadcast($"{{FF8989}}[Warning] {msg}");
        }

        #endregion

        #region Extensions
        public static void SendChatMessage(this IPlayer player, string msg)
        { 
            Send(player, msg);
        }
        #endregion
    }
}

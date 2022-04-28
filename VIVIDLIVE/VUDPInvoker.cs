using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class VUDPInvoker : MonoBehaviour
{
    public static void Invoke(Action action)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("UDP Invoker");
            go.tag = "GameController";
            instance = go.AddComponent<VUDPInvoker>();
        }
        instance.actions.Add(action);
    }
    public static VUDPInvoker instance { get; private set; } List<Action> actions = new List<Action>();
    void Awake() => instance = this; void Update() { if (actions.Count == 0) return; for (int i = 0; i < actions.Count; i++) actions[i](); actions.Clear(); }
}

public class VUDPServer
{
    UdpClient handler; IPEndPoint EndPoint; Action<EndPoint, byte[]> ResponseCallback; public void Close() { handler.Close(); }
    public VUDPServer(int port = 8085, Action<EndPoint, byte[]> ResponseCallBack = null) { ResponseCallback = ResponseCallBack; EndPoint = new IPEndPoint(IPAddress.Any, port); handler = new UdpClient(EndPoint); handler.BeginReceive(OnReceived, null); }
    public void Send(IPEndPoint endPoint, byte[] msg) { handler.Send(msg, msg.Length, endPoint); }
    public void Send(string ip, int port, byte[] msg) { Send(new IPEndPoint(IPAddress.Parse(ip), port), msg); }
    void OnReceived(IAsyncResult ar) { byte[] data = handler.EndReceive(ar, ref EndPoint); handler.BeginReceive(OnReceived, null); VUDPInvoker.Invoke(() => ResponseCallback?.Invoke(EndPoint, data)); }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;


/// <summary>
/// デバイスからモーション入力を受け取るためのマネージャークラス
/// </summary>
public class DeviceInputManeger : MonoBehaviour {
    
    
    [Header("サーバー設定")]
    public string ServerAdress = "ws://127.0.0.1:8000"; 
    public string userId = "TESTUSER1";

    public string clientType = "Unity";

    private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
    public delegate void CleanedDataHandler(string data);
    public event CleanedDataHandler OnCleanedDataReceived;
    private WebSocket ws;

    void Start() {

        var endpoint = $"/store/strength/{userId}-{clientType}";
        var uri = ServerAdress + $"{endpoint}";
        Debug.Log("Start Request " + uri);
        ws = new WebSocket(uri);
        ws.Connect();

        ws.OnOpen += (sender, ev) => {

        };

        ws.OnMessage += (sender, ev) => {
            string cleanedData = ev.Data.Trim('"');
            cleanedData = cleanedData.Replace("\\\"", "\"");
            Debug.Log("Cleaned Data: " + cleanedData);
            OnCleanedDataReceived?.Invoke(cleanedData);
        };

        ws.OnError += (sender, e) => {
            Debug.Log("WebSocket Error Message: " + e.Message);
            ws.Close();
        };

        ws.OnClose += (sender, e) => {
            Debug.Log("WebSocket Close");
            ws.Close();
        };

    }

    public void Connect(string endpoint) {
        var uri = ServerAdress + $"{endpoint}";
        Debug.Log("Start Request " + uri);
        ws = new WebSocket(uri);
    }

    void OnApplicationQuit() {
        ws.Close();
        Debug.Log("WebSocket Connection Closed");
    }

}
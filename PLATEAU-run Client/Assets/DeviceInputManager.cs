using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;


namespace DeviceManager {
/// <summary>
/// デバイスからモーション入力を受け取るためのマネージャークラス
/// TODO:リファクタリング
/// </summary>
public class DeviceInputManager : MonoBehaviour {
    
    
    [Header("サーバー設定")]
    public string ServerAdressFile = "Assets/temp/ipconfig.txt";
    public string userId = "TESTUSER1";

    public string clientType = "Unity";

    private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
    public delegate void CleanedDataHandler(string data);
    public event CleanedDataHandler OnCleanedDataReceived;
    private WebSocket ws;

    void Start() {

        var ipconfig = File.ReadAllText(ServerAdressFile);
        var ServerAdress = "ws://" + ipconfig.Trim();

        var endpoint = $"/store/strength/{userId}-{clientType}";
        var uri = ServerAdress + $"{endpoint}";
        Debug.Log("Start Request " + uri);
        ws = new WebSocket(uri);
        ws.Connect();

        ws.OnOpen += (sender, ev) => {

        };

        ws.OnMessage += (sender, ev) => {
            //データの整形
            string cleanedData = ev.Data.Trim('"');
            cleanedData = cleanedData.Replace("\\\"", "\"");
            //Debug.Log("Cleaned Data: " + cleanedData);
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


    void OnApplicationQuit() {
        ws.Close();
        Debug.Log("WebSocket Connection Closed");
    }

}
}


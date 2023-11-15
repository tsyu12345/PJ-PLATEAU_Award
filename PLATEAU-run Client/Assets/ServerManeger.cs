using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;


/// <summary>
/// デバイスからモーション入力を受け取るためのマネージャークラス
/// </summary>
public class ServerManeger : MonoBehaviour {
    
    
    [Header("サーバー設定")]
    public string ServerAdress = "ws://ws://127.0.0.1:8000"; //いったんローカルホストで

    public string userId = "TESTUSER1";

    private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
    private WebSocket ws;

    void Start() {

        
        ws.OnOpen += (sender, ev) => {

        };

        ws.OnMessage += (sender, ev) => {

        };

        ws.OnError += (sender, e) => {
            Debug.Log("WebSocket Error Message: " + e.Message);
            ws.Close();
        };

        ws.OnClose += (sender, e) => {
            Debug.Log("WebSocket Close");
            ws.Close();
        };
        ws.Connect();
    }


    public void Connect(string endpoint) {
        var uri = ServerAdress + $"{endpoint}";
        Debug.Log("Start Request " + uri);
        ws = new WebSocket(uri);
    }

    public void Disconnet() {
        ws.Close();
    }

    /**
    public void GetData() {
        string cleanedData = e.Data.Trim('"');
        cleanedData = cleanedData.Replace("\\\"", "\"");
        Debug.Log("Cleaned Data: " + cleanedData);
        StepsData data = JsonConvert.DeserializeObject<StepsData>(cleanedData);
    }
    */

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;


public class StepsData {
    public string user_id { get; set; }
    public int steps { get; set; }
}

/***
* Websoketを使ってAppleWatchと通信するやつ
*/
public class Client : MonoBehaviour
{

    public string ServerAdress = "ws://127.0.0.1:8000";

    public string UserID = "TESTUSER1";
    public string ClientType = "unity";

    private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
    private WebSocket ws;

    private Vector3 StartPosition;

    void Start() {

        //初期位置を保存しておく
        StartPosition = transform.position;
        
        var uri = ServerAdress + $"/ws_steps/{UserID}-{ClientType}";
        Debug.Log("Start Request " + uri);
        ws = new WebSocket(uri);


        ws.OnOpen += (sender, e) => {
            Debug.Log("WebSocket Open");
            //JSON文字列を作成
            string json = "{\"user_id\": \"" + UserID + "\", \"client_type\": \"" + ClientType + "\"}";
            Debug.Log("Send Data: " + json);
            ws.Send(json);
        };

        ws.OnMessage += (sender, e) => {
            try {
                Debug.Log("WebSocket Message" + e.Data);
                // 外側のダブルクォートを取り除く
                string cleanedData = e.Data.Trim('"');
                cleanedData = cleanedData.Replace("\\\"", "\"");
                Debug.Log("Cleaned Data: " + cleanedData);
                StepsData data = JsonConvert.DeserializeObject<StepsData>(cleanedData);
                //NOTE:Unity APIはメインスレッド以外から呼び出すとエラーになるので、メインスレッドで実行するよう,キューに追加する
                _mainThreadActions.Enqueue(() => {
                    OnStepsReceived(data.steps);
                });
            } catch (System.Exception ex) {
                Debug.Log("Error: " + ex.Message);
            }
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

        string test = "{\"user_id\": \"TESTUSER1\", \"steps\": 100}";
        Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(test);
        Debug.Log("On Data Received: " + data["steps"]);
    }


    void Update() {
        while (_mainThreadActions.Count > 0) {
                var action = _mainThreadActions.Dequeue();
                action.Invoke();
        }
        //初期位置に戻す
        if (transform.position.z > -90) {
            transform.position = StartPosition;
        }
    }


    // ゲーム終了時に呼び出されるメソッド
    void OnApplicationQuit() {
        ws.Close();
        Debug.Log("WebSocket Connection Closed");
    }


    // コールバック関数
    private void OnStepsReceived(int steps) {
        Debug.Log("Received steps: " + steps);
        //オブジェクトを歩数分だけZ軸方向に移動させる
        //現在の位置＋歩数分だけ移動させる
        Vector3 pos = transform.position;
        pos.z += steps;
        transform.position = pos;
    }
}

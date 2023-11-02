using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;


/***
* Websoketを使ってAppleWatchと通信するやつ
*/
public class Client : MonoBehaviour
{

    public string ServerAdress = "ws://127.0.0.1:8000";

    public string UserID = "TESTUSER1";
    public string ClientType = "unity";
    private WebSocket ws;

    void Start() {
        
        var uri = ServerAdress + $"/ws_steps/{UserID}-{ClientType}";
        Debug.Log("Start Request " + uri);
        ws = new WebSocket(uri);


        ws.OnOpen += (sender, e) => {
            Debug.Log("WebSocket Open");
        };

        ws.OnMessage += (sender, e) => {
            Debug.Log("On Data Received: " + e.Data);
            OnStepsReceived(int.Parse(e.Data));
        };

        ws.OnError += (sender, e) => {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };

        ws.OnClose += (sender, e) => {
            Debug.Log("WebSocket Close");
        };

        ws.Connect();
    }


    void Update() {

    }


    // コールバック関数
    void OnStepsReceived(int steps) {
        Debug.Log("Received steps: " + steps);
        //オブジェクトを歩数分だけZ軸方向に移動させる
        //現在の位置＋歩数分だけ移動させる
        Vector3 pos = transform.position;
        pos.z += steps;
        transform.position = pos;
    }
}

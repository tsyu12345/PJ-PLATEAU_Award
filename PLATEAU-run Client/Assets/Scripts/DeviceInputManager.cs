using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using Newtonsoft.Json;



namespace DeviceManager {
    /// <summary>
    /// デバイスからモーション入力を受け取るためのマネージャークラス
    /// TODO:呼び出し側から汎用的に使えるようにリファクタリング
    /// </summary>
    public class DeviceInputManager : MonoBehaviour {
        
        [Header("サーバー設定")]
        public string APIServer = $"https://plateau-walk-and-run-apiserver.onrender.com";
        public string APIWS = "ws://192.168.11.48:8000";
        public string clientType = "Unity";
        public string userId = "TESTUSER1";
        public string nickname = "UnityUser";
        public delegate void CleanedDataHandler(string data);
        public event CleanedDataHandler OnCleanedDataReceived;
        private static readonly Queue<Action> _mainThreadActions = new Queue<Action>();
        private WebSocket ws;

        void Start() {
            
            //egisterUser(nickname);
            Connect();
        }

        void Update() {
            /*
            while (_mainThreadActions.Count > 0) {
                var action = _mainThreadActions.Dequeue();
                if(action != null) { 
                    action.Invoke();
                }
            }
            */
        }

        void OnApplicationQuit() {
            DisConnect();
        }

        public void AddEventListener(CleanedDataHandler listener) {
            OnCleanedDataReceived += listener;
        }

        // イベントリスナーを削除するメソッド
        public void RemoveEventListener(CleanedDataHandler listener) {
            OnCleanedDataReceived -= listener;
        }


        public void DisConnect() {
            ws.Close();
        }

        public void RegisterUser(string NickName) {
            var endPoint = $"/register/user/{NickName}";
            StartCoroutine(GetRequest(APIServer + endPoint, (response) => {
                var responseBody = JsonConvert.DeserializeObject<User>(response);
                userId = responseBody.uuid;
                Debug.Log($"userId:{userId}");
                Connect();
            }));
        }

        IEnumerator GetRequest(string uri, Action<string> callback) {
            using (var req = UnityWebRequest.Get(uri)) {
            yield return req.SendWebRequest();
                if (req.isNetworkError) {
                    Debug.LogError(req.error);
                } else if (req.isHttpError) {
                    Debug.LogError(req.error);
                } else {
                    Debug.Log(req);
                    callback(req.downloadHandler.text);
                } 
            }
        }


        private void Connect() {
            var endpoint = $"/store/strength/{clientType}-{userId}";
            var ip = GetServerIP();
            ws = new WebSocket(ip + endpoint);
            ws.Connect();
            ws.OnOpen += (sender, ev) => {

            };

            ws.OnMessage += (sender, ev) => {
                try {
                    //データの整形
                    string cleanedData = ev.Data.Trim('"');
                    cleanedData = cleanedData.Replace("\\\"", "\"");
                
                   // メインスレッドで実行するアクションをキューに追加
                    //_mainThreadActions.Enqueue(() => {
                    OnCleanedDataReceived?.Invoke(cleanedData);
                    //});
                } catch (Exception e) {
                    Debug.LogError($"Error: {e}");
                }

            };

            ws.OnError += (sender, e) => {
                Debug.Log("WebSocket Error Message: " + e.Message);
            };

            ws.OnClose += (sender, e) => {
                Debug.Log("WebSocket Close");
            };
        }


        /// <summary>
        /// temp/ipconfig.txtからipアドレスを取得する
        /// </summary>
        private string GetServerIP() {
            var ipconfig = File.ReadAllText("Assets/temp/ipconfig.txt");
            var ServerAddress = "ws://" + ipconfig.Trim();
            return ServerAddress;
        }
    }

    [System.Serializable]
    public class User {
        public string nickname;
        public string uuid;
    }
}


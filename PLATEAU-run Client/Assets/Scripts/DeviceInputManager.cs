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
        public string APIServer = "https://plateau-walk-and-run-apiserver.onrender.com"
        public string clientType = "Unity";
        public string userId = "UnityUser";
        public string nickname = "UnityUser";
        public delegate void CleanedDataHandler(string data);
        public event CleanedDataHandler OnCleanedDataReceived;
        private static readonly Queue<Action> _mainThreadActions = new Queue<Action>();
        private WebSocket ws;

        void Start() {
            
            RegisterUser(nickname);
            ws.OnOpen += (sender, ev) => {

            };

            ws.OnMessage += (sender, ev) => {
                try {
                    //データの整形
                    string cleanedData = ev.Data.Trim('"');
                    cleanedData = cleanedData.Replace("\\\"", "\"");
                
                   // メインスレッドで実行するアクションをキューに追加
                    _mainThreadActions.Enqueue(() => {
                        OnCleanedDataReceived?.Invoke(cleanedData);
                    });
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

        void Update() {
            while (_mainThreadActions.Count > 0) {
                var action = _mainThreadActions.Dequeue();
                if(action != null) { 
                    action.Invoke();
                }
            }
        }

        void OnApplicationQuit() {
            DisConnect();
        }


        public void DisConnect() {
            ws.Close();
        }

        public void RegisterUser(string NickName) {
            var endPoint = $"/register/user/{NickName}";
            StartCoroutine(PostRequest(APIServer + endPoint, (response) => {
                var responseBody = JsonConvert.DeserializeObject<User>(response);
                userId = responseBody.uuid;
                Connect();
            }));
        }

        IEnumerator PostRequest(string uri, Action<string> callback) {
            var request = new UnityWebRequest(uri, "POST");
            
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) {
                Debug.LogError(request.error);
            } else {
                callback(request.downloadHandler.text);
            }
        }


        private void Connect() {
            var endpoint = $"/store/strength/{userId}-{clientType}";
            ws = new WebSocket(APIServer + endpoint);
            ws.Connect();
        }
    }

    [System.Serializable]
    public class User {
        public string nickname;
        public string uuid;
    }
}


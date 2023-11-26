using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class AvatorNameDisplayer : MonoBehaviourPunCallbacks {
    
    void Start() {
        var nameLabel = GetComponent<TextMeshPro>();
        // プレイヤー名とプレイヤーIDを表示する
        nameLabel.text = $"{photonView.Owner.NickName}({photonView.OwnerActorNr})";
    }

    
    void Update() {
        
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Loading : MonoBehaviour {

    public TextMeshProUGUI uiText;
    private float dotTimer = 0.0f;
    private int dotCount = 0;

    void Update() {
        UpdateWaitingText();
    }

    private void UpdateWaitingText() {
        dotTimer += Time.deltaTime;

        if (dotTimer >= 1.0f) {
            dotTimer = 0f;
            dotCount++;
            if (dotCount > 3) {
                dotCount = 0;
            }

            uiText.text = uiText.text + " " + new string('.', dotCount);
        }
    }


}
    

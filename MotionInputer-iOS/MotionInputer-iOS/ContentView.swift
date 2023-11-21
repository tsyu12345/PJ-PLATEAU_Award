import Foundation
import SwiftUI
import CoreMotion

struct ContentView: View {
    @State private var motionStrength = 0.0
    @State private var isMonitoring = false
    private var motionDetector = MotionDetector()
    //TODO:本実装用に設定ファイルから読み込めること
    private var webSocketManager = WebSocketManager(baseURL: URL(string: "ws://127.0.0.1:8000")!)
    private let userId = "TESTUSER1" // ここに適切なユーザーIDを設定

    var body: some View {
        VStack {
            ZStack {
                Rectangle()
                    .frame(width: 250, height: 150)
                    .foregroundColor(colorForStrength(motionStrength))
                    .cornerRadius(30)
                Text(String(format: "%.2f", motionStrength))
                    .foregroundColor(.white)
                    .font(.title)
            }
            Button("Start Monitoring") {
                startMonitoring()
            }.buttonStyle(.borderedProminent)
            
            Button("Stop Monitoring") {
                stopMonitoring()
            }.buttonStyle(.bordered)
        }
        .padding()
    }

    private func startMonitoring() {
        self.isMonitoring = true
        let clientType = "iOS"
        webSocketManager.connect(endpoint: "/store/strength/\(userId)-\(clientType)")
        motionDetector.motionDetected = { strength in
            guard self.isMonitoring else { return }
            DispatchQueue.main.async {
                self.motionStrength = floor(strength)
                self.pushDataForServer(userId: self.userId, strength: Float(strength), clientType: "iOS")
            }
        }
        motionDetector.startMonitoring()
    }


    private func stopMonitoring() {
        self.isMonitoring = false
        motionDetector.stopMonitoring()
        webSocketManager.disconnect()
        DispatchQueue.main.async {
            self.motionStrength = 0.0 // モーション強度をリセット
        }
    }
    
    private func pushDataForServer(userId: String, strength: Float, clientType: String) {
        let endpoint = "/store/strength/\(userId)"
        let data: [String: Any] = ["user_id": userId, "strength": strength, "clientType": clientType]
        webSocketManager.request(endpoint: endpoint, data: data)
    }
    
    private func colorForStrength(_ strength: Double) -> Color {
            // 振りの強さに基づいて色を変更
            let normalizedStrength = min(max(strength, 0), 1) // 0から1の範囲に正規化
            return Color(red: normalizedStrength, green: 0, blue: 1 - normalizedStrength)
    }
    
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}

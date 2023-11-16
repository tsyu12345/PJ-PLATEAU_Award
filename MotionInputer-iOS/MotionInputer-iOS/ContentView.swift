import Foundation
import SwiftUI
import CoreMotion

struct ContentView: View {
    @State private var motionStrength = 0.0
    private var motionDetector = MotionDetector()
    //TODO:本実装用に設定ファイルから読み込めること
    private var webSocketManager = WebSocketManager(baseURL: URL(string: "ws://127.0.0.1:8000")!)
    private let userId = "TESTUSER1" // ここに適切なユーザーIDを設定

    var body: some View {
        VStack {
            Text("Motion Strength: \(motionStrength)")
            Button("Start Monitoring") {
                startMonitoring()
            }
            Button("Stop Monitoring") {
                stopMonitoring()
            }
        }
        .padding()
    }

    private func startMonitoring() {
        webSocketManager.connect(endpoint: "/store/strength/\(userId)")
        motionDetector.motionDetected = { strength in
            DispatchQueue.main.async {
                self.motionStrength = strength
                self.pushDataForServer(userId: self.userId, strength: Float(strength), clientType: "iOS")
            }
        }
        motionDetector.startMonitoring()
    }

    private func stopMonitoring() {
        motionDetector.stopMonitoring()
        webSocketManager.disconnect()
    }
    
    private func pushDataForServer(userId: String, strength: Float, clientType: String) {
        let endpoint = "/store/strength/\(userId)"
        let data: [String: Any] = ["user_id": userId, "strength": strength, "clientType": clientType]
        webSocketManager.request(endpoint: endpoint, data: data)
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}

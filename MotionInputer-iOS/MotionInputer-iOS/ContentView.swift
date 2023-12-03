import Foundation
import SwiftUI
import CoreMotion

struct ContentView: View {
    @State private var motionStrength = 0.0
    @State private var isMonitoring = false
    @State private var ipAddress = "127.0.0.1" // IPアドレスの初期値
    @State private var userId = "TESTUSER1" // ここに適切なユーザーIDを設定
    @State private var previousIPAddresses: [String] = UserDefaults.standard.stringArray(forKey: "previousIPAddresses") ?? []
    @State private var showAlert = false // エラー警告を表示するための状態
    @State private var alertMessage = "" // エラーメッセージを格納するための状態
    @StateObject private var motionDetector = MotionDetector()
    @StateObject private var webSocketManager = WebSocketManager()

    

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
            
            // 以前のIPアドレスを選択するためのUI
            Menu("Select Previous IP") {
                ForEach(previousIPAddresses, id: \.self) { address in
                    Button(address) {
                        ipAddress = address
                    }
                }
            }
            
            
            TextField("Enter Server IP Address", text: $ipAddress)
                 .textFieldStyle(RoundedBorderTextFieldStyle())
                 .multilineTextAlignment(.center) // テキストを中央寄せに設定
                 .padding()
            TextField("Enter Your UserID", text: $userId)
                 .textFieldStyle(RoundedBorderTextFieldStyle())
                 .multilineTextAlignment(.center) // テキストを中央寄せに設定
                 .padding()
            
            Button("Start Monitoring") {
                startMonitoring()
                if !previousIPAddresses.contains(ipAddress) {
                    previousIPAddresses.append(ipAddress)
                    UserDefaults.standard.set(previousIPAddresses, forKey: "previousIPAddresses")
                }
            }.buttonStyle(.borderedProminent)
            
            Button("Stop Monitoring") {
                stopMonitoring()
            }.buttonStyle(.bordered)
        }
        .alert(isPresented: $showAlert) {
            Alert(
                title: Text("Connection Error"),
                message: Text(alertMessage),
                primaryButton: .default(Text("Retry"), action: {
                    startMonitoring()
                }),
                secondaryButton: .cancel(Text("Cancel")) {
                    showAlert = false
                    stopMonitoring()
                }
            )
        }
        .onAppear {
            webSocketManager.onError = { error in
                DispatchQueue.main.async {
                    alertMessage = error.localizedDescription
                    showAlert = true
                }
            }
        }
        .padding()
    }

    private func startMonitoring() {
        self.isMonitoring = true
        let clientType = "iOS"
        // IPアドレスを使用してWebSocketManagerを初期化
        if let url = URL(string: "ws://\(ipAddress)") {
            webSocketManager.setBaseURL(url)
        }
        webSocketManager.connect(endpoint: "/store/strength/\(userId)-\(clientType)")
        motionDetector.motionDetected = { strength in
            guard self.isMonitoring else { return }
            DispatchQueue.main.async {
                var adjustedStrength = strength
                // strengthが1.0未満の場合は、0.00とする
                if adjustedStrength < 1.0 {
                    adjustedStrength = 0.0
                }
                self.motionStrength = adjustedStrength
                self.pushDataForServer(userId: self.userId, strength: Float(adjustedStrength), clientType: "iOS")
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

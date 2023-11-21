import Foundation
import CoreMotion

class MotionDetector {
    private let motionManager = CMMotionManager()
    var motionDetected: ((Double) -> Void)?

    init() {
        motionManager.accelerometerUpdateInterval = 0.1
        motionManager.gyroUpdateInterval = 0.1
    }

    func startMonitoring() {
        #if targetEnvironment(simulator)
        // シミュレーターの場合、乱数を使用してモーションデータをシミュレート
        simulateMotionData()
        #else
        // 実際のデバイスでの処理
        guard motionManager.isAccelerometerAvailable && motionManager.isGyroAvailable else {
            print("Accelerometer and/or Gyroscope not available.")
            return
        }

        motionManager.startAccelerometerUpdates(to: OperationQueue.current!) { [weak self] (data, error) in
            guard error == nil, let acceleration = data?.acceleration else { return }
            let strength = sqrt(acceleration.x * acceleration.x + acceleration.y * acceleration.y + acceleration.z * acceleration.z)
            self?.motionDetected?(strength)
        }
        #endif
    }

    func stopMonitoring() {
        motionManager.stopAccelerometerUpdates()
    }

    #if targetEnvironment(simulator)
    // シミュレーター用のモーションデータシミュレーション
    private func simulateMotionData() {
        Timer.scheduledTimer(withTimeInterval: 0.1, repeats: true) { [weak self] _ in
            let simulatedStrength = Double.random(in: 0...3) // 乱数を生成
            self?.motionDetected?(simulatedStrength)
        }
    }
    #endif
}

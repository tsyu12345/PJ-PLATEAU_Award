//
//  MotionDetector.swift
//  MotionInputer-iOS
//
//  Created by 高林秀 on 2023/11/14.
//

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
        guard motionManager.isAccelerometerAvailable && motionManager.isGyroAvailable else {
            print("Accelerometer and/or Gyroscope not available.")
            return
        }

        motionManager.startAccelerometerUpdates(to: OperationQueue.current!) { [weak self] (data, error) in
            guard error == nil, let acceleration = data?.acceleration else { return }
            let strength = sqrt(acceleration.x * acceleration.x + acceleration.y * acceleration.y + acceleration.z * acceleration.z)
            self?.motionDetected?(strength)
        }
    }

    func stopMonitoring() {
        motionManager.stopAccelerometerUpdates()
    }
}

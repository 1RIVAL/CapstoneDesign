import cv2
import mediapipe as mp
import numpy as np
import socket
import struct

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose

cap = cv2.VideoCapture(0)
cap.set(3, 1280)
cap.set(4, 720)

# 네트워크 설정 (TCP)
tcp_server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
tcp_server_socket.bind(("0.0.0.0", 8080))
tcp_server_socket.listen(0)
print("TCP port 8080...")

# UDP 클라이언트 설정
udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_serverAddressPort = ("127.0.0.1", 5052)
print("UDP port 5052...")

# 계산 변수
counter = 0 # 수행 횟수
stage = None # DOWN or UP

# 클라이언트 연결 대기
tcp_client_socket, tcp_client_address = tcp_server_socket.accept()
print(f"TCP Connection from: {tcp_client_address}")

# 각도 구하기
def calculate_angle(a,b,c):
    a = np.array(a) # First
    b = np.array(b) # Mid
    c = np.array(c) # End

    radians = np.arctan2(c[1]-b[1], c[0]-b[0]) - np.arctan2(a[1]-b[1], a[0]-b[0])
    angle = np.abs(radians * 180.0 / np.pi)

    if angle > 180.0:
        angle = 360 - angle
    return angle

# Setup mediapipe instance
with mp_pose.Pose(min_detection_confidence=0.5,min_tracking_confidence=0.5) as pose:

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break

        # Recolor image to RGB
        image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        image.flags.writeable = False

        # Make detection
        results = pose.process(image)

        # Recolor back to BGR
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

        # Extract Landmarks
        try:
            if results.pose_landmarks:
                landmarks = results.pose_landmarks.landmark

            # Get coordinates
            shoulder = [landmarks[mp_pose.PoseLandmark.LEFT_SHOULDER.value].x,landmarks[mp_pose.PoseLandmark.LEFT_SHOULDER.value].y]
            elbow = [landmarks[mp_pose.PoseLandmark.LEFT_ELBOW.value].x,landmarks[mp_pose.PoseLandmark.LEFT_ELBOW.value].y]
            wrist = [landmarks[mp_pose.PoseLandmark.LEFT_WRIST.value].x,landmarks[mp_pose.PoseLandmark.LEFT_WRIST.value].y]

            # Calculate angle
            angle = calculate_angle(shoulder, elbow, wrist)

            # Visualize angle
            cv2.putText(image, str(angle),
                        tuple(np.multiply(elbow, [640, 480]).astype(int)),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 2, cv2.LINE_AA)

            # Curl counter logic
            if angle > 160 :
                stage = "down"
            if angle < 30 and stage == "down":
                stage = "up"
                counter += 1
                print(counter)

                # 랜드마크 값들을 UDP 프로토콜을 사용하여 Unity에 보냄.
                udp_socket.sendto(str.encode(str(counter)), udp_serverAddressPort)


        except Exception as e:
            print("Pose landmarks error:", e)
            pass

        # Render curl counter
        # Setup status box
        cv2.rectangle(image, (0,0), (225, 73), (245,117,16), -1)

        # Rep data
        cv2.putText(image, 'count', (15,12),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0,0,0), 1, cv2.LINE_AA)
        cv2.putText(image, str(counter),
                    (10, 60),
                    cv2.FONT_HERSHEY_SIMPLEX, 2, (255,255,255), 2, cv2.LINE_AA)

        # Rep data
        cv2.putText(image, 'stage', (65,12),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0,0,0), 1, cv2.LINE_AA)
        cv2.putText(image, stage,
                    (60, 60),
                    cv2.FONT_HERSHEY_SIMPLEX, 2, (255,255,255), 2, cv2.LINE_AA)

        # Render detection
        mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_pose.POSE_CONNECTIONS,
                                  mp_drawing.DrawingSpec(color=(245,117,66),thickness=2,circle_radius=2),
                                  mp_drawing.DrawingSpec(color=(245,66,230),thickness=2,circle_radius=2))



        # 이미지를 JPEG로 인코딩
        _, buffer = cv2.imencode('.jpg', image)
        frame_data = buffer.tobytes()

        # 데이터 패킷 생성
        packet = struct.pack("Q", len(frame_data)) + frame_data

        # 클라이언트로 데이터 전송
        tcp_client_socket.sendall(packet)

        # cv2.imshow('Mediapipe Feed', image) #시각화

        if cv2.waitKey(10) & 0xFF == ord('q'):
            break

cap.release()
tcp_client_socket.close()
tcp_server_socket.close()
udp_socket.close()
cv2.destroyAllWindows()





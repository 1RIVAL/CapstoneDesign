import cv2
import socket
import struct
import numpy as np
from cvzone.PoseModule import PoseDetector

# Parameters
width, height = 1280, 720

# 웹캠 설정
cap = cv2.VideoCapture(0)
cap.set(3, width)
cap.set(4, height)

# 네트워크 설정 (TCP)
tcp_server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
tcp_server_socket.bind(("0.0.0.0", 8080))
tcp_server_socket.listen(0)
print("TCP port 8080...")

# UDP 클라이언트 설정
udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp_serverAddressPort = ("127.0.0.1", 5052)
print("UDP port 5052...")

# 포즈 감지 설정
detector = PoseDetector(staticMode=False,
                        modelComplexity=1,
                        smoothLandmarks=True,
                        enableSegmentation=False,
                        smoothSegmentation=True,
                        detectionCon=0.5,
                        trackCon=0.5)

# 각도 구하기 함수
def calculate_angle(a, b, c):
    a = np.array(a) # First
    b = np.array(b) # Mid
    c = np.array(c) # End

    radians = np.arctan2(c[1]-b[1], c[0]-b[0]) - np.arctan2(a[1]-b[1], a[0]-b[0])
    angle = np.abs(radians * 180.0 / np.pi)

    if angle > 180.0:
        angle = 360.0 - angle
    return angle

# 클라이언트 연결 대기
tcp_client_socket, tcp_client_address = tcp_server_socket.accept()
print(f"TCP Connection from: {tcp_client_address}")

# 계산 변수
counter = 0 # 수행 횟수
stage = None # DOWN or UP

try:
    while cap.isOpened():
        # 웹캠에서 프레임 가져오기
        ret, frame = cap.read()
        if not ret:
            break

        # 포즈 감지 및 프레임 업데이트
        img = detector.findPose(frame)
        lmList, bboxInfo = detector.findPosition(img, draw=False, bboxWithHands=False)

        # LEFT_SHOULDER, LEFT_ELBOW, LEFT_WRIST의 x, y 좌표 추출
        if lmList:
            left_shoulder = lmList[11][1:3] # LEFT_SHOULDER
            left_elbow = lmList[13][1:3]    # LEFT_ELBOW
            left_wrist = lmList[15][1:3]    # LEFT_WRIST

            # 각도 계산
            angle = calculate_angle(left_shoulder, left_elbow, left_wrist)

            # 각도에 따라 DOWN 또는 UP 상태 설정
            if angle > 160:
                stage = "down"
            if angle < 30 and stage == "down":
                stage = "up"
                counter += 1
                print(counter)

            # UDP를 통해 counter 값 전송
            udp_socket.sendto(str(counter).encode(), udp_serverAddressPort)

        # Render curl counter
        # Setup status box
        cv2.rectangle(img, (0, 0), (225, 73), (245, 117, 16), -1)
        # Rep data
        cv2.putText(img, 'count', (15, 12),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1, cv2.LINE_AA)
        cv2.putText(img, str(counter),
                    (10, 60),
                    cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2, cv2.LINE_AA)
        # Rep data
        cv2.putText(img, 'stage', (65, 12),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 0), 1, cv2.LINE_AA)
        cv2.putText(img, stage,
                    (60, 60),
                    cv2.FONT_HERSHEY_SIMPLEX, 2, (255, 255, 255), 2, cv2.LINE_AA)

        # 이미지를 JPEG로 인코딩
        _, buffer = cv2.imencode('.jpg', img)
        frame_data = buffer.tobytes()

        # 데이터 패킷 생성
        packet = struct.pack("Q", len(frame_data)) + frame_data

        # 클라이언트로 데이터 전송
        tcp_client_socket.sendall(packet)


        if cv2.waitKey(1) == ord("q"):  # q 누를 시 웹캠 종료
            break

except Exception as e:
    print("Error:", e)

finally:
    # 자원 해제
    cap.release()
    tcp_client_socket.close()
    tcp_server_socket.close()
    udp_socket.close()
    cv2.destroyAllWindows()

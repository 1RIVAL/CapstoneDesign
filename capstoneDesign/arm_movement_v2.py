# 팔 각도 계산 후 카운트 추가

import cv2
import socket
import pickle
import struct
from cvzone.PoseModule import PoseDetector

# Parameters
width, height = 1280, 720

# 웹캠 설정
cap = cv2.VideoCapture(0)
cap.set(3, width)
cap.set(4, height)

# 네트워크 설정
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(("0.0.0.0", 8080))
server_socket.listen(0)
print("Listening on port 8080...")

# UDP 클라이언트 설정
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)

# 포즈 감지 설정
detector = PoseDetector(staticMode=False,
                        modelComplexity=1,
                        smoothLandmarks=True,
                        enableSegmentation=False,
                        smoothSegmentation=True,
                        detectionCon=0.5,
                        trackCon=0.5)

# 클라이언트 연결 대기
client_socket, client_address = server_socket.accept()
print(f"Connection from: {client_address}")

try:
    while True:
        # 웹캠에서 프레임 가져오기
        ret, frame = cap.read()
        if not ret:
            break

        # 포즈 감지 및 프레임 업데이트
        img = detector.findPose(frame)
        lmList, bboxInfo = detector.findPosition(img, draw=True, bboxWithHands=False)

        # 이미지를 JPEG로 인코딩
        _, buffer = cv2.imencode('.jpg', img)
        frame_data = buffer.tobytes()

        # 데이터 패킷 생성
        packet = struct.pack("Q", len(frame_data)) + frame_data

        # 클라이언트로 데이터 전송
        client_socket.sendall(packet)

        data = []

        # 랜드마크 값들을 UDP 프로토콜을 사용하여 Unity에 보냄.
        if lmList:
            for lm in lmList:
                data.extend([lm[0], height - lm[1], lm[2]])
            sock.sendto(str.encode(str(data)), serverAddressPort)

        # 이미지 화면에 표시
        frame = cv2.resize(img, (0, 0), None, 0.5, 0.5)
        cv2.imshow("Image", frame)

        if cv2.waitKey(1) == ord("q"):  # q 누를 시 웹캠 종료
            break

except Exception as e:
    print("Error:", e)

finally:
    # 자원 해제
    cap.release()
    client_socket.close()
    server_socket.close()

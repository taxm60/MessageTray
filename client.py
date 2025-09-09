import socket
import random

def send_message(message):
    host = "127.0.0.1"
    port = 9000
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((host, port))
        s.sendall(message.encode("utf-8"))

rand_num = random.randint(1,100)
send_message(f"Hello from Python! Rand={rand_num}")

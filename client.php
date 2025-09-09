<?php
$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
socket_connect($socket, "127.0.0.1", 9000);
$message = "Hello from PHP! Rand=" . rand(1,100);
socket_write($socket, $message, strlen($message));
socket_close($socket);

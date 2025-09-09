# TrayMessage

TrayMessage 是一個 **Windows 托盤常駐程式**，可以接收 TCP 訊息並顯示通知氣泡，支援訊息隊列、未讀訊息閃動提示，以及左鍵點擊顯示最近訊息列表。

## 功能特色

- Windows 10 / 11 托盤常駐
- TCP Server 接收訊息（預設 port: 9000）
- 氣泡通知 (Balloon Tip) 顯示訊息
- 訊息隊列，保留最近 10 則訊息
- 未讀訊息圖示閃動提醒
- 左鍵點擊圖示彈出訊息列表 (MessageForm)
- 訊息列表可複製到剪貼簿
- 記憶體管理，程式關閉即清空訊息，不使用資料庫或檔案

## 系統需求

- Windows 10 或 11
- .NET 6 或以上
- Visual Studio 2022 或以上 (可編譯專案)

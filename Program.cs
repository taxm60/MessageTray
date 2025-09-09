using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsApp1
{
    internal static class Program
    {
        static readonly Queue<string> messageQueue = new Queue<string>();
        static bool hasUnread = false;       // 是否有未讀訊息
        static bool iconState = false;       // 用於切換圖示閃動

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            NotifyIcon trayIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Information,
                Text = "訊息接收器",
                Visible = true
            };

            // 建立閃動 Timer
            System.Windows.Forms.Timer flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 500; // 500ms 閃一次
            flashTimer.Tick += (s, e) =>
            {
                if (hasUnread)
                {
                    // 交替閃動圖示
                    trayIcon.Icon = iconState ? System.Drawing.SystemIcons.Information : System.Drawing.SystemIcons.Warning;
                    iconState = !iconState;
                }
                else
                {
                    trayIcon.Icon = System.Drawing.SystemIcons.Information;
                }
            };
            flashTimer.Start();

            // 右鍵選單
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("退出", null, (s, e) => Application.Exit());
            trayIcon.ContextMenuStrip = menu;

            // 左鍵點擊顯示 MessageForm
            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    var form = new MessageForm(messageQueue);
                    form.Show();
                    form.BringToFront();

                    // 打開表示已讀訊息，停止閃動
                    hasUnread = false;
                }
            };

            // 啟動 TCP Server
            Thread tcpThread = new Thread(() => StartTcpServer(trayIcon))
            {
                IsBackground = true
            };
            tcpThread.Start();

            Application.Run();
        }

        static void StartTcpServer(NotifyIcon trayIcon)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();
            Console.WriteLine("TCP Server 啟動，等待訊息...");

            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleClient, new object[] { client, trayIcon });
                }
                catch { }
            }
        }

        static void HandleClient(object state)
        {
            var array = (object[])state;
            TcpClient client = (TcpClient)array[0];
            NotifyIcon trayIcon = (NotifyIcon)array[1];

            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            // 將訊息加入隊列，保持最多 10 則
            lock (messageQueue)
            {
                if (messageQueue.Count >= 10)
                    messageQueue.Dequeue();
                messageQueue.Enqueue(timestampedMessage);
            }

            // 顯示氣泡通知，建議顯示 15 秒
            trayIcon.BalloonTipTitle = "新訊息通知";
            trayIcon.BalloonTipText = message;
            trayIcon.ShowBalloonTip(15000);

            // 標記有未讀訊息，啟動閃動
            hasUnread = true;

            client.Close();
        }
    }
}

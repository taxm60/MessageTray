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
        static bool hasUnread = false;       // �O�_����Ū�T��
        static bool iconState = false;       // �Ω�����ϥܰ{��

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            NotifyIcon trayIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Information,
                Text = "�T��������",
                Visible = true
            };

            // �إ߰{�� Timer
            System.Windows.Forms.Timer flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 500; // 500ms �{�@��
            flashTimer.Tick += (s, e) =>
            {
                if (hasUnread)
                {
                    // ����{�ʹϥ�
                    trayIcon.Icon = iconState ? System.Drawing.SystemIcons.Information : System.Drawing.SystemIcons.Warning;
                    iconState = !iconState;
                }
                else
                {
                    trayIcon.Icon = System.Drawing.SystemIcons.Information;
                }
            };
            flashTimer.Start();

            // �k����
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("�h�X", null, (s, e) => Application.Exit());
            trayIcon.ContextMenuStrip = menu;

            // �����I����� MessageForm
            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    var form = new MessageForm(messageQueue);
                    form.Show();
                    form.BringToFront();

                    // ���}��ܤwŪ�T���A����{��
                    hasUnread = false;
                }
            };

            // �Ұ� TCP Server
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
            Console.WriteLine("TCP Server �ҰʡA���ݰT��...");

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

            // �N�T���[�J���C�A�O���̦h 10 �h
            lock (messageQueue)
            {
                if (messageQueue.Count >= 10)
                    messageQueue.Dequeue();
                messageQueue.Enqueue(timestampedMessage);
            }

            // ��ܮ�w�q���A��ĳ��� 15 ��
            trayIcon.BalloonTipTitle = "�s�T���q��";
            trayIcon.BalloonTipText = message;
            trayIcon.ShowBalloonTip(15000);

            // �аO����Ū�T���A�Ұʰ{��
            hasUnread = true;

            client.Close();
        }
    }
}

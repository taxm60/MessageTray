using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class MessageForm : Form
    {
        private readonly Queue<string> messageQueue;
        private ListBox listBox;

        public MessageForm(Queue<string> queue)
        {
            messageQueue = queue;
            InitUI();
            LoadMessages();
        }

        private void InitUI()
        {
            this.Text = "最近訊息";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10),
                HorizontalScrollbar = true
            };
            this.Controls.Add(listBox);

            Button copyBtn = new Button
            {
                Text = "複製全部",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            copyBtn.Click += (s, e) =>
            {
                string allText = string.Join(Environment.NewLine, listBox.Items.Cast<string>());
                Clipboard.SetText(allText);
                MessageBox.Show("已複製到剪貼簿");
            };
            this.Controls.Add(copyBtn);
        }

        private void LoadMessages()
        {
            listBox.Items.Clear();
            lock (messageQueue)
            {
                foreach (var msg in messageQueue)
                    listBox.Items.Add(msg);
            }
        }
    }
}

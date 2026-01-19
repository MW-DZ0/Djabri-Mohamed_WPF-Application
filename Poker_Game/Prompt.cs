using System.Drawing;
using System.Windows.Forms;

namespace Poker_Game
{
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                Text = caption,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label lbl = new Label() { Left = 10, Top = 10, Text = text };
            TextBox txt = new TextBox() { Left = 10, Top = 35, Width = 360 };
            Button btn = new Button() { Text = "OK", Left = 280, Width = 90, Top = 70 };

            btn.Click += (sender, e) => prompt.Close();

            prompt.Controls.Add(lbl);
            prompt.Controls.Add(txt);
            prompt.Controls.Add(btn);
            prompt.AcceptButton = btn;

            prompt.ShowDialog();
            return txt.Text;
        }
    }
}

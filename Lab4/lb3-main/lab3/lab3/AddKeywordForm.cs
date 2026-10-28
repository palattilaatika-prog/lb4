using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab3
{
    public class AddKeywordForm : Form
    {
        private readonly TextBox keywordTextBox;

        public string Keyword
        {
            get { return keywordTextBox.Text.Trim(); }
        }

        public AddKeywordForm()
        {
            Text = "Add keyword";
            ClientSize = new Size(360, 115);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            Label label = new Label();
            label.Text = "Keyword:";
            label.Left = 12;
            label.Top = 17;
            label.Width = 65;

            keywordTextBox = new TextBox();
            keywordTextBox.Left = 82;
            keywordTextBox.Top = 13;
            keywordTextBox.Width = 260;

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Left = 186;
            okButton.Top = 58;
            okButton.Width = 75;
            okButton.DialogResult = DialogResult.OK;
            okButton.Click += ValidateKeyword;

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Left = 267;
            cancelButton.Top = 58;
            cancelButton.Width = 75;
            cancelButton.DialogResult = DialogResult.Cancel;

            Controls.Add(label);
            Controls.Add(keywordTextBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        private void ValidateKeyword(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(keywordTextBox.Text))
            {
                MessageBox.Show("Enter a keyword first.", "Missing keyword",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }
    }
}

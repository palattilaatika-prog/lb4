using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3
{
    public partial class Form1 : Form
    {
        private readonly List<string> Blockedw = new List<string>
        {
            "facebook",
            "youtube",
            "instagram",
            "twitter",
            "reddit",
            "tiktok"
        };

        private readonly SQLiteHandler database = new SQLiteHandler();
        private MenuStrip databaseMenuStrip;

        public Form1()
        {
            InitializeComponent();

            Trace.Listeners.Add(new TextWriterTraceListener("events.txt"));
            Trace.AutoFlush = true;

            CreateLab4Menus();
            RefreshKeywordComboBox();

            // Pressing Enter in the address field should navigate, not navigate just by focusing it.
            toolStripTextBox1.KeyDown += ToolStripTextBox1_KeyDown;
            FormClosing += Form1_FormClosing;
        }

        private void CreateLab4Menus()
        {
            databaseMenuStrip = new MenuStrip();
            ToolStripMenuItem databaseMenu = new ToolStripMenuItem("Database");
            ToolStripMenuItem connectItem = new ToolStripMenuItem("Connect");
            ToolStripMenuItem disconnectItem = new ToolStripMenuItem("Disconnect");
            ToolStripMenuItem addKeywordItem = new ToolStripMenuItem("AddKeyword");
            ToolStripMenuItem viewKeywordsItem = new ToolStripMenuItem("ViewKeywords");
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("About");

            connectItem.Click += ConnectItem_Click;
            disconnectItem.Click += DisconnectItem_Click;
            addKeywordItem.Click += AddKeywordItem_Click;
            viewKeywordsItem.Click += ViewKeywordsItem_Click;
            aboutItem.Click += AboutItem_Click;

            databaseMenu.DropDownItems.Add(connectItem);
            databaseMenu.DropDownItems.Add(disconnectItem);
            databaseMenu.DropDownItems.Add(new ToolStripSeparator());
            databaseMenu.DropDownItems.Add(addKeywordItem);
            databaseMenu.DropDownItems.Add(viewKeywordsItem);
            helpMenu.DropDownItems.Add(aboutItem);
            databaseMenuStrip.Items.Add(databaseMenu);
            databaseMenuStrip.Items.Add(helpMenu);
            databaseMenuStrip.Dock = DockStyle.Top;

            MainMenuStrip = databaseMenuStrip;
            Controls.Add(databaseMenuStrip);
            Controls.SetChildIndex(databaseMenuStrip, 0);
        }

        private void ConnectItem_Click(object sender, EventArgs e)
        {
            try
            {
                database.ConnectToDb();
                RefreshKeywordComboBox();
                MessageBox.Show("Database connected. The Keywords table is ready.",
                    "Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to the database:" + ex.Message,
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisconnectItem_Click(object sender, EventArgs e)
        {
            database.DisconnectFromDb();
            RefreshKeywordComboBox();
            MessageBox.Show("Database disconnected.", "Database",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddKeywordItem_Click(object sender, EventArgs e)
        {
            if (!database.IsConnected)
            {
                MessageBox.Show("Choose Database > Connect first.", "Database not connected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (AddKeywordForm form = new AddKeywordForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string keyword = form.Keyword;
                    try
                    {
                        bool alreadyExists = database.GetAllKeywords().Any(
                            x => string.Equals(x, keyword, StringComparison.OrdinalIgnoreCase));

                        if (alreadyExists)
                        {
                            MessageBox.Show("This keyword already exists in the database.",
                                "Duplicate keyword", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        database.InsertKeyword(keyword);
                        RefreshKeywordComboBox();
                        MessageBox.Show("Keyword added.", "Database",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not add the keyword:" + ex.Message,
                            "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ViewKeywordsItem_Click(object sender, EventArgs e)
        {
            if (!database.IsConnected)
            {
                MessageBox.Show("Choose Database > Connect first.", "Database not connected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                List<string> keywords = database.GetAllKeywords();
                string message = keywords.Count == 0
                    ? "No keywords have been saved yet."
                    : string.Join(Environment.NewLine, keywords);
                MessageBox.Show(message, "Saved keywords", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                RefreshKeywordComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not read the keywords:" + ex.Message,
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AboutItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Web BrowserLab 3 + Lab 4ADO.NET and SQLite",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshKeywordComboBox()
        {
            toolStripComboBox1.Items.Clear();
            foreach (string keyword in Blockedw)
                toolStripComboBox1.Items.Add(keyword);

            if (database.IsConnected)
            {
                foreach (string keyword in database.GetAllKeywords())
                {
                    bool exists = toolStripComboBox1.Items.Cast<object>().Any(
                        item => string.Equals(item.ToString(), keyword, StringComparison.OrdinalIgnoreCase));
                    if (!exists)
                        toolStripComboBox1.Items.Add(keyword);
                }
            }
        }

        private void ToolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                NavigateToAddress();
            }
        }

        private async void NavigateToAddress()
        {
            string url = toolStripTextBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!url.Contains("://"))
                url = "https://" + url;

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                MessageBox.Show("Please enter a valid URL.", "Invalid address",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await LogEventAsync("User clicked Navigate/pressed Enter");
            bool blocked = await IsBlockedAsync(uri.ToString());
            if (blocked)
            {
                MessageBox.Show("This website is blocked.", "URL blocked",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            webBrowser1.Navigate(uri);
        }

        private Task<bool> IsBlockedAsync(string url)
        {
            bool blocked = Blockedw.Any(keyword =>
                url.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!blocked && database.IsConnected)
            {
                blocked = database.GetAllKeywords().Any(keyword =>
                    url.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return Task.FromResult(blocked);
        }

        private async Task LogEventAsync(string message)
        {
            await Task.Run(() =>
            {
                Trace.WriteLine(string.Format("{0:dd.MM.yyyy HH:mm:ss.fff} - {1}",
                    DateTime.Now, message));
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            database.DisconnectFromDb();
            Trace.Flush();
        }

        // Existing Lab 3 event handlers are retained.
        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            _ = LogEventAsync("User clicked Home");
            webBrowser1.GoHome();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            _ = LogEventAsync("User clicked Back");
            if (webBrowser1.CanGoBack)
                webBrowser1.GoBack();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            _ = LogEventAsync("User clicked Forward");
            if (webBrowser1.CanGoForward)
                webBrowser1.GoForward();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            NavigateToAddress();
        }

        private void toolStripTextBox1_Enter(object sender, EventArgs e)
        {
            // Navigation is handled by the Go button or Enter key.
        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string keyword = textBox1.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                MessageBox.Show("Enter a keyword first.");
                return;
            }

            bool exists = Blockedw.Any(x => string.Equals(x, keyword, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                Blockedw.Add(keyword);
                RefreshKeywordComboBox();
                MessageBox.Show("Keyword added to the Lab 3 blocked-word list.");
            }
            else
            {
                MessageBox.Show("This keyword already exists.");
            }

            textBox1.Clear();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
    }
}

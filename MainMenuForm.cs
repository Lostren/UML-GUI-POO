namespace Shoot_Out_Game_MOO_ICT
{
    public enum GameMode
    {
        Normal,
        Survival
    }

    internal sealed class MainMenuForm : Form
    {
        private readonly Panel mainPanel = new Panel();
        private readonly Panel settingsPanel = new Panel();
        private readonly RadioButton normalMode = new RadioButton();
        private readonly RadioButton survivalMode = new RadioButton();
        private GameMode selectedMode = GameMode.Normal;

        public MainMenuForm()
        {
            Text = "Zombie Shootout Game";
            ClientSize = new Size(924, 661);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(64, 64, 64);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            BuildMainPanel();
            BuildSettingsPanel();
            Controls.Add(settingsPanel);
            Controls.Add(mainPanel);
            ShowMainPanel(null, EventArgs.Empty);
        }

        private void BuildMainPanel()
        {
            mainPanel.Dock = DockStyle.Fill;

            Label title = CreateLabel("ZOMBIE SHOOTOUT", 28, FontStyle.Bold);
            title.Dock = DockStyle.Top;
            title.Height = 100;
            title.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(title);

            TableLayoutPanel buttons = CreateButtonLayout();
            buttons.Controls.Add(CreateButton("Iniciar", StartGame), 0, 0);
            buttons.Controls.Add(CreateButton("Configurações", ShowSettingsPanel), 0, 1);
            buttons.Controls.Add(CreateButton("Sair", ExitGame), 0, 2);
            mainPanel.Controls.Add(buttons);
        }

        private void BuildSettingsPanel()
        {
            settingsPanel.Dock = DockStyle.Fill;
            settingsPanel.Visible = false;

            Label title = CreateLabel("CONFIGURAÇÕES", 24, FontStyle.Bold);
            title.Dock = DockStyle.Top;
            title.Height = 90;
            title.TextAlign = ContentAlignment.MiddleCenter;
            settingsPanel.Controls.Add(title);

            normalMode.Text = "Jogo Normal";
            normalMode.Checked = true;
            survivalMode.Text = "Modo Sobrevivência";
            normalMode.AutoSize = true;
            survivalMode.AutoSize = true;
            normalMode.Font = survivalMode.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Regular);
            normalMode.ForeColor = survivalMode.ForeColor = Color.White;
            normalMode.BackColor = survivalMode.BackColor = Color.Transparent;
            normalMode.CheckedChanged += ModeChanged;

            FlowLayoutPanel modes = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Anchor = AnchorStyles.None,
                BackColor = Color.Transparent
            };
            modes.Controls.Add(normalMode);
            modes.Controls.Add(survivalMode);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            layout.Controls.Add(modes, 0, 0);

            Button back = CreateButton("Voltar", ShowMainPanel);
            back.Anchor = AnchorStyles.None;
            layout.Controls.Add(back, 0, 1);
            settingsPanel.Controls.Add(layout);
        }

        private TableLayoutPanel CreateButtonLayout()
        {
            return new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(300, 130, 300, 130),
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
        }

        private static Label CreateLabel(string text, float size, FontStyle style)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", size, style),
                AutoSize = false
            };
        }

        private static Button CreateButton(string text, EventHandler click)
        {
            Button button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                UseVisualStyleBackColor = true
            };
            button.Click += click;
            return button;
        }

        private void StartGame(object? sender, EventArgs e)
        {
            Hide();
            using (Form1 game = new Form1(selectedMode))
            {
                game.ShowDialog(this);
            }
            if (!IsDisposed)
            {
                Show();
            }
        }

        private void ModeChanged(object? sender, EventArgs e)
        {
            selectedMode = normalMode.Checked ? GameMode.Normal : GameMode.Survival;
        }

        private void ShowSettingsPanel(object? sender, EventArgs e)
        {
            mainPanel.Visible = false;
            settingsPanel.Visible = true;
        }

        private void ShowMainPanel(object? sender, EventArgs e)
        {
            settingsPanel.Visible = false;
            mainPanel.Visible = true;
        }

        private void ExitGame(object? sender, EventArgs e)
        {
            Close();
        }
    }
}

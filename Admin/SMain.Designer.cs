using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Admin
{
    partial class SMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SMain));
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InterfaceTimer = new System.Windows.Forms.Timer(this.components);
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.textBoxNickName = new System.Windows.Forms.TextBox();
            this.queryBtn = new System.Windows.Forms.Button();
            this.giveGoldBtn = new System.Windows.Forms.Button();
            this.giveCreditBtn = new System.Windows.Forms.Button();
            this.textBoxCredit = new System.Windows.Forms.TextBox();
            this.textBoxGold = new System.Windows.Forms.TextBox();
            this.queryOnlineBtn = new System.Windows.Forms.Button();
            this.rmGoldBtn = new System.Windows.Forms.Button();
            this.textBoxRmGold = new System.Windows.Forms.TextBox();
            this.rmCreditBtn = new System.Windows.Forms.Button();
            this.textBoxRmCredit = new System.Windows.Forms.TextBox();
            this.sda1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ReloadMonstersBtn = new System.Windows.Forms.Button();
            this.ReloadItemsButton = new System.Windows.Forms.Button();
            this.reloaddrops = new System.Windows.Forms.Button();
            this.SyncLogCheckBox = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.QueryAllCharactersBtn = new System.Windows.Forms.Button();
            this.LevelBtn = new System.Windows.Forms.Button();
            this.LevelTextBox = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.AnlysicComboBox = new System.Windows.Forms.ComboBox();
            this.mainChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.scanBtn = new System.Windows.Forms.Button();
            this.dirTextBox = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.OnlinePlayersListView = new Admin.ListViewNF();
            this.indexHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.levelHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            this.monthCalendar1 = new System.Windows.Forms.MonthCalendar();
            this.BtnReloadRegion = new System.Windows.Forms.Button();
            this.MainMenu.SuspendLayout();
            this.sda1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainChart)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.controlToolStripMenuItem});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(770, 25);
            this.MainMenu.TabIndex = 3;
            this.MainMenu.Text = "menuStrip1";
            // 
            // controlToolStripMenuItem
            // 
            this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.configToolStripMenuItem});
            this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            this.controlToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.controlToolStripMenuItem.Text = "文件";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.newToolStripMenuItem.Text = "新建";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.openToolStripMenuItem.Text = "打开";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // configToolStripMenuItem
            // 
            this.configToolStripMenuItem.Name = "configToolStripMenuItem";
            this.configToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.configToolStripMenuItem.Text = "配置";
            this.configToolStripMenuItem.Click += new System.EventHandler(this.configToolStripMenuItem_Click_1);
            // 
            // InterfaceTimer
            // 
            this.InterfaceTimer.Enabled = true;
            this.InterfaceTimer.Tick += new System.EventHandler(this.InterfaceTimer_Tick);
            // 
            // LogTextBox
            // 
            this.LogTextBox.Location = new System.Drawing.Point(12, 298);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(490, 102);
            this.LogTextBox.TabIndex = 4;
            // 
            // textBoxNickName
            // 
            this.textBoxNickName.Location = new System.Drawing.Point(96, 6);
            this.textBoxNickName.Name = "textBoxNickName";
            this.textBoxNickName.Size = new System.Drawing.Size(98, 21);
            this.textBoxNickName.TabIndex = 5;
            // 
            // queryBtn
            // 
            this.queryBtn.Location = new System.Drawing.Point(6, 6);
            this.queryBtn.Name = "queryBtn";
            this.queryBtn.Size = new System.Drawing.Size(75, 23);
            this.queryBtn.TabIndex = 7;
            this.queryBtn.Text = "查询角色";
            this.queryBtn.UseVisualStyleBackColor = true;
            this.queryBtn.Click += new System.EventHandler(this.queryBtn_Click);
            // 
            // giveGoldBtn
            // 
            this.giveGoldBtn.Location = new System.Drawing.Point(6, 45);
            this.giveGoldBtn.Name = "giveGoldBtn";
            this.giveGoldBtn.Size = new System.Drawing.Size(75, 23);
            this.giveGoldBtn.TabIndex = 9;
            this.giveGoldBtn.Text = "发放金币";
            this.giveGoldBtn.UseVisualStyleBackColor = true;
            this.giveGoldBtn.Click += new System.EventHandler(this.giveGoldBtn_Click);
            // 
            // giveCreditBtn
            // 
            this.giveCreditBtn.Location = new System.Drawing.Point(6, 80);
            this.giveCreditBtn.Name = "giveCreditBtn";
            this.giveCreditBtn.Size = new System.Drawing.Size(75, 23);
            this.giveCreditBtn.TabIndex = 10;
            this.giveCreditBtn.Text = "发放元宝";
            this.giveCreditBtn.UseVisualStyleBackColor = true;
            this.giveCreditBtn.Click += new System.EventHandler(this.giveCreditBtn_Click);
            // 
            // textBoxCredit
            // 
            this.textBoxCredit.Location = new System.Drawing.Point(96, 80);
            this.textBoxCredit.Name = "textBoxCredit";
            this.textBoxCredit.Size = new System.Drawing.Size(100, 21);
            this.textBoxCredit.TabIndex = 11;
            // 
            // textBoxGold
            // 
            this.textBoxGold.Location = new System.Drawing.Point(96, 45);
            this.textBoxGold.Name = "textBoxGold";
            this.textBoxGold.Size = new System.Drawing.Size(100, 21);
            this.textBoxGold.TabIndex = 12;
            // 
            // queryOnlineBtn
            // 
            this.queryOnlineBtn.Location = new System.Drawing.Point(372, 156);
            this.queryOnlineBtn.Name = "queryOnlineBtn";
            this.queryOnlineBtn.Size = new System.Drawing.Size(75, 23);
            this.queryOnlineBtn.TabIndex = 13;
            this.queryOnlineBtn.Text = "查询在线";
            this.queryOnlineBtn.UseVisualStyleBackColor = true;
            this.queryOnlineBtn.Click += new System.EventHandler(this.queryOnlineBtn_Click);
            // 
            // rmGoldBtn
            // 
            this.rmGoldBtn.Location = new System.Drawing.Point(200, 4);
            this.rmGoldBtn.Name = "rmGoldBtn";
            this.rmGoldBtn.Size = new System.Drawing.Size(75, 23);
            this.rmGoldBtn.TabIndex = 14;
            this.rmGoldBtn.Text = "扣除金币";
            this.rmGoldBtn.UseVisualStyleBackColor = true;
            this.rmGoldBtn.Click += new System.EventHandler(this.rmGoldBtn_Click);
            // 
            // textBoxRmGold
            // 
            this.textBoxRmGold.Location = new System.Drawing.Point(290, 5);
            this.textBoxRmGold.Name = "textBoxRmGold";
            this.textBoxRmGold.Size = new System.Drawing.Size(100, 21);
            this.textBoxRmGold.TabIndex = 15;
            // 
            // rmCreditBtn
            // 
            this.rmCreditBtn.Location = new System.Drawing.Point(200, 43);
            this.rmCreditBtn.Name = "rmCreditBtn";
            this.rmCreditBtn.Size = new System.Drawing.Size(75, 23);
            this.rmCreditBtn.TabIndex = 16;
            this.rmCreditBtn.Text = "扣除元宝";
            this.rmCreditBtn.UseVisualStyleBackColor = true;
            this.rmCreditBtn.Click += new System.EventHandler(this.rmCreditBtn_Click);
            // 
            // textBoxRmCredit
            // 
            this.textBoxRmCredit.Location = new System.Drawing.Point(290, 45);
            this.textBoxRmCredit.Name = "textBoxRmCredit";
            this.textBoxRmCredit.Size = new System.Drawing.Size(100, 21);
            this.textBoxRmCredit.TabIndex = 17;
            // 
            // sda1
            // 
            this.sda1.Controls.Add(this.tabPage1);
            this.sda1.Controls.Add(this.tabPage2);
            this.sda1.Controls.Add(this.tabPage3);
            this.sda1.Location = new System.Drawing.Point(12, 28);
            this.sda1.Name = "sda1";
            this.sda1.SelectedIndex = 0;
            this.sda1.Size = new System.Drawing.Size(490, 264);
            this.sda1.TabIndex = 19;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.BtnReloadRegion);
            this.tabPage1.Controls.Add(this.ReloadMonstersBtn);
            this.tabPage1.Controls.Add(this.ReloadItemsButton);
            this.tabPage1.Controls.Add(this.reloaddrops);
            this.tabPage1.Controls.Add(this.SyncLogCheckBox);
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.QueryAllCharactersBtn);
            this.tabPage1.Controls.Add(this.LevelBtn);
            this.tabPage1.Controls.Add(this.LevelTextBox);
            this.tabPage1.Controls.Add(this.queryBtn);
            this.tabPage1.Controls.Add(this.textBoxRmCredit);
            this.tabPage1.Controls.Add(this.textBoxNickName);
            this.tabPage1.Controls.Add(this.rmCreditBtn);
            this.tabPage1.Controls.Add(this.giveGoldBtn);
            this.tabPage1.Controls.Add(this.textBoxRmGold);
            this.tabPage1.Controls.Add(this.giveCreditBtn);
            this.tabPage1.Controls.Add(this.rmGoldBtn);
            this.tabPage1.Controls.Add(this.textBoxCredit);
            this.tabPage1.Controls.Add(this.queryOnlineBtn);
            this.tabPage1.Controls.Add(this.textBoxGold);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(482, 238);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "玩家管理";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ReloadMonstersBtn
            // 
            this.ReloadMonstersBtn.Location = new System.Drawing.Point(217, 156);
            this.ReloadMonstersBtn.Name = "ReloadMonstersBtn";
            this.ReloadMonstersBtn.Size = new System.Drawing.Size(75, 23);
            this.ReloadMonstersBtn.TabIndex = 26;
            this.ReloadMonstersBtn.Text = "ReloadMonsters";
            this.ReloadMonstersBtn.UseVisualStyleBackColor = true;
            this.ReloadMonstersBtn.Click += new System.EventHandler(this.ReloadMonstersBtn_Click);
            // 
            // ReloadItemsButton
            // 
            this.ReloadItemsButton.Location = new System.Drawing.Point(217, 122);
            this.ReloadItemsButton.Name = "ReloadItemsButton";
            this.ReloadItemsButton.Size = new System.Drawing.Size(75, 23);
            this.ReloadItemsButton.TabIndex = 25;
            this.ReloadItemsButton.Text = "ReloadItems";
            this.ReloadItemsButton.UseVisualStyleBackColor = true;
            this.ReloadItemsButton.Click += new System.EventHandler(this.ReloadItemsButton_Click);
            // 
            // reloaddrops
            // 
            this.reloaddrops.Location = new System.Drawing.Point(217, 91);
            this.reloaddrops.Name = "reloaddrops";
            this.reloaddrops.Size = new System.Drawing.Size(75, 23);
            this.reloaddrops.TabIndex = 24;
            this.reloaddrops.Text = "ReloadDrops";
            this.reloaddrops.UseVisualStyleBackColor = true;
            this.reloaddrops.Click += new System.EventHandler(this.reloaddrops_Click);
            // 
            // SyncLogCheckBox
            // 
            this.SyncLogCheckBox.AutoSize = true;
            this.SyncLogCheckBox.Location = new System.Drawing.Point(267, 198);
            this.SyncLogCheckBox.Name = "SyncLogCheckBox";
            this.SyncLogCheckBox.Size = new System.Drawing.Size(72, 16);
            this.SyncLogCheckBox.TabIndex = 23;
            this.SyncLogCheckBox.Text = "同步日志";
            this.SyncLogCheckBox.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(372, 91);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 22;
            this.button2.Text = "登陆";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 156);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 21;
            this.button1.Text = "发送";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // QueryAllCharactersBtn
            // 
            this.QueryAllCharactersBtn.Location = new System.Drawing.Point(372, 198);
            this.QueryAllCharactersBtn.Name = "QueryAllCharactersBtn";
            this.QueryAllCharactersBtn.Size = new System.Drawing.Size(85, 23);
            this.QueryAllCharactersBtn.TabIndex = 20;
            this.QueryAllCharactersBtn.Text = "查询在线详细";
            this.QueryAllCharactersBtn.UseVisualStyleBackColor = true;
            this.QueryAllCharactersBtn.Click += new System.EventHandler(this.QueryOnlineBtn_Click);
            // 
            // LevelBtn
            // 
            this.LevelBtn.Location = new System.Drawing.Point(6, 122);
            this.LevelBtn.Name = "LevelBtn";
            this.LevelBtn.Size = new System.Drawing.Size(75, 23);
            this.LevelBtn.TabIndex = 18;
            this.LevelBtn.Text = "调等级";
            this.LevelBtn.UseVisualStyleBackColor = true;
            this.LevelBtn.Click += new System.EventHandler(this.LevelBtn_Click);
            // 
            // LevelTextBox
            // 
            this.LevelTextBox.Location = new System.Drawing.Point(96, 122);
            this.LevelTextBox.Name = "LevelTextBox";
            this.LevelTextBox.Size = new System.Drawing.Size(100, 21);
            this.LevelTextBox.TabIndex = 19;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.AnlysicComboBox);
            this.tabPage2.Controls.Add(this.mainChart);
            this.tabPage2.Controls.Add(this.scanBtn);
            this.tabPage2.Controls.Add(this.dirTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(482, 238);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "数据分析";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // AnlysicComboBox
            // 
            this.AnlysicComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AnlysicComboBox.FormattingEnabled = true;
            this.AnlysicComboBox.Location = new System.Drawing.Point(368, 11);
            this.AnlysicComboBox.Name = "AnlysicComboBox";
            this.AnlysicComboBox.Size = new System.Drawing.Size(108, 20);
            this.AnlysicComboBox.TabIndex = 5;
            this.AnlysicComboBox.SelectedIndexChanged += new System.EventHandler(this.AnlysicComboBox_SelectedIndexChanged);
            // 
            // mainChart
            // 
            chartArea1.Name = "ChartArea1";
            this.mainChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.mainChart.Legends.Add(legend1);
            this.mainChart.Location = new System.Drawing.Point(7, 38);
            this.mainChart.Name = "mainChart";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.mainChart.Series.Add(series1);
            this.mainChart.Size = new System.Drawing.Size(469, 193);
            this.mainChart.TabIndex = 4;
            this.mainChart.Text = "chart1";
            // 
            // scanBtn
            // 
            this.scanBtn.Location = new System.Drawing.Point(168, 11);
            this.scanBtn.Name = "scanBtn";
            this.scanBtn.Size = new System.Drawing.Size(75, 23);
            this.scanBtn.TabIndex = 3;
            this.scanBtn.Text = "浏览";
            this.scanBtn.UseVisualStyleBackColor = true;
            this.scanBtn.Click += new System.EventHandler(this.scanBtn_Click);
            // 
            // dirTextBox
            // 
            this.dirTextBox.Location = new System.Drawing.Point(7, 13);
            this.dirTextBox.Name = "dirTextBox";
            this.dirTextBox.Size = new System.Drawing.Size(155, 21);
            this.dirTextBox.TabIndex = 2;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.OnlinePlayersListView);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(482, 238);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "在线玩家";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // OnlinePlayersListView
            // 
            this.OnlinePlayersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.indexHeader,
            this.nameHeader,
            this.levelHeader});
            this.OnlinePlayersListView.FullRowSelect = true;
            this.OnlinePlayersListView.GridLines = true;
            this.OnlinePlayersListView.Location = new System.Drawing.Point(6, 6);
            this.OnlinePlayersListView.Name = "OnlinePlayersListView";
            this.OnlinePlayersListView.Size = new System.Drawing.Size(470, 226);
            this.OnlinePlayersListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.OnlinePlayersListView.TabIndex = 0;
            this.OnlinePlayersListView.UseCompatibleStateImageBehavior = false;
            this.OnlinePlayersListView.View = System.Windows.Forms.View.Details;
            // 
            // indexHeader
            // 
            this.indexHeader.Text = "ID";
            // 
            // nameHeader
            // 
            this.nameHeader.Text = "名字";
            // 
            // levelHeader
            // 
            this.levelHeader.Text = "等级";
            // 
            // FileNameTextBox
            // 
            this.FileNameTextBox.Location = new System.Drawing.Point(515, 243);
            this.FileNameTextBox.Name = "FileNameTextBox";
            this.FileNameTextBox.Size = new System.Drawing.Size(243, 21);
            this.FileNameTextBox.TabIndex = 24;
            this.FileNameTextBox.TextChanged += new System.EventHandler(this.FileNameTextBox_TextChanged);
            // 
            // monthCalendar1
            // 
            this.monthCalendar1.Location = new System.Drawing.Point(532, 49);
            this.monthCalendar1.Name = "monthCalendar1";
            this.monthCalendar1.TabIndex = 26;
            this.monthCalendar1.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateChanged);
            // 
            // BtnReloadRegion
            // 
            this.BtnReloadRegion.Location = new System.Drawing.Point(155, 198);
            this.BtnReloadRegion.Name = "BtnReloadRegion";
            this.BtnReloadRegion.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.BtnReloadRegion.Size = new System.Drawing.Size(75, 23);
            this.BtnReloadRegion.TabIndex = 27;
            this.BtnReloadRegion.Text = "ReloadRegion";
            this.BtnReloadRegion.UseVisualStyleBackColor = true;
            this.BtnReloadRegion.Click += new System.EventHandler(this.button3_Click);
            // 
            // SMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(770, 418);
            this.Controls.Add(this.FileNameTextBox);
            this.Controls.Add(this.monthCalendar1);
            this.Controls.Add(this.sda1);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.MainMenu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SMain";
            this.Text = "Admin";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SMain_FormClosing);
            this.Load += new System.EventHandler(this.SMain_Load);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.sda1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainChart)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MenuStrip MainMenu;
        private ToolStripMenuItem controlToolStripMenuItem;
        private Timer InterfaceTimer;
        private TextBox LogTextBox;
        private TextBox textBoxNickName;
        private Button queryBtn;
        private Button giveGoldBtn;
        private Button giveCreditBtn;
        private TextBox textBoxCredit;
        private TextBox textBoxGold;
        private Button queryOnlineBtn;
        private Button rmGoldBtn;
        private TextBox textBoxRmGold;
        private Button rmCreditBtn;
        private TextBox textBoxRmCredit;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem configToolStripMenuItem;
        private TabControl sda1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox dirTextBox;
        private Button scanBtn;
        private System.Windows.Forms.DataVisualization.Charting.Chart mainChart;
        private ComboBox AnlysicComboBox;
        private Button LevelBtn;
        private TextBox LevelTextBox;
        private TabPage tabPage3;
        private ListViewNF OnlinePlayersListView;
        private ColumnHeader nameHeader;
        private ColumnHeader levelHeader;
        private Button QueryAllCharactersBtn;
        private ColumnHeader indexHeader;
        private Button button1;
        private Button button2;
        private CheckBox SyncLogCheckBox;
        private TextBox FileNameTextBox;
        private MonthCalendar monthCalendar1;
        private Button reloaddrops;
        private Button ReloadItemsButton;
        private Button ReloadMonstersBtn;
        private Button BtnReloadRegion;
    }
}


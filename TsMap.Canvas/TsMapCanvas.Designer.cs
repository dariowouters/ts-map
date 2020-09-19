namespace TsMap.Canvas
{
    partial class TsMapCanvas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.MainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.mainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GenerateTileMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemVisibilityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FullMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.CityStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.itemVisibilityToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.localizationSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.MapPanel = new TsMap.Canvas.MapPanel();
            this.MainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenuStrip
            // 
            this.MainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainToolStripMenuItem,
            this.itemVisibilityToolStripMenuItem});
            this.MainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuStrip.Name = "MainMenuStrip";
            this.MainMenuStrip.Size = new System.Drawing.Size(802, 24);
            this.MainMenuStrip.TabIndex = 0;
            this.MainMenuStrip.Text = "menuStrip1";
            // 
            // mainToolStripMenuItem
            // 
            this.mainToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GenerateTileMapToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.mainToolStripMenuItem.Name = "mainToolStripMenuItem";
            this.mainToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.mainToolStripMenuItem.Text = "Main";
            // 
            // GenerateTileMapToolStripMenuItem
            // 
            this.GenerateTileMapToolStripMenuItem.Name = "GenerateTileMapToolStripMenuItem";
            this.GenerateTileMapToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.GenerateTileMapToolStripMenuItem.Text = "Generate Tile Map";
            this.GenerateTileMapToolStripMenuItem.Click += new System.EventHandler(this.GenerateTileMapToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // itemVisibilityToolStripMenuItem
            // 
            this.itemVisibilityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mapToolStripMenuItem,
            this.itemVisibilityToolStripMenuItem1,
            this.localizationSettingsToolStripMenuItem,
            this.paletteToolStripMenuItem});
            this.itemVisibilityToolStripMenuItem.Name = "itemVisibilityToolStripMenuItem";
            this.itemVisibilityToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.itemVisibilityToolStripMenuItem.Text = "View";
            // 
            // mapToolStripMenuItem
            // 
            this.mapToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FullMapToolStripMenuItem,
            this.ResetMapToolStripMenuItem,
            this.toolStripSeparator1,
            this.CityStripComboBox});
            this.mapToolStripMenuItem.Name = "mapToolStripMenuItem";
            this.mapToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.mapToolStripMenuItem.Text = "Map";
            // 
            // FullMapToolStripMenuItem
            // 
            this.FullMapToolStripMenuItem.Name = "FullMapToolStripMenuItem";
            this.FullMapToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.FullMapToolStripMenuItem.Text = "Full";
            this.FullMapToolStripMenuItem.Click += new System.EventHandler(this.FullMapToolStripMenuItem_Click);
            // 
            // ResetMapToolStripMenuItem
            // 
            this.ResetMapToolStripMenuItem.Name = "ResetMapToolStripMenuItem";
            this.ResetMapToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.ResetMapToolStripMenuItem.Text = "Reset";
            this.ResetMapToolStripMenuItem.Click += new System.EventHandler(this.ResetMapToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(217, 6);
            // 
            // CityStripComboBox
            // 
            this.CityStripComboBox.AutoSize = false;
            this.CityStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CityStripComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.CityStripComboBox.Name = "CityStripComboBox";
            this.CityStripComboBox.Size = new System.Drawing.Size(160, 23);
            this.CityStripComboBox.Sorted = true;
            this.CityStripComboBox.SelectedIndexChanged += new System.EventHandler(this.CityStripComboBox_SelectedIndexChanged);
            // 
            // itemVisibilityToolStripMenuItem1
            // 
            this.itemVisibilityToolStripMenuItem1.Name = "itemVisibilityToolStripMenuItem1";
            this.itemVisibilityToolStripMenuItem1.Size = new System.Drawing.Size(182, 22);
            this.itemVisibilityToolStripMenuItem1.Text = "Item Visibility";
            this.itemVisibilityToolStripMenuItem1.Click += new System.EventHandler(this.ItemVisibilityToolStripMenuItem_Click);
            // 
            // localizationSettingsToolStripMenuItem
            // 
            this.localizationSettingsToolStripMenuItem.Name = "localizationSettingsToolStripMenuItem";
            this.localizationSettingsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.localizationSettingsToolStripMenuItem.Text = "Localization Settings";
            this.localizationSettingsToolStripMenuItem.Click += new System.EventHandler(this.localizationSettingsToolStripMenuItem_Click);
            // 
            // paletteToolStripMenuItem
            // 
            this.paletteToolStripMenuItem.Name = "paletteToolStripMenuItem";
            this.paletteToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.paletteToolStripMenuItem.Text = "Palette";
            this.paletteToolStripMenuItem.Click += new System.EventHandler(this.paletteToolStripMenuItem_Click);
            // 
            // MapPanel
            // 
            this.MapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MapPanel.Location = new System.Drawing.Point(0, 24);
            this.MapPanel.Name = "MapPanel";
            this.MapPanel.Size = new System.Drawing.Size(802, 568);
            this.MapPanel.TabIndex = 1;
            this.MapPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.MapPanel_Paint);
            // 
            // TsMapCanvas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 592);
            this.Controls.Add(this.MapPanel);
            this.Controls.Add(this.MainMenuStrip);
            this.Name = "TsMapCanvas";
            this.Text = "TsMapCanvas";
            this.MainMenuStrip.ResumeLayout(false);
            this.MainMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem mainToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem GenerateTileMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemVisibilityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemVisibilityToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem paletteToolStripMenuItem;
        private MapPanel MapPanel;
        private System.Windows.Forms.ToolStripMenuItem localizationSettingsToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolStripMenuItem mapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FullMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ResetMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox CityStripComboBox;
    }
}
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
            this.exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemVisibilityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemVisibilityToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.paletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportFileDialog = new System.Windows.Forms.SaveFileDialog();
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
            this.exportImageToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.mainToolStripMenuItem.Name = "mainToolStripMenuItem";
            this.mainToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.mainToolStripMenuItem.Text = "Main";
            // 
            // exportImageToolStripMenuItem
            // 
            this.exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            this.exportImageToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.exportImageToolStripMenuItem.Text = "Export Image";
            this.exportImageToolStripMenuItem.Click += new System.EventHandler(this.ExportImageToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // itemVisibilityToolStripMenuItem
            // 
            this.itemVisibilityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemVisibilityToolStripMenuItem1,
            this.paletteToolStripMenuItem});
            this.itemVisibilityToolStripMenuItem.Name = "itemVisibilityToolStripMenuItem";
            this.itemVisibilityToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.itemVisibilityToolStripMenuItem.Text = "View";
            // 
            // itemVisibilityToolStripMenuItem1
            // 
            this.itemVisibilityToolStripMenuItem1.Name = "itemVisibilityToolStripMenuItem1";
            this.itemVisibilityToolStripMenuItem1.Size = new System.Drawing.Size(145, 22);
            this.itemVisibilityToolStripMenuItem1.Text = "Item Visibility";
            this.itemVisibilityToolStripMenuItem1.Click += new System.EventHandler(this.ItemVisibilityToolStripMenuItem_Click);
            // 
            // paletteToolStripMenuItem
            // 
            this.paletteToolStripMenuItem.Name = "paletteToolStripMenuItem";
            this.paletteToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.paletteToolStripMenuItem.Text = "Palette";
            this.paletteToolStripMenuItem.Click += new System.EventHandler(this.paletteToolStripMenuItem_Click);
            // 
            // exportFileDialog
            // 
            this.exportFileDialog.DefaultExt = "png";
            this.exportFileDialog.FileName = "map-export";
            this.exportFileDialog.Filter = "PNG File|*.png";
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
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog exportFileDialog;
        private System.Windows.Forms.ToolStripMenuItem itemVisibilityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemVisibilityToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem paletteToolStripMenuItem;
        private MapPanel MapPanel;
    }
}
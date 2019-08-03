namespace TsMap.Canvas
{
    partial class TileMapGeneratorForm
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
            this.GenerateBtn = new System.Windows.Forms.Button();
            this.TileZoomLevelText = new System.Windows.Forms.TextBox();
            this.TileZoomLevelLabel = new System.Windows.Forms.Label();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // GenerateBtn
            // 
            this.GenerateBtn.Location = new System.Drawing.Point(12, 77);
            this.GenerateBtn.Name = "GenerateBtn";
            this.GenerateBtn.Size = new System.Drawing.Size(239, 23);
            this.GenerateBtn.TabIndex = 2;
            this.GenerateBtn.Text = "Generate";
            this.GenerateBtn.UseVisualStyleBackColor = true;
            this.GenerateBtn.Click += new System.EventHandler(this.GenerateBtn_Click);
            // 
            // TileZoomLevelText
            // 
            this.TileZoomLevelText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TileZoomLevelText.Location = new System.Drawing.Point(12, 25);
            this.TileZoomLevelText.Name = "TileZoomLevelText";
            this.TileZoomLevelText.Size = new System.Drawing.Size(239, 20);
            this.TileZoomLevelText.TabIndex = 0;
            // 
            // TileZoomLevelLabel
            // 
            this.TileZoomLevelLabel.AutoSize = true;
            this.TileZoomLevelLabel.Location = new System.Drawing.Point(12, 9);
            this.TileZoomLevelLabel.Name = "TileZoomLevelLabel";
            this.TileZoomLevelLabel.Size = new System.Drawing.Size(83, 13);
            this.TileZoomLevelLabel.TabIndex = 2;
            this.TileZoomLevelLabel.Text = "Tile Zoom Level";
            // 
            // WarningLabel
            // 
            this.WarningLabel.AutoSize = true;
            this.WarningLabel.ForeColor = System.Drawing.Color.Red;
            this.WarningLabel.Location = new System.Drawing.Point(12, 48);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(230, 26);
            this.WarningLabel.TabIndex = 3;
            this.WarningLabel.Text = "Level 8 = detailed city view on normal ets2 map\r\nTakes quite a while to generate";
            // 
            // TileMapGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 108);
            this.Controls.Add(this.WarningLabel);
            this.Controls.Add(this.TileZoomLevelLabel);
            this.Controls.Add(this.TileZoomLevelText);
            this.Controls.Add(this.GenerateBtn);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(279, 147);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(279, 147);
            this.Name = "TileMapGeneratorForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ImageExportOptionForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GenerateBtn;
        private System.Windows.Forms.TextBox TileZoomLevelText;
        private System.Windows.Forms.Label TileZoomLevelLabel;
        private System.Windows.Forms.Label WarningLabel;
    }
}
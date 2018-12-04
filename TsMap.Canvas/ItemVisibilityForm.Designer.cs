namespace TsMap.Canvas
{
    partial class ItemVisibilityForm
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
            this.CityNamesCheckBox = new System.Windows.Forms.CheckBox();
            this.MapAreasCheckBox = new System.Windows.Forms.CheckBox();
            this.FerryConnectionsCheckBox = new System.Windows.Forms.CheckBox();
            this.RoadsCheckBox = new System.Windows.Forms.CheckBox();
            this.MapOverlaysCheckBox = new System.Windows.Forms.CheckBox();
            this.PrefabsCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // CityNamesCheckBox
            // 
            this.CityNamesCheckBox.AutoSize = true;
            this.CityNamesCheckBox.Location = new System.Drawing.Point(12, 127);
            this.CityNamesCheckBox.Name = "CityNamesCheckBox";
            this.CityNamesCheckBox.Size = new System.Drawing.Size(76, 17);
            this.CityNamesCheckBox.TabIndex = 5;
            this.CityNamesCheckBox.Text = "CityNames";
            this.CityNamesCheckBox.UseVisualStyleBackColor = true;
            this.CityNamesCheckBox.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // MapAreasCheckBox
            // 
            this.MapAreasCheckBox.AutoSize = true;
            this.MapAreasCheckBox.Location = new System.Drawing.Point(12, 58);
            this.MapAreasCheckBox.Name = "MapAreasCheckBox";
            this.MapAreasCheckBox.Size = new System.Drawing.Size(74, 17);
            this.MapAreasCheckBox.TabIndex = 2;
            this.MapAreasCheckBox.Text = "MapAreas";
            this.MapAreasCheckBox.UseVisualStyleBackColor = true;
            this.MapAreasCheckBox.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // FerryConnectionsCheckBox
            // 
            this.FerryConnectionsCheckBox.AutoSize = true;
            this.FerryConnectionsCheckBox.Location = new System.Drawing.Point(12, 104);
            this.FerryConnectionsCheckBox.Name = "FerryConnectionsCheckBox";
            this.FerryConnectionsCheckBox.Size = new System.Drawing.Size(108, 17);
            this.FerryConnectionsCheckBox.TabIndex = 4;
            this.FerryConnectionsCheckBox.Text = "FerryConnections";
            this.FerryConnectionsCheckBox.UseVisualStyleBackColor = true;
            this.FerryConnectionsCheckBox.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // RoadsCheckBox
            // 
            this.RoadsCheckBox.AutoSize = true;
            this.RoadsCheckBox.Location = new System.Drawing.Point(12, 35);
            this.RoadsCheckBox.Name = "RoadsCheckBox";
            this.RoadsCheckBox.Size = new System.Drawing.Size(57, 17);
            this.RoadsCheckBox.TabIndex = 1;
            this.RoadsCheckBox.Text = "Roads";
            this.RoadsCheckBox.UseVisualStyleBackColor = true;
            this.RoadsCheckBox.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // MapOverlaysCheckBox
            // 
            this.MapOverlaysCheckBox.AutoSize = true;
            this.MapOverlaysCheckBox.Location = new System.Drawing.Point(12, 81);
            this.MapOverlaysCheckBox.Name = "MapOverlaysCheckBox";
            this.MapOverlaysCheckBox.Size = new System.Drawing.Size(88, 17);
            this.MapOverlaysCheckBox.TabIndex = 3;
            this.MapOverlaysCheckBox.Text = "MapOverlays";
            this.MapOverlaysCheckBox.UseVisualStyleBackColor = true;
            this.MapOverlaysCheckBox.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // PrefabsCheckBox
            // 
            this.PrefabsCheckBox.AutoSize = true;
            this.PrefabsCheckBox.Location = new System.Drawing.Point(12, 12);
            this.PrefabsCheckBox.Name = "PrefabsCheckBox";
            this.PrefabsCheckBox.Size = new System.Drawing.Size(62, 17);
            this.PrefabsCheckBox.TabIndex = 0;
            this.PrefabsCheckBox.Text = "Prefabs";
            this.PrefabsCheckBox.UseVisualStyleBackColor = true;
            this.PrefabsCheckBox.CheckedChanged += new System.EventHandler(this.CheckChanged);
            // 
            // ItemVisibilityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(125, 149);
            this.Controls.Add(this.CityNamesCheckBox);
            this.Controls.Add(this.MapAreasCheckBox);
            this.Controls.Add(this.FerryConnectionsCheckBox);
            this.Controls.Add(this.RoadsCheckBox);
            this.Controls.Add(this.MapOverlaysCheckBox);
            this.Controls.Add(this.PrefabsCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ItemVisibilityForm";
            this.Text = "Item Visibility";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CityNamesCheckBox;
        private System.Windows.Forms.CheckBox MapAreasCheckBox;
        private System.Windows.Forms.CheckBox FerryConnectionsCheckBox;
        private System.Windows.Forms.CheckBox RoadsCheckBox;
        private System.Windows.Forms.CheckBox MapOverlaysCheckBox;
        private System.Windows.Forms.CheckBox PrefabsCheckBox;
    }
}
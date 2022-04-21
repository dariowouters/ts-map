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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Generate TileMap Info");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Localized Names");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Generate City List", new System.Windows.Forms.TreeNode[] {
            treeNode2});
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Localized Names");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Generate Country List", new System.Windows.Forms.TreeNode[] {
            treeNode4});
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Export As PNG");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Generate Overlay List", new System.Windows.Forms.TreeNode[] {
            treeNode6});
            this.GenerateBtn = new System.Windows.Forms.Button();
            this.StartLabel = new System.Windows.Forms.Label();
            this.EndLabel = new System.Windows.Forms.Label();
            this.StartZoomLevelBox = new System.Windows.Forms.NumericUpDown();
            this.EndZoomLevelBox = new System.Windows.Forms.NumericUpDown();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.CityNamesCheckBox = new System.Windows.Forms.CheckBox();
            this.MapAreasCheckBox = new System.Windows.Forms.CheckBox();
            this.FerryConnectionsCheckBox = new System.Windows.Forms.CheckBox();
            this.RoadsCheckBox = new System.Windows.Forms.CheckBox();
            this.MapOverlaysCheckBox = new System.Windows.Forms.CheckBox();
            this.PrefabsCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.GenTilesCheck = new System.Windows.Forms.CheckBox();
            this.triStateTreeView1 = new TsMap.Canvas.TriStateTreeView();
            this.SecretRoadsCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.StartZoomLevelBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EndZoomLevelBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // GenerateBtn
            // 
            this.GenerateBtn.Location = new System.Drawing.Point(10, 194);
            this.GenerateBtn.Name = "GenerateBtn";
            this.GenerateBtn.Size = new System.Drawing.Size(298, 23);
            this.GenerateBtn.TabIndex = 2;
            this.GenerateBtn.Text = "Generate";
            this.GenerateBtn.UseVisualStyleBackColor = true;
            this.GenerateBtn.Click += new System.EventHandler(this.GenerateBtn_Click);
            // 
            // StartLabel
            // 
            this.StartLabel.AutoSize = true;
            this.StartLabel.Location = new System.Drawing.Point(7, 35);
            this.StartLabel.Name = "StartLabel";
            this.StartLabel.Size = new System.Drawing.Size(29, 13);
            this.StartLabel.TabIndex = 4;
            this.StartLabel.Text = "Start";
            // 
            // EndLabel
            // 
            this.EndLabel.AutoSize = true;
            this.EndLabel.Location = new System.Drawing.Point(97, 35);
            this.EndLabel.Name = "EndLabel";
            this.EndLabel.Size = new System.Drawing.Size(26, 13);
            this.EndLabel.TabIndex = 4;
            this.EndLabel.Text = "End";
            // 
            // StartZoomLevelBox
            // 
            this.StartZoomLevelBox.Location = new System.Drawing.Point(44, 33);
            this.StartZoomLevelBox.Maximum = new decimal(new int[] {
            18,
            0,
            0,
            0});
            this.StartZoomLevelBox.Name = "StartZoomLevelBox";
            this.StartZoomLevelBox.Size = new System.Drawing.Size(45, 20);
            this.StartZoomLevelBox.TabIndex = 0;
            // 
            // EndZoomLevelBox
            // 
            this.EndZoomLevelBox.Location = new System.Drawing.Point(131, 33);
            this.EndZoomLevelBox.Maximum = new decimal(new int[] {
            18,
            0,
            0,
            0});
            this.EndZoomLevelBox.Name = "EndZoomLevelBox";
            this.EndZoomLevelBox.Size = new System.Drawing.Size(45, 20);
            this.EndZoomLevelBox.TabIndex = 1;
            this.EndZoomLevelBox.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // CityNamesCheckBox
            // 
            this.CityNamesCheckBox.AutoSize = true;
            this.CityNamesCheckBox.Checked = true;
            this.CityNamesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CityNamesCheckBox.Location = new System.Drawing.Point(15, 155);
            this.CityNamesCheckBox.Name = "CityNamesCheckBox";
            this.CityNamesCheckBox.Size = new System.Drawing.Size(76, 17);
            this.CityNamesCheckBox.TabIndex = 12;
            this.CityNamesCheckBox.Text = "CityNames";
            this.CityNamesCheckBox.UseVisualStyleBackColor = true;
            // 
            // MapAreasCheckBox
            // 
            this.MapAreasCheckBox.AutoSize = true;
            this.MapAreasCheckBox.Checked = true;
            this.MapAreasCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MapAreasCheckBox.Location = new System.Drawing.Point(15, 88);
            this.MapAreasCheckBox.Name = "MapAreasCheckBox";
            this.MapAreasCheckBox.Size = new System.Drawing.Size(74, 17);
            this.MapAreasCheckBox.TabIndex = 9;
            this.MapAreasCheckBox.Text = "MapAreas";
            this.MapAreasCheckBox.UseVisualStyleBackColor = true;
            // 
            // FerryConnectionsCheckBox
            // 
            this.FerryConnectionsCheckBox.AutoSize = true;
            this.FerryConnectionsCheckBox.Checked = true;
            this.FerryConnectionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.FerryConnectionsCheckBox.Location = new System.Drawing.Point(15, 134);
            this.FerryConnectionsCheckBox.Name = "FerryConnectionsCheckBox";
            this.FerryConnectionsCheckBox.Size = new System.Drawing.Size(108, 17);
            this.FerryConnectionsCheckBox.TabIndex = 11;
            this.FerryConnectionsCheckBox.Text = "FerryConnections";
            this.FerryConnectionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // RoadsCheckBox
            // 
            this.RoadsCheckBox.AutoSize = true;
            this.RoadsCheckBox.Checked = true;
            this.RoadsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RoadsCheckBox.Location = new System.Drawing.Point(15, 42);
            this.RoadsCheckBox.Name = "RoadsCheckBox";
            this.RoadsCheckBox.Size = new System.Drawing.Size(57, 17);
            this.RoadsCheckBox.TabIndex = 7;
            this.RoadsCheckBox.Text = "Roads";
            this.RoadsCheckBox.UseVisualStyleBackColor = true;
            // 
            // MapOverlaysCheckBox
            // 
            this.MapOverlaysCheckBox.AutoSize = true;
            this.MapOverlaysCheckBox.Checked = true;
            this.MapOverlaysCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MapOverlaysCheckBox.Location = new System.Drawing.Point(15, 111);
            this.MapOverlaysCheckBox.Name = "MapOverlaysCheckBox";
            this.MapOverlaysCheckBox.Size = new System.Drawing.Size(88, 17);
            this.MapOverlaysCheckBox.TabIndex = 10;
            this.MapOverlaysCheckBox.Text = "MapOverlays";
            this.MapOverlaysCheckBox.UseVisualStyleBackColor = true;
            // 
            // PrefabsCheckBox
            // 
            this.PrefabsCheckBox.AutoSize = true;
            this.PrefabsCheckBox.Checked = true;
            this.PrefabsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PrefabsCheckBox.Location = new System.Drawing.Point(15, 19);
            this.PrefabsCheckBox.Name = "PrefabsCheckBox";
            this.PrefabsCheckBox.Size = new System.Drawing.Size(62, 17);
            this.PrefabsCheckBox.TabIndex = 6;
            this.PrefabsCheckBox.Text = "Prefabs";
            this.PrefabsCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.PrefabsCheckBox);
            this.groupBox1.Controls.Add(this.CityNamesCheckBox);
            this.groupBox1.Controls.Add(this.MapOverlaysCheckBox);
            this.groupBox1.Controls.Add(this.MapAreasCheckBox);
            this.groupBox1.Controls.Add(this.SecretRoadsCheckBox);
            this.groupBox1.Controls.Add(this.RoadsCheckBox);
            this.groupBox1.Controls.Add(this.FerryConnectionsCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(183, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(125, 181);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Items To Render";
            // 
            // GenTilesCheck
            // 
            this.GenTilesCheck.AutoSize = true;
            this.GenTilesCheck.Checked = true;
            this.GenTilesCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GenTilesCheck.Location = new System.Drawing.Point(10, 12);
            this.GenTilesCheck.Name = "GenTilesCheck";
            this.GenTilesCheck.Size = new System.Drawing.Size(95, 17);
            this.GenTilesCheck.TabIndex = 5;
            this.GenTilesCheck.Text = "Generate Tiles";
            this.GenTilesCheck.UseVisualStyleBackColor = true;
            this.GenTilesCheck.CheckedChanged += new System.EventHandler(this.GenTilesCheck_CheckedChanged);
            // 
            // triStateTreeView1
            // 
            this.triStateTreeView1.Location = new System.Drawing.Point(10, 59);
            this.triStateTreeView1.Name = "triStateTreeView1";
            treeNode1.Checked = true;
            treeNode1.Name = "GenTileMapInfo";
            treeNode1.StateImageIndex = 1;
            treeNode1.Text = "Generate TileMap Info";
            treeNode2.Name = "GenCityLocalizedNames";
            treeNode2.StateImageIndex = 0;
            treeNode2.Text = "Localized Names";
            treeNode3.Name = "GenCityList";
            treeNode3.StateImageIndex = 0;
            treeNode3.Text = "Generate City List";
            treeNode4.Name = "GenCountryLocalizedNames";
            treeNode4.StateImageIndex = 0;
            treeNode4.Text = "Localized Names";
            treeNode5.Name = "GenCountryList";
            treeNode5.StateImageIndex = 0;
            treeNode5.Text = "Generate Country List";
            treeNode6.Name = "GenOverlayPNGs";
            treeNode6.StateImageIndex = 0;
            treeNode6.Text = "Export As PNG";
            treeNode7.Name = "GenOverlayList";
            treeNode7.StateImageIndex = 0;
            treeNode7.Text = "Generate Overlay List";
            this.triStateTreeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode3,
            treeNode5,
            treeNode7});
            this.triStateTreeView1.Size = new System.Drawing.Size(166, 129);
            this.triStateTreeView1.TabIndex = 13;
            // 
            // SecretRoadsCheckBox
            // 
            this.SecretRoadsCheckBox.AutoSize = true;
            this.SecretRoadsCheckBox.Checked = true;
            this.SecretRoadsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SecretRoadsCheckBox.Location = new System.Drawing.Point(15, 65);
            this.SecretRoadsCheckBox.Name = "SecretRoadsCheckBox";
            this.SecretRoadsCheckBox.Size = new System.Drawing.Size(91, 17);
            this.SecretRoadsCheckBox.TabIndex = 8;
            this.SecretRoadsCheckBox.Text = "Secret Roads";
            this.SecretRoadsCheckBox.UseVisualStyleBackColor = true;
            // 
            // TileMapGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 224);
            this.Controls.Add(this.triStateTreeView1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.GenTilesCheck);
            this.Controls.Add(this.EndZoomLevelBox);
            this.Controls.Add(this.StartZoomLevelBox);
            this.Controls.Add(this.EndLabel);
            this.Controls.Add(this.StartLabel);
            this.Controls.Add(this.GenerateBtn);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TileMapGeneratorForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Export as Web Tile Map";
            ((System.ComponentModel.ISupportInitialize)(this.StartZoomLevelBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EndZoomLevelBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GenerateBtn;
        private System.Windows.Forms.Label StartLabel;
        private System.Windows.Forms.Label EndLabel;
        private System.Windows.Forms.NumericUpDown StartZoomLevelBox;
        private System.Windows.Forms.NumericUpDown EndZoomLevelBox;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox CityNamesCheckBox;
        private System.Windows.Forms.CheckBox MapAreasCheckBox;
        private System.Windows.Forms.CheckBox FerryConnectionsCheckBox;
        private System.Windows.Forms.CheckBox RoadsCheckBox;
        private System.Windows.Forms.CheckBox MapOverlaysCheckBox;
        private System.Windows.Forms.CheckBox PrefabsCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox GenTilesCheck;
        private TriStateTreeView triStateTreeView1;
        private System.Windows.Forms.CheckBox SecretRoadsCheckBox;
    }
}
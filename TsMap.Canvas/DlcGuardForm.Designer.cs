namespace TsMap.Canvas
{
    partial class DlcGuardForm
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
            this.DlcGuardCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // DlcGuardCheckedListBox
            // 
            this.DlcGuardCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DlcGuardCheckedListBox.CheckOnClick = true;
            this.DlcGuardCheckedListBox.FormattingEnabled = true;
            this.DlcGuardCheckedListBox.Location = new System.Drawing.Point(12, 12);
            this.DlcGuardCheckedListBox.Name = "DlcGuardCheckedListBox";
            this.DlcGuardCheckedListBox.Size = new System.Drawing.Size(204, 244);
            this.DlcGuardCheckedListBox.TabIndex = 0;
            this.DlcGuardCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.DlcGuardCheckedListBox_ItemCheck);
            // 
            // DlcGuardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(228, 275);
            this.Controls.Add(this.DlcGuardCheckedListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DlcGuardForm";
            this.Text = "DLC Guards";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox DlcGuardCheckedListBox;
    }
}
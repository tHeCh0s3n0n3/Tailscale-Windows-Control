namespace Tailscale_Windows_Control
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnGetStatus = new System.Windows.Forms.Button();
            this.flpExitNodeButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.lblStatusText = new System.Windows.Forms.Label();
            this.flpExitNodeButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGetStatus
            // 
            this.btnGetStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGetStatus.Location = new System.Drawing.Point(3, 3);
            this.btnGetStatus.Name = "btnGetStatus";
            this.btnGetStatus.Size = new System.Drawing.Size(150, 46);
            this.btnGetStatus.TabIndex = 0;
            this.btnGetStatus.Text = "Get Status";
            this.btnGetStatus.UseVisualStyleBackColor = true;
            this.btnGetStatus.Click += new System.EventHandler(this.BtnGetStatus_Click);
            // 
            // flpExitNodeButtons
            // 
            this.flpExitNodeButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpExitNodeButtons.Controls.Add(this.btnGetStatus);
            this.flpExitNodeButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpExitNodeButtons.Location = new System.Drawing.Point(12, 85);
            this.flpExitNodeButtons.Name = "flpExitNodeButtons";
            this.flpExitNodeButtons.Size = new System.Drawing.Size(280, 322);
            this.flpExitNodeButtons.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 32);
            this.label1.TabIndex = 3;
            this.label1.Text = "Status:";
            // 
            // lblStatusText
            // 
            this.lblStatusText.AutoSize = true;
            this.lblStatusText.Location = new System.Drawing.Point(12, 50);
            this.lblStatusText.Name = "lblStatusText";
            this.lblStatusText.Size = new System.Drawing.Size(78, 32);
            this.lblStatusText.TabIndex = 3;
            this.lblStatusText.Text = "label1";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(304, 419);
            this.Controls.Add(this.lblStatusText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flpExitNodeButtons);
            this.ForeColor = System.Drawing.Color.LightGray;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMain";
            this.Text = "Tailscale Control";
            this.flpExitNodeButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnGetStatus;
        private FlowLayoutPanel flpExitNodeButtons;
        private Label label1;
        private Label lblStatusText;
    }
}
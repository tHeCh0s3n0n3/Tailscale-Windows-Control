namespace Tailscale_Windows_Control
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnGetStatus = new Button();
            flpExitNodeButtons = new FlowLayoutPanel();
            label1 = new Label();
            lblStatusText = new Label();
            flpExitNodeButtons.SuspendLayout();
            SuspendLayout();
            // 
            // btnGetStatus
            // 
            btnGetStatus.BackColor = SystemColors.Control;
            btnGetStatus.FlatStyle = FlatStyle.System;
            btnGetStatus.ForeColor = SystemColors.ControlText;
            btnGetStatus.Location = new Point(3, 3);
            btnGetStatus.Name = "btnGetStatus";
            btnGetStatus.Size = new Size(150, 46);
            btnGetStatus.TabIndex = 0;
            btnGetStatus.Text = "Get Status";
            btnGetStatus.UseVisualStyleBackColor = false;
            btnGetStatus.Click += BtnGetStatus_Click;
            // 
            // flpExitNodeButtons
            // 
            flpExitNodeButtons.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpExitNodeButtons.BackColor = SystemColors.Control;
            flpExitNodeButtons.Controls.Add(btnGetStatus);
            flpExitNodeButtons.FlowDirection = FlowDirection.TopDown;
            flpExitNodeButtons.ForeColor = SystemColors.ControlText;
            flpExitNodeButtons.Location = new Point(12, 85);
            flpExitNodeButtons.Name = "flpExitNodeButtons";
            flpExitNodeButtons.Size = new Size(280, 322);
            flpExitNodeButtons.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(83, 32);
            label1.TabIndex = 3;
            label1.Text = "Status:";
            // 
            // lblStatusText
            // 
            lblStatusText.AutoSize = true;
            lblStatusText.Location = new Point(12, 50);
            lblStatusText.Name = "lblStatusText";
            lblStatusText.Size = new Size(78, 32);
            lblStatusText.TabIndex = 3;
            lblStatusText.Text = "label1";
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(304, 419);
            Controls.Add(lblStatusText);
            Controls.Add(label1);
            Controls.Add(flpExitNodeButtons);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmMain";
            Text = "Tailscale Control";
            flpExitNodeButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button btnGetStatus;
        private FlowLayoutPanel flpExitNodeButtons;
        private Label label1;
        private Label lblStatusText;
    }
}
namespace VirtualMicrophoneApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.CheckedListBox checkedListBoxSources;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label labelStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                foreach (var captureDevice in captureDevices)
                {
                    captureDevice.Dispose();
                }
                loopbackCapture?.Dispose();
                playbackDevice?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.checkedListBoxSources = new System.Windows.Forms.CheckedListBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkedListBoxSources
            // 
            this.checkedListBoxSources.CheckOnClick = true;
            this.checkedListBoxSources.FormattingEnabled = true;
            this.checkedListBoxSources.Location = new System.Drawing.Point(12, 12);
            this.checkedListBoxSources.Name = "checkedListBoxSources";
            this.checkedListBoxSources.Size = new System.Drawing.Size(360, 116);
            this.checkedListBoxSources.TabIndex = 0;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 140);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(175, 35);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Start Virtual Microphone";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(197, 140);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(175, 35);
            this.buttonStop.TabIndex = 2;
            this.buttonStop.Text = "Stop Virtual Microphone";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(12, 185);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(360, 23);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Status: Ready";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AcceptButton = this.buttonStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 217);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.checkedListBoxSources);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Virtual Microphone App";
            this.ResumeLayout(false);
        }
    }
}

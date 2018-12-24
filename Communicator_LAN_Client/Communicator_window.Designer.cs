namespace Communicator_LAN_Client
{
    partial class Communicator_window
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
            this.CurrentServer_label = new System.Windows.Forms.Label();
            this.UsersPanel = new System.Windows.Forms.Panel();
            this.Speak_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CurrentServer_label
            // 
            this.CurrentServer_label.AutoSize = true;
            this.CurrentServer_label.Location = new System.Drawing.Point(9, 9);
            this.CurrentServer_label.Name = "CurrentServer_label";
            this.CurrentServer_label.Size = new System.Drawing.Size(100, 13);
            this.CurrentServer_label.TabIndex = 0;
            this.CurrentServer_label.Text = "Jesteś na serwerze:";
            // 
            // UsersPanel
            // 
            this.UsersPanel.AutoScroll = true;
            this.UsersPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.UsersPanel.Location = new System.Drawing.Point(12, 29);
            this.UsersPanel.Name = "UsersPanel";
            this.UsersPanel.Size = new System.Drawing.Size(230, 231);
            this.UsersPanel.TabIndex = 1;
            // 
            // Speak_button
            // 
            this.Speak_button.Location = new System.Drawing.Point(12, 266);
            this.Speak_button.Name = "Speak_button";
            this.Speak_button.Size = new System.Drawing.Size(230, 52);
            this.Speak_button.TabIndex = 2;
            this.Speak_button.Text = "Przytrzymaj, by mówić...";
            this.Speak_button.UseVisualStyleBackColor = true;
            this.Speak_button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Speak_button_MouseDown);
            this.Speak_button.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Speak_button_MouseUp);
            // 
            // Communicator_window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 329);
            this.Controls.Add(this.Speak_button);
            this.Controls.Add(this.UsersPanel);
            this.Controls.Add(this.CurrentServer_label);
            this.MinimumSize = new System.Drawing.Size(270, 368);
            this.Name = "Communicator_window";
            this.Text = "Communicator_window";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Communicator_window_FormClosing);
            this.Resize += new System.EventHandler(this.Communicator_window_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label CurrentServer_label;
        private System.Windows.Forms.Panel UsersPanel;
        private System.Windows.Forms.Button Speak_button;
    }
}
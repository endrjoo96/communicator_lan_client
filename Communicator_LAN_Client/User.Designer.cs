namespace Communicator_LAN_Client
{
    partial class User
    {
        /// <summary> 
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod wygenerowany przez Projektanta składników

        /// <summary> 
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować 
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.Username = new System.Windows.Forms.Label();
            this.Mute_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Username
            // 
            this.Username.AutoSize = true;
            this.Username.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.Username.Location = new System.Drawing.Point(3, 5);
            this.Username.Name = "Username";
            this.Username.Size = new System.Drawing.Size(63, 15);
            this.Username.TabIndex = 0;
            this.Username.Text = "username";
            // 
            // Mute_button
            // 
            this.Mute_button.Location = new System.Drawing.Point(288, 2);
            this.Mute_button.Name = "Mute_button";
            this.Mute_button.Size = new System.Drawing.Size(40, 23);
            this.Mute_button.TabIndex = 1;
            this.Mute_button.Text = "Mute";
            this.Mute_button.UseVisualStyleBackColor = true;
            // 
            // User
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Mute_button);
            this.Controls.Add(this.Username);
            this.Name = "User";
            this.Size = new System.Drawing.Size(331, 27);
            this.Resize += new System.EventHandler(this.User_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label Username;
        public System.Windows.Forms.Button Mute_button;
    }
}

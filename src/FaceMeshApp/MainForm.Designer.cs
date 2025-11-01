namespace FaceMeshApp
{
    partial class MainForm
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
            pictureBoxVideo = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)(pictureBoxVideo)).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxVideo
            // 
            pictureBoxVideo.Dock = DockStyle.Fill;
            pictureBoxVideo.Location = new Point(0, 0);
            pictureBoxVideo.Name = "pictureBoxVideo";
            pictureBoxVideo.Size = new Size(640, 480);
            pictureBoxVideo.TabIndex = 0;
            pictureBoxVideo.TabStop = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 480);
            Controls.Add(pictureBoxVideo);
            Name = "MainForm";
            Text = "Face Mesh Detection - Mediapipe";
            Load += new EventHandler(MainForm_Load);
            FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(pictureBoxVideo)).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBoxVideo;
    }
}
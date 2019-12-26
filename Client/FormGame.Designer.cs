
namespace Tetris
{
    partial class FormGame 
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
            this.my_game = new System.Windows.Forms.Panel();
            this.otherGame = new System.Windows.Forms.Panel();
            this.main_timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // my_game
            // 
            this.my_game.BackColor = System.Drawing.Color.Gray;
            this.my_game.Location = new System.Drawing.Point(12, 12);
            this.my_game.Name = "my_game";
            this.my_game.Size = new System.Drawing.Size(500, 600);
            this.my_game.TabIndex = 0;
            this.my_game.Paint += new System.Windows.Forms.PaintEventHandler(this.my_game_Paint);
            // 
            // otherGame
            // 
            this.otherGame.BackColor = System.Drawing.Color.Gray;
            this.otherGame.Location = new System.Drawing.Point(518, 12);
            this.otherGame.Name = "otherGame";
            this.otherGame.Size = new System.Drawing.Size(500, 600);
            this.otherGame.TabIndex = 1;
            this.otherGame.Paint += new System.Windows.Forms.PaintEventHandler(this.otherGame_Paint);
            // 
            // main_timer
            // 
            this.main_timer.Enabled = true;
            this.main_timer.Tick += new System.EventHandler(this.main_timer_Tick);
            // 
            // FormGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 627);
            this.Controls.Add(this.otherGame);
            this.Controls.Add(this.my_game);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormGame";
            this.Text = "FormGame";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGame_FormClosing);
            this.Load += new System.EventHandler(this.FormGame_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormGame_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel my_game;
        private System.Windows.Forms.Panel otherGame;
        private System.Windows.Forms.Timer main_timer;
    }
}
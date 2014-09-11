
namespace Moritz.AssistantPerformer
{
    partial class Keyboard
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
            if(disposing && (components != null))
            {
                _borderPen.Dispose();
                _selectedBorderPen.Dispose();
                _unselectedBorderPen.Dispose();
                _backBuffer.Dispose();

                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.KeyboardNumberPanel = new System.Windows.Forms.Panel();
            this.KeyboardNumberLabel = new System.Windows.Forms.Label();
            this.KeyboardNumberPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // KeyboardNumberPanel
            // 
            this.KeyboardNumberPanel.Controls.Add(this.KeyboardNumberLabel);
            this.KeyboardNumberPanel.ForeColor = System.Drawing.Color.Black;
            this.KeyboardNumberPanel.Location = new System.Drawing.Point(4, 12);
            this.KeyboardNumberPanel.Name = "KeyboardNumberPanel";
            this.KeyboardNumberPanel.Size = new System.Drawing.Size(24, 29);
            this.KeyboardNumberPanel.TabIndex = 1;
            this.KeyboardNumberPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.KeyboardNumberPanel_Paint);
            // 
            // KeyboardNumberLabel
            // 
            this.KeyboardNumberLabel.AutoSize = true;
            this.KeyboardNumberLabel.BackColor = System.Drawing.Color.Transparent;
            this.KeyboardNumberLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyboardNumberLabel.Location = new System.Drawing.Point(0, 5);
            this.KeyboardNumberLabel.Name = "KeyboardNumberLabel";
            this.KeyboardNumberLabel.Size = new System.Drawing.Size(24, 17);
            this.KeyboardNumberLabel.TabIndex = 0;
            this.KeyboardNumberLabel.Text = "88";
            this.KeyboardNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Keyboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.KeyboardNumberPanel);
            this.Name = "Keyboard";
            this.Size = new System.Drawing.Size(929, 53);
            this.KeyboardNumberPanel.ResumeLayout(false);
            this.KeyboardNumberPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel KeyboardNumberPanel;
        public System.Windows.Forms.Label KeyboardNumberLabel;



    }
}

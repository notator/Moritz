using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Moritz
{
    partial class AboutMoritzDialog : Form
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
			this.DateLabel = new System.Windows.Forms.Label();
			this.AuthorLabel = new System.Windows.Forms.Label();
			this.DateText = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.WebsiteLinkLabel = new System.Windows.Forms.LinkLabel();
			this.ThanksLabel = new System.Windows.Forms.Label();
			this.AcknowledgmentsLabel = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.MaxUndMoritzLinkLabel = new System.Windows.Forms.LinkLabel();
			this.LeslieSanfordLinkLabel = new System.Windows.Forms.LinkLabel();
			this.label8 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.WebsiteLabel = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// DateLabel
			// 
			this.DateLabel.AutoSize = true;
			this.DateLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DateLabel.Location = new System.Drawing.Point(25, 89);
			this.DateLabel.Name = "DateLabel";
			this.DateLabel.Size = new System.Drawing.Size(43, 14);
			this.DateLabel.TabIndex = 2;
			this.DateLabel.Text = "history:";
			this.DateLabel.Click += new System.EventHandler(this.Anything_Click);
			// 
			// AuthorLabel
			// 
			this.AuthorLabel.AutoSize = true;
			this.AuthorLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AuthorLabel.Location = new System.Drawing.Point(25, 44);
			this.AuthorLabel.Name = "AuthorLabel";
			this.AuthorLabel.Size = new System.Drawing.Size(41, 14);
			this.AuthorLabel.TabIndex = 4;
			this.AuthorLabel.Text = "author:";
			this.AuthorLabel.Click += new System.EventHandler(this.Anything_Click);
			// 
			// DateText
			// 
			this.DateText.AutoSize = true;
			this.DateText.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DateText.Location = new System.Drawing.Point(80, 89);
			this.DateText.Name = "DateText";
			this.DateText.Size = new System.Drawing.Size(162, 56);
			this.DateText.TabIndex = 5;
			this.DateText.Text = "version 1: 2008\r\nversion 2: May 2011\r\nversion 3: December 2014\r\nversion 4: begun " +
    "February 2025";
			this.DateText.Click += new System.EventHandler(this.Anything_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(25, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(97, 15);
			this.label1.TabIndex = 10;
			this.label1.Text = "Moritz version 4";
			this.label1.Click += new System.EventHandler(this.Anything_Click);
			// 
			// WebsiteLinkLabel
			// 
			this.WebsiteLinkLabel.AutoSize = true;
			this.WebsiteLinkLabel.Font = new System.Drawing.Font("Arial", 8F);
			this.WebsiteLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 27);
			this.WebsiteLinkLabel.Location = new System.Drawing.Point(80, 65);
			this.WebsiteLinkLabel.Name = "WebsiteLinkLabel";
			this.WebsiteLinkLabel.Size = new System.Drawing.Size(157, 17);
			this.WebsiteLinkLabel.TabIndex = 29;
			this.WebsiteLinkLabel.TabStop = true;
			this.WebsiteLinkLabel.Text = "www.james-ingram-act-two.de\r\n";
			this.WebsiteLinkLabel.UseCompatibleTextRendering = true;
			this.WebsiteLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.JamesIngramLinkLabel_LinkClicked);
			// 
			// ThanksLabel
			// 
			this.ThanksLabel.AutoSize = true;
			this.ThanksLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ThanksLabel.Location = new System.Drawing.Point(25, 266);
			this.ThanksLabel.Name = "ThanksLabel";
			this.ThanksLabel.Size = new System.Drawing.Size(222, 28);
			this.ThanksLabel.TabIndex = 32;
			this.ThanksLabel.Text = "And many, many thanks to my partner Heinz.\r\nWithout him, this software would not " +
    "exist !";
			this.ThanksLabel.Click += new System.EventHandler(this.Anything_Click);
			// 
			// AcknowledgmentsLabel
			// 
			this.AcknowledgmentsLabel.AutoSize = true;
			this.AcknowledgmentsLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AcknowledgmentsLabel.Location = new System.Drawing.Point(25, 152);
			this.AcknowledgmentsLabel.Name = "AcknowledgmentsLabel";
			this.AcknowledgmentsLabel.Size = new System.Drawing.Size(106, 14);
			this.AcknowledgmentsLabel.TabIndex = 33;
			this.AcknowledgmentsLabel.Text = "Acknowledgements:";
			this.AcknowledgmentsLabel.Click += new System.EventHandler(this.Anything_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Arial", 7F);
			this.label6.Location = new System.Drawing.Point(25, 243);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(220, 13);
			this.label6.TabIndex = 36;
			this.label6.Text = "The classic childrens\' story by Wilhelm Busch.";
			this.label6.Click += new System.EventHandler(this.Anything_Click);
			// 
			// MaxUndMoritzLinkLabel
			// 
			this.MaxUndMoritzLinkLabel.AutoSize = true;
			this.MaxUndMoritzLinkLabel.Font = new System.Drawing.Font("Arial", 7F);
			this.MaxUndMoritzLinkLabel.Location = new System.Drawing.Point(25, 229);
			this.MaxUndMoritzLinkLabel.Name = "MaxUndMoritzLinkLabel";
			this.MaxUndMoritzLinkLabel.Size = new System.Drawing.Size(80, 13);
			this.MaxUndMoritzLinkLabel.TabIndex = 37;
			this.MaxUndMoritzLinkLabel.TabStop = true;
			this.MaxUndMoritzLinkLabel.Text = "Max and Moritz";
			this.MaxUndMoritzLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.MaxUndMoritzLinkLabel_LinkClicked);
			// 
			// LeslieSanfordLinkLabel
			// 
			this.LeslieSanfordLinkLabel.AutoSize = true;
			this.LeslieSanfordLinkLabel.Font = new System.Drawing.Font("Arial", 7F);
			this.LeslieSanfordLinkLabel.Location = new System.Drawing.Point(25, 169);
			this.LeslieSanfordLinkLabel.Name = "LeslieSanfordLinkLabel";
			this.LeslieSanfordLinkLabel.Size = new System.Drawing.Size(156, 13);
			this.LeslieSanfordLinkLabel.TabIndex = 39;
			this.LeslieSanfordLinkLabel.TabStop = true;
			this.LeslieSanfordLinkLabel.Text = "Leslie Sanford\'s C# MIDI Toolkit";
			this.LeslieSanfordLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LeslieSanfordLinkLabel_LinkClicked);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Arial", 7F);
			this.label8.Location = new System.Drawing.Point(25, 183);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(169, 13);
			this.label8.TabIndex = 38;
			this.label8.Text = "Moritz uses version 4 of this library.";
			this.label8.Click += new System.EventHandler(this.Anything_Click);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Font = new System.Drawing.Font("Arial", 7F);
			this.label14.Location = new System.Drawing.Point(25, 212);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(204, 13);
			this.label14.TabIndex = 46;
			this.label14.Text = "Unfortunately no longer available on line.";
			// 
			// WebsiteLabel
			// 
			this.WebsiteLabel.AutoSize = true;
			this.WebsiteLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WebsiteLabel.Location = new System.Drawing.Point(25, 65);
			this.WebsiteLabel.Name = "WebsiteLabel";
			this.WebsiteLabel.Size = new System.Drawing.Size(49, 14);
			this.WebsiteLabel.TabIndex = 48;
			this.WebsiteLabel.Text = "website:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(80, 44);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(73, 14);
			this.label7.TabIndex = 49;
			this.label7.Text = "James Ingram";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 7F);
			this.label2.ForeColor = System.Drawing.Color.Blue;
			this.label2.Location = new System.Drawing.Point(25, 199);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(155, 13);
			this.label2.TabIndex = 50;
			this.label2.Text = "Jeff Glatt\'s MIDI documentation";
			// 
			// AboutMoritzDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Honeydew;
			this.ClientSize = new System.Drawing.Size(266, 316);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.WebsiteLabel);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.LeslieSanfordLinkLabel);
			this.Controls.Add(this.MaxUndMoritzLinkLabel);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.AcknowledgmentsLabel);
			this.Controls.Add(this.ThanksLabel);
			this.Controls.Add(this.WebsiteLinkLabel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.DateText);
			this.Controls.Add(this.AuthorLabel);
			this.Controls.Add(this.DateLabel);
			this.Controls.Add(this.label8);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "AboutMoritzDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About Moritz";
			this.Deactivate += new System.EventHandler(this.AboutMoritzDialog_Deactivate);
			this.Click += new System.EventHandler(this.Anything_Click);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.AboutMoritzDialog_Paint);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.Label AuthorLabel;
		private System.Windows.Forms.Label DateText;
        private System.Windows.Forms.Label label1;
        private LinkLabel WebsiteLinkLabel;
		private Label ThanksLabel;
        private Label AcknowledgmentsLabel;
		private Label label6;
		private LinkLabel MaxUndMoritzLinkLabel;
		private LinkLabel LeslieSanfordLinkLabel;
		private Label label8;
        private Label label14;
        private Label WebsiteLabel;
        private Label label7;
		private Label label2;
    }
}
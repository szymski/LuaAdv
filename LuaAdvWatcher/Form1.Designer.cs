﻿using System.Drawing;
using System.Windows.Forms;

namespace LuaAdvWatcher
{
    partial class Form1
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
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // trayIcon
            // 
            this.trayIcon.Text = "LuaAdv Watcher";
            this.trayIcon.Visible = true;
            ///
            /// richTextBox
            ///
            this.richTextBox = new RichTextBox();
            this.Controls.Add(this.richTextBox);
            this.richTextBox.Dock = DockStyle.Fill;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "LuaAdv Watcher";
            this.Text = "LuaAdv Watcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.FormClosing += this.Form1_Closing;
            this.ResumeLayout(true);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon trayIcon;
        private RichTextBox richTextBox;
    }
}


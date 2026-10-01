using System;

namespace Biometric_Scanner
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Designer controls
        private System.Windows.Forms.Label StatusText;
        private System.Windows.Forms.PictureBox fImage;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonSave_Click;
        private System.Windows.Forms.Button buttonStop_Click;

        // Responsive layout controls
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.TableLayoutPanel controlsRow;
        private System.Windows.Forms.FlowLayoutPanel controlsPanel;
        #endregion

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
        /// This version uses docking and layout panels so the UI is responsive.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.StatusText = new System.Windows.Forms.Label();
            this.fImage = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonSave_Click = new System.Windows.Forms.Button();
            this.buttonStop_Click = new System.Windows.Forms.Button();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.controlsRow = new System.Windows.Forms.TableLayoutPanel();
            this.controlsPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.fImage)).BeginInit();
            this.mainLayout.SuspendLayout();
            this.controlsRow.SuspendLayout();
            this.controlsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusText
            // 
            this.StatusText.AutoSize = true;
            this.StatusText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.StatusText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusText.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusText.ForeColor = System.Drawing.Color.Black;
            this.StatusText.Location = new System.Drawing.Point(3, 0);
            this.StatusText.Name = "StatusText";
            this.StatusText.Padding = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.StatusText.Size = new System.Drawing.Size(364, 57);
            this.StatusText.TabIndex = 4;
            this.StatusText.Text = "Status: ready";
            this.StatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // fImage
            // 
            this.fImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.fImage.Image = ((System.Drawing.Image)(resources.GetObject("fImage.Image")));
            this.fImage.Location = new System.Drawing.Point(3, 3);
            this.fImage.Name = "fImage";
            this.fImage.Size = new System.Drawing.Size(759, 532);
            this.fImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.fImage.TabIndex = 1;
            this.fImage.TabStop = false;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(9, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(137, 33);
            this.button1.TabIndex = 5;
            this.button1.Text = "Start Capture";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonSave_Click
            // 
            this.buttonSave_Click.AutoSize = true;
            this.buttonSave_Click.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSave_Click.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSave_Click.Location = new System.Drawing.Point(290, 9);
            this.buttonSave_Click.Name = "buttonSave_Click";
            this.buttonSave_Click.Size = new System.Drawing.Size(84, 33);
            this.buttonSave_Click.TabIndex = 7;
            this.buttonSave_Click.Text = "Save as";
            this.buttonSave_Click.UseVisualStyleBackColor = true;
            this.buttonSave_Click.Click += new System.EventHandler(this.buttonSave_Click_Click_1);
            // 
            // buttonStop_Click
            // 
            this.buttonStop_Click.AutoSize = true;
            this.buttonStop_Click.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStop_Click.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStop_Click.Location = new System.Drawing.Point(152, 9);
            this.buttonStop_Click.Name = "buttonStop_Click";
            this.buttonStop_Click.Size = new System.Drawing.Size(132, 33);
            this.buttonStop_Click.TabIndex = 6;
            this.buttonStop_Click.Text = "Stop Capture";
            this.buttonStop_Click.UseVisualStyleBackColor = true;
            this.buttonStop_Click.Click += new System.EventHandler(this.buttonStop_Click_Click);
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.controlsRow, 0, 1);
            this.mainLayout.Controls.Add(this.fImage, 0, 0);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 2;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.Size = new System.Drawing.Size(765, 601);
            this.mainLayout.TabIndex = 0;
            // 
            // controlsRow
            // 
            this.controlsRow.AutoSize = true;
            this.controlsRow.ColumnCount = 2;
            this.controlsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.controlsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.controlsRow.Controls.Add(this.StatusText, 0, 0);
            this.controlsRow.Controls.Add(this.controlsPanel, 1, 0);
            this.controlsRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsRow.Location = new System.Drawing.Point(3, 541);
            this.controlsRow.Name = "controlsRow";
            this.controlsRow.RowCount = 1;
            this.controlsRow.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.controlsRow.Size = new System.Drawing.Size(759, 57);
            this.controlsRow.TabIndex = 2;
            // 
            // controlsPanel
            // 
            this.controlsPanel.AutoSize = true;
            this.controlsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.controlsPanel.Controls.Add(this.button1);
            this.controlsPanel.Controls.Add(this.buttonStop_Click);
            this.controlsPanel.Controls.Add(this.buttonSave_Click);
            this.controlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsPanel.Location = new System.Drawing.Point(373, 3);
            this.controlsPanel.Name = "controlsPanel";
            this.controlsPanel.Padding = new System.Windows.Forms.Padding(6);
            this.controlsPanel.Size = new System.Drawing.Size(383, 51);
            this.controlsPanel.TabIndex = 3;
            this.controlsPanel.WrapContents = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(765, 601);
            this.Controls.Add(this.mainLayout);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(480, 340);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Biometric Scanner";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.fImage)).EndInit();
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.controlsRow.ResumeLayout(false);
            this.controlsRow.PerformLayout();
            this.controlsPanel.ResumeLayout(false);
            this.controlsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}


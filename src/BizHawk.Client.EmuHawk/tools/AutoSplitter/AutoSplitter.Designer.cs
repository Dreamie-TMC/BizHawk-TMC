using BizHawk.Client.Common;
using System.ComponentModel;

namespace BizHawk.Client.EmuHawk.AutoSplitter
{
	partial class AutoSplitter
	{

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
            this.LoadSplitFile = new System.Windows.Forms.Button();
            this.SplitFile = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CurrentSplit = new System.Windows.Forms.Label();
            this.NextSplit = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SkipSplitUpdate = new System.Windows.Forms.Button();
            this.SkipSplitNoUpdate = new System.Windows.Forms.Button();
            this.UndoSplitUpdate = new System.Windows.Forms.Button();
            this.UndoSplitNoUpdate = new System.Windows.Forms.Button();
            this.StartAutosplitter = new System.Windows.Forms.Button();
            this.ResetSplits = new System.Windows.Forms.Button();
            this.AutosplitterStatus = new System.Windows.Forms.Label();
            this.OpenSplits = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // LoadSplitFile
            // 
            this.LoadSplitFile.Cursor = System.Windows.Forms.Cursors.No;
            this.LoadSplitFile.Enabled = false;
            this.LoadSplitFile.Location = new System.Drawing.Point(219, 12);
            this.LoadSplitFile.Name = "LoadSplitFile";
            this.LoadSplitFile.Size = new System.Drawing.Size(201, 23);
            this.LoadSplitFile.TabIndex = 0;
            this.LoadSplitFile.Text = "Load Split File";
            this.LoadSplitFile.UseVisualStyleBackColor = true;
            this.LoadSplitFile.Click += new System.EventHandler(this.LoadSplitFile_Click);
            // 
            // SplitFile
            // 
            this.SplitFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SplitFile.Location = new System.Drawing.Point(12, 12);
            this.SplitFile.Name = "SplitFile";
            this.SplitFile.Size = new System.Drawing.Size(201, 23);
            this.SplitFile.TabIndex = 1;
            this.SplitFile.Text = "No Split File Selected, Please Load a File";
            this.SplitFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Current Split:";
            // 
            // CurrentSplit
            // 
            this.CurrentSplit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentSplit.Location = new System.Drawing.Point(85, 47);
            this.CurrentSplit.Name = "CurrentSplit";
            this.CurrentSplit.Size = new System.Drawing.Size(128, 13);
            this.CurrentSplit.TabIndex = 3;
            this.CurrentSplit.Text = "No Splits Loaded";
            // 
            // NextSplit
            // 
            this.NextSplit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NextSplit.Location = new System.Drawing.Point(280, 47);
            this.NextSplit.Name = "NextSplit";
            this.NextSplit.Size = new System.Drawing.Size(140, 13);
            this.NextSplit.TabIndex = 5;
            this.NextSplit.Text = "No Splits Loaded";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(219, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Next Split:";
            // 
            // SkipSplitUpdate
            // 
            this.SkipSplitUpdate.Cursor = System.Windows.Forms.Cursors.No;
            this.SkipSplitUpdate.Enabled = false;
            this.SkipSplitUpdate.Location = new System.Drawing.Point(12, 130);
            this.SkipSplitUpdate.Name = "SkipSplitUpdate";
            this.SkipSplitUpdate.Size = new System.Drawing.Size(201, 23);
            this.SkipSplitUpdate.TabIndex = 6;
            this.SkipSplitUpdate.Text = "Skip Split and Update Livesplit";
            this.SkipSplitUpdate.UseVisualStyleBackColor = true;
            this.SkipSplitUpdate.Click += new System.EventHandler(this.SkipSplitUpdate_Click);
            // 
            // SkipSplitNoUpdate
            // 
            this.SkipSplitNoUpdate.Cursor = System.Windows.Forms.Cursors.No;
            this.SkipSplitNoUpdate.Enabled = false;
            this.SkipSplitNoUpdate.Location = new System.Drawing.Point(218, 130);
            this.SkipSplitNoUpdate.Name = "SkipSplitNoUpdate";
            this.SkipSplitNoUpdate.Size = new System.Drawing.Size(201, 23);
            this.SkipSplitNoUpdate.TabIndex = 7;
            this.SkipSplitNoUpdate.Text = "Skip Split and Do Not Update Livesplit";
            this.SkipSplitNoUpdate.UseVisualStyleBackColor = true;
            this.SkipSplitNoUpdate.Click += new System.EventHandler(this.SkipSplitNoUpdate_Click);
            // 
            // UndoSplitUpdate
            // 
            this.UndoSplitUpdate.Cursor = System.Windows.Forms.Cursors.No;
            this.UndoSplitUpdate.Enabled = false;
            this.UndoSplitUpdate.Location = new System.Drawing.Point(11, 159);
            this.UndoSplitUpdate.Name = "UndoSplitUpdate";
            this.UndoSplitUpdate.Size = new System.Drawing.Size(201, 23);
            this.UndoSplitUpdate.TabIndex = 8;
            this.UndoSplitUpdate.Text = "Undo Split and Update Livesplit";
            this.UndoSplitUpdate.UseVisualStyleBackColor = true;
            this.UndoSplitUpdate.Click += new System.EventHandler(this.UndoSplitUpdate_Click);
            // 
            // UndoSplitNoUpdate
            // 
            this.UndoSplitNoUpdate.Cursor = System.Windows.Forms.Cursors.No;
            this.UndoSplitNoUpdate.Enabled = false;
            this.UndoSplitNoUpdate.Location = new System.Drawing.Point(218, 159);
            this.UndoSplitNoUpdate.Name = "UndoSplitNoUpdate";
            this.UndoSplitNoUpdate.Size = new System.Drawing.Size(201, 23);
            this.UndoSplitNoUpdate.TabIndex = 9;
            this.UndoSplitNoUpdate.Text = "Undo Split and Do Not Update Livesplit";
            this.UndoSplitNoUpdate.UseVisualStyleBackColor = true;
            this.UndoSplitNoUpdate.Click += new System.EventHandler(this.UndoSplitNoUpdate_Click);
            // 
            // StartAutosplitter
            // 
            this.StartAutosplitter.Cursor = System.Windows.Forms.Cursors.No;
            this.StartAutosplitter.Enabled = false;
            this.StartAutosplitter.Location = new System.Drawing.Point(218, 72);
            this.StartAutosplitter.Name = "StartAutosplitter";
            this.StartAutosplitter.Size = new System.Drawing.Size(201, 23);
            this.StartAutosplitter.TabIndex = 10;
            this.StartAutosplitter.Text = "Start Autosplitter";
            this.StartAutosplitter.UseVisualStyleBackColor = true;
            this.StartAutosplitter.Click += new System.EventHandler(this.StartAutosplitter_Click);
            // 
            // ResetSplits
            // 
            this.ResetSplits.Cursor = System.Windows.Forms.Cursors.No;
            this.ResetSplits.Enabled = false;
            this.ResetSplits.Location = new System.Drawing.Point(218, 101);
            this.ResetSplits.Name = "ResetSplits";
            this.ResetSplits.Size = new System.Drawing.Size(202, 23);
            this.ResetSplits.TabIndex = 11;
            this.ResetSplits.Text = "Reset Splits";
            this.ResetSplits.UseVisualStyleBackColor = true;
            this.ResetSplits.Click += new System.EventHandler(this.ResetSplits_Click);
            // 
            // AutosplitterStatus
            // 
            this.AutosplitterStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AutosplitterStatus.Location = new System.Drawing.Point(12, 72);
            this.AutosplitterStatus.Name = "AutosplitterStatus";
            this.AutosplitterStatus.Size = new System.Drawing.Size(201, 23);
            this.AutosplitterStatus.TabIndex = 12;
            this.AutosplitterStatus.Text = "Autosplitter is not started";
            this.AutosplitterStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // OpenSplits
            // 
            this.OpenSplits.Filter = "Split Files (*.json)|*.json";
            // 
            // AutoSplitter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 195);
            this.Controls.Add(this.AutosplitterStatus);
            this.Controls.Add(this.ResetSplits);
            this.Controls.Add(this.StartAutosplitter);
            this.Controls.Add(this.UndoSplitNoUpdate);
            this.Controls.Add(this.UndoSplitUpdate);
            this.Controls.Add(this.SkipSplitNoUpdate);
            this.Controls.Add(this.SkipSplitUpdate);
            this.Controls.Add(this.NextSplit);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CurrentSplit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SplitFile);
            this.Controls.Add(this.LoadSplitFile);
            this.Name = "AutoSplitter";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button LoadSplitFile;
		private System.Windows.Forms.Label SplitFile;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label CurrentSplit;
		private System.Windows.Forms.Label NextSplit;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button SkipSplitUpdate;
		private System.Windows.Forms.Button SkipSplitNoUpdate;
		private System.Windows.Forms.Button UndoSplitUpdate;
		private System.Windows.Forms.Button UndoSplitNoUpdate;
		private System.Windows.Forms.Button StartAutosplitter;
		private System.Windows.Forms.Button ResetSplits;
		private System.Windows.Forms.Label AutosplitterStatus;
		private System.Windows.Forms.OpenFileDialog OpenSplits;
	}
}
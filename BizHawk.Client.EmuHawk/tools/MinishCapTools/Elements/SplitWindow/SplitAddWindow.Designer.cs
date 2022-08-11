using System.ComponentModel;

namespace MinishCapTools.Elements.SplitWindow
{
    partial class SplitAddWindow
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
            this.label1 = new System.Windows.Forms.Label();
            this.SplitName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SplitTypes = new System.Windows.Forms.ComboBox();
            this.MemoryDomains = new System.Windows.Forms.ComboBox();
            this.Enabled = new System.Windows.Forms.CheckBox();
            this.SaveSplit = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.AddressText = new System.Windows.Forms.Label();
            this.Address = new System.Windows.Forms.TextBox();
            this.ValueText = new System.Windows.Forms.Label();
            this.Value = new System.Windows.Forms.TextBox();
            this.HelpButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Split Name:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SplitName
            // 
            this.SplitName.Location = new System.Drawing.Point(118, 10);
            this.SplitName.Name = "SplitName";
            this.SplitName.Size = new System.Drawing.Size(155, 20);
            this.SplitName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "Split Type:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 18);
            this.label3.TabIndex = 3;
            this.label3.Text = "Memory Domain:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SplitTypes
            // 
            this.SplitTypes.FormattingEnabled = true;
            this.SplitTypes.Location = new System.Drawing.Point(118, 40);
            this.SplitTypes.Name = "SplitTypes";
            this.SplitTypes.Size = new System.Drawing.Size(155, 21);
            this.SplitTypes.TabIndex = 4;
            this.SplitTypes.SelectedIndexChanged += new System.EventHandler(this.SplitTypes_SelectedIndexChanged);
            // 
            // MemoryDomains
            // 
            this.MemoryDomains.FormattingEnabled = true;
            this.MemoryDomains.Location = new System.Drawing.Point(118, 70);
            this.MemoryDomains.Name = "MemoryDomains";
            this.MemoryDomains.Size = new System.Drawing.Size(155, 21);
            this.MemoryDomains.TabIndex = 5;
            // 
            // Enabled
            // 
            this.Enabled.AllowDrop = true;
            this.Enabled.Checked = true;
            this.Enabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Enabled.Location = new System.Drawing.Point(12, 162);
            this.Enabled.Name = "Enabled";
            this.Enabled.Size = new System.Drawing.Size(127, 24);
            this.Enabled.TabIndex = 6;
            this.Enabled.Text = "Enable Split";
            this.Enabled.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Enabled.UseVisualStyleBackColor = true;
            // 
            // SaveSplit
            // 
            this.SaveSplit.Location = new System.Drawing.Point(12, 192);
            this.SaveSplit.Name = "SaveSplit";
            this.SaveSplit.Size = new System.Drawing.Size(127, 23);
            this.SaveSplit.TabIndex = 7;
            this.SaveSplit.Text = "Save Split";
            this.SaveSplit.UseVisualStyleBackColor = true;
            this.SaveSplit.Click += new System.EventHandler(this.SaveSplit_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(146, 192);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(127, 23);
            this.Cancel.TabIndex = 8;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // AddressText
            // 
            this.AddressText.Location = new System.Drawing.Point(12, 100);
            this.AddressText.Name = "AddressText";
            this.AddressText.Size = new System.Drawing.Size(100, 18);
            this.AddressText.TabIndex = 9;
            this.AddressText.Text = "Area ID:";
            this.AddressText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Address
            // 
            this.Address.Location = new System.Drawing.Point(118, 100);
            this.Address.Name = "Address";
            this.Address.Size = new System.Drawing.Size(155, 20);
            this.Address.TabIndex = 10;
            // 
            // ValueText
            // 
            this.ValueText.Location = new System.Drawing.Point(12, 130);
            this.ValueText.Name = "ValueText";
            this.ValueText.Size = new System.Drawing.Size(100, 18);
            this.ValueText.TabIndex = 11;
            this.ValueText.Text = "Room ID:";
            this.ValueText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Value
            // 
            this.Value.Location = new System.Drawing.Point(118, 130);
            this.Value.Name = "Value";
            this.Value.Size = new System.Drawing.Size(155, 20);
            this.Value.TabIndex = 12;
            // 
            // HelpButton
            // 
            this.HelpButton.Location = new System.Drawing.Point(118, 162);
            this.HelpButton.Name = "HelpButton";
            this.HelpButton.Size = new System.Drawing.Size(154, 23);
            this.HelpButton.TabIndex = 13;
            this.HelpButton.Text = "What are these values?";
            this.HelpButton.UseVisualStyleBackColor = true;
            this.HelpButton.Click += new System.EventHandler(this.HelpButton_Click);
            // 
            // SplitAddWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 227);
            this.Controls.Add(this.HelpButton);
            this.Controls.Add(this.Value);
            this.Controls.Add(this.ValueText);
            this.Controls.Add(this.Address);
            this.Controls.Add(this.AddressText);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.SaveSplit);
            this.Controls.Add(this.Enabled);
            this.Controls.Add(this.MemoryDomains);
            this.Controls.Add(this.SplitTypes);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SplitName);
            this.Controls.Add(this.label1);
            this.Name = "SplitAddWindow";
            this.Text = "Add/Edit Split";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button HelpButton;

        private System.Windows.Forms.TextBox Value;

        private System.Windows.Forms.Label AddressText;
        private System.Windows.Forms.TextBox Address;
        private System.Windows.Forms.Label ValueText;

        private System.Windows.Forms.ComboBox SplitTypes;
        private System.Windows.Forms.ComboBox MemoryDomains;
        private System.Windows.Forms.CheckBox Enabled;
        private System.Windows.Forms.Button SaveSplit;
        private System.Windows.Forms.Button Cancel;

        private System.Windows.Forms.TextBox SplitName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;

        private System.Windows.Forms.Label label1;

        #endregion
    }
}
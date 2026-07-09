namespace Plc_Modbus
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvPlc = new DataGridView();
            cmbCoil1 = new ComboBox();
            cmbCoil2 = new ComboBox();
            panel1 = new Panel();
            lblCoil = new Label();
            btnWrite = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvPlc).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvPlc
            // 
            dgvPlc.BackgroundColor = SystemColors.Control;
            dgvPlc.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPlc.Location = new Point(105, 114);
            dgvPlc.Name = "dgvPlc";
            dgvPlc.RowHeadersVisible = false;
            dgvPlc.RowHeadersWidth = 51;
            dgvPlc.Size = new Size(609, 383);
            dgvPlc.TabIndex = 0;
            // 
            // cmbCoil1
            // 
            cmbCoil1.FormattingEnabled = true;
            cmbCoil1.Location = new Point(3, 37);
            cmbCoil1.Name = "cmbCoil1";
            cmbCoil1.Size = new Size(230, 28);
            cmbCoil1.TabIndex = 1;
            // 
            // cmbCoil2
            // 
            cmbCoil2.FormattingEnabled = true;
            cmbCoil2.Location = new Point(3, 90);
            cmbCoil2.Name = "cmbCoil2";
            cmbCoil2.Size = new Size(230, 28);
            cmbCoil2.TabIndex = 2;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnWrite);
            panel1.Controls.Add(cmbCoil2);
            panel1.Controls.Add(cmbCoil1);
            panel1.Location = new Point(843, 197);
            panel1.Name = "panel1";
            panel1.Size = new Size(237, 165);
            panel1.TabIndex = 3;
            // 
            // lblCoil
            // 
            lblCoil.AutoSize = true;
            lblCoil.Location = new Point(871, 188);
            lblCoil.Name = "lblCoil";
            lblCoil.Size = new Size(84, 20);
            lblCoil.TabIndex = 4;
            lblCoil.Text = "스위치조절";
            // 
            // btnWrite
            // 
            btnWrite.Location = new Point(3, 124);
            btnWrite.Name = "btnWrite";
            btnWrite.Size = new Size(83, 30);
            btnWrite.TabIndex = 3;
            btnWrite.Text = "저장하기";
            btnWrite.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 673);
            Controls.Add(lblCoil);
            Controls.Add(panel1);
            Controls.Add(dgvPlc);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgvPlc).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvPlc;
        private ComboBox cmbCoil1;
        private ComboBox cmbCoil2;
        private Panel panel1;
        private Label lblCoil;
        private Button btnWrite;
    }
}

namespace TicketQRCodeReader
{
    partial class FrmCodeReader
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pbCamera = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblTicketCategory = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblActivityName = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtQRCode = new System.Windows.Forms.TextBox();
            this.btnChange = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCamera
            // 
            this.pbCamera.Location = new System.Drawing.Point(41, 12);
            this.pbCamera.Name = "pbCamera";
            this.pbCamera.Size = new System.Drawing.Size(496, 279);
            this.pbCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbCamera.TabIndex = 0;
            this.pbCamera.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(72, 390);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 50);
            this.label1.TabIndex = 1;
            this.label1.Text = "姓名:";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblName.Location = new System.Drawing.Point(189, 390);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(0, 50);
            this.lblName.TabIndex = 2;
            // 
            // lblTicketCategory
            // 
            this.lblTicketCategory.AutoSize = true;
            this.lblTicketCategory.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTicketCategory.Location = new System.Drawing.Point(189, 561);
            this.lblTicketCategory.Name = "lblTicketCategory";
            this.lblTicketCategory.Size = new System.Drawing.Size(0, 50);
            this.lblTicketCategory.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(32, 561);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(151, 50);
            this.label4.TabIndex = 4;
            this.label4.Text = "票種類:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(562, 561);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 50);
            this.label5.TabIndex = 5;
            this.label5.Text = "場次:";
            // 
            // lblActivityName
            // 
            this.lblActivityName.AutoSize = true;
            this.lblActivityName.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblActivityName.Location = new System.Drawing.Point(679, 390);
            this.lblActivityName.Name = "lblActivityName";
            this.lblActivityName.Size = new System.Drawing.Size(0, 50);
            this.lblActivityName.TabIndex = 6;
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTime.Location = new System.Drawing.Point(679, 561);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(0, 50);
            this.lblTime.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label8.Location = new System.Drawing.Point(482, 390);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(191, 50);
            this.label8.TabIndex = 8;
            this.label8.Text = "活動名稱:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微軟正黑體", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label9.Location = new System.Drawing.Point(562, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(190, 50);
            this.label9.TabIndex = 9;
            this.label9.Text = "QRCode:";
            // 
            // txtQRCode
            // 
            this.txtQRCode.BackColor = System.Drawing.SystemColors.Control;
            this.txtQRCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtQRCode.Enabled = false;
            this.txtQRCode.Font = new System.Drawing.Font("新細明體", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtQRCode.Location = new System.Drawing.Point(571, 91);
            this.txtQRCode.Multiline = true;
            this.txtQRCode.Name = "txtQRCode";
            this.txtQRCode.ReadOnly = true;
            this.txtQRCode.Size = new System.Drawing.Size(532, 200);
            this.txtQRCode.TabIndex = 10;
            // 
            // btnChange
            // 
            this.btnChange.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnChange.Location = new System.Drawing.Point(40, 297);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new System.Drawing.Size(143, 47);
            this.btnChange.TabIndex = 11;
            this.btnChange.Text = "切換設備";
            this.btnChange.UseVisualStyleBackColor = true;
            this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
            // 
            // FrmCodeReader
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1134, 735);
            this.Controls.Add(this.btnChange);
            this.Controls.Add(this.txtQRCode);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblActivityName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblTicketCategory);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pbCamera);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmCodeReader";
            this.Text = "電子票掃描機";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmCodeReader_FormClosing);
            this.Load += new System.EventHandler(this.FrmCodeReader_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbCamera;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblTicketCategory;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblActivityName;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtQRCode;
        private System.Windows.Forms.Button btnChange;
    }
}


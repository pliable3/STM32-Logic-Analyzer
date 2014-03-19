namespace LogicAnalyzer
{
    partial class SamplingConfig
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
            this.label1 = new System.Windows.Forms.Label();
            this.channels = new System.Windows.Forms.ComboBox();
            this.samplingRate = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.startTrigger = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.compression = new System.Windows.Forms.CheckBox();
            this.dataModeTransitions = new System.Windows.Forms.RadioButton();
            this.dataModeContinuous = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.samplingTime = new System.Windows.Forms.TextBox();
            this.serialPortName = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.testController = new System.Windows.Forms.RadioButton();
            this.ethernetController = new System.Windows.Forms.RadioButton();
            this.serialPortController = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(77, 197);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "No. of channels to sample:";
            // 
            // channels
            // 
            this.channels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.channels.FormattingEnabled = true;
            this.channels.Location = new System.Drawing.Point(242, 197);
            this.channels.Name = "channels";
            this.channels.Size = new System.Drawing.Size(143, 21);
            this.channels.TabIndex = 3;
            // 
            // samplingRate
            // 
            this.samplingRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.samplingRate.FormattingEnabled = true;
            this.samplingRate.Location = new System.Drawing.Point(242, 243);
            this.samplingRate.Name = "samplingRate";
            this.samplingRate.Size = new System.Drawing.Size(143, 21);
            this.samplingRate.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(148, 243);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Sampling rate:";
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(119, 389);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 35);
            this.ok.TabIndex = 98;
            this.ok.Text = "Ok";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(269, 389);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 35);
            this.cancel.TabIndex = 99;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // startTrigger
            // 
            this.startTrigger.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.startTrigger.Enabled = false;
            this.startTrigger.FormattingEnabled = true;
            this.startTrigger.Items.AddRange(new object[] {
            "No Trigger",
            "CH1 HI",
            "CH1 LO",
            "CH2 HI",
            "CH2 LO",
            "CH3 HI",
            "CH3 LO",
            "CH4 HI",
            "CH4 LO",
            "CH5 HI",
            "CH5 LO",
            "CH6 HI",
            "CH6 LO",
            "CH7 HI",
            "CH7 LO",
            "CH8 HI",
            "CH8 LO"});
            this.startTrigger.Location = new System.Drawing.Point(242, 335);
            this.startTrigger.Name = "startTrigger";
            this.startTrigger.Size = new System.Drawing.Size(143, 21);
            this.startTrigger.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Enabled = false;
            this.label3.Location = new System.Drawing.Point(158, 335);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Start trigger:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.compression);
            this.groupBox1.Controls.Add(this.dataModeTransitions);
            this.groupBox1.Controls.Add(this.dataModeContinuous);
            this.groupBox1.ForeColor = System.Drawing.Color.Blue;
            this.groupBox1.Location = new System.Drawing.Point(235, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(210, 100);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data Mode";
            // 
            // compression
            // 
            this.compression.AutoSize = true;
            this.compression.ForeColor = System.Drawing.SystemColors.ControlText;
            this.compression.Location = new System.Drawing.Point(17, 77);
            this.compression.Name = "compression";
            this.compression.Size = new System.Drawing.Size(186, 17);
            this.compression.TabIndex = 2;
            this.compression.Text = "Compress the sample stream";
            this.compression.UseVisualStyleBackColor = true;
            // 
            // dataModeTransitions
            // 
            this.dataModeTransitions.AutoSize = true;
            this.dataModeTransitions.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dataModeTransitions.Location = new System.Drawing.Point(17, 42);
            this.dataModeTransitions.Name = "dataModeTransitions";
            this.dataModeTransitions.Size = new System.Drawing.Size(114, 17);
            this.dataModeTransitions.TabIndex = 1;
            this.dataModeTransitions.TabStop = true;
            this.dataModeTransitions.Text = "Transitions only";
            this.dataModeTransitions.UseVisualStyleBackColor = true;
            this.dataModeTransitions.CheckedChanged += new System.EventHandler(this.dataModeTransitions_CheckedChanged);
            // 
            // dataModeContinuous
            // 
            this.dataModeContinuous.AutoSize = true;
            this.dataModeContinuous.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dataModeContinuous.Location = new System.Drawing.Point(17, 19);
            this.dataModeContinuous.Name = "dataModeContinuous";
            this.dataModeContinuous.Size = new System.Drawing.Size(129, 17);
            this.dataModeContinuous.TabIndex = 0;
            this.dataModeContinuous.TabStop = true;
            this.dataModeContinuous.Text = "Continuous stream";
            this.dataModeContinuous.UseVisualStyleBackColor = true;
            this.dataModeContinuous.CheckedChanged += new System.EventHandler(this.dataModeTransitions_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(120, 289);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 101;
            this.label4.Text = "Sampling time (ms):";
            // 
            // samplingTime
            // 
            this.samplingTime.Location = new System.Drawing.Point(242, 289);
            this.samplingTime.Name = "samplingTime";
            this.samplingTime.Size = new System.Drawing.Size(143, 20);
            this.samplingTime.TabIndex = 5;
            // 
            // serialPortName
            // 
            this.serialPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.serialPortName.FormattingEnabled = true;
            this.serialPortName.Location = new System.Drawing.Point(242, 151);
            this.serialPortName.Name = "serialPortName";
            this.serialPortName.Size = new System.Drawing.Size(143, 21);
            this.serialPortName.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(133, 151);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 13);
            this.label5.TabIndex = 103;
            this.label5.Text = "Serial port name:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.testController);
            this.groupBox2.Controls.Add(this.ethernetController);
            this.groupBox2.Controls.Add(this.serialPortController);
            this.groupBox2.ForeColor = System.Drawing.Color.Blue;
            this.groupBox2.Location = new System.Drawing.Point(17, 25);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(210, 100);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Controller";
            // 
            // testController
            // 
            this.testController.AutoSize = true;
            this.testController.ForeColor = System.Drawing.SystemColors.ControlText;
            this.testController.Location = new System.Drawing.Point(17, 65);
            this.testController.Name = "testController";
            this.testController.Size = new System.Drawing.Size(108, 17);
            this.testController.TabIndex = 2;
            this.testController.Text = "Test Controller";
            this.testController.UseVisualStyleBackColor = true;
            this.testController.CheckedChanged += new System.EventHandler(this.serialPortController_CheckedChanged);
            // 
            // ethernetController
            // 
            this.ethernetController.AutoSize = true;
            this.ethernetController.Enabled = false;
            this.ethernetController.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ethernetController.Location = new System.Drawing.Point(17, 42);
            this.ethernetController.Name = "ethernetController";
            this.ethernetController.Size = new System.Drawing.Size(131, 17);
            this.ethernetController.TabIndex = 1;
            this.ethernetController.Text = "Ethernet Controller";
            this.ethernetController.UseVisualStyleBackColor = true;
            // 
            // serialPortController
            // 
            this.serialPortController.AutoSize = true;
            this.serialPortController.Checked = true;
            this.serialPortController.ForeColor = System.Drawing.SystemColors.ControlText;
            this.serialPortController.Location = new System.Drawing.Point(17, 19);
            this.serialPortController.Name = "serialPortController";
            this.serialPortController.Size = new System.Drawing.Size(142, 17);
            this.serialPortController.TabIndex = 0;
            this.serialPortController.TabStop = true;
            this.serialPortController.Text = "Serial Port Controller";
            this.serialPortController.UseVisualStyleBackColor = true;
            this.serialPortController.CheckedChanged += new System.EventHandler(this.serialPortController_CheckedChanged);
            // 
            // SamplingConfig
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(462, 457);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.serialPortName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.samplingTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.startTrigger);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.samplingRate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.channels);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SamplingConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sampling Configuration";
            this.Load += new System.EventHandler(this.SamplingConfig_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox channels;
        private System.Windows.Forms.ComboBox samplingRate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.ComboBox startTrigger;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox compression;
        private System.Windows.Forms.RadioButton dataModeTransitions;
        private System.Windows.Forms.RadioButton dataModeContinuous;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox samplingTime;
        private System.Windows.Forms.ComboBox serialPortName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton testController;
        private System.Windows.Forms.RadioButton ethernetController;
        private System.Windows.Forms.RadioButton serialPortController;
    }
}
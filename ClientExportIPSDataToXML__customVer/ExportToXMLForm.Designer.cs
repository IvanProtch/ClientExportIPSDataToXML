
namespace ClientExportIPSDataToXML
{
    partial class ExportToXMLForm
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
            this.exportModes_comboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.selectObjsToExp_button = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.runExport_button = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.конфигурацииToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.загрузитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.selectedObjs_comboBox = new System.Windows.Forms.ComboBox();
            this.selectIPSConfig_button = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.configurationIPS_label = new System.Windows.Forms.Label();
            this.exportPath_textBox = new System.Windows.Forms.TextBox();
            this.exportPathSelect_button = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.procInfo_label = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // exportModes_comboBox
            // 
            this.exportModes_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.exportModes_comboBox.FormattingEnabled = true;
            this.exportModes_comboBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.exportModes_comboBox.Items.AddRange(new object[] {
            "Через извещение",
            "Через сборку"});
            this.exportModes_comboBox.Location = new System.Drawing.Point(14, 83);
            this.exportModes_comboBox.Name = "exportModes_comboBox";
            this.exportModes_comboBox.Size = new System.Drawing.Size(138, 21);
            this.exportModes_comboBox.TabIndex = 0;
            this.exportModes_comboBox.SelectedIndexChanged += new System.EventHandler(this.exportModes_comboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Режим выгрузки";
            // 
            // selectObjsToExp_button
            // 
            this.selectObjsToExp_button.Location = new System.Drawing.Point(385, 96);
            this.selectObjsToExp_button.Name = "selectObjsToExp_button";
            this.selectObjsToExp_button.Size = new System.Drawing.Size(149, 23);
            this.selectObjsToExp_button.TabIndex = 2;
            this.selectObjsToExp_button.Text = "Экспортируемые объекты";
            this.selectObjsToExp_button.UseVisualStyleBackColor = true;
            this.selectObjsToExp_button.Click += new System.EventHandler(this.selectObjsToExp_button_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(11, 221);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(359, 23);
            this.progressBar.TabIndex = 4;
            // 
            // runExport_button
            // 
            this.runExport_button.Location = new System.Drawing.Point(385, 221);
            this.runExport_button.Name = "runExport_button";
            this.runExport_button.Size = new System.Drawing.Size(149, 23);
            this.runExport_button.TabIndex = 5;
            this.runExport_button.Text = "Начать выгрузку";
            this.runExport_button.UseVisualStyleBackColor = true;
            this.runExport_button.Click += new System.EventHandler(this.runExport_button_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.конфигурацииToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(546, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // конфигурацииToolStripMenuItem
            // 
            this.конфигурацииToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.загрузитьToolStripMenuItem,
            this.сохранитьToolStripMenuItem});
            this.конфигурацииToolStripMenuItem.Name = "конфигурацииToolStripMenuItem";
            this.конфигурацииToolStripMenuItem.Size = new System.Drawing.Size(101, 20);
            this.конфигурацииToolStripMenuItem.Text = "Конфигурации";
            // 
            // загрузитьToolStripMenuItem
            // 
            this.загрузитьToolStripMenuItem.Name = "загрузитьToolStripMenuItem";
            this.загрузитьToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.загрузитьToolStripMenuItem.Text = "Загрузить";
            // 
            // сохранитьToolStripMenuItem
            // 
            this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            this.сохранитьToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.сохранитьToolStripMenuItem.Text = "Сохранить";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 160);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Объекты";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Выбрано:";
            // 
            // selectedObjs_comboBox
            // 
            this.selectedObjs_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectedObjs_comboBox.FormattingEnabled = true;
            this.selectedObjs_comboBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.selectedObjs_comboBox.Location = new System.Drawing.Point(73, 157);
            this.selectedObjs_comboBox.Name = "selectedObjs_comboBox";
            this.selectedObjs_comboBox.Size = new System.Drawing.Size(461, 21);
            this.selectedObjs_comboBox.TabIndex = 14;
            // 
            // selectIPSConfig_button
            // 
            this.selectIPSConfig_button.Location = new System.Drawing.Point(385, 64);
            this.selectIPSConfig_button.Name = "selectIPSConfig_button";
            this.selectIPSConfig_button.Size = new System.Drawing.Size(149, 23);
            this.selectIPSConfig_button.TabIndex = 15;
            this.selectIPSConfig_button.Text = "Конфигурация экспорта IPS";
            this.selectIPSConfig_button.UseVisualStyleBackColor = true;
            this.selectIPSConfig_button.Click += new System.EventHandler(this.selectIPSConfig_button_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 181);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(150, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Конфигурация экспорта IPS";
            // 
            // configurationIPS_label
            // 
            this.configurationIPS_label.AutoSize = true;
            this.configurationIPS_label.Location = new System.Drawing.Point(167, 181);
            this.configurationIPS_label.Name = "configurationIPS_label";
            this.configurationIPS_label.Size = new System.Drawing.Size(10, 13);
            this.configurationIPS_label.TabIndex = 19;
            this.configurationIPS_label.Text = "-";
            // 
            // exportPath_textBox
            // 
            this.exportPath_textBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.exportPath_textBox.Location = new System.Drawing.Point(14, 34);
            this.exportPath_textBox.Name = "exportPath_textBox";
            this.exportPath_textBox.Size = new System.Drawing.Size(490, 20);
            this.exportPath_textBox.TabIndex = 23;
            // 
            // exportPathSelect_button
            // 
            this.exportPathSelect_button.Location = new System.Drawing.Point(510, 34);
            this.exportPathSelect_button.Name = "exportPathSelect_button";
            this.exportPathSelect_button.Size = new System.Drawing.Size(24, 21);
            this.exportPathSelect_button.TabIndex = 22;
            this.exportPathSelect_button.Text = "...";
            this.exportPathSelect_button.UseVisualStyleBackColor = true;
            this.exportPathSelect_button.Click += new System.EventHandler(this.exportPathSelect_button_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 13);
            this.label3.TabIndex = 24;
            // 
            // procInfo_label
            // 
            this.procInfo_label.AutoSize = true;
            this.procInfo_label.Location = new System.Drawing.Point(11, 251);
            this.procInfo_label.Name = "procInfo_label";
            this.procInfo_label.Size = new System.Drawing.Size(0, 13);
            this.procInfo_label.TabIndex = 25;
            // 
            // ExportToXMLForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 268);
            this.Controls.Add(this.procInfo_label);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.exportPath_textBox);
            this.Controls.Add(this.exportPathSelect_button);
            this.Controls.Add(this.configurationIPS_label);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.selectIPSConfig_button);
            this.Controls.Add(this.selectedObjs_comboBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.runExport_button);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.selectObjsToExp_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.exportModes_comboBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximumSize = new System.Drawing.Size(562, 307);
            this.MinimumSize = new System.Drawing.Size(562, 307);
            this.Name = "ExportToXMLForm";
            this.Text = "Экспорт в XML";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox exportModes_comboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button selectObjsToExp_button;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button runExport_button;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem конфигурацииToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem загрузитьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox selectedObjs_comboBox;
        private System.Windows.Forms.Button selectIPSConfig_button;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label configurationIPS_label;
        private System.Windows.Forms.TextBox exportPath_textBox;
        private System.Windows.Forms.Button exportPathSelect_button;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label procInfo_label;
    }
}
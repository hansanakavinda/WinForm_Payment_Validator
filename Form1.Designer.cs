namespace Payment_Validator
{
    partial class mainForm
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
            btnUpload = new Button();
            btnValidate = new Button();
            dataView = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dataView).BeginInit();
            SuspendLayout();
            // 
            // btnUpload
            // 
            btnUpload.BackColor = SystemColors.GradientActiveCaption;
            btnUpload.Location = new Point(39, 35);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(141, 43);
            btnUpload.TabIndex = 0;
            btnUpload.Text = "Upload File";
            btnUpload.UseVisualStyleBackColor = false;
            btnUpload.Click += btnUpload_Click;
            // 
            // btnValidate
            // 
            btnValidate.BackColor = SystemColors.GradientActiveCaption;
            btnValidate.Location = new Point(299, 35);
            btnValidate.Name = "btnValidate";
            btnValidate.Size = new Size(141, 43);
            btnValidate.TabIndex = 1;
            btnValidate.Text = "Validate Payments";
            btnValidate.UseVisualStyleBackColor = false;
            btnValidate.Click += btnValidate_Click;
            // 
            // dataView
            // 
            dataView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataView.Location = new Point(39, 122);
            dataView.Name = "dataView";
            dataView.ReadOnly = true;
            dataView.RowHeadersWidth = 51;
            dataView.Size = new Size(734, 460);
            dataView.TabIndex = 2;
            // 
            // mainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(815, 620);
            Controls.Add(dataView);
            Controls.Add(btnValidate);
            Controls.Add(btnUpload);
            Name = "mainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Payment Validator";
            ((System.ComponentModel.ISupportInitialize)dataView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnUpload;
        private Button btnValidate;
        private DataGridView dataView;
    }
}

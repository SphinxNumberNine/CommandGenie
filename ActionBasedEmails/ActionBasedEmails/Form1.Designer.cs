namespace ActionBasedEmails
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
            this.edgeCredentialsLabel = new System.Windows.Forms.Label();
            this.edgeUsernameBox = new System.Windows.Forms.TextBox();
            this.edgeUsernameLabel = new System.Windows.Forms.Label();
            this.edgePasswordLabel = new System.Windows.Forms.Label();
            this.edgePasswordBox = new System.Windows.Forms.TextBox();
            this.submitButton = new System.Windows.Forms.Button();
            this.uploadToEdgeCheckBox = new System.Windows.Forms.CheckBox();
            this.priviledgedUsersBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.newPriviledgedUserBox = new System.Windows.Forms.TextBox();
            this.addPriviledgedUserBox = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.edgeUploadPathLabel = new System.Windows.Forms.Label();
            this.edgeUploadPathBox = new System.Windows.Forms.TextBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.exchangeUsernameBox = new System.Windows.Forms.TextBox();
            this.exchangePasswordBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.responseEmailAddressBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // edgeCredentialsLabel
            // 
            this.edgeCredentialsLabel.AutoSize = true;
            this.edgeCredentialsLabel.Location = new System.Drawing.Point(106, 341);
            this.edgeCredentialsLabel.Name = "edgeCredentialsLabel";
            this.edgeCredentialsLabel.Size = new System.Drawing.Size(87, 13);
            this.edgeCredentialsLabel.TabIndex = 0;
            this.edgeCredentialsLabel.Text = "Edge Credentials";
            this.edgeCredentialsLabel.Visible = false;
            // 
            // edgeUsernameBox
            // 
            this.edgeUsernameBox.Location = new System.Drawing.Point(52, 357);
            this.edgeUsernameBox.Name = "edgeUsernameBox";
            this.edgeUsernameBox.Size = new System.Drawing.Size(222, 20);
            this.edgeUsernameBox.TabIndex = 1;
            this.edgeUsernameBox.Visible = false;
            // 
            // edgeUsernameLabel
            // 
            this.edgeUsernameLabel.AutoSize = true;
            this.edgeUsernameLabel.Location = new System.Drawing.Point(11, 360);
            this.edgeUsernameLabel.Name = "edgeUsernameLabel";
            this.edgeUsernameLabel.Size = new System.Drawing.Size(32, 13);
            this.edgeUsernameLabel.TabIndex = 2;
            this.edgeUsernameLabel.Text = "User:";
            this.edgeUsernameLabel.Visible = false;
            // 
            // edgePasswordLabel
            // 
            this.edgePasswordLabel.AutoSize = true;
            this.edgePasswordLabel.Location = new System.Drawing.Point(11, 386);
            this.edgePasswordLabel.Name = "edgePasswordLabel";
            this.edgePasswordLabel.Size = new System.Drawing.Size(33, 13);
            this.edgePasswordLabel.TabIndex = 3;
            this.edgePasswordLabel.Text = "Pass:";
            this.edgePasswordLabel.Visible = false;
            // 
            // edgePasswordBox
            // 
            this.edgePasswordBox.Location = new System.Drawing.Point(52, 383);
            this.edgePasswordBox.Name = "edgePasswordBox";
            this.edgePasswordBox.PasswordChar = '*';
            this.edgePasswordBox.Size = new System.Drawing.Size(222, 20);
            this.edgePasswordBox.TabIndex = 4;
            this.edgePasswordBox.Visible = false;
            // 
            // submitButton
            // 
            this.submitButton.Location = new System.Drawing.Point(197, 434);
            this.submitButton.Name = "submitButton";
            this.submitButton.Size = new System.Drawing.Size(75, 25);
            this.submitButton.TabIndex = 5;
            this.submitButton.Text = "Submit";
            this.submitButton.UseVisualStyleBackColor = true;
            this.submitButton.Click += new System.EventHandler(this.submitButton_Click);
            // 
            // uploadToEdgeCheckBox
            // 
            this.uploadToEdgeCheckBox.AutoSize = true;
            this.uploadToEdgeCheckBox.Location = new System.Drawing.Point(14, 321);
            this.uploadToEdgeCheckBox.Name = "uploadToEdgeCheckBox";
            this.uploadToEdgeCheckBox.Size = new System.Drawing.Size(151, 17);
            this.uploadToEdgeCheckBox.TabIndex = 6;
            this.uploadToEdgeCheckBox.Text = "Upload responses to Edge";
            this.uploadToEdgeCheckBox.UseVisualStyleBackColor = true;
            this.uploadToEdgeCheckBox.CheckedChanged += new System.EventHandler(this.uploadToEdgeCheckBox_CheckedChanged);
            // 
            // priviledgedUsersBox
            // 
            this.priviledgedUsersBox.Location = new System.Drawing.Point(12, 197);
            this.priviledgedUsersBox.Multiline = true;
            this.priviledgedUsersBox.Name = "priviledgedUsersBox";
            this.priviledgedUsersBox.ReadOnly = true;
            this.priviledgedUsersBox.Size = new System.Drawing.Size(258, 94);
            this.priviledgedUsersBox.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(77, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Priviledged Users Email Ids";
            // 
            // newPriviledgedUserBox
            // 
            this.newPriviledgedUserBox.Location = new System.Drawing.Point(14, 171);
            this.newPriviledgedUserBox.Name = "newPriviledgedUserBox";
            this.newPriviledgedUserBox.Size = new System.Drawing.Size(179, 20);
            this.newPriviledgedUserBox.TabIndex = 9;
            // 
            // addPriviledgedUserBox
            // 
            this.addPriviledgedUserBox.Location = new System.Drawing.Point(199, 168);
            this.addPriviledgedUserBox.Name = "addPriviledgedUserBox";
            this.addPriviledgedUserBox.Size = new System.Drawing.Size(71, 23);
            this.addPriviledgedUserBox.TabIndex = 10;
            this.addPriviledgedUserBox.Text = "Add";
            this.addPriviledgedUserBox.UseVisualStyleBackColor = true;
            this.addPriviledgedUserBox.Click += new System.EventHandler(this.addPriviledgedUserBox_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 294);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(265, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "___________________________________________";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 131);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(265, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "___________________________________________";
            // 
            // edgeUploadPathLabel
            // 
            this.edgeUploadPathLabel.AutoSize = true;
            this.edgeUploadPathLabel.Location = new System.Drawing.Point(11, 410);
            this.edgeUploadPathLabel.Name = "edgeUploadPathLabel";
            this.edgeUploadPathLabel.Size = new System.Drawing.Size(69, 13);
            this.edgeUploadPathLabel.TabIndex = 13;
            this.edgeUploadPathLabel.Text = "Upload Path:";
            this.edgeUploadPathLabel.Visible = false;
            // 
            // edgeUploadPathBox
            // 
            this.edgeUploadPathBox.Location = new System.Drawing.Point(87, 407);
            this.edgeUploadPathBox.Name = "edgeUploadPathBox";
            this.edgeUploadPathBox.Size = new System.Drawing.Size(187, 20);
            this.edgeUploadPathBox.TabIndex = 14;
            this.edgeUploadPathBox.Visible = false;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(39, 9);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(207, 17);
            this.titleLabel.TabIndex = 15;
            this.titleLabel.Text = "Action Emails Configuration";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(265, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "___________________________________________";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(102, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Mail Configuration";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Username: ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Password:";
            // 
            // exchangeUsernameBox
            // 
            this.exchangeUsernameBox.Location = new System.Drawing.Point(80, 69);
            this.exchangeUsernameBox.Name = "exchangeUsernameBox";
            this.exchangeUsernameBox.Size = new System.Drawing.Size(190, 20);
            this.exchangeUsernameBox.TabIndex = 20;
            // 
            // exchangePasswordBox
            // 
            this.exchangePasswordBox.Location = new System.Drawing.Point(80, 92);
            this.exchangePasswordBox.Name = "exchangePasswordBox";
            this.exchangePasswordBox.PasswordChar = '*';
            this.exchangePasswordBox.Size = new System.Drawing.Size(190, 20);
            this.exchangePasswordBox.TabIndex = 21;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 118);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(98, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Send responses to:";
            // 
            // responseEmailAddressBox
            // 
            this.responseEmailAddressBox.Location = new System.Drawing.Point(109, 115);
            this.responseEmailAddressBox.Name = "responseEmailAddressBox";
            this.responseEmailAddressBox.Size = new System.Drawing.Size(161, 20);
            this.responseEmailAddressBox.TabIndex = 23;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 465);
            this.Controls.Add(this.responseEmailAddressBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.exchangePasswordBox);
            this.Controls.Add(this.exchangeUsernameBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.edgeUploadPathBox);
            this.Controls.Add(this.edgeUploadPathLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.addPriviledgedUserBox);
            this.Controls.Add(this.newPriviledgedUserBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.priviledgedUsersBox);
            this.Controls.Add(this.uploadToEdgeCheckBox);
            this.Controls.Add(this.submitButton);
            this.Controls.Add(this.edgePasswordBox);
            this.Controls.Add(this.edgePasswordLabel);
            this.Controls.Add(this.edgeUsernameLabel);
            this.Controls.Add(this.edgeUsernameBox);
            this.Controls.Add(this.edgeCredentialsLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label edgeCredentialsLabel;
        private System.Windows.Forms.TextBox edgeUsernameBox;
        private System.Windows.Forms.Label edgeUsernameLabel;
        private System.Windows.Forms.Label edgePasswordLabel;
        private System.Windows.Forms.TextBox edgePasswordBox;
        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.CheckBox uploadToEdgeCheckBox;
        private System.Windows.Forms.TextBox priviledgedUsersBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox newPriviledgedUserBox;
        private System.Windows.Forms.Button addPriviledgedUserBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label edgeUploadPathLabel;
        private System.Windows.Forms.TextBox edgeUploadPathBox;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox exchangeUsernameBox;
        private System.Windows.Forms.TextBox exchangePasswordBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox responseEmailAddressBox;
    }
}


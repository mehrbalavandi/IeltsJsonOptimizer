namespace IeltsJsonOptimizer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            rtbInput = new RichTextBox();
            rtbOutput = new RichTextBox();
            btnProcess = new Button();
            SuspendLayout();
            // 
            // rtbInput
            // 
            rtbInput.Location = new Point(12, 12);
            rtbInput.Name = "rtbInput";
            rtbInput.Size = new Size(552, 606);
            rtbInput.TabIndex = 0;
            rtbInput.Text = resources.GetString("rtbInput.Text");
            // 
            // rtbOutput
            // 
            rtbOutput.Location = new Point(570, 12);
            rtbOutput.Name = "rtbOutput";
            rtbOutput.Size = new Size(552, 606);
            rtbOutput.TabIndex = 1;
            rtbOutput.Text = "";
            // 
            // btnProcess
            // 
            btnProcess.Location = new Point(1046, 629);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(75, 23);
            btnProcess.TabIndex = 2;
            btnProcess.Text = "Process";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += btnProcess_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1133, 664);
            Controls.Add(btnProcess);
            Controls.Add(rtbOutput);
            Controls.Add(rtbInput);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox rtbInput;
        private RichTextBox rtbOutput;
        private Button btnProcess;
    }
}

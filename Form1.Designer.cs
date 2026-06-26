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
            rtbJsonToSend = new RichTextBox();
            rtbAiResponse = new RichTextBox();
            btnGeneratePayload = new Button();
            btnMergeAiResponse = new Button();
            SuspendLayout();
            // 
            // rtbInput
            // 
            rtbInput.Location = new Point(12, 12);
            rtbInput.Name = "rtbInput";
            rtbInput.Size = new Size(285, 606);
            rtbInput.TabIndex = 0;
            rtbInput.Text = resources.GetString("rtbInput.Text");
            // 
            // rtbOutput
            // 
            rtbOutput.Location = new Point(885, 12);
            rtbOutput.Name = "rtbOutput";
            rtbOutput.Size = new Size(285, 606);
            rtbOutput.TabIndex = 1;
            rtbOutput.Text = "";
            // 
            // btnProcess
            // 
            btnProcess.Location = new Point(241, 631);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(75, 23);
            btnProcess.TabIndex = 2;
            btnProcess.Text = "Process";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += btnProcess_Click;
            // 
            // rtbJsonToSend
            // 
            rtbJsonToSend.Location = new Point(303, 12);
            rtbJsonToSend.Name = "rtbJsonToSend";
            rtbJsonToSend.Size = new Size(285, 606);
            rtbJsonToSend.TabIndex = 3;
            rtbJsonToSend.Text = "\n";
            // 
            // rtbAiResponse
            // 
            rtbAiResponse.Location = new Point(594, 12);
            rtbAiResponse.Name = "rtbAiResponse";
            rtbAiResponse.Size = new Size(285, 606);
            rtbAiResponse.TabIndex = 4;
            rtbAiResponse.Text = "\n";
            // 
            // btnGeneratePayload
            // 
            btnGeneratePayload.AutoSize = true;
            btnGeneratePayload.Location = new Point(12, 629);
            btnGeneratePayload.Name = "btnGeneratePayload";
            btnGeneratePayload.Size = new Size(106, 25);
            btnGeneratePayload.TabIndex = 5;
            btnGeneratePayload.Text = "GeneratePayload";
            btnGeneratePayload.UseVisualStyleBackColor = true;
            btnGeneratePayload.Click += btnGeneratePayload_Click;
            // 
            // btnMergeAiResponse
            // 
            btnMergeAiResponse.AutoSize = true;
            btnMergeAiResponse.Location = new Point(123, 629);
            btnMergeAiResponse.Name = "btnMergeAiResponse";
            btnMergeAiResponse.Size = new Size(112, 25);
            btnMergeAiResponse.TabIndex = 6;
            btnMergeAiResponse.Text = "MergeAiResponse";
            btnMergeAiResponse.UseVisualStyleBackColor = true;
            btnMergeAiResponse.Click += btnMergeAiResponse_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1182, 664);
            Controls.Add(btnMergeAiResponse);
            Controls.Add(btnGeneratePayload);
            Controls.Add(rtbAiResponse);
            Controls.Add(rtbJsonToSend);
            Controls.Add(btnProcess);
            Controls.Add(rtbOutput);
            Controls.Add(rtbInput);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtbInput;
        private RichTextBox rtbOutput;
        private Button btnProcess;
        private RichTextBox rtbJsonToSend;
        private RichTextBox rtbAiResponse;
        private Button btnGeneratePayload;
        private Button btnMergeAiResponse;
    }
}

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
            rtbJsonToSend = new RichTextBox();
            rtbAiResponse = new RichTextBox();
            btnGeneratePayload = new Button();
            btnMergeAiResponse = new Button();
            btnCopyPayload = new Button();
            btnCopyOutput = new Button();
            btnPasteInput = new Button();
            btnPasteResponse = new Button();
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
            rtbAiResponse.Text = resources.GetString("rtbAiResponse.Text");
            // 
            // btnGeneratePayload
            // 
            btnGeneratePayload.AutoSize = true;
            btnGeneratePayload.Location = new Point(12, 683);
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
            btnMergeAiResponse.Location = new Point(124, 683);
            btnMergeAiResponse.Name = "btnMergeAiResponse";
            btnMergeAiResponse.Size = new Size(112, 25);
            btnMergeAiResponse.TabIndex = 6;
            btnMergeAiResponse.Text = "MergeAiResponse";
            btnMergeAiResponse.UseVisualStyleBackColor = true;
            btnMergeAiResponse.Click += btnMergeAiResponse_Click;
            // 
            // btnCopyPayload
            // 
            btnCopyPayload.AutoSize = true;
            btnCopyPayload.Location = new Point(559, 593);
            btnCopyPayload.Name = "btnCopyPayload";
            btnCopyPayload.Size = new Size(29, 25);
            btnCopyPayload.TabIndex = 9;
            btnCopyPayload.Text = "⧉";
            btnCopyPayload.UseVisualStyleBackColor = true;
            btnCopyPayload.Click += btnCopyPayload_Click;
            // 
            // btnCopyOutput
            // 
            btnCopyOutput.AutoSize = true;
            btnCopyOutput.Location = new Point(1141, 593);
            btnCopyOutput.Name = "btnCopyOutput";
            btnCopyOutput.Size = new Size(29, 25);
            btnCopyOutput.TabIndex = 11;
            btnCopyOutput.Text = "⧉";
            btnCopyOutput.UseVisualStyleBackColor = true;
            btnCopyOutput.Click += btnCopyOutput_Click;
            // 
            // btnPasteInput
            // 
            btnPasteInput.AutoSize = true;
            btnPasteInput.Location = new Point(12, 593);
            btnPasteInput.Name = "btnPasteInput";
            btnPasteInput.Size = new Size(29, 25);
            btnPasteInput.TabIndex = 12;
            btnPasteInput.Text = "📋";
            btnPasteInput.UseVisualStyleBackColor = true;
            btnPasteInput.Click += btnPasteInput_Click;
            // 
            // btnPasteResponse
            // 
            btnPasteResponse.AutoSize = true;
            btnPasteResponse.Location = new Point(594, 593);
            btnPasteResponse.Name = "btnPasteResponse";
            btnPasteResponse.Size = new Size(29, 25);
            btnPasteResponse.TabIndex = 13;
            btnPasteResponse.Text = "📋";
            btnPasteResponse.UseVisualStyleBackColor = true;
            btnPasteResponse.Click += btnPasteResponse_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1182, 720);
            Controls.Add(btnPasteResponse);
            Controls.Add(btnPasteInput);
            Controls.Add(btnCopyOutput);
            Controls.Add(btnCopyPayload);
            Controls.Add(btnMergeAiResponse);
            Controls.Add(btnGeneratePayload);
            Controls.Add(rtbAiResponse);
            Controls.Add(rtbJsonToSend);
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
        private RichTextBox rtbJsonToSend;
        private RichTextBox rtbAiResponse;
        private Button btnGeneratePayload;
        private Button btnMergeAiResponse;
        private Button btnCopyPayload;
        private Button btnCopyOutput;
        private Button btnPasteInput;
        private Button btnPasteResponse;
    }
}

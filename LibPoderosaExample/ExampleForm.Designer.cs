using System.Windows.Forms;

namespace LibPoderosaExample
{
    partial class ExampleForm
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
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.TopSplitContainer = new System.Windows.Forms.SplitContainer();
            this.BottomSplitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).BeginInit();
            this.TopSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).BeginInit();
            this.BottomSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.TopSplitContainer);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.BottomSplitContainer);
            this.MainSplitContainer.Size = new System.Drawing.Size(292, 266);
            this.MainSplitContainer.SplitterDistance = 131;
            this.MainSplitContainer.TabIndex = 0;
            // 
            // TopSplitContainer
            // 
            this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.TopSplitContainer.Name = "TopSplitContainer";
            // 
            // TopSplitContainer.Panel1
            // 
            this.TopSplitContainer.Panel1.BackColor = System.Drawing.Color.Black;
            // 
            // TopSplitContainer.Panel2
            // 
            this.TopSplitContainer.Panel2.BackColor = System.Drawing.Color.Black;
            this.TopSplitContainer.Size = new System.Drawing.Size(292, 131);
            this.TopSplitContainer.SplitterDistance = 147;
            this.TopSplitContainer.TabIndex = 0;
            // 
            // BottomSplitContainer
            // 
            this.BottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BottomSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.BottomSplitContainer.Name = "BottomSplitContainer";
            // 
            // BottomSplitContainer.Panel1
            // 
            this.BottomSplitContainer.Panel1.BackColor = System.Drawing.Color.Black;
            // 
            // BottomSplitContainer.Panel2
            // 
            this.BottomSplitContainer.Panel2.BackColor = System.Drawing.Color.Black;
            this.BottomSplitContainer.Size = new System.Drawing.Size(292, 131);
            this.BottomSplitContainer.SplitterDistance = 147;
            this.BottomSplitContainer.TabIndex = 0;
            // 
            // ExampleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.MainSplitContainer);
            this.Name = "ExampleForm";
            this.Text = "LibPoderosa Example";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).EndInit();
            this.TopSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BottomSplitContainer)).EndInit();
            this.BottomSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SplitContainer MainSplitContainer;
        private SplitContainer TopSplitContainer;
        private SplitContainer BottomSplitContainer;
    }
}


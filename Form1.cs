using ImageMagick;
using RUAG_Random_Unique_Art_Generator_.Utils;

namespace RUAG_Random_Unique_Art_Generator_
{
    public partial class Form1 : Form
    {
        private string inputFolder, outputFolder, description, collectionName, baseImgUri;
        private int collSize, maxNfts, totalFiles;
        private List<Layer> layers;
        private Random r;
        private int idxListBox;
        private CancellationTokenSource tokenSource;
        public Form1()
        {
            InitializeComponent();
            MagickNET.Initialize();
            r = new Random();
            tokenSource = new CancellationTokenSource();
            layers = new List<Layer>();
            inputFolder = "";
            outputFolder = "";
            generateButton.Enabled = false;
            cancelButton.Enabled = false;
            sizeNumericUpDown.Enabled = false;
            layersListBox.Enabled = false;
            nameTextBox.Enabled = false;
            descriptionTextBox.Enabled = false;
            toolStripStatusLabel1.Text = "Nothing to do.";
            toolStripProgressBar1.Enabled = false;
            this.description = "";
            this.collectionName = "";
            this.baseImgUri = "http://";
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            backgroundWorker1.ReportProgress(0, "Creating layers...");
            int k = 1;
            layers.Clear();
            var folders = Directory.GetDirectories(inputFolder);
            if (folders is not null)
            {
                foreach (var folder in folders)
                {
                    uint i = 0;
                    var files = Directory.GetFiles(folder);
                    if (files is not null)
                    {
                        List<Element> tmpElements = new List<Element>();
                        foreach (var file in files)
                        {
                            tmpElements.Add(new Element(i++, Path.GetFileNameWithoutExtension(file), file, r.Next(1, 101)));
                        }
                        Layer tmpLayer = new Layer(Path.GetFileName(folder), tmpElements);
                        var tempImageInfo = new MagickImageInfo(files[0]);
                        tmpLayer.Width = tempImageInfo.Width;
                        tmpLayer.Height = tempImageInfo.Height;
                        this.layers.Add(tmpLayer);
                        totalFiles += files.Length;
                        maxNfts *= files.Length;
                        backgroundWorker1.ReportProgress(((k++) * 100) / folders.Length, "Creating layers...");
                    }
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
            toolStripStatusLabel1.Text = e.UserState.ToString();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "An error ocurred... Stopped.";
                flowLayoutPanel1.Controls.Clear();
                generateButton.Enabled = false;
            }
            else
            {
                toolStripStatusLabel1.Text = "Done.";
                foreach (var layer in layers)
                {
                    layersListBox.Items.Add(layer.Name[2..]);
                }
                nameTextBox.Enabled = true;
                descriptionTextBox.Enabled = true;
            }
            if (nameTextBox.Text.Trim() != "" && descriptionTextBox.Text.Trim() != "" && outputFolder != null && outputTextBox.Text.Trim() != "")
            {
                generateButton.Enabled = true;
            }
            else
            {
                generateButton.Enabled = false;
            }
            layersListBox.SelectedIndex = 0;
            this.sizeNumericUpDown.Maximum = this.maxNfts;
            this.sizeNumericUpDown.Enabled = true;
            layersListBox.Enabled = true;
            inputButton.Enabled = true;
            toolStripProgressBar1.Enabled = false;
            outputButton.Enabled = true;
        }

        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            backgroundWorker2.ReportProgress(0, "Working...");
            NFTCollection coll = new NFTCollection(this.collectionName, this.description, this.baseImgUri, this.collSize, this.layers);
            coll.GenerateCollection(this.outputFolder, this.backgroundWorker2, e, ref tokenSource);
        }

        private void backgroundWorker2_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
            toolStripStatusLabel1.Text = e.UserState.ToString();
        }

        private void sizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            this.collSize = (int)sizeNumericUpDown.Value;
            if (nameTextBox.Text.Trim() != "" && descriptionTextBox.Text.Trim() != "" && outputFolder != null && outputTextBox.Text.Trim() != "")
            {
                generateButton.Enabled = true;
            }
            else
            {
                generateButton.Enabled = false;
            }
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (nameTextBox.Text.Trim() != "" && descriptionTextBox.Text.Trim() != "" && outputFolder != null && outputTextBox.Text.Trim() != "")
            {
                generateButton.Enabled = true;
            }
            else
            {
                generateButton.Enabled = false;
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                toolStripStatusLabel1.Text = "Operation was canceled.";
            }
            else if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "An error ocurred... Stopped.";
                flowLayoutPanel1.Controls.Clear();
            }
            else
            {
                toolStripStatusLabel1.Text = "Done.";
            }
            generateButton.Enabled = true;
            cancelButton.Enabled = false;
            inputButton.Enabled = true;
            outputButton.Enabled = true;
            sizeNumericUpDown.Enabled = true;
            nameTextBox.Enabled = true;
            descriptionTextBox.Enabled = true;
            layersListBox.Enabled = true;
            flowLayoutPanel1.Enabled = true;
            toolStripProgressBar1.Enabled = false;
            toolStripProgressBar1.Value = 100;
        }

        private void inputButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                inputFolder = folderBrowserDialog.SelectedPath;
                inputTextBox.Text = inputFolder;
                layersListBox.Items.Clear();
                totalFiles = 0;
                maxNfts = 1;
                inputButton.Enabled = false;
                toolStripProgressBar1.Enabled = true;
                outputButton.Enabled = false;
                nameTextBox.Enabled = false;
                descriptionTextBox.Enabled = false;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void outputButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                outputFolder = folderBrowserDialog.SelectedPath;
            }
            else
            {
                outputFolder = Environment.CurrentDirectory;
            }
            outputTextBox.Text = outputFolder;
            if (nameTextBox.Text.Trim() != "" && descriptionTextBox.Text.Trim() != "" && outputFolder != null && outputTextBox.Text.Trim() != "")
            {
                generateButton.Enabled = true;
            }
            else
            {
                generateButton.Enabled = false;
            }
        }

        private void layersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                idxListBox = layersListBox.SelectedIndex;
                var files = Directory.GetFiles(inputFolder + "\\" + layers[idxListBox].Name);
                flowLayoutPanel1.Controls.Clear();
                foreach (var file in files)
                {
                    GroupBox gb = new GroupBox();
                    TrackBar tb = new TrackBar();
                    PictureBox pb = new PictureBox();
                    Label lbI = new Label();
                    Label lbD = new Label();

                    gb.Size = new Size(flowLayoutPanel1.Width - 40, 105);
                    gb.Text = Path.GetFileName(file);

                    pb.Name = Path.GetFileName(file);
                    pb.Size = new Size(50, 50);
                    pb.Location = new Point(25, 30);
                    pb.ImageLocation = file;
                    pb.SizeMode = PictureBoxSizeMode.Zoom;

                    tb.Name = Path.GetFileName(file);
                    tb.Size = new Size(flowLayoutPanel1.Width - 160, 15);
                    tb.Location = new Point(25 + pb.Width + 25, 35);
                    tb.Minimum = 1;
                    tb.Maximum = 100;
                    tb.Value = layers[idxListBox].Elements.Find(x => x.Name + ".png" == gb.Text).Weight;
                    tb.ValueChanged += new EventHandler(trackBar_ValueChanged);

                    lbI.AutoSize = true;
                    lbD.AutoSize = true;
                    lbI.Text = "Rare";
                    lbD.Text = "Common";
                    lbI.Location = new Point(pb.Width + 50, 35 + 45);
                    lbD.Location = new Point(tb.Width + 50 + pb.Width / 2 - 20, 35 + 45);

                    gb.Controls.Add(pb);
                    gb.Controls.Add(tb);
                    gb.Controls.Add(lbI);
                    gb.Controls.Add(lbD);
                    flowLayoutPanel1.Controls.Add(gb);
                }
            }
            catch (Exception ex)
            {
                flowLayoutPanel1.Controls.Clear();
            }
        }
        private void trackBar_ValueChanged(object? sender, EventArgs e)
        {
            try
            {
                TrackBar trackBar = (TrackBar)sender;
                layers[idxListBox].Elements.Find(x => x.Name + ".png" == trackBar.Name).Weight = trackBar.Value;
            }
            catch (Exception ex)
            {
                flowLayoutPanel1.Controls.Clear();
            }

        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            this.collSize = (int)sizeNumericUpDown.Value;
            this.collectionName = nameTextBox.Text;
            this.description = descriptionTextBox.Text;
            generateButton.Enabled = false;
            cancelButton.Enabled = true;
            inputButton.Enabled = false;
            outputButton.Enabled = false;
            sizeNumericUpDown.Enabled = false;
            nameTextBox.Enabled = false;
            descriptionTextBox.Enabled = false;
            layersListBox.Enabled = false;
            flowLayoutPanel1.Enabled = false;
            toolStripProgressBar1.Enabled = true;
            backgroundWorker2.RunWorkerAsync();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            backgroundWorker2.CancelAsync();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace CURSE
{
    public partial class Form1 : Form
    {
        private DataStorage storage;
        private DBClass DBClass;
        private Calculations calculations;
        private Help help;

        private FDL fdl;
        private FDL fdlforSDL;

        private SDL sdl;
        private List<int> SDLelements1;
        private List<int> SDLelements2;

        private bool isBdConnected;
        private bool isBdChanged;
        private bool allowTabSwitch;

        public static int cycleAmount;
        public Form1()
        {
            InitializeComponent();
            InitializeChart();
            help = new Help();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DBClass = new DBClass();
            cycleAmount = 27;
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    if (MessageBox.Show("Вы уверены, что хотите удалить выбранные строки? Это действие нельзя отменить.", "Удаление строки", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                            {
                                int rowIndex = row.Index;
                                DataRow dataRow = ((DataRowView)row.DataBoundItem).Row;
                                storage.DTable.Rows.Remove(dataRow);
                            }
                            MessageBox.Show("Выбранные строки успешно удалены из базы данных.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при удалении строк из базы данных: " + ex.Message);
                        }
                    }
                }
            }
        }
        private void UISetImage()
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = storage.image;
        }
        private void UISetDatagridview()
        {
            dataGridView1.DataSource = storage.DTable;
            storage.DTableCalc = calculations.RemoveEpochFromDT(storage.DTable);
        }
        private void UISetTextboxes()
        {
            textBox1.Text = storage.A.ToString("");
            textBox2.Text = storage.E.ToString("");
            textBox3.Text = storage.blocks.ToString();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "SQLite files (*.sqlite)|*.sqlite|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            isBdConnected = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string dbFilePath = openFileDialog.FileName;
                DBClass.ConnectToDatabase(dbFilePath);
                if (storage == null)
                {
                    storage = new DataStorage(DBClass);
                    calculations = new Calculations(storage);
                }
                PopulateComboBoxWithTableNames();
                UISetImage();
                UISetTextboxes();
                UISetDatagridview();

            }
        }
        private void PopulateComboBoxWithTableNames()
            {
                comboBox1.Items.Clear();

            List<string> tableNames = DBClass.GetTableNames();

            foreach (string tableName in tableNames)
            {
                comboBox1.Items.Add(tableName);
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            isBdChanged = true;
            string selectedTableName = comboBox1.SelectedItem.ToString();

            DataTable selectedTableData = DBClass.LoadDataTableFromDatabase(selectedTableName);

            dataGridView1.DataSource = selectedTableData;
            storage.DTable = selectedTableData;
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (isBdConnected)
            {
                DBClass.SaveChangesToDatabase(storage.image, storage.A, storage.E, storage.blocks, storage.DTable);
            }
            else
            {
                MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadImageFromComputer();
            UISetImage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isBdConnected)
            {
                int rowAmount = storage.DTable.Rows.Count;
                double dMax = calculations.CalculateDmax();
                List<double> nextRow = calculations.CalculateNextRow(dMax, storage.DTable.Rows[rowAmount - 1]);
                DataRow newRow = storage.DTable.NewRow();
                for (int i = 0; i < nextRow.Count; i++)
                {
                    nextRow[i] = Math.Round(nextRow[i], 4);
                    newRow[i] = nextRow[i];
                }
                int preLastRowIndex = rowAmount - 1;
                DataRow preLastRow = storage.DTable.Rows[preLastRowIndex];
                newRow[0] = Convert.ToInt32(preLastRow[0]) + 1;
                storage.DTable.Rows.Add(newRow);
                storage.DTableCalc = calculations.RemoveEpochFromDT(storage.DTable);

                isBdChanged = true;
            }
            else {
                MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }

        }
        private void LoadImageFromComputer()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;
                    storage.image = Image.FromFile(selectedFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке изображения: " + ex.Message);
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            updateChart1();
        }
        private void updateChart1()
        {
            chart1.Series.Clear();

            foreach (Control control in groupBox4.Controls)
            {
                Console.WriteLine(control.Text);
                if (control is System.Windows.Forms.CheckBox checkBox && checkBox.Checked)
                {
                    switch (checkBox.Name)
                    {
                        case "checkBox1":
                            AddSeriesToChart1("Data1", fdl.msMinus, fdl.alphasMinusC);
                            break;
                        case "checkBox2":
                            AddSeriesToChart1("Data2", fdl.ms, fdl.alphasC);
                            break;
                        case "checkBox3":
                            AddSeriesToChart1("Data3", fdl.msPlus, fdl.alphasPlusC);
                            break;
                        case "checkBox4":
                            AddSeriesToChart1("Data4", fdl.msCPrMinus, fdl.alphasMinusCPr);
                            break;
                        case "checkBox5":
                            AddSeriesToChart1("Data5", fdl.msCPr, fdl.alphasCPr);
                            break;
                        case "checkBox6":
                            AddSeriesToChart1("Data6", fdl.msCPrPlus, fdl.alphasPlusCPr);
                            break;

                    }
                }

            }
            foreach (Series series in chart1.Series)
            {
                ApplyVisualStyleToSeries1(series);
            }
        }
        private void updateChart2()
        {
            chart2.Series.Clear();
            foreach (Control control in groupBox5.Controls)
            {
                if (control is System.Windows.Forms.CheckBox checkBox && checkBox.Checked)
                {
                    switch (checkBox.Name)
                    {
                        case "checkBox7":
                            AddSeriesToChart2("Data7", fdl.MsMinusPredicts);
                            break;
                        case "checkBox8":
                            AddSeriesToChart2("Data8", fdl.MsPredicts);
                            break;
                        case "checkBox9":
                            AddSeriesToChart2("Data9", fdl.MsPlusPredicts);
                            break;
                        case "checkBox10":
                            AddSeriesToChart2("Data10", fdl.AlphasMinusPredicts);
                            break;
                        case "checkBox11":
                            AddSeriesToChart2("Data11", fdl.AlphasPredicts);
                            break;
                        case "checkBox12":
                            AddSeriesToChart2("Data12", fdl.AlphasPlusPredicts);
                            break;

                    }
                }
            }
            foreach (Series series in chart2.Series)
            {
                ApplyVisualStyleToSeries1(series);
            }
        }
        private void updateChart3()
        {
            chart3.Series.Clear();
            foreach (Control control in groupBox6.Controls)
            {
                if (control is System.Windows.Forms.CheckBox checkBox && checkBox.Checked)
                {
                    switch (checkBox.Name)
                    {
                        case "checkBox13":
                            AddSeriesToChart3("Data13", fdlforSDL.msMinus, fdlforSDL.alphasMinusC);
                            break;
                        case "checkBox14":
                            AddSeriesToChart3("Data14", fdlforSDL.ms, fdlforSDL.alphasC);
                            break;
                        case "checkBox15":
                            AddSeriesToChart3("Data15", fdlforSDL.msPlus, fdlforSDL.alphasPlusC);
                            break;
                        case "checkBox16":
                            AddSeriesToChart3("Data16", fdlforSDL.msCPrMinus, fdlforSDL.alphasMinusCPr);
                            break;
                        case "checkBox17":
                            AddSeriesToChart3("Data17", fdlforSDL.msCPr, fdlforSDL.alphasCPr);
                            break;
                        case "checkBox18":
                            AddSeriesToChart3("Data18", fdlforSDL.msCPrPlus, fdlforSDL.alphasPlusCPr);
                            break;

                    }
                }
            }
            foreach (Series series in chart3.Series)
            {
                ApplyVisualStyleToSeries1(series);
            }
        }
        private void updateChart4()
        {
            chart4.Series.Clear();

            foreach (Control control in groupBox7.Controls)
            {
                if (control is System.Windows.Forms.CheckBox checkBox && checkBox.Checked)
                {
                    switch (checkBox.Name)
                    {
                        case "checkBox19":
                            AddSeriesToChart4("Data19", fdlforSDL.MsMinusPredicts);
                            break;
                        case "checkBox20":
                            AddSeriesToChart4("Data20", fdlforSDL.MsPredicts);
                            break;
                        case "checkBox21":
                            AddSeriesToChart4("Data21", fdlforSDL.MsPlusPredicts);
                            break;
                        case "checkBox22":
                            AddSeriesToChart4("Data22", fdlforSDL.AlphasMinusPredicts);
                            break;
                        case "checkBox23":
                            AddSeriesToChart4("Data23", fdlforSDL.AlphasPredicts);
                            break;
                        case "checkBox24":
                            AddSeriesToChart4("Data24", fdlforSDL.AlphasPlusPredicts);
                            break;

                    }
                }
            }
            foreach (Series series in chart4.Series)
            {
                ApplyVisualStyleToSeries1(series);
            }
        }
        private void checkBox_CheckedChanged2(object sender, EventArgs e)
        {
            foreach (Control control in groupBox5.Controls)
            {
                if (control is System.Windows.Forms.CheckBox checkBox)
                {
                    if (control is System.Windows.Forms.CheckBox otherCheckBox && otherCheckBox != sender)
                    {
                        otherCheckBox.Checked = false;

                    }
                }
            }
            updateChart2();
        }
        private void checkBox_CheckedChanged3(object sender, EventArgs e)
        {

            updateChart3();
        }
        private void checkBox_CheckedChanged4(object sender, EventArgs e)
        {
            foreach (Control control in groupBox7.Controls)
            {
                if (control is System.Windows.Forms.CheckBox otherCheckBox && otherCheckBox != sender)
                {
                    otherCheckBox.Checked = false;

                }
            }
            updateChart4();
        }
        private void ApplyVisualStyleToSeries1(Series series)
        {
            series.ChartType = SeriesChartType.Spline;
            series.Color = Color.Blue;
            series.BorderWidth = 2;
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerColor = Color.Black;
            series.MarkerSize = 8;
        }
        private void InitializeChart()
        {
            InitializeCommonSettings(chart1, 575);
            chart1.ChartAreas[0].AxisX.Title = "Значение М";
            chart1.ChartAreas[0].AxisY.Title = "Значение alpha";
            InitializeCommonSettings(chart2, 575);
            InitializeCommonSettings(chart3, 575);
            InitializeCommonSettings(chart4, 575);
            InitializeCommonSettings(chart5, 750);
            InitializeCommonSettings(chart6);
            InitializeCommonSettings(chart7);
        }
        private void InitializeCommonSettings(Chart chart, int width = 575)
        {
            chart.Width = width;
            chart.ChartAreas[0].InnerPlotPosition.Auto = false;
            chart.ChartAreas[0].InnerPlotPosition.Width = 83;
            chart.ChartAreas[0].InnerPlotPosition.Height = 83;
            chart.ChartAreas[0].InnerPlotPosition.X = 7;
            chart.ChartAreas[0].InnerPlotPosition.Y = 7;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "0.####";
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "0.####";
            if (chart.Series.Count > 0)
            {
                chart.Series["Series1"].ChartType = SeriesChartType.Spline;
                chart.Series["Series1"].Color = Color.Blue;
                chart.Series["Series1"].BorderWidth = 2;
                chart.Series["Series1"].MarkerStyle = MarkerStyle.Circle;
                chart.Series["Series1"].MarkerColor = Color.Black;
                chart.Series["Series1"].MarkerSize = 8;
            }
        }
        private void AddSeriesToChart1(string seriesName, List<double> xData, List<double> yData)
        {
            Series series = new Series(seriesName);
            for (int i = 0; i < xData.Count; i++)
            {
                Console.WriteLine("Adding X: " + xData[i]);
                Console.WriteLine("Adding Y: " + yData[i]);
                series.Points.AddXY(xData[i], yData[i]);
                series.Points[i].Label = $"{i}";
            }
            chart1.Series.Add(series);
        }
        private void AddSeriesToChart2(string seriesName, List<List<double>> stuff)
        {
            int seriesCount = 1;
            foreach (var sublist in stuff)
            {
                string uniqueSeriesName = $"{seriesName}_{seriesCount}";
                Series series = new Series(uniqueSeriesName);

                for (int i = 0; i < sublist.Count; i++)
                {

                    series.Points.AddXY(i, sublist[i]);
                }

                chart2.Series.Add(series);

                double minY = sublist.Min();
                double maxY = sublist.Max();
                double padding = 0.1 * (maxY - minY);
                chart2.ChartAreas[0].AxisY.Minimum = minY - padding;
                chart2.ChartAreas[0].AxisY.Maximum = maxY + padding;
                seriesCount++;
            }

        }
        private void AddSeriesToChart3(string seriesName, List<double> xData, List<double> yData)
        {
            Series series = new Series(seriesName);

            for (int i = 0; i < xData.Count; i++)
            {

                series.Points.AddXY(xData[i], yData[i]);
                series.Points[i].Label = $"{i}";
            }

            chart3.Series.Add(series);
        }
        private void AddSeriesToChart4(string seriesName, List<List<double>> stuff)
        {
            int seriesCount = 1;
            foreach (var sublist in stuff)
            {
                string uniqueSeriesName = $"{seriesName}_{seriesCount}";
                Series series = new Series(uniqueSeriesName);

                for (int i = 0; i < sublist.Count; i++)
                {

                    series.Points.AddXY(i, sublist[i]);
                }
                chart4.Series.Add(series);

                double minY = sublist.Min();
                double maxY = sublist.Max();
                double padding = 0.1 * (maxY - minY);
                chart2.ChartAreas[0].AxisY.Minimum = minY - padding;
                chart2.ChartAreas[0].AxisY.Maximum = maxY + padding;
                chart4.ChartAreas[0].AxisY.Minimum = minY - padding;
                chart4.ChartAreas[0].AxisY.Maximum = maxY + padding;
                seriesCount++;
            }

        }
        private void RecommendationsButton_Click(object sender, EventArgs e)
        {
            bool a = false;
            for (int i = 0; i < storage.isStable.Count; i++)
            {
                if (!storage.isStable[i])
                {

                    a = true;
                }
            }
            if (!a)
            {
                MessageBox.Show("Всё в порядке.");
            }
            else
            {
                MessageBox.Show("Есть изменяемые точки, нужно перейти к следующему уровню декомпозиции.");
            }
        }
        public void initFDL()
        {
            if(isBdConnected)
            {
                if (Convert.ToInt32(storage.DTable.Rows[0][0]) == 0)
                storage.DTableCalc = calculations.RemoveEpochFromDT(storage.DTable);
                else
                {
                    storage.DTableCalc = storage.DTable;
                }

                fdl = new FDL(calculations, storage);
                fdl.calculateFDL(storage.DTableCalc, storage.A);
                List<double> Ls = new List<double>();
                List<double> Ds = new List<double>();
                fdl.GetLD(out Ls, out Ds);
                FDLStuff(dataGridView2, dataGridView3, fdl.GetFDLMainTable(), fdl.GetFDLStateTable(), Ls, Ds, fdl.rowCount);
            }
            else
            {
                MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }
        }
        public void FDLStuff(DataGridView dataGridViewA, DataGridView dataGridViewB, DataTable FLODMainTable, DataTable FLODStateTable, List<double> Ls, List<double> Ds, int count)

        {
            dataGridViewA.DataSource = FLODMainTable;
            dataGridViewB.DataSource = FLODStateTable;
            for (int i = 0; i < count; i++)
            {
                if (Ls[i] < Ds[i])
                {
                    dataGridViewB.Rows[i].Cells[3].Style.BackColor = Color.Green;
                }
                else
                {
                    dataGridViewB.Rows[i].Cells[3].Style.BackColor = Color.Red;
                }
            }
            dataGridViewB.Rows[count].Cells[0].Value = "Прогноз: ";
            if (Ls[count] < Ds[count])
            {
                dataGridViewB.Rows[count].Cells[3].Style.BackColor = Color.Green;
            }
            else
            {
                dataGridViewB.Rows[count].Cells[3].Style.BackColor = Color.Red;
            }
        }
        private void initSDL()
        {
            if (isBdConnected)
            {
                sdl = new SDL();
                SDLListboxAllpoints.Items.Clear();
                SDLListboxBlockpoints.Items.Clear();
                for (int i = 0; i < storage.DTableCalc.Columns.Count; i++)
                {
                    SDLListboxAllpoints.Items.Add(i.ToString());
                }
                SDLelements1 = new List<int>();
                SDLelements2 = new List<int>();
                SDLComboboxblocks.SelectedIndex = 0;
                SDLPictureBox.Image = storage.image;
                SDLPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (isBdConnected) { storage.E = Convert.ToDouble(textBox2.Text); }
            else
            {
                MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (isBdConnected) { storage.A = Convert.ToDouble(textBox1.Text); }
            else
            {
                MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }
        }
        private int prevIndex;
        private void updateCharts()
        {
            updateChart1();
            updateChart2();
            updateChart3();
            updateChart4();
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        { 
            int exportValue = prevIndex;
            prevIndex = tabControl1.SelectedIndex;
            switch ((sender as TabControl).SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    initFDL();
                    updateCharts();
                    break;
                case 2:
                    initSDL();
                    updateCharts();
                    tabControl4.SelectedTab = tabPage11;
                    break;
                case 3:
                    initLastDL();
                    break;
                case 4:
                    help = new Help();
                    help.Show();
                    help.OpenHelp(exportValue+1);
                    break;
            }
        }

        private void tabPage6_Click(object sender, EventArgs e)
        {

        }
        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            int totalItems = SDLListboxAllpoints.Items.Count;
            int itemsPerList = totalItems / 2;

            // Проверяем, является ли общее количество элементов нечетным
            bool oddCount = totalItems % 2 != 0;

            foreach (var item in SDLListboxAllpoints.Items)
            {
                if (SDLelements1.Count < itemsPerList)
                {
                    SDLelements1.Add(Convert.ToInt32(item));
                }
                else if (SDLelements2.Count < itemsPerList || oddCount)
                {
                    SDLelements2.Add(Convert.ToInt32(item));
                    // Если общее количество элементов нечетное, устанавливаем флаг в false,
                    // чтобы следующий элемент остался в первом списке
                    oddCount = false;
                }
            }

            SDLListboxAllpoints.Items.Clear();

            if (SDLComboboxblocks.SelectedItem.ToString() == "A")
            {
                foreach (int item in SDLelements1)
                {
                    SDLListboxBlockpoints.Items.Add(item.ToString());
                }
            }
            else if (SDLComboboxblocks.SelectedItem.ToString() == "B")
            {
                foreach (int item in SDLelements2)
                {
                    SDLListboxBlockpoints.Items.Add(item.ToString());
                }
            }
        }

        private void SDLComboboxblocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            SDLListboxBlockpoints.Items.Clear();
            if (SDLComboboxblocks.SelectedItem.ToString() == "A")
            {
                foreach (int item in SDLelements1)
                {
                    SDLListboxBlockpoints.Items.Add(item.ToString());
                }
            }
            else if (SDLComboboxblocks.SelectedItem.ToString() == "B")
            {
                foreach (int item in SDLelements2)
                {
                    SDLListboxBlockpoints.Items.Add(item.ToString());
                }
            }
        }
        private void SDLListboxAllpoints_MouseDown(object sender, MouseEventArgs e)
        {
            if (SDLListboxAllpoints.SelectedItem != null)
            {
                SDLListboxAllpoints.DoDragDrop(SDLListboxAllpoints.SelectedItem, DragDropEffects.Move);
            }
        }
        private void SDLListboxAllpoints_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private void SDLListboxAllpoints_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
            {
                string item = (string)e.Data.GetData(typeof(string));
                if (!SDLListboxAllpoints.Items.Contains(item))
                {
                    if (SDLComboboxblocks.SelectedItem.ToString() == "A")
                    {
                        SDLelements1.Remove(Convert.ToInt32(item));
                    }
                    else if (SDLComboboxblocks.SelectedItem.ToString() == "B")
                    {
                        SDLelements2.Remove(Convert.ToInt32(item));
                    }
                    SDLListboxAllpoints.Items.Add(item);
                    SDLListboxBlockpoints.Items.Remove(item);
                }
            }
        }
        private void SDLListboxBlockpoints_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
            {
                string item = (string)e.Data.GetData(typeof(string));
                if (!SDLListboxBlockpoints.Items.Contains(item))
                {
                    if (SDLComboboxblocks.SelectedItem.ToString() == "A")
                    {
                        SDLelements1.Add(Convert.ToInt32(item));
                    }
                    else if (SDLComboboxblocks.SelectedItem.ToString() == "B")
                    {
                        SDLelements2.Add(Convert.ToInt32(item));
                    }
                    SDLListboxBlockpoints.Items.Add(item);
                    SDLListboxAllpoints.Items.Remove(item);
                }
            }
        }

        private void SDLListboxBlockpoints_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void SDLListboxBlockpoints_MouseDown(object sender, MouseEventArgs e)
        {
            if (SDLListboxBlockpoints.SelectedItem != null)
            {
                SDLListboxBlockpoints.DoDragDrop(SDLListboxBlockpoints.SelectedItem, DragDropEffects.Move);
            }
        }
        private void SDLButtonAccept_Click(object sender, EventArgs e)
        {
            if (SDLListboxAllpoints.Items.Count > 1)
            {
                MessageBox.Show("Не все контрольные точки привязаны к блокам. Привяжите все точки и попробуйте еще раз. ");
                return;
            }
            else if(SDLelements1.Count > SDLelements2.Count || SDLelements2.Count > SDLelements1.Count)
            {
                MessageBox.Show("Блоки должны содержать равное количество точек. Оставьте одну, если она не вмещается.");
                return;
            } else
            {
                allowTabSwitch = true;
                tabControl4.SelectedIndex = 1;
                allowTabSwitch = false;
                SDLelements1.Sort();
                SDLelements2.Sort();
                fdlforSDL = new FDL(calculations, storage);
                SplitDataTable(storage.DTableCalc, SDLelements1, SDLelements2, out sdl.dataTable1, out sdl.dataTable2);
                SDLBlockCombobox.SelectedIndex = SDLComboboxblocks.SelectedIndex;

                if (SDLComboboxblocks.SelectedItem.ToString() == "A")
                {
                    fdlforSDL.calculateFDL(sdl.dataTable1, storage.A);

                }
                else if (SDLComboboxblocks.SelectedItem.ToString() == "B")
                {
                    fdlforSDL.calculateFDL(sdl.dataTable2, storage.A);
                }

                List<double> Ls = new List<double>();
                List<double> Ds = new List<double>();
                fdlforSDL.GetLD(out Ls, out Ds);
                FDLStuff(dataGridView9, dataGridView8, fdlforSDL.GetFDLMainTable(), fdlforSDL.GetFDLStateTable(), Ls, Ds, fdlforSDL.rowCount);


            }
        }
        public static void SplitDataTable(DataTable originalTable, List<int> indexes1, List<int> indexes2, out DataTable dataTable1, out DataTable dataTable2)
        {
            dataTable1 = new DataTable();
            dataTable2 = new DataTable();

            foreach (int index in indexes1)
            {
                dataTable1.Columns.Add(originalTable.Columns[index].ColumnName, originalTable.Columns[index].DataType);
            }

            foreach (int index in indexes2)
            {
                dataTable2.Columns.Add(originalTable.Columns[index].ColumnName, originalTable.Columns[index].DataType);
            }
            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow1 = dataTable1.NewRow();
                DataRow newRow2 = dataTable2.NewRow();

                foreach (int index in indexes1)
                {
                    newRow1[indexes1.IndexOf(index)] = row[index];
                }

                foreach (int index in indexes2)
                {
                    newRow2[indexes2.IndexOf(index)] = row[index];
                }

                dataTable1.Rows.Add(newRow1);
                dataTable2.Rows.Add(newRow2);
            }
        }

        private void SDLBlockCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SDLBlockCombobox.SelectedItem.ToString() == "A")
            {
                fdlforSDL.calculateFDL(sdl.dataTable1, storage.A);

            }
            else if (SDLBlockCombobox.SelectedItem.ToString() == "B")
            {
                fdlforSDL.calculateFDL(sdl.dataTable2, storage.A);
            }

            List<double> Ls = new List<double>();
            List<double> Ds = new List<double>();
            fdlforSDL.GetLD(out Ls, out Ds);
            FDLStuff(dataGridView9, dataGridView8, fdlforSDL.GetFDLMainTable(), fdlforSDL.GetFDLStateTable(), Ls, Ds, fdlforSDL.rowCount);
        }
        static bool IsFirstColumnEpoch(DataTable table)
        {
            if (table.Rows.Count == 0)
                return false;
            int expectedValue = 0;
            foreach (DataRow row in table.Rows)
            {
                int actualValue = Convert.ToInt32(row[0]);
                if (actualValue != expectedValue)
                    return false;
                expectedValue++;
            }

            return true;
        }
        private void initLastDL()
        {
            if (isBdConnected)
            {
                if (isBdChanged || checkedListBox1.Items.Count == 0)
                {
                    checkedListBox1.Items.Clear();
                    chart5.Series.Clear();
                    bool containsSequential = IsFirstColumnEpoch(storage.DTable);

                    if (containsSequential)
                    {
                        for (int i = 1; i < storage.DTable.Columns.Count; i++)
                        {
                            checkedListBox1.Items.Add(i);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < storage.DTable.Columns.Count; i++)
                        {
                            checkedListBox1.Items.Add(i+1);
                        }
                    }

                    isBdChanged = false;
                }

            }
            else
            {
            MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }
        }   
        private void AddButton_Click(object sender, EventArgs e)
        {
            chart5.Series.Clear();
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (var item in checkedListBox1.CheckedItems)
            {
                string columnName = item.ToString();

                Series series = new Series(columnName);
                series.ChartType = SeriesChartType.Line;
                series.BorderWidth = 2;
                series.MarkerStyle = MarkerStyle.Circle;
                series.MarkerSize = 8;

                foreach (DataRow row in storage.DTable.Rows)
                {
                    double value = Convert.ToDouble(row[columnName]);
                    series.Points.AddY(value);

                    if (value < min)
                        min = value;
                    if (value > max)
                        max = value;
                }

                chart5.Series.Add(series);
                chart5.ChartAreas[0].AxisY.Minimum = Math.Floor(min);
                chart5.ChartAreas[0].AxisY.Maximum = Math.Ceiling(max);
                chart5.ChartAreas[0].AxisX.Interval = 1;
            }
            chart5.Refresh();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            chart5.Series.Clear();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {   
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (isBdConnected) { storage.blocks = Convert.ToInt32(textBox3.Text); }
            else
            {
                MessageBox.Show("Нет подключения с базой данных.");
                help = new Help();
                help.Show();
                help.OpenHelp(1);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void SecondDecompLevelButton_Click(object sender, EventArgs e)
        {
            initSDL();
            tabControl1.SelectedTab = tabPage3;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            initLastDL();
            tabControl1.SelectedTab = tabPage5;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool a = false;
            for (int i = 0; i < storage.isStable.Count; i++)
            {
                if (!storage.isStable[i])
                {

                    a = true;
                }
            }
            if (!a)
            {
                MessageBox.Show("Всё в порядке.");
            }
            else
            {
                MessageBox.Show("Есть изменяемые точки, нужно перейти к следующему уровню декомпозиции.");
            }   
        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            isBdChanged = true;
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridView dataGridView = sender as DataGridView;
                object newValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                DataRowView drv = dataGridView.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (drv != null)
                {
                    drv.Row[e.ColumnIndex] = newValue;
                }
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void chart6_Click(object sender, EventArgs e)
        {

        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl2.SelectedIndex == 1)
            {
                Chart chart = chart6;
                ChartArea chartArea = chart.ChartAreas[0];

                double minY = double.MaxValue;
                double maxY = double.MinValue;

                // Determine the size of the arrays
                int mainDataSize = fdl.ms.Count;
                int predictDataSize = fdl.mPredicts.Count;

                // Create main and predict series outside the loop
                Series[] mainSeries = new Series[3];
                Series[] predictSeries = new Series[3];

                // Define common chart properties
                Action<Series> configureSeries = series =>
                {
                    series.ChartType = SeriesChartType.Line;
                    series.BorderWidth = 2;
                    series.MarkerStyle = MarkerStyle.Circle;
                    series.MarkerSize = 8;
                };

                // Initialize series
                for (int i = 0; i < 3; i++)
                {
                    mainSeries[i] = new Series();
                    predictSeries[i] = new Series();
                    configureSeries(mainSeries[i]);
                    configureSeries(predictSeries[i]);
                    chart.Series.Add(mainSeries[i]);
                    chart.Series.Add(predictSeries[i]);
                }

                // Set colors for series
                Color[] mainColors = { Color.Red, Color.OrangeRed, Color.IndianRed };
                Color[] predictColors = { Color.Blue, Color.BurlyWood, Color.BlueViolet };

                for (int i = 0; i < mainSeries.Length; i++)
                {
                    mainSeries[i].Color = mainColors[i];
                    predictSeries[i].Color = predictColors[i];
                }

                // Add data points
                for (int i = 0; i < Math.Max(mainDataSize, predictDataSize); i++)
                {
                    if (i < mainDataSize)
                    {
                        double value = Convert.ToDouble(fdl.ms[i]);

                        for (int j = 0; j < 3; j++)
                        {
                            // Add main data points
                            mainSeries[j].Points.AddY(value);

                            // Update Y-axis range
                            minY = Math.Min(minY, value);
                            maxY = Math.Max(maxY, value);
                        }
                    }

                    if (i < predictDataSize)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            double predictValue = Convert.ToDouble(GetPredictValue(fdl, i, j));

                            // Add predict data points
                            predictSeries[j].Points.AddY(predictValue);

                            // Update Y-axis range
                            minY = Math.Min(minY, predictValue);
                            maxY = Math.Max(maxY, predictValue);
                        }
                    }
                }

                // Set Y-axis range
                chartArea.AxisY.Minimum = minY;
                chartArea.AxisY.Maximum = maxY;
                chartArea.AxisX.Interval = 1;
            }
        }
        private void tabControl5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl5.SelectedIndex == 1)
            {
                Chart chart = chart7;
                ChartArea chartArea = chart.ChartAreas[0];

                double minY = double.MaxValue;
                double maxY = double.MinValue;

                int mainDataSize = fdl.ms.Count;
                int predictDataSize = fdl.mPredicts.Count;

                Series[] mainSeries = new Series[3];
                Series[] predictSeries = new Series[3];

                Action<Series> configureSeries = series =>
                {
                    series.ChartType = SeriesChartType.Line;
                    series.BorderWidth = 2;
                    series.MarkerStyle = MarkerStyle.Circle;
                    series.MarkerSize = 8;
                };

                for (int i = 0; i < 3; i++)
                {
                    mainSeries[i] = new Series();
                    predictSeries[i] = new Series();
                    configureSeries(mainSeries[i]);
                    configureSeries(predictSeries[i]);
                    chart.Series.Add(mainSeries[i]);
                    chart.Series.Add(predictSeries[i]);
                }

                Color[] mainColors = { Color.Red, Color.OrangeRed, Color.IndianRed };
                Color[] predictColors = { Color.Blue, Color.BurlyWood, Color.BlueViolet };

                for (int i = 0; i < mainSeries.Length; i++)
                {
                    mainSeries[i].Color = mainColors[i];
                    predictSeries[i].Color = predictColors[i];
                }

                for (int i = 0; i < Math.Max(mainDataSize, predictDataSize); i++)
                {
                    if (i < mainDataSize)
                    {
                        double value = Convert.ToDouble(fdl.ms[i]);

                        for (int j = 0; j < 3; j++)
                        {
                            mainSeries[j].Points.AddY(value);

                            minY = Math.Min(minY, value);
                            maxY = Math.Max(maxY, value);
                        }
                    }

                    if (i < predictDataSize)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            double predictValue = Convert.ToDouble(GetPredictValue(fdl, i, j));

                            predictSeries[j].Points.AddY(predictValue);

                            minY = Math.Min(minY, predictValue);
                            maxY = Math.Max(maxY, predictValue);
                        }
                    }
                }
                chartArea.AxisY.Minimum = minY;
                chartArea.AxisY.Maximum = maxY;
                chartArea.AxisX.Interval = 1;
            }
        }

        private object GetPredictValue(FDL fdl, int index, int predictIndex)
        {
            switch (predictIndex)
            {
                case 0: return fdl.mPredicts[index];
                case 1: return fdl.mPlusPredicts[index];
                case 2: return fdl.mMinusPredicts[index];
                default: throw new ArgumentOutOfRangeException(nameof(predictIndex));
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tabControl4_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == 1 && !allowTabSwitch)
            {
                MessageBox.Show("Переход на вкладу расчётов нужно осуществлять через кнопку 'Подтвердить'");
                e.Cancel = true;
            }
        }
    }
}

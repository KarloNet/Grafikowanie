using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Reflection;
using System.Drawing.Printing;
using System.Threading;

namespace Grafikowanie
{

    public partial class Form1 : Form
    {
        string HASH;
        int USERID = 0;
        string IMIE = "";
        string NAZWISKO = "";
        bool editGrafik = false;
        Color defColor = new Color();
        Color editColor = Color.LightGoldenrodYellow;

        PaperSize paperSize = new PaperSize("papersize", 210, 297);//set the paper size
        int totalnumber = 0;//this is for total number of items of the list or array
        int itemperpage = 0;//this is for no of item per page


        bool DEBUG = false;
        string connStr = "Data Source = 127.0.0.1\\SQLSERVER; Initial Catalog = GRAFIK; User ID = TEST; Password = Tkip999!!@@##";
        bool dgvInitDone = false;
        //DataSet dS = new DataSet();
        DataTable dT = new DataTable();
        int firstDayCell = 0;
        DataTable dKierowcy = new DataTable();
        DataTable dZadania = new DataTable();
        DataTable dZadDzien = new DataTable();
        DataTable dPojazdy = new DataTable();
        DataTable dWolne = new DataTable();
        DateTime start;
        DateTime end;

        DataTable dExtKierowcy = new DataTable();

        DataTable addGrafik = new DataTable();
        DataTable remGrafik = new DataTable();
        DataTable updGrafik = new DataTable();

        string cpyBuffer;

        int selectedRow = -1;
        int selectedCell = -1;

        private void UpdateForTest()
        {
            int addedCount = 0;
            int deletedCount = 0;
            int detachedCount = 0;
            int modifiedCount = 0;
            int unchangedCount = 0;
            int incorrectCount = 0;

            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                SqlCommand cmd = conn.CreateCommand();
                conn.Open();
                modifiedCount++;
                cmd.CommandText = "UPDATE TOP 1 FROM GRAFIK SET g_KIEROWCA = @zad WHERE g_DATA = @id AND g_ZADANIE";
                //cmd.Parameters.AddWithValue("@zad", r["z_ZADANIE"]);
                //cmd.Parameters.AddWithValue("@bryg", r["z_BRYGADA"]);
                //cmd.Parameters.AddWithValue("@tab", r["z_TABOR"]);
                //cmd.Parameters.AddWithValue("@pocz", r["z_POCZATEK"]);
                //cmd.Parameters.AddWithValue("@kon", r["z_KONIEC"]);
                //cmd.Parameters.AddWithValue("@podm", r["z_PODMIANA"]);
                //cmd.Parameters.AddWithValue("@cpra", r["z_CZAS_PRACY"]);
                //cmd.Parameters.AddWithValue("@cjaz", r["z_CZAS_JAZDY"]);
                //cmd.Parameters.AddWithValue("@grup", r["z_GRUPA"]);
                //cmd.Parameters.AddWithValue("@id", r["z_ID"]);
                //cmd.ExecuteNonQuery();
                conn.Close();
                MessageBox.Show("Made changes to database are:\n1) Added: " + addedCount.ToString() + "\n2) Deleted: " + deletedCount.ToString() + "\n3) Modified: " + modifiedCount.ToString() + "\n4) Detached: " + detachedCount.ToString() + "\n5) Unchanged: " + unchangedCount.ToString() + "\n6) Incorrect: " + incorrectCount.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public DataTable GetDataTable(string oQuery)
        {
            DataTable t1 = new DataTable();
            if (oQuery == null) return t1;

            //SqlConnection conn = new SqlConnection("Data Source = 83.238.167.3\\OPTIMA; Initial Catalog = WARBUS_GRAFIK; User ID = sa; Password = qazWSX123!@#");
            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                conn.Open();
                if(DEBUG) MessageBox.Show("[GetDataTable]\n" + "Connection string : " + oQuery);
                SqlCommand cmd = new SqlCommand(oQuery, conn);

                using (SqlDataAdapter a = new SqlDataAdapter(cmd))
                {
                    a.Fill(t1);
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return t1;
        }

        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;

            editGrafik = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            contextMenuStrip1.Enabled = false;

            defColor = button7.BackColor;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //logowanie
            Form6 f6 = new Form6(connStr);
            f6.StartPosition = FormStartPosition.CenterParent;
            DialogResult re = f6.ShowDialog(); // Shows Form6
            if(re == DialogResult.Cancel)
            {
                Close();
            }
            if(re == DialogResult.OK)
            {
                HASH = f6.hash;
                USERID = f6.userId;
                NAZWISKO = f6.nazwisko;
                IMIE = f6.imie;
            }
            label34.Text = "Sesion ID: [" + HASH + "] ";
            //MessageBox.Show("In Load");
        }

        private void UpdateZadania(DateTime data)
        {
            DateTime tmpS = data;

            label1.Text = "Zadania na dzień: " + tmpS.ToShortDateString();
            listView1.Clear();
            //Add column header
            listView1.Columns.Add("ID", 20);
            listView1.Columns.Add("Zadanie", 80);
            listView1.Columns.Add("Brygada", 50);
            listView1.Columns.Add("Kierowca", 150);
            //listView1.Columns.Add("ID", 10);

            //Add items in the listview
            string[] arr = new string[4];
            ListViewItem itm;

            DataRow[] curZad = dZadDzien.Select("g_DATA = '" + tmpS.ToShortDateString() + "'");
            if (DEBUG) MessageBox.Show("[UpdateZadania]\n" + "Found " + curZad.Length.ToString() + " zadan na dzień : " + tmpS.ToShortDateString());
            foreach (DataRow r in curZad)
            {
                int kierowcaID = 0;
                DataRow[] zad = dZadania.Select("z_ID = " + r["g_ZADANIE"]);
                if (!r.IsNull("g_KIEROWCA"))
                {
                    kierowcaID = (int)r["g_KIEROWCA"];
                    //MessageBox.Show("Kierowca ID: " + kierowcaID.ToString());
                }

                if (zad.Length != 1)
                {
                    //MessageBox.Show("Zadanie: " + r["g_ZADANIE"].ToString());
                    //MessageBox.Show("ERROR - BŁĄD W ZADANIACH [dataGridView1_CellClick] COUNT: " + zad.Length.ToString());
                }
                else
                {
                    //Add item
                    arr[0] = zad[0]["z_ID"].ToString();
                    arr[1] = (string)zad[0]["z_ZADANIE"];
                    arr[2] = (string)zad[0]["z_BRYGADA"];

                    //spr czy zadanie jest na liscie usuniętych zadan - remGrafik
                    if (kierowcaID != 0)
                    {
                        string str = "g_ZADANIE = " + zad[0]["z_ID"].ToString() + " AND g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + kierowcaID.ToString();
                        DataRow[] zar = remGrafik.Select(str);
                        if (zar.Length == 1)
                        {
                            kierowcaID = 0;
                        }
                    }

                    //spr czy zadanie jest na liscie addGrafik ( dodanych zadań)
                    if (kierowcaID == 0)
                    {
                        string st = "g_ZADANIE = " + zad[0]["z_ID"].ToString() + " AND g_DATA = '" + tmpS.ToShortDateString() + "'";
                        DataRow[] za = addGrafik.Select(st);
                        if (za.Length == 1)
                        {
                            kierowcaID = (int)za[0]["g_KIEROWCA"];
                        }
                    }

                    if (kierowcaID != 0)
                    {
                        DataRow[] kie = dKierowcy.Select("k_ID = " + kierowcaID.ToString());
                        if (kie.Length == 1)
                        {
                            arr[3] = (string)kie[0]["k_NAZWISKO"] + " " + (string)kie[0]["k_IMIE"];
                        }
                        else
                        {
                            kierowcaID = 0;
                            TimeSpan pocz = (TimeSpan)zad[0]["z_POCZATEK"];
                            TimeSpan koni = (TimeSpan)zad[0]["z_KONIEC"];
                            arr[3] = pocz.ToString() + " : " + koni.ToString();
                        }
                    }
                    else
                    {
                        TimeSpan pocz = (TimeSpan)zad[0]["z_POCZATEK"];
                        TimeSpan koni = (TimeSpan)zad[0]["z_KONIEC"];
                        arr[3] = pocz.ToString() + " : " + koni.ToString();
                    }
                    itm = new ListViewItem(arr);
                    if (kierowcaID == 0) itm.ForeColor = Color.Red;
                    listView1.Items.Add(itm);
                }
            }

        }

        private void ShowInfo(DateTime date)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            /*
            */
        }

        private void dataGridView1_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if(DEBUG) MessageBox.Show("[dataGridView1_CellContextMenuStripNeeded]\n" + "Clicked row: " + e.RowIndex.ToString() + " Column: " + e.ColumnIndex.ToString());
            if (e.RowIndex >= 0 && e.ColumnIndex >= 4)
            {
                selectedRow = e.RowIndex;
                selectedCell = e.ColumnIndex;
                e.ContextMenuStrip = contextMenuStrip1;
                dataGridView1.CurrentCell = dataGridView1.Rows[selectedRow].Cells[selectedCell];
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            DateTime st;
            DateTime en;
            
            st = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            en =  new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            //st = new DateTime(2017,1,31);
            //en = new DateTime(2017,2,5);
            LoadGrafik(st, en);
        }

        private void SetDGVColors(int zadID, DataGridViewCell c)
        {
            switch (zadID)
            {
                case ZADANIA.praca:
                    c.Style.BackColor = Color.LightYellow;
                    break;
                case ZADANIA.prefA:
                    c.Style.BackColor = Color.LightSkyBlue;
                    break;
                case ZADANIA.prefB:
                    c.Style.BackColor = Color.LightBlue;
                    break;
                case ZADANIA.wolne:
                    c.Style.BackColor = Color.LightGreen;
                    break;
                case ZADANIA.wolneNZ:
                    c.Style.BackColor = Color.GreenYellow;
                    break;
                case ZADANIA.wolneND:
                    c.Style.BackColor = Color.LightSeaGreen;
                    break;
                case ZADANIA.urlop:
                    c.Style.BackColor = Color.Green;
                    break;
                case ZADANIA.chorobowe:
                    c.Style.BackColor = Color.DarkGreen;
                    break;
                default:
                    c.Style.BackColor = Color.White;
                    break;
            }
        }

        private void SetDGVColors()
        {
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                foreach (DataGridViewCell c in r.Cells)
                {
                    if (c.Value != null)
                    {
                        if (string.Compare(c.Value.ToString(), "P") == 0)
                        {
                            c.Style.BackColor = Color.LightYellow;
                        }
                        if (string.Compare(c.Value.ToString(), "PA") == 0)
                        {
                            c.Style.BackColor = Color.LightSkyBlue;
                        }
                        if (string.Compare(c.Value.ToString(), "PB") == 0)
                        {
                            c.Style.BackColor = Color.LightBlue;
                        }
                        if (string.Compare(c.Value.ToString(), "W") == 0)
                        {
                            c.Style.BackColor = Color.LightGreen;
                        }
                        if (string.Compare(c.Value.ToString(), "WNZ") == 0)
                        {
                            c.Style.BackColor = Color.GreenYellow;
                        }
                        if (string.Compare(c.Value.ToString(), "WND") == 0)
                        {
                            c.Style.BackColor = Color.LightSeaGreen;
                        }
                        if (string.Compare(c.Value.ToString(), "UP") == 0)
                        {
                            c.Style.BackColor = Color.Green;
                        }
                        if (string.Compare(c.Value.ToString(), "L4") == 0)
                        {
                            c.Style.BackColor = Color.DarkGreen;
                        }
                    }
                }
            }
        }

        private void SetDGVText(int zadID, DataGridViewCell c)
        {
            switch (zadID)
            {
                case ZADANIA.praca:
                    c.Value = "P";
                    break;
                case ZADANIA.prefA:
                    c.Value = "PA";
                    break;
                case ZADANIA.prefB:
                    c.Value = "PB";
                    break;
                case ZADANIA.wolne:
                    c.Value = "W";
                    break;
                case ZADANIA.wolneNZ:
                    c.Value = "WNZ";
                    break;
                case ZADANIA.wolneND:
                    c.Value = "WND";
                    break;
                case ZADANIA.urlop:
                    c.Value = "UP";
                    break;
                case ZADANIA.chorobowe:
                    c.Value = "L4";
                    break;
                default:
                    c.Value = "ERROR";
                    break;
            }
        }

        private void LoadGrafik(DateTime from, DateTime to)
        {
            DataTable tD;
            dgvInitDone = false;
            cpyBuffer = "";

            dataGridView1.DataSource = null;
            dataGridView1.Update();
            dataGridView1.Refresh();

            //Set Double buffering on the Grid using reflection and the bindingflags enum.
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView1, new object[] { true });


            start = from;
            end = to;
            //Prepare DataTable
            dT = new DataTable();
            dT.Columns.Add("K_ID", typeof(int));
            dT.Columns.Add("G_ID", typeof(int));
            dT.Columns.Add("PRACA_DNI", typeof(int));
            dT.Columns.Add("Kierowca", typeof(string));
            firstDayCell = 4;//    WERRY IMPORTANT -> indicate from what cell number start days

            if (DEBUG) MessageBox.Show("[LoadGrafik]\n" + "Loading data from: " + from.Date.ToShortDateString() + " to: " + to.Date.ToShortDateString());
            //Default load current month
            if ( DateTime.Compare(start, end) > 0  )
            {
                DateTime tmp = to;
                from = end;
                to = start;
                start = from;
                end = to;
            }

            dateTimePicker1.Value = start;
            dateTimePicker2.Value = end;

            int days = Convert.ToInt32((to - from).TotalDays);


            DateTime startTime = from;
            DateTime endTime = to;
            while (true)
            {
                dT.Columns.Add(startTime.ToShortDateString() + "\n" + startTime.ToString("        ddd"));
                startTime = startTime.AddDays(1);
                if (startTime > endTime) break;
            }

            //Fill DataTable with nessesery data.
            //Load Kierowcy
            tD = GetDataTable("SELECT k_ID,k_NAZWISKO,k_IMIE FROM KIEROWCY");
            foreach (DataRow row in tD.Rows)
            {
                DataRow dRow = dT.NewRow();
                dT.Rows.Add(dRow);
                dT.Rows[dT.Rows.Count - 1]["K_ID"] = row["k_ID"];
                dT.Rows[dT.Rows.Count - 1]["Kierowca"] = (string)row["k_NAZWISKO"] + " " + row["k_IMIE"];
            }

            //create help structor of GRAFIK Table
            addGrafik = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = -1");
            remGrafik = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = -1");
            updGrafik = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = -1");
            addGrafik.Clear();
            remGrafik.Clear();
            updGrafik.Clear();

            dWolne = GetDataTable("SELECT * FROM WOLNE WHERE ROK_KALENDARZOWY = " + start.Year.ToString());


            dataGridView2.DataSource = addGrafik;
            dataGridView3.DataSource = remGrafik;

            //Load zadania
            dZadania = GetDataTable("SELECT * FROM ZADANIA");

            //Load pojazdy
            dPojazdy = GetDataTable("SELECT * FROM POJAZDY");

            //Load kierowcy
            dKierowcy = GetDataTable("SELECT * FROM KIEROWCY");

            //Load zadania for specyfic days range
            dZadDzien = GetDataTable("SELECT * FROM GRAFIK WHERE g_DATA BETWEEN '" + start.Date.ToShortDateString() + "' AND '" + end.Date.ToShortDateString() + "'");
            //dataGridView2.DataSource = dZadDzien;

            //Load Grafik for specyfic Kierowca
            foreach (DataRow row in dT.Rows)
            {
                tD = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = " + row["K_ID"] + " AND g_DATA BETWEEN '" + start.Date.ToShortDateString() + "' AND '" + end.Date.ToShortDateString() + "'");
                if (DEBUG) MessageBox.Show("[LoadGrafik]\n" + "Loaded " + tD.Rows.Count.ToString() + " ZADAN");
                foreach (DataRow row2 in tD.Rows)
                {
                    DateTime enTime = new DateTime(((DateTime)row2["g_Data"]).Year, ((DateTime)row2["g_Data"]).Month, ((DateTime)row2["g_Data"]).Day);
                    int difTime = (enTime - start).Days;
                    //MessageBox.Show("Zadanie: " + row2["g_ZADANIE"].ToString());
                    if (((int)(row2["g_ZADANIE"])) <= 0)
                    {
                        switch (((int)row2["g_ZADANIE"]))
                        {
                            case ZADANIA.praca:
                                row[firstDayCell + difTime] = "P";
                                break;
                            case ZADANIA.prefA:
                                row[firstDayCell + difTime] = "PA";
                                break;
                            case ZADANIA.prefB:
                                row[firstDayCell + difTime] = "PB";
                                break;
                            case ZADANIA.wolne:
                                row[firstDayCell + difTime] = "W";
                                break;
                            case ZADANIA.wolneNZ:
                                row[firstDayCell + difTime] = "WNZ";
                                break;
                            case ZADANIA.wolneND:
                                row[firstDayCell + difTime] = "WND";
                                break;
                            case ZADANIA.urlop:
                                row[firstDayCell + difTime] = "UP";
                                break;
                            case ZADANIA.chorobowe:
                                row[firstDayCell + difTime] = "L4";
                                break;
                            default:
                                row[firstDayCell + difTime] = "INNE";
                                break;
                        }
                    }
                    else
                    {
                        DataRow[] zad = dZadania.Select("z_ID = " + row2["g_ZADANIE"]);
                        row[firstDayCell + difTime] = (string)zad[0]["z_ZADANIE"] + " (" + (string)zad[0]["z_BRYGADA"] + ")\n" + zad[0]["z_POCZATEK"].ToString().Remove(zad[0]["z_POCZATEK"].ToString().Length - 3) + " - " + zad[0]["z_KONIEC"].ToString().Remove(zad[0]["z_KONIEC"].ToString().Length - 3);
                    }
                }
            }
            dT.AcceptChanges();

            dataGridView1.DataSource = dT;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns["K_ID"].Frozen = true;
            dataGridView1.Columns["K_ID"].Visible = false;
            dataGridView1.Columns["G_ID"].Visible = false;
            dataGridView1.Columns["PRACA_DNI"].Visible = false;
            dataGridView1.Columns["Kierowca"].Frozen = true;
            dataGridView1.Columns["Kierowca"].Width = 180;
            dataGridView1.Columns["Kierowca"].DefaultCellStyle.BackColor = Color.Khaki;

            dataGridView1.EnableHeadersVisualStyles = false;
            startTime = start;
            for (int i = firstDayCell; i < dataGridView1.Columns.Count; i++)
            {
                if (startTime.DayOfWeek == DayOfWeek.Saturday)
                {
                    //dataGridView1.Columns[i].DefaultCellStyle.ForeColor = Color.Orange;
                    dataGridView1.Columns[i].HeaderCell.Style.ForeColor = Color.Orange;
                }
                if (startTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    //dataGridView1.Columns[i].DefaultCellStyle.ForeColor = Color.Red;
                    dataGridView1.Columns[i].HeaderCell.Style.ForeColor = Color.Red;
                }
                startTime = startTime.AddDays(1);
            }

            SetDGVColors();

            //kierowca
            label6.Text = "";
            //telefon
            label8.Text = "";
            //uwagi
            textBox1.Text = "";

            //zadanie
            label11.Text = "";
            //brygada
            label13.Text = "";
            //tabor
            label15.Text = "";
            //poczatek
            label17.Text = "";
            //koniec
            label19.Text = "";
            //podmiana
            label21.Text = "";

            //dni pracy pod rząd
            label23.Text = "";

            //pojazd
            label25.Text = "";
            //numer rej.
            label27.Text = "";

            //ilość godz pracy w miesiacu
            label29.Text = "";
            //ilość dni pod rząd
            label31.Text = "";
            //ilosc godz w tygodniu
            label33.Text = "";


            DataRow[] hlpR = dWolne.Select("ID = " + start.Month.ToString() );
            if(hlpR.Length != 1)
            {
                if(DEBUG) MessageBox.Show("Can't load work days data from WOLNE for month: " + start.Month.ToString());
                label36.Text = "?";
            }
            else
            {
                label36.Text = hlpR[0]["DNI_WOLNE"].ToString();
            }
            

            PostProcesGrafik();
            dgvInitDone = true;
        }

        private void grafikoweToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.wolne);
        }

        private void prefAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.prefA);
        }

        private void pracaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.praca);
        }

        private void prefBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.prefB);
        }

        private void urlopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.urlop);
        }

        private void naŻądanieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.wolneNZ);
        }

        private void opiekaNadDzieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.wolneND);
        }

        private void choroboweToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeGrafik(ZADANIA.chorobowe);
        }

        private void pokażToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //zadania
            Form2 f2 = new Form2(connStr);
            f2.StartPosition = FormStartPosition.CenterParent;
            f2.ShowDialog(); // Shows Form2
        }

        private void pokażToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //kierowcy
            Form3 f3 = new Form3(connStr);
            f3.StartPosition = FormStartPosition.CenterParent;
            f3.ShowDialog(); // Shows Form3
        }

        private void edytujToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Grafik zadań
            Form4 f4 = new Form4(connStr);
            f4.StartPosition = FormStartPosition.CenterParent;
            f4.ShowDialog(); // Shows Form3
        }

        private void dodajToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //kierowcy
            Form5 f5 = new Form5(connStr);
            f5.StartPosition = FormStartPosition.CenterParent;
            f5.ShowDialog(); // Shows Form3
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (editGrafik)
            {
                DialogResult dialogResult = MessageBox.Show("Zapisać zmiany?", "Uwaga", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    SaveGrafik();
                }
            }
            LoadGrafik(dateTimePicker1.Value, dateTimePicker2.Value);
        }

        private void kopiujToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //kopiuj
            cpyBuffer = dT.Rows[selectedRow][selectedCell].ToString();
        }

        private void wklejToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //wklej
            //if(cpyBuffer.Length > 0) dT.Rows[selectedRow][selectedCell] = cpyBuffer;
            MessageBox.Show("Need more work");
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            //MessageBox.Show("Sorted");
            dataGridView1.CurrentCell = null;
            SetDGVColors();
        }

        private void button6_Click(object sender, EventArgs e)// save
        {
            if (editGrafik)
            {
                SaveGrafik();
            }
            else
            {
                MessageBox.Show("Grafik nie jest w trybie edycji.\nNie można wprowadzić zmian.");
            }
        }

        private void SaveGrafik()
        {
            int addedCount = 0;
            int deletedCount = 0;
            int detachedCount = 0;
            int modifiedCount = 0;
            int unchangedCount = 0;
            int incorrectCount = 0;

            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                SqlCommand cmd = conn.CreateCommand();
                conn.Open();
                foreach (DataRow r in remGrafik.Rows)
                {
                    if (((int)r["g_ZADANIE"]) > 0)
                    {
                        cmd.CommandText = "";
                        cmd.Parameters.Clear();
                        //Update:
                        modifiedCount++;
                        cmd.CommandText = "UPDATE GRAFIK SET g_KIEROWCA = @kie WHERE g_ZADANIE = @za AND g_DATA = @da";
                        cmd.Parameters.AddWithValue("@kie", DBNull.Value);
                        cmd.Parameters.AddWithValue("@za", r["g_ZADANIE"]);
                        cmd.Parameters.AddWithValue("@da", r["g_DATA"]);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd.CommandText = "";
                        cmd.Parameters.Clear();
                        //Deleted:
                        deletedCount++;
                        cmd.CommandText = "DELETE FROM GRAFIK WHERE g_ZADANIE = @id AND g_KIEROWCA = @ki AND g_DATA = @da";
                        cmd.Parameters.AddWithValue("@id", r["g_ZADANIE"]);
                        cmd.Parameters.AddWithValue("@ki", r["g_KIEROWCA"]);
                        cmd.Parameters.AddWithValue("@da", r["g_DATA"]);
                        cmd.ExecuteNonQuery();
                    }
                }

                foreach (DataRow r in addGrafik.Rows)
                {
                    if (((int)r["g_ZADANIE"]) > 0)
                    {
                        cmd.CommandText = "";
                        cmd.Parameters.Clear();
                        //Update
                        modifiedCount++;
                        cmd.CommandText = "UPDATE GRAFIK SET g_KIEROWCA = @kie WHERE g_ZADANIE = @za AND g_DATA = @da";
                        cmd.Parameters.AddWithValue("@kie", r["g_KIEROWCA"]);
                        cmd.Parameters.AddWithValue("@za", r["g_ZADANIE"]);
                        cmd.Parameters.AddWithValue("@da", r["g_DATA"]);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd.CommandText = "";
                        cmd.Parameters.Clear();
                        //ADD
                        addedCount++;
                        cmd.CommandText = "INSERT INTO GRAFIK (g_ROK, g_MIESIAC, g_DZIEN, g_ZADANIE, g_KIEROWCA, g_DATA) VALUES (@rok, @mie, @dzi, @zad, @kie, @dat)";
                        cmd.Parameters.AddWithValue("@rok", r["g_ROK"]);
                        cmd.Parameters.AddWithValue("@mie", r["g_MIESIAC"]);
                        cmd.Parameters.AddWithValue("@dzi", r["g_DZIEN"]);
                        cmd.Parameters.AddWithValue("@zad", r["g_ZADANIE"]);
                        cmd.Parameters.AddWithValue("@kie", r["g_KIEROWCA"]);
                        cmd.Parameters.AddWithValue("@dat", r["g_DATA"]);
                        cmd.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            MessageBox.Show("Made changes to database are:\n1) Added: " + addedCount.ToString() + "\n2) Deleted: " + deletedCount.ToString() + "\n3) Modified: " + modifiedCount.ToString() + "\n4) Detached: " + detachedCount.ToString() + "\n5) Unchanged: " + unchangedCount.ToString() + "\n6) Incorrect: " + incorrectCount.ToString());
        }

        private void ChangeGrafik(int zadID)
        {
            //int sZadccc = Convert.ToInt32(listView1.SelectedItems[0].SubItems[0].Text);

            DateTime tmpS;
            tmpS = start.AddDays(dataGridView1.SelectedCells[0].ColumnIndex - firstDayCell);
            int kierowcaID = (int)dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value;
            int oldWork = DayWorkType(kierowcaID, tmpS);
            DataRow[] eK = dExtKierowcy.Select("k_ID = " + kierowcaID.ToString());
            if (eK.Length >= 1)
            {
                if (eK.Length > 1)
                {
                    MessageBox.Show("ChangeGrafik 001a\n" + "No Existing driver");
                }
            }
            else
            {
                MessageBox.Show("ChangeGrafik 001b\n" + "No Existing driver");
                return;
            }

            DataRow[] oZad = dZadania.Select("z_ID = " + oldWork.ToString());
            DataRow[] cZad = dZadania.Select("z_ID = " + zadID.ToString());
            DataRow[] selZad = dZadDzien.Select("g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + kierowcaID.ToString());
            DataRow[] selRemZad = remGrafik.Select("g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + kierowcaID.ToString());
            DataRow[] selAddZad = addGrafik.Select("g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + kierowcaID.ToString());
            int sZad = zadID;// Convert.ToInt32(listView1.SelectedItems[0].SubItems[0].Text);
            bool isSameZad = false;
            bool isSameRemZad = false;
            bool isSameAddZad = false;
            if (selZad.Length > 0) if (((int)(selZad[0]["g_ZADANIE"])) == sZad) isSameZad = true;
            if (selRemZad.Length > 0) if (((int)(selRemZad[0]["g_ZADANIE"])) == sZad) isSameRemZad = true;
            if (selAddZad.Length > 0) if (((int)(selAddZad[0]["g_ZADANIE"])) == sZad) isSameAddZad = true;

            //juz jest to zadanie aktywne -> nic nie rob
            if (isSameZad && !isSameRemZad)
            {
                //MessageBox.Show("Juz jest to zadanie dodane");
                return;
            }
            //dla tego kierowcy nie ma nigdzie zadnego zadania -> dodaj wybrane
            if (selZad.Length == 0 && selAddZad.Length == 0 && selRemZad.Length == 0)
            {
                //MessageBox.Show("Dodajemy nowe zadanie");
                DataRow row2 = addGrafik.NewRow();
                row2["g_ROK"] = tmpS.Year;
                row2["g_MIESIAC"] = tmpS.Month;
                row2["g_DZIEN"] = tmpS.Day;
                row2["g_ZADANIE"] = sZad;//Convert.ToInt32(listView1.SelectedItems[0].SubItems[0].Text);
                row2["g_KIEROWCA"] = kierowcaID;
                row2["g_DATA"] = tmpS.ToShortDateString();
                addGrafik.Rows.Add(row2);
                if (sZad <= 0)
                {
                    //dataGridView1.SelectedCells[0].Value = sZad.ToString();
                    SetDGVColors(sZad, dataGridView1.SelectedCells[0]);
                    SetDGVText(sZad, dataGridView1.SelectedCells[0]);
                    if(sZad == ZADANIA.praca)
                    {
                        eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                        eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(TimeSpan.FromHours(8));
                    }
                }
                else
                {
                    dataGridView1.SelectedCells[0].Style.BackColor = Color.White;
                    dataGridView1.SelectedCells[0].Value = cZad[0]["z_ZADANIE"].ToString() + " (" + cZad[0]["z_BRYGADA"].ToString() + ")\n" + cZad[0]["z_POCZATEK"].ToString().Remove(cZad[0]["z_POCZATEK"].ToString().Length - 3) + " - " + cZad[0]["z_KONIEC"].ToString().Remove(cZad[0]["z_KONIEC"].ToString().Length - 3);//listView1.SelectedItems[0].SubItems[1].Text + " (" + listView1.SelectedItems[0].SubItems[2].Text + ")";
                    eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                    eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add( (TimeSpan)cZad[0]["z_CZAS_PRACY"]);
                }
                UpdateZadania(tmpS);
                label23.Text = eK[0]["il_dni_pracy"].ToString();
                label29.Text = string.Format("{0}:{1:mm}:{2:ss}", (int)((TimeSpan)eK[0]["il_godz_pracy"]).TotalHours, (TimeSpan)eK[0]["il_godz_pracy"], (TimeSpan)eK[0]["il_godz_pracy"]);
                return;
            }

            if (selAddZad.Length > 0)
            {
                if (isSameAddZad)
                {
                    //MessageBox.Show("Takie zadanie juz jest dodane");
                    return;
                }
                //MessageBox.Show("Juz jest jakies dodane zadanie dla tego kierowcy wiec skasuj je i dodaj nowe");
                addGrafik.Rows.Remove(selAddZad[0]);
                //MessageBox.Show("Dodajemy nowe zadanie");
                DataRow row2 = addGrafik.NewRow();
                row2["g_ROK"] = tmpS.Year;
                row2["g_MIESIAC"] = tmpS.Month;
                row2["g_DZIEN"] = tmpS.Day;
                row2["g_ZADANIE"] = sZad;//Convert.ToInt32(listView1.SelectedItems[0].SubItems[0].Text);
                row2["g_KIEROWCA"] = kierowcaID;
                row2["g_DATA"] = tmpS.ToShortDateString();
                addGrafik.Rows.Add(row2);
                if(oldWork <= 0)
                {
                    if(oldWork == ZADANIA.praca)
                    {
                        eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] - 1;
                        eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(TimeSpan.FromHours(-8));
                    }
                }
                else
                {
                    eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] - 1;
                    eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add( -((TimeSpan)oZad[0]["z_CZAS_PRACY"]));
                }

                if (sZad <= 0)
                {
                    //dataGridView1.SelectedCells[0].Value = sZad.ToString();
                    SetDGVColors(sZad, dataGridView1.SelectedCells[0]);
                    SetDGVText(sZad, dataGridView1.SelectedCells[0]);
                    if (sZad == ZADANIA.praca)
                    {
                        eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                        eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(TimeSpan.FromHours(8));
                    }
                }
                else
                {
                    dataGridView1.SelectedCells[0].Style.BackColor = Color.White;
                    dataGridView1.SelectedCells[0].Value = cZad[0]["z_ZADANIE"].ToString() + " (" + cZad[0]["z_BRYGADA"].ToString() + ")\n" + cZad[0]["z_POCZATEK"].ToString().Remove(cZad[0]["z_POCZATEK"].ToString().Length - 3) + " - " + cZad[0]["z_KONIEC"].ToString().Remove(cZad[0]["z_KONIEC"].ToString().Length - 3); //listView1.SelectedItems[0].SubItems[1].Text + " (" + listView1.SelectedItems[0].SubItems[2].Text + ")";
                    eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                    eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add((TimeSpan)cZad[0]["z_CZAS_PRACY"]);
                }
                UpdateZadania(tmpS);
                label23.Text = eK[0]["il_dni_pracy"].ToString();
                label29.Text = string.Format("{0}:{1:mm}:{2:ss}", (int)((TimeSpan)eK[0]["il_godz_pracy"]).TotalHours, (TimeSpan)eK[0]["il_godz_pracy"], (TimeSpan)eK[0]["il_godz_pracy"]);
                return;
            }

            if (selZad.Length == 0 && selAddZad.Length == 0)
            {
                //MessageBox.Show("Nie ma żadnego aktywnie przydzielonego zadania wiec dodaj nowe");
                DataRow row2 = addGrafik.NewRow();
                row2["g_ROK"] = tmpS.Year;
                row2["g_MIESIAC"] = tmpS.Month;
                row2["g_DZIEN"] = tmpS.Day;
                row2["g_ZADANIE"] = sZad;//Convert.ToInt32(listView1.SelectedItems[0].SubItems[0].Text);
                row2["g_KIEROWCA"] = kierowcaID;
                row2["g_DATA"] = tmpS.ToShortDateString();
                addGrafik.Rows.Add(row2);
                if (sZad <= 0)
                {
                    //dataGridView1.SelectedCells[0].Value = sZad.ToString();
                    SetDGVColors(sZad, dataGridView1.SelectedCells[0]);
                    SetDGVText(sZad, dataGridView1.SelectedCells[0]);
                    if (sZad == ZADANIA.praca)
                    {
                        eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                        eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(TimeSpan.FromHours(8));
                    }
                }
                else
                {
                    dataGridView1.SelectedCells[0].Style.BackColor = Color.White;
                    dataGridView1.SelectedCells[0].Value = cZad[0]["z_ZADANIE"].ToString() + " (" + cZad[0]["z_BRYGADA"].ToString() + ")\n" + cZad[0]["z_POCZATEK"].ToString().Remove(cZad[0]["z_POCZATEK"].ToString().Length - 3) + " - " + cZad[0]["z_KONIEC"].ToString().Remove(cZad[0]["z_KONIEC"].ToString().Length - 3); //listView1.SelectedItems[0].SubItems[1].Text + " (" + listView1.SelectedItems[0].SubItems[2].Text + ")";
                    eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                    eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add((TimeSpan)cZad[0]["z_CZAS_PRACY"]);
                }
                UpdateZadania(tmpS);
                label23.Text = eK[0]["il_dni_pracy"].ToString();
                label29.Text = string.Format("{0}:{1:mm}:{2:ss}", (int)((TimeSpan)eK[0]["il_godz_pracy"]).TotalHours, (TimeSpan)eK[0]["il_godz_pracy"], (TimeSpan)eK[0]["il_godz_pracy"]);
                return;
            }

            //dla tego kierowcy juz jest jakies zadanie w tym dniu
            if (selZad.Length > 0)
            {
                //MessageBox.Show("Coś już mamy usun to cos i daj nowe");
                DataRow row = remGrafik.NewRow();
                row["g_ROK"] = tmpS.Year;
                row["g_MIESIAC"] = tmpS.Month;
                row["g_DZIEN"] = tmpS.Day;
                row["g_ZADANIE"] = selZad[0]["g_ZADANIE"];
                row["g_KIEROWCA"] = kierowcaID;
                row["g_DATA"] = tmpS.ToShortDateString();
                remGrafik.Rows.Add(row);
                selZad[0]["g_KIEROWCA"] = DBNull.Value;

                DataRow row2 = addGrafik.NewRow();
                row2["g_ROK"] = tmpS.Year;
                row2["g_MIESIAC"] = tmpS.Month;
                row2["g_DZIEN"] = tmpS.Day;
                row2["g_ZADANIE"] = sZad;//Convert.ToInt32(listView1.SelectedItems[0].SubItems[0].Text);
                row2["g_KIEROWCA"] = kierowcaID;
                row2["g_DATA"] = tmpS.ToShortDateString();
                addGrafik.Rows.Add(row2);

                if (oldWork <= 0)
                {
                    if (oldWork == ZADANIA.praca)
                    {
                        eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] - 1;
                        eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(TimeSpan.FromHours(-8));
                    }
                }
                else
                {
                    eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] - 1;
                    eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(-((TimeSpan)oZad[0]["z_CZAS_PRACY"]));
                }

                if (sZad <= 0)
                {
                    //dataGridView1.SelectedCells[0].Value = sZad.ToString();
                    SetDGVColors(sZad, dataGridView1.SelectedCells[0]);
                    SetDGVText(sZad, dataGridView1.SelectedCells[0]);
                    if (sZad == ZADANIA.praca)
                    {
                        eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                        eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(TimeSpan.FromHours(8));
                    }
                }
                else
                {
                    dataGridView1.SelectedCells[0].Style.BackColor = Color.White;
                    dataGridView1.SelectedCells[0].Value = cZad[0]["z_ZADANIE"].ToString() + " (" + cZad[0]["z_BRYGADA"].ToString() + ")\n" + cZad[0]["z_POCZATEK"].ToString().Remove(cZad[0]["z_POCZATEK"].ToString().Length - 3) + " - " + cZad[0]["z_KONIEC"].ToString().Remove(cZad[0]["z_KONIEC"].ToString().Length - 3); //listView1.SelectedItems[0].SubItems[1].Text + " (" + listView1.SelectedItems[0].SubItems[2].Text + ")";
                    eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] + 1;
                    eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add((TimeSpan)cZad[0]["z_CZAS_PRACY"]);
                }
                UpdateZadania(tmpS);
                label23.Text = eK[0]["il_dni_pracy"].ToString();
                label29.Text = string.Format("{0}:{1:mm}:{2:ss}", (int)((TimeSpan)eK[0]["il_godz_pracy"]).TotalHours, (TimeSpan)eK[0]["il_godz_pracy"], (TimeSpan)eK[0]["il_godz_pracy"]);
                return;
            }
            MessageBox.Show("ChangeGrafik\n" + "Nic nie pasuje.. STAN: {" + isSameZad.ToString() + "," + isSameAddZad.ToString() + "," + isSameRemZad.ToString() + "} Il. zadań: {" + selZad.Length.ToString() + "," + selAddZad.Length.ToString() + "," + selRemZad.Length.ToString() + "}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && dataGridView1.SelectedCells.Count > 0)
            {
                //MessageBox.Show("Curent color = " + listView1.SelectedItems[0].ForeColor.ToString());
                if (listView1.SelectedItems[0].ForeColor == Color.Black)
                {
                    //MessageBox.Show("Ktos juz ma to zadanie");
                    return;
                }
                int zadId = Convert.ToInt32(listView1.SelectedItems[0].SubItems[0].Text);
                ChangeGrafik(zadId);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                DateTime tmpS;
                tmpS = start.AddDays(dataGridView1.SelectedCells[0].ColumnIndex - firstDayCell);
                DataRow[] zad = dZadDzien.Select("g_KIEROWCA =" + dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value.ToString() + " AND g_DATA ='" + tmpS.ToShortDateString() + "'");
                if (zad.Length == 1)
                {
                    dataGridView1.SelectedCells[0].Value = "";
                    DataRow row = remGrafik.NewRow();
                    row["g_ROK"] = tmpS.Year;
                    row["g_MIESIAC"] = tmpS.Month;
                    row["g_DZIEN"] = tmpS.Day;
                    row["g_ZADANIE"] = zad[0]["g_ZADANIE"];
                    row["g_KIEROWCA"] = dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value;
                    row["g_DATA"] = tmpS.ToShortDateString();
                    remGrafik.Rows.Add(row);

                    zad[0]["g_KIEROWCA"] = DBNull.Value;
                }
                else
                {
                    zad = addGrafik.Select("g_KIEROWCA =" + dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value.ToString() + " AND g_DATA ='" + tmpS.ToShortDateString() + "'");
                    if (zad.Length == 1)
                    {
                        dataGridView1.SelectedCells[0].Value = "";
                        addGrafik.Rows.Remove(zad[0]);
                    }
                    else
                    {
                        MessageBox.Show("Wybież zadanie do usunięcia");
                    }
                }
                dataGridView1.SelectedCells[0].Style.BackColor = Color.White;
                /*
                CurrencyManager xCM = (CurrencyManager)dataGridView1.BindingContext[dataGridView1.DataSource, dataGridView1.DataMember];
                DataRowView xDRV = (DataRowView)xCM.Current;
                DataRow xRow = xDRV.Row;
                MessageBox.Show("Well...: " + xRow[0].ToString());
                 */
            }
            else
            {
                MessageBox.Show("Musi być wybrany kierowca!");
            }
        }

        private void usuńWszystkoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                DateTime tmpS;
                tmpS = start.AddDays(dataGridView1.SelectedCells[0].ColumnIndex - firstDayCell);

                int kierowcaID = (int)dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value;
                int oldWork = DayWorkType(kierowcaID, tmpS);
                DataRow[] eK = dExtKierowcy.Select("k_ID = " + kierowcaID.ToString());
                if (eK.Length >= 1)
                {
                    if (eK.Length > 1)
                    {
                        MessageBox.Show("ChangeGrafik 001a\n" + "No Existing driver");
                    }
                }
                else
                {
                    MessageBox.Show("ChangeGrafik 001b\n" + "No Existing driver");
                    return;
                }

                DataRow[] oZad = dZadania.Select("z_ID = " + oldWork.ToString());
                DataRow[] selZad = dZadDzien.Select("g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value);
                DataRow[] selRemZad = remGrafik.Select("g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value);
                DataRow[] selAddZad = addGrafik.Select("g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value);
                if (selZad.Length > 0)
                {
                    DataRow row = remGrafik.NewRow();
                    row["g_ROK"] = tmpS.Year;
                    row["g_MIESIAC"] = tmpS.Month;
                    row["g_DZIEN"] = tmpS.Day;
                    row["g_ZADANIE"] = selZad[0]["g_ZADANIE"];
                    row["g_KIEROWCA"] = dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value;
                    row["g_DATA"] = tmpS.ToShortDateString();
                    remGrafik.Rows.Add(row);
                    selZad[0]["g_KIEROWCA"] = DBNull.Value;
                }
                if (selAddZad.Length > 0)
                {
                    addGrafik.Rows.Remove(selAddZad[0]);
                }
                dataGridView1.SelectedCells[0].Value = "";
                dataGridView1.SelectedCells[0].Style.BackColor = Color.White;
                UpdateZadania(tmpS);
                //MessageBox.Show("usuńWszystkoToolStripMenuItem_Click\n" + "CALL -> PostProcesGrafikL");
                PostProcesGrafikL(kierowcaID);
                if (oldWork <= 0)
                {
                    if (oldWork == ZADANIA.praca)
                    {
                        eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] - 1;
                        eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(TimeSpan.FromHours(-8));
                    }
                }
                else
                {
                    eK[0]["il_dni_pracy"] = (int)eK[0]["il_dni_pracy"] - 1;
                    eK[0]["il_godz_pracy"] = ((TimeSpan)eK[0]["il_godz_pracy"]).Add(-((TimeSpan)oZad[0]["z_CZAS_PRACY"]));
                }
                label23.Text = eK[0]["il_dni_pracy"].ToString();
                label29.Text = string.Format("{0}:{1:mm}:{2:ss}", (int)((TimeSpan)eK[0]["il_godz_pracy"]).TotalHours, (TimeSpan)eK[0]["il_godz_pracy"], (TimeSpan)eK[0]["il_godz_pracy"]);
            }
        }

        private void PostProcesGrafik()
        {
            DataTable tD;

            int dniWolne = 0;
            int[] fDays = new int[31];// tab for check free days

            dExtKierowcy = new DataTable();
            dExtKierowcy.Columns.Add("k_ID", typeof(int));
            dExtKierowcy.Columns.Add("il_dni_pracy", typeof(int));
            dExtKierowcy.Columns.Add("il_godz_pracy", typeof(TimeSpan));

            dExtKierowcy.Columns.Add("il_dni_pracy_przed_data", typeof(int));
            dExtKierowcy.Columns.Add("il_dni_pracy_ciag_przed_data", typeof(int));
            dExtKierowcy.Columns.Add("il_godz_pracy_przed_data", typeof(TimeSpan));
            dExtKierowcy.Columns.Add("zad_ostatni_dzien", typeof(int));
            dExtKierowcy.Columns.Add("dni_wolne", typeof(int));

            //Get working days and time count for all drivers
            foreach (DataRow r in dKierowcy.Rows)
            {
                DateTime st = start.AddDays(-1);
                DateTime en;
                int ile_dni = 0;
                TimeSpan ile_godz = new TimeSpan();
                int zad_przed = 0;

                tD = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = " + r["k_ID"].ToString() + " AND g_DATA = '" + st.Date.ToShortDateString() + "'");
                if (tD.Rows.Count > 0)
                {
                    if (tD.Rows.Count > 1)
                    {
                        MessageBox.Show("PostProcesGrafik\n" + "Error 001 - too much data");
                        zad_przed = (int)tD.Rows[0]["g_ZADANIE"];
                    }
                    else
                    {
                        zad_przed = (int)tD.Rows[0]["g_ZADANIE"];
                    }
                }
                else
                {
                    zad_przed = 0;
                }


                for (int i = 0; i < 31; i++)//zero table of free days
                {
                    fDays[i] = 0;
                }

                st = new DateTime(start.Year, start.Month, 1);
                en = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month));
                tD = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = " + r["k_ID"].ToString() + " AND g_DATA BETWEEN '" + st.Date.ToShortDateString() + "' AND '" + en.Date.ToShortDateString() + "'");
                foreach (DataRow row in tD.Rows)
                {
                    if ((int)row["g_ZADANIE"] > 0 || (int)row["g_ZADANIE"] == ZADANIA.praca)
                    {
                        fDays[((DateTime)row["g_DATA"]).Day] = 1;
                        ile_dni++;
                        if ((int)row["g_ZADANIE"] == ZADANIA.praca)
                        {
                            ile_godz = ile_godz.Add(TimeSpan.FromHours(8));
                        }
                        else
                        {
                            DataRow[] selZad = dZadania.Select("z_ID = " + row["g_ZADANIE"].ToString());
                            if (selZad.Length > 0)
                            {
                                ile_godz = ile_godz.Add((TimeSpan)selZad[0]["z_CZAS_PRACY"]);
                                if(DEBUG) MessageBox.Show("[PostProcesGrafik]\n" + "Czas pracy w zadaniu: " + ((TimeSpan)selZad[0]["z_CZAS_PRACY"]).ToString());
                            }
                        }
                    }
                }

                if (DEBUG) MessageBox.Show("[PostProcesGrafik]\n" + "Ilość dni pracy w miesiącu: " + ile_dni.ToString() + "\nCzas pracy kierowcy w miesiącu: " + ile_godz.ToString());
                dniWolne = 0;
                for (int i = 0; i < end.Day; i++)
                {
                    if (fDays[i] == 0)
                    {
                        dniWolne++;
                    }
                }
                dExtKierowcy.Rows.Add(r["k_ID"], ile_dni, ile_godz, 0, 0 , TimeSpan.Zero, zad_przed, dniWolne);
            }

            //Check for too many working days and too short break
            foreach (DataRow r in dKierowcy.Rows)
            {
                DateTime st = start.AddDays(-6);
                DateTime en = end;
                int ileDni = 0;
                DateTime lastDay = st;
                int lastDayZadanie = 0;
                TimeSpan ileGodz = new TimeSpan();
                tD = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = " + r["k_ID"].ToString() + " AND g_DATA BETWEEN '" + st.Date.ToShortDateString() + "' AND '" + en.Date.ToShortDateString() + "'" + " ORDER BY g_DATA");
                foreach (DataRow row in tD.Rows)
                {
                    if ((int)row["g_ZADANIE"] > 0 || (int)row["g_ZADANIE"] == ZADANIA.praca)
                    {
                        //MessageBox.Show("[PostProcesGrafik]\n" + " KIEROWCA: " + r["k_ID"].ToString() + " St: " + lastDay.ToShortDateString() + " En: " + ((DateTime)row["g_DATA"]).ToShortDateString() +  "\n Diff days: " + (((DateTime)row["g_DATA"]) - lastDay).Days.ToString() + " Working days: " + ileDni.ToString() );
                        if( (((DateTime)row["g_DATA"]) - lastDay).Days == 1)
                        {
                            //MessageBox.Show("[PostProcesGrafik]\n" + "Zadanie L: " + lastDayZadanie.ToString() + " Zadanie N: " + row["g_ZADANIE"].ToString());
                            //spr czy przerwa mniejsza niż 10 godz.
                            TimeSpan endLast = new TimeSpan();
                            TimeSpan startNew = new TimeSpan();
                            DataRow[] sel = dZadania.Select("z_ID = " + row["g_ZADANIE"].ToString());
                            if (sel.Length > 0)
                            {
                                startNew = startNew.Add((TimeSpan)sel[0]["z_POCZATEK"]);
                            }
                            sel = dZadania.Select("z_ID = " + lastDayZadanie.ToString());
                            if (sel.Length > 0)
                            {
                                endLast = endLast.Add((TimeSpan)sel[0]["z_KONIEC"]);
                            }
                            if (endLast.TotalMinutes > 0 && startNew.TotalMinutes > 0)
                            {
                                TimeSpan midnight = new TimeSpan(24, 0, 0);
                                TimeSpan diff = midnight.Subtract(endLast) + startNew;
                                TimeSpan breakTimeNorma = new TimeSpan(10, 0, 0);
                                TimeSpan breakTimeShort = new TimeSpan(9, 0, 0);
                                //MessageBox.Show("[PostProcesGrafik]\n" + "Koniec: " + endLast.ToString() + " Poczatek: " + startNew.ToString() + "\nDIFF TIME: " + diff.ToString());
                                if (diff < breakTimeNorma)
                                {
                                    if (diff < breakTimeShort)
                                    {
                                        foreach (DataGridViewRow r2 in dataGridView1.Rows)
                                        {
                                            //MessageBox.Show("DGV K_ID : " + r2.Cells["K_ID"].Value.ToString() + " SEL: " + r["k_ID"].ToString() );
                                            if ((int)r2.Cells["K_ID"].Value == (int)r["k_ID"])
                                            {
                                                r2.Cells[firstDayCell + (((DateTime)row["g_DATA"]) - start).Days].Style.BackColor = Color.Red;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataGridViewRow r2 in dataGridView1.Rows)
                                        {
                                            //MessageBox.Show("DGV K_ID : " + r2.Cells["K_ID"].Value.ToString() + " SEL: " + r["k_ID"].ToString() );
                                            if ((int)r2.Cells["K_ID"].Value == (int)r["k_ID"])
                                            {
                                                r2.Cells[firstDayCell + (((DateTime)row["g_DATA"]) - start).Days].Style.BackColor = Color.OrangeRed;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            //spr czy wiecej niz 6 dni pod rząd
                            ileDni++;
                            if (ileDni >= 6)
                            {
                                if (((DateTime)row["g_DATA"]) >= start)
                                {
                                    foreach(DataGridViewRow r2 in dataGridView1.Rows)
                                    {
                                        //MessageBox.Show("DGV K_ID : " + r2.Cells["K_ID"].Value.ToString() + " SEL: " + r["k_ID"].ToString() );
                                        if( (int)r2.Cells["K_ID"].Value == (int)r["k_ID"])
                                        {
                                            r2.Cells[firstDayCell + (((DateTime)row["g_DATA"]) - start).Days].Style.BackColor = Color.DarkRed;
                                            break;
                                        }
                                    }
                                    //MessageBox.Show("7 day work....");
                                }
                            }
                        }
                        else
                        {
                            ileDni = 0;
                        }
                        lastDay = ((DateTime)row["g_DATA"]);
                        lastDayZadanie = ((int)row["g_ZADANIE"]);
                    }
                    else
                    {
                        ileDni = 0;
                    }
                }
            }

            foreach (DataRow r in dExtKierowcy.Rows)
            {
                DateTime st = start.AddDays(-6);
                int ileDni = 0;
                int ileDniCiag = 0;
                DateTime lastDay = st;
                int lastDayZadanie = 0;
                TimeSpan ileGodz = new TimeSpan();

                for(int i = 0; i < 6; i++)
                {
                    tD = GetDataTable("SELECT * FROM GRAFIK WHERE g_KIEROWCA = " + r["k_ID"].ToString() + " AND g_DATA = '" + st.Date.ToShortDateString() + "'");
                    if(tD.Rows.Count >= 1)
                    {
                        if (tD.Rows.Count > 1)
                        {
                            MessageBox.Show("Błąd w grafiku - więcej niż jedno zadanie na dzień!");
                            if (DEBUG) MessageBox.Show("[PostProcesGrafik]\n" + "Error more then 1 work for day.\nCount: " + tD.Rows.Count.ToString() + " Data: " + st.ToShortDateString());
                        }
                        //always checkk first returned record..
                        if ((int)tD.Rows[0]["g_ZADANIE"] > 0 || (int)tD.Rows[0]["g_ZADANIE"] == ZADANIA.praca)
                        {
                            ileDni++;
                            ileDniCiag++;
                        }
                    }
                    else
                    {
                        ileDniCiag = 0;
                    }
                    st = st.AddDays(1);
                }

                //dExtKierowcy.Columns.Add("il_dni_pracy_przed_data", typeof(int));
                //dExtKierowcy.Columns.Add("il_dni_pracy_ciag_przed_data", typeof(int));
                //dExtKierowcy.Columns.Add("il_godz_pracy_przed_data", typeof(TimeSpan));
                r["il_dni_pracy_przed_data"] = ileDni;
                r["il_dni_pracy_ciag_przed_data"] = ileDniCiag;
            }

        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            /*
            */
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridView1.SelectedCells.Count > 0)
            {
                DateTime tmpS;
                tmpS = start.AddDays(e.ColumnIndex - firstDayCell);
                UpdateZadania(tmpS);

                //kierowca
                label6.Text = "";
                //telefon
                label8.Text = "";
                //uwagi
                textBox1.Text = "";

                //zadanie
                label11.Text = "";
                //brygada
                label13.Text = "";
                //tabor
                label15.Text = "";
                //poczatek
                label17.Text = "";
                //koniec
                label19.Text = "";
                //podmiana
                label21.Text = "";

                //ilosc dni pracy w miesiacu
                label23.Text = "";

                //pojazd
                label25.Text = "";
                //numer rej.
                label27.Text = "";

                //ilość godz pracy w miesiacu
                label29.Text = "";
                //ilość dni pod rząd
                label31.Text = "";
                //ilosc godz w tygodniu
                label33.Text = "";

                int kierowca = Convert.ToInt32(dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value.ToString());
                if (kierowca > 0)
                {
                    DataRow[] curZad = dZadDzien.Select("g_DATA = '" + tmpS.ToShortDateString() + "' AND g_KIEROWCA = " + kierowca.ToString());
                    DataRow[] curKier = dKierowcy.Select("k_ID = " + kierowca.ToString());
                    if (curZad.Length > 0)
                    {
                        DataRow[] zad1 = dZadania.Select("z_ID = " + curZad[0]["g_ZADANIE"].ToString());
                        if (zad1.Length > 0)
                        {
                            //zadanie
                            label11.Text = (string)zad1[0]["z_ZADANIE"];
                            //brygada
                            label13.Text = (string)zad1[0]["z_BRYGADA"];
                            //tabor
                            label15.Text = (string)zad1[0]["z_TABOR"];
                            //poczatek
                            label17.Text = zad1[0]["z_POCZATEK"].ToString();
                            //koniec
                            label19.Text = zad1[0]["z_KONIEC"].ToString();
                            //podmiana
                            label21.Text = (string)zad1[0]["z_PODMIANA"];
                        }
                    }
                    if (curKier.Length > 0)
                    {
                        //kierowca
                        label6.Text = curKier[0]["k_NAZWISKO"].ToString() + " " + curKier[0]["k_IMIE"].ToString();
                        //telefon
                        label8.Text = curKier[0]["k_TELEFON"].ToString();
                        //uwagi
                        textBox1.Text = curKier[0]["k_UWAGI"].ToString();

                        DataRow[] selInf = dExtKierowcy.Select("k_ID = " + curKier[0]["k_ID"].ToString());
                        if (selInf.Length > 0)
                        {
                            //Dni pracy w miesiącu
                            label23.Text = selInf[0]["il_dni_pracy"].ToString();
                            //Godziny pracy w miesiącu\
                            label29.Text = string.Format("{0}:{1:mm}:{2:ss}", (int)((TimeSpan)selInf[0]["il_godz_pracy"]).TotalHours, (TimeSpan)selInf[0]["il_godz_pracy"], (TimeSpan)selInf[0]["il_godz_pracy"]);
                            label38.Text = selInf[0]["dni_wolne"].ToString();
                        }
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////       TO JESZCZE DO UZUPEŁNIENIA 


                    //pojazd
                    label25.Text = "";
                    //numer rej.
                    label27.Text = "";

                    //ilość dni pod rząd
                    label31.Text = "";
                    //ilosc godz w tygodniu
                    label33.Text = "";
                }
            }
            else
            {
                label1.Text = "Zadania na dzień: ?";
                listView1.Clear();
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridView1.SelectedCells.Count > 0)
            {
                int kierowca = Convert.ToInt32(dataGridView1.SelectedCells[0].OwningRow.Cells["K_ID"].Value.ToString());
                //MessageBox.Show("dataGridView1_CellValueChanged\n" + " CALL -> PostProcesGrafikL");
                PostProcesGrafikL(kierowca);
                //dExtKierowcy.Columns.Add("il_dni_pracy", typeof(int));
                //dExtKierowcy.Columns.Add("il_godz_pracy", typeof(TimeSpan));
            }
            else
            {
                MessageBox.Show("dataGridView1_CellValueChanged\n" + "Error - no selected cell");
            }
        }

        private void PostProcesGrafikL(int kierowcaID)
        {
            if (dataGridView1.SelectedCells.Count <= 0)
            {
                MessageBox.Show("PostProcesGrafik\n" + "Error - No cell selected..");
                return;
            }
            DataRow[] selK = dExtKierowcy.Select("k_ID = " + kierowcaID.ToString());
            if (selK.Length > 0)
            {
                //process grafik data
                int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
                int columnIndex = dataGridView1.SelectedCells[0].ColumnIndex;
                DateTime selD = start.AddDays(dataGridView1.SelectedCells[0].ColumnIndex - firstDayCell);
                DateTime tmpD = selD;
                //MessageBox.Show("Kierowca ID: " + kierowcaID.ToString() + " Selected cell data: " + dataGridView1.SelectedCells[0].Value.ToString() + "\nRow: " + rowIndex.ToString() + " Column: " + columnIndex.ToString() + "\nSelected data: " + selD.ToShortDateString() );

                //check day of work
                int startingD = 0;
                int daysOfWork = 0;
                if ((dataGridView1.SelectedCells[0].ColumnIndex - firstDayCell) > 7)
                {
                    startingD = dataGridView1.SelectedCells[0].ColumnIndex - 7;
                    daysOfWork = 0;
                }
                else
                {
                    startingD = firstDayCell;
                    daysOfWork = (int)selK[0]["il_dni_pracy_ciag_przed_data"];
                }
                tmpD = selD;
                //bool isWorking = IsWorkingDay(kierowcaID, selD);
                //if (isWorking) MessageBox.Show("This is working day");
                //else MessageBox.Show("This is free day");
                int howManyWorkingDays = 0;
                while (true)//check working days on left side <--
                {
                    if(IsWorkingDay(kierowcaID, tmpD))
                    {
                        tmpD = tmpD.AddDays(-1);
                        howManyWorkingDays++;
                        if(tmpD < start)
                        {
                            howManyWorkingDays += (int)selK[0]["il_dni_pracy_ciag_przed_data"];
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    if (howManyWorkingDays > 6) break;//dont check more then 7 working days
                }

                tmpD = selD;
                howManyWorkingDays--;
                if (howManyWorkingDays < 0) howManyWorkingDays = 0; 
                while (true)//check working days on right side -->
                {
                    if (IsWorkingDay(kierowcaID, tmpD))
                    {
                        howManyWorkingDays++;
                        if (howManyWorkingDays >= 7)
                        {
                            if (tmpD >= start && tmpD <= end)
                            {
                                dataGridView1.Rows[rowIndex].Cells[firstDayCell + (tmpD - start).Days].Style.BackColor = Color.DarkRed;
                                //MessageBox.Show("Make it red xD");
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (tmpD >= start && tmpD <= end)
                            {
                                SetDGVColors(DayWorkType(kierowcaID, tmpD), dataGridView1.Rows[rowIndex].Cells[firstDayCell + (tmpD - start).Days]);
                                //MessageBox.Show("Make it normall xD");
                            }
                            else
                            {
                                break;
                            }
                        }
                        tmpD = tmpD.AddDays(1);
                        if (tmpD >= end)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (tmpD != selD) break;
                        else tmpD = tmpD.AddDays(1);
                    }
                }

                //spr czy przerwa mniejsza niż 10 godz.
                TimeSpan endLast = new TimeSpan();
                TimeSpan startNew = new TimeSpan();
                int workType = 0;
                tmpD = selD;
                //MessageBox.Show("Checking time diff for data: " + tmpD.ToShortDateString());
                while (true)
                {
                    startNew = TimeSpan.Zero;
                    endLast = TimeSpan.Zero;
                    workType = 0;
                    if (tmpD >= start && tmpD <= end)
                    {
                        workType = DayWorkType(kierowcaID, tmpD);
                        if (workType <= 0) // check time diff only for full specified work time jobs
                        {
                            //MessageBox.Show("Break - No full time set job");
                            break;
                        }
                        DataRow[] sel = dZadania.Select("z_ID = " + workType.ToString());
                        if (sel.Length > 0)
                        {
                            startNew = startNew.Add((TimeSpan)sel[0]["z_POCZATEK"]);
                            //MessageBox.Show("Poczatek dla daty: " + tmpD.ToShortDateString() + " TIME: " + startNew.ToString());
                        }
                        if (tmpD <= start)
                        {
                            //dExtKierowcy.Columns.Add("zad_ostatni_dzien", typeof(int));
                            DataRow[] tmK = dExtKierowcy.Select("k_ID = " + kierowcaID.ToString());
                            if (tmK.Length > 0)
                            {
                                workType = (int)tmK[0]["zad_ostatni_dzien"];
                            }
                            else
                            {
                                MessageBox.Show("PostProcesGrafikL\n" + " Error 001 - no existing data for sel. kierowca");
                            }
                        }
                        else
                        {
                            workType = DayWorkType(kierowcaID, tmpD.AddDays(-1));
                        }
                        sel = dZadania.Select("z_ID = " + workType.ToString());// po co sprawdzac jezeli worktype  <= 0 na pewno nie bedzie w dZadania
                        if (sel.Length > 0)
                        {
                            endLast = endLast.Add((TimeSpan)sel[0]["z_KONIEC"]);

                        }
                        else
                        {
                            MessageBox.Show("PostProcesGrafikL\n" + "Not selected zad [" + workType.ToString() + "]");
                        }
                        //MessageBox.Show("Endtime: " + endLast.ToString() + " StartNew: " + startNew.ToString());
                        if (endLast.TotalMinutes > 0 && startNew.TotalMinutes > 0)
                        {
                            TimeSpan midnight = new TimeSpan(24, 0, 0);
                            TimeSpan diff = midnight.Subtract(endLast) + startNew;
                            TimeSpan breakTimeNorma = new TimeSpan(10, 0, 0);
                            TimeSpan breakTimeShort = new TimeSpan(9, 0, 0);
                            //MessageBox.Show("[PostProcesGrafik]\n" + "Koniec: " + endLast.ToString() + " Poczatek: " + startNew.ToString() + "\nDIFF TIME: " + diff.ToString());
                            if (diff < breakTimeNorma)
                            {
                                if (diff < breakTimeShort)
                                {
                                    //MessageBox.Show("WARNING - break time too short - RED [" + (firstDayCell + (tmpD - start).Days).ToString());
                                    dataGridView1.Rows[rowIndex].Cells[firstDayCell + (tmpD - start).Days].Style.BackColor = Color.Red;
                                }
                                else
                                {
                                    //MessageBox.Show("WARNING - break time short - ORANGE");
                                    dataGridView1.Rows[rowIndex].Cells[firstDayCell + (tmpD - start).Days].Style.BackColor = Color.OrangeRed;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                    tmpD = tmpD.AddDays(1);
                }
                //MessageBox.Show("Dni pracy pod rząd: " + howManyWorkingDays.ToString() );
            }
            else
            {
                MessageBox.Show("PostProcesGrafik\n" + "Error - No existing kierowca ID");
            }
        }

        private bool IsWorkingDay(int kierowcaID, DateTime date)
        {
            bool isWorking = false;
            DataRow[] selG = dZadDzien.Select("g_KIEROWCA = " + kierowcaID.ToString() + " AND g_DATA = '" + date.ToShortDateString() + "'");
            if(selG.Length > 0)
            {
                if (selG.Length > 1) MessageBox.Show("IsWorkingDay:dZadDzien\n" + "Error Zdublowane zadania dla daty: " + date.ToShortDateString());
                if ((int)selG[0]["g_ZADANIE"] > 0 || (int)selG[0]["g_ZADANIE"] == ZADANIA.praca) isWorking = true;
            }
            selG = remGrafik.Select("g_KIEROWCA = " + kierowcaID.ToString() + " AND g_DATA = '" + date.ToShortDateString() + "'");
            if (selG.Length > 0)
            {
                if (selG.Length > 1) MessageBox.Show("IsWorkingDay:remGrafik\n" + "Error Zdublowane zadania dla daty: " + date.ToShortDateString());
                isWorking = false;
            }
            selG = addGrafik.Select("g_KIEROWCA = " + kierowcaID.ToString() + " AND g_DATA = '" + date.ToShortDateString() + "'");
            if (selG.Length > 0)
            {
                if (selG.Length > 1) MessageBox.Show("IsWorkingDay:addGrafik\n" + "Error Zdublowane zadania dla daty: " + date.ToShortDateString());
                if ((int)selG[0]["g_ZADANIE"] > 0 || (int)selG[0]["g_ZADANIE"] == ZADANIA.praca) isWorking = true;
            }
            return isWorking;
        }

        private int DayWorkType(int kierowcaID, DateTime date)
        {
            int workType = 0;
            DataRow[] selG = dZadDzien.Select("g_KIEROWCA = " + kierowcaID.ToString() + " AND g_DATA = '" + date.ToShortDateString() + "'");
            if (selG.Length > 0)
            {
                if (selG.Length > 1) MessageBox.Show("WhatDayWork:dZadDzien\n" + "Error Zdublowane zadania dla daty: " + date.ToShortDateString());
                workType = (int)selG[0]["g_ZADANIE"];
            }
            selG = remGrafik.Select("g_KIEROWCA = " + kierowcaID.ToString() + " AND g_DATA = '" + date.ToShortDateString() + "'");
            if (selG.Length > 0)
            {
                if (selG.Length > 1) MessageBox.Show("WhatDayWork:remGrafik\n" + "Error Zdublowane zadania dla daty: " + date.ToShortDateString());
                workType = 0;
            }
            selG = addGrafik.Select("g_KIEROWCA = " + kierowcaID.ToString() + " AND g_DATA = '" + date.ToShortDateString() + "'");
            if (selG.Length > 0)
            {
                if (selG.Length > 1) MessageBox.Show("WhatDayWork:addGrafik\n" + "Error Zdublowane zadania dla daty: " + date.ToShortDateString());
                workType = (int)selG[0]["g_ZADANIE"];
            }
            return workType;
        }

        private void grafikTygodniowyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            itemperpage = totalnumber = 0;
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            MessageBox.Show("Start fill page");

            Font fn = new Font(DefaultFont, FontStyle.Bold);
            float cY = 50;// declare  one variable for height measurement
            float cX = 50;
            float maxX = 827;//printDocument1.DefaultPageSettings.PaperSize.Width;
            float maxY = 1169;//printDocument1.DefaultPageSettings.PaperSize.Height;

            int kierowcaID = 4;
            string imie = "";
            string nazwisko = "";

            DataRow[] kSel = dKierowcy.Select("k_ID = " + kierowcaID.ToString()) ;
            if(kSel.Length > 0)
            {
                imie = kSel[0]["k_IMIE"].ToString();
                nazwisko = kSel[0]["k_NAZWISKO"].ToString();
            }
            //e.Graphics.DrawString("Print in Multiple Pages\n", DefaultFont, Brushes.Black, 10, currentY);//this will print one heading/title in every page of the document
            //currentY += 15;
            //e.Graphics.DrawString("WTF..." + totalnumber.ToString(), DefaultFont, Brushes.Black, 100, currentY);

            //MessageBox.Show("Paper size: " + printDocument1.DefaultPageSettings.PaperSize.ToString() );
            //e.Graphics.DrawString("1", DefaultFont, Brushes.Black, 50, 50 );
            //e.Graphics.DrawString("2", DefaultFont, Brushes.Black, 827 - 100, 50);
            //e.Graphics.DrawString("3", DefaultFont, Brushes.Black, 50, 1169 - 100 );
            //e.Graphics.DrawString("4", DefaultFont, Brushes.Black, 827 - 100, 1169 - 100 );
            
            //e.Graphics.DrawString("--------------------------------------------------------------------------------------------------------------------------------------------------------------", DefaultFont, Brushes.Black, cX, cY);
            //cY += 15;

            int count = 0;
            int lines = 0;
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells["k_ID"].Value.ToString() == kierowcaID.ToString())
                {
                    cX = 50;
                    e.Graphics.DrawString("KIEROWCA: " + nazwisko.ToString() + " " + imie.ToString(), fn, Brushes.Black, cX, cY);
                    cY += 15;
                    e.Graphics.DrawString("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", DefaultFont, Brushes.Black, 50, cY);
                    cY += 15;
                    count = 0;
                    //lines = 0;
                    for (int i = firstDayCell; i < r.Cells.Count; i++)
                    {
                        e.Graphics.DrawString(dataGridView1.Columns[i].HeaderText.Substring(0, 10), DefaultFont, Brushes.Black, cX, cY);
                        e.Graphics.DrawString(r.Cells[i].Value.ToString(), DefaultFont, Brushes.Black, cX, cY + 20);
                        //this is place that you need to edit
                        e.Graphics.DrawString(lines.ToString() + "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", DefaultFont, Brushes.Black, 50, cY + 45);
                        count++;
                        cX += 100;
                        if (count > 6)
                        {
                            count = 0;
                            cY += 60;
                            cX = 50;
                            lines++;
                            if (lines > 12)
                            {
                                MessageBox.Show("Add new page");
                                //e.HasMorePages = true;// automaticly call this function again ( for naw its endless loop xD )
                                //cX = 50;
                                //cY = 0;
                                //lines = 0;
                            }
                        }
                    }
                    cY += 60;
                    break;
                }
            }
        }

        private void grafikMiesięcznyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            itemperpage = totalnumber = 0;
            printDialog1.Document = printDocument1;
            printDocument1.DefaultPageSettings.PaperSize = paperSize;
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDocument1.Print();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!editGrafik)
            {
                DataTable tmp = GetDataTable("SELECT * FROM LOGIN WHERE ID = 2");
                if(tmp.Rows.Count != 1)
                {
                    MessageBox.Show("button7_Click\n" + "Nie można rozpocząć edycji danch.\nBłąd 0x001");
                }
                else
                {
                    if( (int)tmp.Rows[0]["STATUS"] != 0 )
                    {
                        if (tmp.Rows[0]["START_EDIT"] != null)
                        {
                            DateTime begin = (DateTime)tmp.Rows[0]["START_EDIT"];
                            //MessageBox.Show("Dif time: " + (DateTime.Now - begin).TotalSeconds.ToString() );
                            if( (DateTime.Now - begin).TotalSeconds > 120)
                            {
                                //MessageBox.Show("Too long no respond so we force to take controll");
                                SqlConnection conn = new SqlConnection(connStr);
                                try
                                {
                                    //DateTime timeTmp = new DateTime();
                                    //timeTmp = DateTime.Now;
                                    //MessageBox.Show("cur time: " + DateTime.Now.ToLongTimeString() );
                                    SqlCommand cmd = conn.CreateCommand();
                                    conn.Open();
                                    cmd.CommandText = "";
                                    cmd.Parameters.Clear();
                                    //Update:
                                    cmd.CommandText = "UPDATE LOGIN SET STATUS = @kie , START_EDIT = @edi , HASH = @has , NAZWISKO = @naz , IMIE = @imi WHERE ID = 2";
                                    cmd.Parameters.AddWithValue("@kie", USERID);
                                    cmd.Parameters.Add("@edi", SqlDbType.SmallDateTime);
                                    cmd.Parameters["@edi"].Value = DateTime.Now;
                                    cmd.Parameters.AddWithValue("@has", HASH);
                                    cmd.Parameters.AddWithValue("@naz", NAZWISKO);
                                    cmd.Parameters.AddWithValue("@imi", IMIE);
                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                    editGrafik = true;
                                    if(!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                button1.Enabled = true;
                                button2.Enabled = true;
                                button3.Enabled = true;
                                button4.Enabled = true;
                                contextMenuStrip1.Enabled = true;
                                button7.BackColor = editColor;
                            }
                            else
                            {
                                MessageBox.Show("Nie można rozpocząć edycji.\nGrafik jest aktualnie edytowany przez: " + tmp.Rows[0]["NAZWISKO"].ToString() + " " + tmp.Rows[0]["IMIE"].ToString());
                            }
                        }
                        else
                        {
                            SqlConnection conn = new SqlConnection(connStr);
                            try
                            {
                                SqlCommand cmd = conn.CreateCommand();
                                conn.Open();
                                cmd.CommandText = "";
                                cmd.Parameters.Clear();
                                //Update:
                                cmd.CommandText = "UPDATE LOGIN SET STATUS = @kie , START_EDIT = @edi , HASH = @has , NAZWISKO = @naz , IMIE = @imi WHERE ID = 2";
                                cmd.Parameters.AddWithValue("@kie", USERID);
                                cmd.Parameters.Add("@edi", SqlDbType.SmallDateTime);
                                cmd.Parameters["@edi"].Value = DateTime.Now;
                                cmd.Parameters.AddWithValue("@has", HASH);
                                cmd.Parameters.AddWithValue("@naz", NAZWISKO);
                                cmd.Parameters.AddWithValue("@imi", IMIE);
                                cmd.ExecuteNonQuery();
                                conn.Close();
                                editGrafik = true;
                                if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
                                //MessageBox.Show("Start Edit");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                            button1.Enabled = true;
                            button2.Enabled = true;
                            button3.Enabled = true;
                            button4.Enabled = true;
                            contextMenuStrip1.Enabled = true;
                            button7.BackColor = editColor;
                        }
                    }
                    else
                    {
                        SqlConnection conn = new SqlConnection(connStr);
                        try
                        {
                            //DateTime timeTmp = new DateTime();
                            //timeTmp = DateTime.Now;
                            //MessageBox.Show("cur time: " + DateTime.Now.ToLongTimeString() );
                            SqlCommand cmd = conn.CreateCommand();
                            conn.Open();
                            cmd.CommandText = "";
                            cmd.Parameters.Clear();
                            //Update:
                            cmd.CommandText = "UPDATE LOGIN SET STATUS = @kie , START_EDIT = @edi , HASH = @has , NAZWISKO = @naz , IMIE = @imi WHERE ID = 2";
                            cmd.Parameters.AddWithValue("@kie", USERID);
                            cmd.Parameters.Add("@edi", SqlDbType.SmallDateTime);
                            cmd.Parameters["@edi"].Value = DateTime.Now;
                            cmd.Parameters.AddWithValue("@has", HASH);
                            cmd.Parameters.AddWithValue("@naz", NAZWISKO);
                            cmd.Parameters.AddWithValue("@imi", IMIE);
                            cmd.ExecuteNonQuery();
                            conn.Close();
                            editGrafik = true;
                            if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
                            //MessageBox.Show("No one start to edit so we can now");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        button1.Enabled = true;
                        button2.Enabled = true;
                        button3.Enabled = true;
                        button4.Enabled = true;
                        contextMenuStrip1.Enabled = true;
                        button7.BackColor = editColor;
                    }
                }
            }
            else//we edited GRAFIK and wana stop it
            {
                DialogResult dialogResult = MessageBox.Show("Zapisać zmiany?", "Uwaga", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    SaveGrafik();
                }
                else if (dialogResult == DialogResult.No)
                {
                    //do something else
                }
                SqlConnection conn = new SqlConnection(connStr);
                try
                {
                    SqlCommand cmd = conn.CreateCommand();
                    conn.Open();
                    cmd.CommandText = "";
                    cmd.Parameters.Clear();
                    //Update:
                    cmd.CommandText = "UPDATE LOGIN SET STATUS = @kie WHERE ID = 2";
                    cmd.Parameters.AddWithValue("@kie", 0);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                //Check if background worker is doing anything and send a cancellation if it is
                if (backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.CancelAsync();
                }
                editGrafik = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                contextMenuStrip1.Enabled = false;
                button7.BackColor = defColor;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            DataTable t1 = new DataTable();
            //string connStr = "Data Source = 83.238.167.3\\OPTIMA; Initial Catalog = WARBUS_GRAFIK; User ID = WARBUS; Password = Tkip999!!@@##";
            while (true)
            {
                Thread.Sleep(60000);// every one minute
                //Check if there is a request to cancel the process
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    e.Result = "CANCELED";
                    return;
                }

                t1.Clear();
                SqlConnection conn = new SqlConnection(connStr);
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT HASH FROM LOGIN WHERE ID = 2", conn);
                    using (SqlDataAdapter a = new SqlDataAdapter(cmd))
                    {
                        a.Fill(t1);
                    }

                    if (t1.Rows.Count != 1)
                    {
                        e.Result = "WRONG USERS COUNT";
                        return;
                    }
                    else
                    {
                        if (String.CompareOrdinal((string)t1.Rows[0]["HASH"], HASH) == 0)
                        {
                            cmd.CommandText = "";
                            cmd.Parameters.Clear();
                            //Update:
                            cmd.CommandText = "UPDATE LOGIN SET START_EDIT = @edi WHERE ID = 2";
                            cmd.Parameters.Add("@edi", SqlDbType.SmallDateTime);
                            cmd.Parameters["@edi"].Value = DateTime.Now;
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            e.Result = "INCORRECT HASH";
                            return;
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            editGrafik = false;
            if(!e.Cancelled) MessageBox.Show("WARNING\nThread ended: " + e.Result.ToString());
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            contextMenuStrip1.Enabled = false;
            button7.BackColor = defColor;
            //throw new NotImplementedException();
        }

        private void wymuśEdycjęGraafikuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                //DateTime timeTmp = new DateTime();
                //timeTmp = DateTime.Now;
                //MessageBox.Show("cur time: " + DateTime.Now.ToLongTimeString() );
                SqlCommand cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = "";
                cmd.Parameters.Clear();
                //Update:
                cmd.CommandText = "UPDATE LOGIN SET STATUS = @kie , START_EDIT = @edi , HASH = @has , NAZWISKO = @naz , IMIE = @imi WHERE ID = 2";
                cmd.Parameters.AddWithValue("@kie", USERID);
                cmd.Parameters.Add("@edi", SqlDbType.SmallDateTime);
                cmd.Parameters["@edi"].Value = DateTime.Now;
                cmd.Parameters.AddWithValue("@has", HASH);
                cmd.Parameters.AddWithValue("@naz", NAZWISKO);
                cmd.Parameters.AddWithValue("@imi", IMIE);
                cmd.ExecuteNonQuery();
                conn.Close();
                editGrafik = true;
                if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            contextMenuStrip1.Enabled = true;
            button7.BackColor = editColor;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (editGrafik)
            {
                DialogResult dialogResult = MessageBox.Show("Zapisać zmiany?", "Uwaga", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    SaveGrafik();
                }
                else if (dialogResult == DialogResult.No)
                {
                    //do something else
                }
                SqlConnection conn = new SqlConnection(connStr);
                try
                {
                    SqlCommand cmd = conn.CreateCommand();
                    conn.Open();
                    cmd.CommandText = "";
                    cmd.Parameters.Clear();
                    //Update:
                    cmd.CommandText = "UPDATE LOGIN SET STATUS = @kie WHERE ID = 2";
                    cmd.Parameters.AddWithValue("@kie", 0);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                //Check if background worker is doing anything and send a cancellation if it is
                if (backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.CancelAsync();
                }
                editGrafik = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                contextMenuStrip1.Enabled = false;
                button7.BackColor = defColor;
            }
        }
    }

    public class ZADANIA
    {
        public const int praca = -1;
        public const int wolne = -2;
        public const int prefA = -3;
        public const int prefB = -4;
        public const int wolneNZ = -5;
        public const int wolneND = -6;
        public const int urlop = -7;
        public const int chorobowe = -8;
    }

}


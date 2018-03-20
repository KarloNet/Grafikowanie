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

namespace Grafikowanie
{
    public partial class Form4 : Form
    {
        string connStr = "";
        bool DEBUG = false;
        DataTable dZadania;
        DataTable pDays;
        DataTable mAddGrafik;
        DataTable mRemGrafik;

        DataTable dT = new DataTable();
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();
        DateTime curTime = new DateTime();

        int selectedRow = -1;
        int selectedCell = -1;

        public DataTable GetDataTable(string oQuery)
        {
            DataTable t1 = new DataTable();
            if (oQuery == null) return t1;

            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                conn.Open();
                if (DEBUG) MessageBox.Show("Connection string : " + oQuery);

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

        private void LoadZadaniaDay(int rok, int miesiac, int dzien)
        {
            DateTime day = new DateTime(rok, miesiac, dzien);
            LoadZadaniaDay(day);
        }

        private void LoadZadaniaDay(DateTime day)
        {
            DataRow[] zadania;
            listView2.Clear();
            listView2.Columns.Add("ID", 30);
            listView2.Columns.Add("Zadanie", 80);
            listView2.Columns.Add("Brygada", 80);
            listView2.Columns.Add("Tabor", 50);

            string[] arr = new string[4];
            ListViewItem itm;
            //monthCalendar1.SetDate(day);
            label2.Text = "Wybrany dzień: " + day.ToLongDateString();

            if (day >= startTime && day <= endTime)
            {
                zadania = pDays.Select("g_DATA = '" + day.ToShortDateString() + "'");
                if(DEBUG) MessageBox.Show("Ilość zadań w dniu " + day.ToShortDateString() + " = " + zadania.Length.ToString());
            }
            else
            {
                //Load Zadania for specyfic day
                startTime = day.AddDays(-15);
                endTime = startTime.AddDays(30);
                UseWaitCursor = true;
                Application.DoEvents();
                pDays = GetDataTable("SELECT * FROM GRAFIK WHERE g_DATA BETWEEN '" + startTime.ToShortDateString() + "' and '" + endTime.ToShortDateString() + "'" );
                UseWaitCursor = false;
                zadania = pDays.Select("g_DATA = '" + day.ToShortDateString() + "'");
                if (DEBUG) MessageBox.Show("Ilość zadań w dniu " + day.ToShortDateString() + " = " + zadania.Length.ToString());
            }
            foreach (DataRow r in zadania)
            {
                if (DEBUG) MessageBox.Show("Zadania: " + (r["g_ROK"]).ToString() + (r["g_MIESIAC"]).ToString() + (r["g_DZIEN"]).ToString() + " ID: " + (r["g_ZADANIE"]).ToString());
                DataRow[] zadanie = dZadania.Select("z_ID = " + r["g_ZADANIE"]);
                if (zadanie.Length <= 0)
                {
                    if (DEBUG) MessageBox.Show("BŁĄD: Nie znaleziono zadania o ID = " + r["g_ZADANIE"]);
                }
                else
                {
                    arr[0] = (zadanie[0]["z_ID"]).ToString();
                    arr[1] = (string)zadanie[0]["z_ZADANIE"];
                    arr[2] = (string)zadanie[0]["z_BRYGADA"];
                    arr[3] = (string)zadanie[0]["z_TABOR"];
                    itm = new ListViewItem(arr);
                    listView2.Items.Add(itm);
                }
            }

            zadania = mAddGrafik.Select("g_DATA = '" + day.ToShortDateString() + "'");
            if (DEBUG) MessageBox.Show("Ilość zmodyfikowanych zadań w dniu " + day.ToShortDateString() + " = " + zadania.Length.ToString());
            foreach (DataRow r in zadania)
            {
                if (DEBUG) MessageBox.Show("Zadania: " + (r["g_ROK"]).ToString() + (r["g_MIESIAC"]).ToString() + (r["g_DZIEN"]).ToString() + " ID: " + (r["g_ZADANIE"]).ToString());
                DataRow[] zadanie = dZadania.Select("z_ID = " + r["g_ZADANIE"]);
                if (zadanie.Length <= 0)
                {
                    if (DEBUG) MessageBox.Show("BŁĄD: Nie znaleziono zadania o ID = " + r["g_ZADANIE"]);
                }
                else
                {
                    arr[0] = (zadanie[0]["z_ID"]).ToString();
                    arr[1] = (string)zadanie[0]["z_ZADANIE"];
                    arr[2] = (string)zadanie[0]["z_BRYGADA"];
                    arr[3] = (string)zadanie[0]["z_TABOR"];
                    itm = new ListViewItem(arr);
                    listView2.Items.Add(itm);
                }
            }
            
            zadania = mRemGrafik.Select("g_DATA = '" + day.ToShortDateString() + "'");
            if (DEBUG) MessageBox.Show("Ilość usunietych zadań w dniu " + day.ToShortDateString() + " = " + zadania.Length.ToString());
            foreach (DataRow r in zadania)
            {
                for (int i = listView2.Items.Count; i > 0; i-- )
                {
                    if (Convert.ToInt32(listView2.Items[i-1].SubItems[0].Text) == (int)r["g_ZADANIE"])
                    {
                        listView2.Items.RemoveAt(i-1);
                    }
                }
            }

        }

        public Form4(string connStr)
        {
            this.connStr = connStr;
            InitializeComponent();
        }

        private void Form4_Shown(object sender, EventArgs e)
        {
            mAddGrafik = new DataTable();
            mAddGrafik.Clear();
            mAddGrafik.Columns.Add("g_ROK", typeof(int));
            mAddGrafik.Columns.Add("g_MIESIAC", typeof(int));
            mAddGrafik.Columns.Add("g_DZIEN", typeof(int));
            mAddGrafik.Columns.Add("g_ZADANIE", typeof(int));
            mAddGrafik.Columns.Add("g_KIEROWCA", typeof(int));
            mAddGrafik.Columns.Add("g_DATA", typeof(string));
            dataGridView2.DataSource = mAddGrafik;

            mRemGrafik = new DataTable();
            mRemGrafik.Clear();
            mRemGrafik.Columns.Add("g_ROK", typeof(int));
            mRemGrafik.Columns.Add("g_MIESIAC", typeof(int));
            mRemGrafik.Columns.Add("g_DZIEN", typeof(int));
            mRemGrafik.Columns.Add("g_ZADANIE", typeof(int));
            mRemGrafik.Columns.Add("g_KIEROWCA", typeof(int));
            mRemGrafik.Columns.Add("g_DATA", typeof(string));
            dataGridView3.DataSource = mRemGrafik;

            //modGrafik.Columns.Add("g_DZIEN", typeof(int));
            //modGrafik.Columns.Add("g_ZADANIE", typeof(string));

            //Load zadania
            dZadania = GetDataTable("SELECT * FROM ZADANIA");
            listView1.Clear();
            //Add column header
            listView1.Columns.Add("ID", 30);
            listView1.Columns.Add("Zadanie", 80);
            listView1.Columns.Add("Brygada", 60);
            //Add items in the listview
            string[] arr = new string[4];
            ListViewItem itm;
            foreach (DataRow row in dZadania.Rows)
            {
                arr[0] = (row["z_ID"]).ToString();
                arr[1] = (string)row["z_Zadanie"];
                arr[2] = (string)row["z_Brygada"];
                itm = new ListViewItem(arr);
                listView1.Items.Add(itm);
            }

            listView2.Clear();
            listView2.Columns.Add("ID", 30);
            listView2.Columns.Add("Zadanie", 80);
            listView2.Columns.Add("Brygada", 80);
            listView2.Columns.Add("Tabor", 50);

            //Prepare DataTable
            dT.Clear();
            //dT.Columns.Add("K_ID", typeof(int));
            //dT.Columns.Add("G_ID", typeof(int));
            //dT.Columns.Add("Z_ID", typeof(int));
            //dT.Columns.Add("Kierowca", typeof(string));
            //firstDayCell = 4;//                              WERRY IMPORTANT -> indicate from what cell number start days

            //Default load current month
            //int days = DateTime.DaysInMonth(sYear, sMonth);
            //eDay = days;

            startTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            endTime = startTime.AddDays(30);
            curTime = startTime;

            //Load Zadania for specyfic day
            this.UseWaitCursor = true;
            Application.DoEvents();
            pDays = GetDataTable("SELECT * FROM GRAFIK WHERE g_DATA BETWEEN '" + startTime.ToShortDateString() + "' AND '" + endTime.ToShortDateString() + "'");
            this.UseWaitCursor = false;
            monthCalendar1.SetDate(curTime);
            label2.Text = "Wybrany dzień: " + curTime.ToLongDateString();

            DataRow[] zadania = pDays.Select("g_DATA = '" + curTime.ToShortDateString() + "'");
            foreach (DataRow r in zadania)
            {
                if (DEBUG) MessageBox.Show("Zadania: " + (r["g_ROK"]).ToString() + (r["g_MIESIAC"]).ToString() + (r["g_DZIEN"]).ToString() + " ID: " + (r["g_ZADANIE"]).ToString());
                DataRow[] zadanie = dZadania.Select("z_ID = " + r["g_ZADANIE"]);
                if (zadanie.Length <= 0)
                {
                    if (DEBUG) MessageBox.Show("BŁĄD: Nie znaleziono zadania o ID = " + r["g_ZADANIE"]);
                }
                else
                {
                    arr[0] = (zadanie[0]["z_ID"]).ToString();
                    arr[1] = (string)zadanie[0]["z_ZADANIE"];
                    arr[2] = (string)zadanie[0]["z_BRYGADA"];
                    arr[3] = (string)zadanie[0]["z_TABOR"];
                    itm = new ListViewItem(arr);
                    listView2.Items.Add(itm);
                }
            }

            DateTime tmpTime = new DateTime();
            tmpTime = curTime;
            while (true)
            {
                dT.Columns.Add(tmpTime.ToShortDateString() + "\n" + tmpTime.ToString("        ddd"));
                DataRow[] zad = pDays.Select("g_ROK = " + tmpTime.Year.ToString() + " AND g_MIESIAC = " + tmpTime.Month.ToString() + " AND g_DZIEN = " + tmpTime.Day.ToString());
                //if(DEBUG) MessageBox.Show("FOR : " + startTime.ToShortDateString() + " FOUND: " + zad.Length.ToString());
                foreach (DataRow row in zad)
                {
                    //DateTime stTime = new DateTime(sYear, sMonth, sDay);
                    //DateTime enTime = new DateTime((int)row2["G_ROK"], (int)row2["G_MIESIAC"], (int)row2["G_DZIEN"]);
                    //int difTime = (enTime - stTime).Days;
                    //DataRow[] zad = dZadania.Select("Z_ID = " + row2["G_ZADANIE"]);
                    //row[firstDayCell + difTime ] = "PRACA";
                    //row[firstDayCell + difTime] = (string)zad[0]["Z_ZADANIE"] + " (" + (string)zad[0]["Z_BRYGADA"] + ")";
                    //MessageBox.Show( stTime.ToShortDateString() + " TO " + enTime.ToShortDateString() + " = " + difTime.ToString() + "\n" + "PRACA " + row2["G_ROK"].ToString() + row2["G_MIESIAC"].ToString() + row2["G_DZIEN"].ToString());
                    //row[tNext] = row2["z_ZADANIE"];
                    //tNext++;
                }
                tmpTime = tmpTime.AddDays(1);
                if (tmpTime > endTime) break;
            }

            for (int i = 0; i < 10; ++i )
            {
                DataRow dRow = dT.NewRow();
                dT.Rows.Add(dRow);
            }

            dataGridView1.DataSource = pDays;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            curTime = curTime.AddDays(1);
            LoadZadaniaDay(curTime);
            OldGrafikProtection(curTime);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            curTime = curTime.AddDays(-1);
            LoadZadaniaDay(curTime);
            OldGrafikProtection(curTime);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] arr = new string[4];
            ListViewItem itm;
            bool alreadyExist = false;

            if (listView1.SelectedItems.Count > 0)
            {
                foreach( ListViewItem lvi in listView1.SelectedItems)
                {
                    alreadyExist = false;
                    foreach (ListViewItem lv in listView2.Items)
                    {
                        if (lv.SubItems[0].Text == lvi.SubItems[0].Text)
                        {
                            MessageBox.Show("Zadanie nie może być zdublowane");
                            alreadyExist = true;
                            break;
                        }
                    }
                    if (!alreadyExist)
                    {
                        alreadyExist = false;
                        DataRow[] r = dZadania.Select("z_ID = " + lvi.SubItems[0].Text);
                        if (r.Length == 1)
                        {
                            DataRow[] zadania;
                            zadania = pDays.Select("g_DATA = '" + curTime.ToShortDateString() + "'");
                            foreach (DataRow ro in zadania)
                            {
                                if ((int)ro["g_ZADANIE"] == (int)r[0]["z_ID"])
                                {
                                    alreadyExist = true;
                                    break;
                                }
                            }

                            arr[0] = r[0]["z_ID"].ToString();
                            arr[1] = (string)r[0]["z_ZADANIE"];
                            arr[2] = (string)r[0]["z_BRYGADA"];
                            arr[3] = (string)r[0]["z_TABOR"];
                            itm = new ListViewItem(arr);
                            listView2.Items.Add(itm);
                            if (!alreadyExist)
                            {
                                //and add new zadanie to datatable
                                DataRow dRow = mAddGrafik.NewRow();
                                dRow["g_ROK"] = curTime.Year;
                                dRow["g_MIESIAC"] = curTime.Month;
                                dRow["g_DZIEN"] = curTime.Day;
                                dRow["g_ZADANIE"] = r[0]["z_ID"];
                                dRow["g_KIEROWCA"] = DBNull.Value;
                                dRow["g_DATA"] = curTime.Date.ToShortDateString();
                                mAddGrafik.Rows.Add(dRow);
                            }
                            DataRow[] rZadania;
                            rZadania = mRemGrafik.Select("g_DATA = '" + curTime.ToShortDateString() + "'");
                            if (DEBUG) MessageBox.Show("Found in mRemGrafik : " + rZadania.Length.ToString());
                            foreach (DataRow ro in rZadania)
                            {
                                if (DEBUG) MessageBox.Show("ro[g_ZADANIE] = " + ro["g_ZADANIE"].ToString() + " and r[0][z_ID] = " + r[0]["z_ID"].ToString());
                                if ((int)ro["g_ZADANIE"] == (int)r[0]["z_ID"])
                                {
                                    if(DEBUG) MessageBox.Show("Removing row from mRemGrafik");
                                    mRemGrafik.Rows.Remove(ro);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Błąd!\nNie znaleziono lub znaleziono więcej niż jedno pasujące zadanie.");
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool found = false;
            DataRow[] mZadania;
            DataRow[] oZadania;
            if (listView2.SelectedItems.Count > 0)
            {
                foreach(ListViewItem lvi in listView2.SelectedItems)
                {
                    found = false;
                    mZadania = mAddGrafik.Select("g_DATA = '" + curTime.ToShortDateString() + "'");
                    foreach (DataRow r in mZadania)
                    {
                        if ((int)r["g_ZADANIE"] == Convert.ToInt32(lvi.SubItems[0].Text))
                        {
                            found = true;
                            DataRow dRow = mRemGrafik.NewRow();
                            dRow["g_ROK"] = r["g_ROK"];
                            dRow["g_MIESIAC"] = r["g_MIESIAC"];
                            dRow["g_DZIEN"] = r["g_DZIEN"];
                            dRow["g_ZADANIE"] = r["g_ZADANIE"];
                            dRow["g_KIEROWCA"] = r["g_KIEROWCA"];
                            dRow["g_DATA"] = r["g_DATA"];
                            //mRemGrafik.Rows.Add(dRow);
                            mAddGrafik.Rows.Remove(r);
                            break;
                        }
                    }
                    if (!found)
                    {
                        oZadania = pDays.Select("g_DATA = '" + curTime.ToShortDateString() + "'");
                        foreach (DataRow r in oZadania)
                        {
                            if ((int)r["g_ZADANIE"] == Convert.ToInt32(lvi.SubItems[0].Text))
                            {
                                DataRow dRow = mRemGrafik.NewRow();
                                dRow["g_ROK"] = r["g_ROK"];
                                dRow["g_MIESIAC"] = r["g_MIESIAC"];
                                dRow["g_DZIEN"] = r["g_DZIEN"];
                                dRow["g_ZADANIE"] = r["g_ZADANIE"];
                                dRow["g_KIEROWCA"] = r["g_KIEROWCA"];
                                dRow["g_DATA"] = ((DateTime)(r["g_DATA"])).ToShortDateString();
                                mRemGrafik.Rows.Add(dRow);
                                break;
                            }
                        }
                    }
                    listView2.Items.Remove(lvi);
                }
            }
        }

        private void OldGrafikProtection(DateTime day)
        {
            if (checkBox1.Checked)
            {
                if (day.Date < DateTime.Now.Date)
                {
                    if (DEBUG) MessageBox.Show("Selected date: " + day.ToShortDateString() + " and current date: " + DateTime.Now.ToShortDateString());
                    button1.Enabled = false;
                    button2.Enabled = false;
                }
                else
                {
                    button1.Enabled = true;
                    button2.Enabled = true;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                OldGrafikProtection(curTime);
            }
            else
            {
                MessageBox.Show("UWAGA!\nModyfikacja wykonanych zadań może doprowadzić do niespójności bazy danych!");
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int addedCount = 0;
            int deletedCount = 0;
            UseWaitCursor = true;
            Application.DoEvents();
            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                SqlCommand cmd = conn.CreateCommand();
                conn.Open();
                foreach (DataRow r in mRemGrafik.Rows)
                {
                    cmd.CommandText = "";
                    cmd.Parameters.Clear();
                    deletedCount++;
                    cmd.CommandText = "DELETE FROM GRAFIK WHERE g_ZADANIE = @id AND g_DATA = @data";
                    cmd.Parameters.AddWithValue("@id", r["g_ZADANIE"]);
                    cmd.Parameters.AddWithValue("@data", r["g_DATA"]);
                    cmd.ExecuteNonQuery();
                }

                foreach (DataRow r in mAddGrafik.Rows)
                {
                    cmd.CommandText = "";
                    cmd.Parameters.Clear();
                    addedCount++;
                    cmd.CommandText = "INSERT INTO GRAFIK (g_ROK, g_MIESIAC, g_DZIEN, g_ZADANIE, g_KIEROWCA, g_DATA) VALUES (@rok, @mies, @dzien, @zad, @kier, @data)";
                    cmd.Parameters.AddWithValue("@rok", r["g_ROK"]);
                    cmd.Parameters.AddWithValue("@mies", r["g_MIESIAC"]);
                    cmd.Parameters.AddWithValue("@dzien", r["g_DZIEN"]);
                    cmd.Parameters.AddWithValue("@zad", r["g_ZADANIE"]);
                    cmd.Parameters.AddWithValue("@kier", r["g_KIEROWCA"]);
                    cmd.Parameters.AddWithValue("@data", r["g_DATA"]);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                MessageBox.Show("Made changes to database are:\n1) Added: " + addedCount.ToString() + "\n2) Deleted: " + deletedCount.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            UseWaitCursor = false;
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            curTime = monthCalendar1.SelectionStart;
            LoadZadaniaDay(curTime);
            OldGrafikProtection(curTime);
        }
    }

}

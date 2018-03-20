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
    public partial class Form2 : Form
    {
        string connStr = "";
        bool DEBUG = false;
        DataTable dZadania;

        int selectedRow = -1;
        int selectedCell = -1;

        public Form2(string connStr)
        {
            InitializeComponent();
            this.connStr = connStr;
        }

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

        private void Form2_Shown(object sender, EventArgs e)
        {
            /*
            listView1.Clear();
            //Add column header
            listView1.Columns.Add("Zadanie", 80);
            listView1.Columns.Add("Brygada", 50);
            listView1.Columns.Add("Tabor", 30);
            listView1.Columns.Add("Od", 80);
            listView1.Columns.Add("Do", 80);
            listView1.Columns.Add("Podmiana", 100);
            listView1.Columns.Add("Czas jazdy", 80);
            listView1.Columns.Add("Czas pracy", 80);
            //Add items in the listview
            string[] arr = new string[4];
            ListViewItem itm;

            //Load zadania
            dZadania = GetDataTable("SELECT * FROM ZADANIA");
            foreach (DataRow r in dZadania.Rows)
            {

            }
            */

            //Load zadania
            dZadania = GetDataTable("SELECT * FROM ZADANIA");
            dataGridView1.DataSource = dZadania;

            //dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns["Z_ZADANIE"].Frozen = true;
            dataGridView1.Columns["Z_ID"].Visible = false;
            dataGridView1.Columns["Z_USUNIETE"].Visible = false;
            dataGridView1.Columns["Z_ZADANIE"].DefaultCellStyle.BackColor = Color.Khaki;
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells["z_USUNIETE"].Value != null)
                {
                    //MessageBox.Show("z_USUNIETE: " + r.Cells["z_USUNIETE"].Value.ToString());
                    if ((Boolean)r.Cells["z_USUNIETE"].Value == true)
                    {
                        r.Visible = false;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string _insertSQL = "INSERT INTO ZADANIA (field1, field2) VALUES (@field1, @field2) WHERE Z_ID = @field3";
            //string _updateSQL = "UPDATE ZADANIA SET Z_ZADANIE = @field1 WHERE Z_ID = @field2";
            //string _deleteSQL = "DELETE FROM ZADANIA WHERE Z_ID = @field1";

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
                foreach (DataRow r in dZadania.Rows)
                {
                    cmd.CommandText = "";
                    cmd.Parameters.Clear();
                    switch (r.RowState)
                    {
                        case DataRowState.Added:
                            addedCount++;
                            cmd.CommandText = "INSERT INTO ZADANIA (z_ZADANIE, z_BRYGADA, z_TABOR, z_POCZATEK, z_KONIEC, z_PODMIANA, z_CZAS_PRACY, z_CZAS_JAZDY, z_GRUPA, z_USUNIETE) VALUES (@zad, @bryg, @tab, @pocz, @kon, @podm, @cpra, @cjaz, @grup, @usun)";
                            cmd.Parameters.AddWithValue("@zad", r["z_ZADANIE"]);
                            cmd.Parameters.AddWithValue("@bryg", r["z_BRYGADA"]);
                            cmd.Parameters.AddWithValue("@tab", r["z_TABOR"]);
                            cmd.Parameters.AddWithValue("@pocz", r["z_POCZATEK"]);
                            cmd.Parameters.AddWithValue("@kon", r["z_KONIEC"]);
                            cmd.Parameters.AddWithValue("@podm", r["z_PODMIANA"]);
                            cmd.Parameters.AddWithValue("@cpra", r["z_CZAS_PRACY"]);
                            cmd.Parameters.AddWithValue("@cjaz", r["z_CZAS_JAZDY"]);
                            cmd.Parameters.AddWithValue("@grup", r["z_GRUPA"]);
                            cmd.Parameters.AddWithValue("@usun", false);
                            cmd.ExecuteNonQuery();
                            break;
                        case DataRowState.Deleted:
                            deletedCount++;
                            //nie usuwamy zadan - bo jezeli kiedys ktos mial je i zostanie suniete to nie przeliczy czasu pracy i moze sie wszystko rozsypac....
                            //ustawiamy status na usuniete i ukrywamy wyswietlanie.
                            //cmd.CommandText = "DELETE FROM ZADANIA WHERE z_ID = @id";
                            //cmd.Parameters.AddWithValue("@id", r["z_ID", DataRowVersion.Original]);
                            //cmd.ExecuteNonQuery();
                            cmd.CommandText = "UPDATE ZADANIA SET z_USUNIETE = @usun WHERE z_ID = @id";
                            cmd.Parameters.AddWithValue("@usun", true);
                            cmd.Parameters.AddWithValue("@id", r["z_ID", DataRowVersion.Original]);
                            cmd.ExecuteNonQuery();
                            break;
                        case DataRowState.Detached:
                            detachedCount++;
                            break;
                        case DataRowState.Modified:
                            modifiedCount++;
                            cmd.CommandText = "UPDATE ZADANIA SET z_ZADANIE = @zad, z_BRYGADA = @bryg, z_TABOR = @tab, z_POCZATEK = @pocz, z_KONIEC = @kon, z_PODMIANA = @podm, z_CZAS_PRACY = @cpra, z_CZAS_JAZDY = @cjaz, z_GRUPA = @grup WHERE z_ID = @id";
                            cmd.Parameters.AddWithValue("@zad", r["z_ZADANIE"]);
                            cmd.Parameters.AddWithValue("@bryg", r["z_BRYGADA"]);
                            cmd.Parameters.AddWithValue("@tab", r["z_TABOR"]);
                            cmd.Parameters.AddWithValue("@pocz", r["z_POCZATEK"]);
                            cmd.Parameters.AddWithValue("@kon", r["z_KONIEC"]);
                            cmd.Parameters.AddWithValue("@podm", r["z_PODMIANA"]);
                            cmd.Parameters.AddWithValue("@cpra", r["z_CZAS_PRACY"]);
                            cmd.Parameters.AddWithValue("@cjaz", r["z_CZAS_JAZDY"]);
                            cmd.Parameters.AddWithValue("@grup", r["z_GRUPA"]);
                            cmd.Parameters.AddWithValue("@id", r["z_ID"]);
                            cmd.ExecuteNonQuery();
                            break;
                        case DataRowState.Unchanged:
                            unchangedCount++;
                            break;
                        default:
                            incorrectCount++;
                            break;
                    }
                }
                conn.Close();
                MessageBox.Show("Made changes to database are:\n1) Added: " + addedCount.ToString() + "\n2) Deleted: " + deletedCount.ToString() + "\n3) Modified: " + modifiedCount.ToString() + "\n4) Detached: " + detachedCount.ToString() + "\n5) Unchanged: " + unchangedCount.ToString() + "\n6) Incorrect: " + incorrectCount.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void pokażUsunięteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.CurrentCell = null;
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells["z_USUNIETE"].Value != null)
                {
                    if ((Boolean)r.Cells["z_USUNIETE"].Value == true)
                    {
                        if (pokażUsunięteToolStripMenuItem.Checked)
                        {
                            r.Visible = true;
                            r.DefaultCellStyle.ForeColor = Color.Red;
                        }
                        else
                        {
                            r.Selected = false;
                            r.Visible = false;
                        }
                    }
                }
            }
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Visible == true)
                {
                    dataGridView1.CurrentCell = r.Cells["z_ZADANIE"];
                    break;
                }
            }
        }

        private void usuńZadaniaToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                conn.Open();
                cmd.CommandText = "UPDATE ZADANIA SET z_USUNIETE = @usun WHERE z_ID = @id";
                cmd.Parameters.AddWithValue("@usun", true);
                cmd.Parameters.AddWithValue("@id", dataGridView1.Rows[selectedRow].Cells["z_ID"].Value);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.ToString());
            }
            if (pokażUsunięteToolStripMenuItem.Checked)
            {
                dataGridView1.Rows[selectedRow].Cells["z_USUNIETE"].Value = true;
                dataGridView1.Rows[selectedRow].DefaultCellStyle.ForeColor = Color.Red;
            }
            else
            {
                dataGridView1.Rows[selectedRow].Cells["z_USUNIETE"].Value = true;
                dataGridView1.Rows.Remove(dataGridView1.Rows[selectedRow]);
            }
        }

        private void przywróćZadanieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isOk = true;
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = conn.CreateCommand();
            try
            {
                conn.Open();
                cmd.CommandText = "UPDATE ZADANIA SET z_USUNIETE = @usun WHERE z_ID = @id";
                cmd.Parameters.AddWithValue("@usun", false);
                cmd.Parameters.AddWithValue("@id", dataGridView1.Rows[selectedRow].Cells["z_ID"].Value);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                isOk = false;
                MessageBox.Show("ERROR: " + ex.ToString());
            }
            if (isOk)
            {
                dataGridView1.Rows[selectedRow].Cells["z_USUNIETE"].Value = false;
                dataGridView1.Rows[selectedRow].DefaultCellStyle.ForeColor = Color.Black;
                dataGridView1.Rows[selectedRow].Visible = true;
            }
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            // sort reset all settings so set them back....
            dataGridView1.CurrentCell = null;
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells["z_USUNIETE"].Value != null)
                {
                    if ((Boolean)r.Cells["z_USUNIETE"].Value == true)
                    {
                        if (pokażUsunięteToolStripMenuItem.Checked)
                        {
                            r.Visible = true;
                            r.DefaultCellStyle.ForeColor = Color.Red;
                        }
                        else
                        {
                            //r.Selected = false;
                            r.Visible = false;
                        }
                    }
                }
            }
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Visible == true)
                {
                    dataGridView1.CurrentCell = r.Cells["z_ZADANIE"];
                    break;
                }
            }
        }

        private void dataGridView1_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            //MessageBox.Show("Clicked row: " + e.RowIndex.ToString() + " Column: " + e.ColumnIndex.ToString());
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1)
            {
                selectedRow = e.RowIndex;
                selectedCell = e.ColumnIndex;
                e.ContextMenuStrip = contextMenuStrip1;
            }
        }
    }
}

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
    public partial class Form3 : Form
    {
        string connStr = "";
        bool DEBUG = false;
        DataTable dKierowcy = new DataTable();

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

        public Form3(string connStr)
        {
            this.connStr = connStr;
            InitializeComponent();
        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            //Load kierowcy
            dKierowcy = GetDataTable("SELECT * FROM KIEROWCY");

            dataGridView1.DataSource = dKierowcy;

            //dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns["K_IMIE"].Frozen = true;
            dataGridView1.Columns["K_NAZWISKO"].Frozen = true;
            dataGridView1.Columns["K_ID"].Visible = false;
            dataGridView1.Columns["K_IMIE"].DefaultCellStyle.BackColor = Color.Khaki;
            dataGridView1.Columns["K_NAZWISKO"].DefaultCellStyle.BackColor = Color.Khaki;
        }

        private void button2_Click(object sender, EventArgs e)
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
                foreach (DataRow r in dKierowcy.Rows)
                {
                    cmd.CommandText = "";
                    cmd.Parameters.Clear();
                    switch (r.RowState)
                    {
                        case DataRowState.Added:
                            addedCount++;
                            cmd.CommandText = "INSERT INTO KIEROWCY (k_NAZWISKO, k_IMIE, k_TELEFON, k_UWAGI) VALUES (@nazwisko, @imie, @tel, @uwagi)";
                            cmd.Parameters.AddWithValue("@nazwisko", r["k_NAZWISKO"]);
                            cmd.Parameters.AddWithValue("@imie", r["k_IMIE"]);
                            cmd.Parameters.AddWithValue("@tel", r["k_TELEFON"]);
                            cmd.Parameters.AddWithValue("@uwagi", r["k_UWAGI"]);
                            cmd.ExecuteNonQuery();
                            break;
                        case DataRowState.Deleted:
                            deletedCount++;
                            cmd.CommandText = "DELETE FROM KIEROWCY WHERE k_ID = @id";
                            cmd.Parameters.AddWithValue("@id", r["k_ID", DataRowVersion.Original]);
                            cmd.ExecuteNonQuery();
                            break;
                        case DataRowState.Detached:
                            detachedCount++;
                            break;
                        case DataRowState.Modified:
                            modifiedCount++;
                            cmd.CommandText = "UPDATE KIEROWCY SET k_NAZWISKO = @nazwisko, k_IMIE = @imie, k_TELEFON = @tel, k_UWAGI = @uwagi WHERE k_ID = @id";
                            cmd.Parameters.AddWithValue("@nazwisko", r["k_NAZWISKO"]);
                            cmd.Parameters.AddWithValue("@imie", r["k_IMIE"]);
                            cmd.Parameters.AddWithValue("@tel", r["k_TELEFON"]);
                            cmd.Parameters.AddWithValue("@uwagi", r["k_UWAGI"]);
                            cmd.Parameters.AddWithValue("@id", r["k_ID"]);
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

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

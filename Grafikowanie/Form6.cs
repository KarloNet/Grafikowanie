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
    public partial class Form6 : Form
    {
        bool DEBUG = false;
        string connStr;
        public string hash;
        public int userId = 0;
        public string imie = "";
        public string nazwisko = "";
        string invalidChars = "!@#$%^&*()_+ ";
        public DataTable GetDataTable(string oQuery)
        {
            DataTable t1 = new DataTable();
            if (oQuery == null) return t1;

            //SqlConnection conn = new SqlConnection("Data Source = 83.238.167.3\\OPTIMA; Initial Catalog = WARBUS_GRAFIK; User ID = sa; Password = qazWSX123!@#");
            SqlConnection conn = new SqlConnection(connStr);
            try
            {
                conn.Open();
                if (DEBUG) MessageBox.Show("[GetDataTable]\n" + "Connection string : " + oQuery);

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

        public Form6(string connStr)
        {
            this.connStr = connStr;
            InitializeComponent();
            hash = CryptoHash.GetRandomAlphanumericString(20);
        }

        private bool IsValidChar( char c)
        {
            for(int i = 0; i < invalidChars.Length; i++)
            {
                if(c == invalidChars[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Length <= 1 || textBox2.Text.Length <= 1)
            {
                MessageBox.Show("Błąd logowania [0x0001a]");
                return;
            }
            for(int i = 0; i < textBox1.Text.Length; i++)
            {
                if (!IsValidChar(textBox1.Text[i]))
                {
                    MessageBox.Show("Błąd logowania [0x0001b]");
                    return;
                }
            }

            DataTable dUsers = new DataTable();
            dUsers = GetDataTable("SELECT * FROM LOGIN WHERE LOGIN = '" + textBox1.Text + "' AND PASS = '" + textBox2.Text + "'");
            if(dUsers.Rows.Count > 0)
            {
                if(dUsers.Rows.Count > 1)
                {
                    MessageBox.Show("Błąd logowania [0x002]");
                }
                else
                {
                    SqlConnection conn = new SqlConnection(connStr);
                    try
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        conn.Open();
                        cmd.CommandText = "UPDATE LOGIN SET HASH = @has WHERE LOGIN = @id AND PASS = @pas";
                        cmd.Parameters.AddWithValue("@has", hash);
                        cmd.Parameters.AddWithValue("@id", textBox1.Text);
                        cmd.Parameters.AddWithValue("@pas", textBox2.Text);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    userId = (int)dUsers.Rows[0]["ID"];
                    imie = (string)dUsers.Rows[0]["IMIE"];
                    nazwisko = (string)dUsers.Rows[0]["NAZWISKO"];
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Błąd logowania [0x003]");
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

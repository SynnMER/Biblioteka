using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Xml;

namespace Biblioteka
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SQLiteConnection conn;
        private void выбратьБДToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                conn = new SQLiteConnection("DataSource="+filename);
                conn.Open();
                string sqltext = "select name from sqlite_master where type='table'";
                using (SQLiteCommand cmd = new SQLiteCommand(sqltext, conn))
                {
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    создатьТаблицыToolStripMenuItem.Enabled = !reader.HasRows;
                    if (reader.HasRows) 
                    {
                        FillAutor();
                        FillBooks();
                    }
                    reader.Close();
                    conn.Close();
                }
                
            }
        }

        private void создатьТаблицыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sqltext = "create table Autors (id int primary key, name varchar(20), description varchar(100));" +
                "create table Biblios(id int primary key, id_autor int, name varchar(20), description varchar(100));";
            using (SQLiteCommand cmd = new SQLiteCommand(sqltext, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            создатьТаблицыToolStripMenuItem.Enabled = false;
        }
        private DataTable dtAutor;
        private DataTable dtBooks;
        private SQLiteDataAdapter adAutor;
        private SQLiteDataAdapter adBooks;
        private void FillAutor()
        {
            string sqltext = "select id, name, description from autors";
            adAutor= new SQLiteDataAdapter(sqltext,conn);
            adAutor.SelectCommand = new SQLiteCommand (sqltext,conn);
            SQLiteCommandBuilder cb = new SQLiteCommandBuilder(adAutor);
            dtAutor = new DataTable();           
            adAutor.Fill(dtAutor);
            dgAutor.DataSource= dtAutor;

            dtAutor.Columns[0].AutoIncrement = true;
            dtAutor.Columns[0].AutoIncrementStep = 1;
            dtAutor.Columns[0].AutoIncrementSeed = IncrementSeed("autors");

        }
        private void FillBooks(string autor_id="")
        {
            string sqltext = "select Biblios.id,Biblios.id_autor," +
                " autors.name, Biblios.name as book_name, Biblios.description from Biblios " +
                " join autors on autors.id=Biblios.id_autor";
            if (!string.IsNullOrEmpty(autor_id))
                sqltext = sqltext + " where id_autor=" + autor_id;
            dtBooks = new DataTable();
            adBooks = new SQLiteDataAdapter(sqltext, conn);
            adBooks.SelectCommand = new SQLiteCommand(sqltext,conn);
            sqltext = @"insert into Biblios values (@id,@id_autor,@name,@desc);";
            adBooks.InsertCommand= new SQLiteCommand(sqltext,conn);
            adBooks.InsertCommand.Parameters.Add("id", DbType.Int32, 5, "id");
            adBooks.InsertCommand.Parameters.Add("id_autor", DbType.Int32, 5, "id_autor");
            adBooks.InsertCommand.Parameters.Add("name", DbType.String, 20, "book_name");
            adBooks.InsertCommand.Parameters.Add("desc", DbType.String, 20, "description");
            sqltext = @"update Biblios set id_autor=@id_autor, name=@name, description=@desc where id=@id;";
            adBooks.UpdateCommand = new SQLiteCommand(sqltext, conn);
            adBooks.UpdateCommand.Parameters.Add("id", DbType.Int32, 5, "id");
            adBooks.UpdateCommand.Parameters.Add("id_autor", DbType.Int32, 5, "id_autor");
            adBooks.UpdateCommand.Parameters.Add("name", DbType.String, 20, "book_name");
            adBooks.UpdateCommand.Parameters.Add("desc", DbType.String, 20, "description");
            sqltext = @"delete from Biblios where id=@id;";
            adBooks.DeleteCommand = new SQLiteCommand(sqltext, conn);
            adBooks.DeleteCommand.Parameters.Add("id", DbType.Int32, 5, "id");
            //*/ SQLiteCommandBuilder cb1 = new SQLiteCommandBuilder(adBooks);
            adBooks.Fill(dtBooks);
            dgBooks.DataSource = dtBooks;
            // dgBooks.Columns[1].Visible = false;
            dtBooks.Columns[0].AutoIncrement = true;
            dtBooks.Columns[0].AutoIncrementStep = 1;
            dtBooks.Columns[0].AutoIncrementSeed = IncrementSeed("Biblios");
            (dgAutor.Columns["autor_name"] as DataGridViewComboBoxColumn).DataSource = dtAutor;
            (dgAutor.Columns["autor_name"] as DataGridViewComboBoxColumn).DataPropertyName = "id_autor";
            (dgAutor.Columns["autor_name"] as DataGridViewComboBoxColumn).ValueMember = "id";
            (dgAutor.Columns["autor_name"] as DataGridViewComboBoxColumn).DisplayMember = "Name";
        }
        private void сохранитьИзмененияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adAutor.Update(dtAutor);
            adBooks.Update(dtBooks);            
        }

        private void dgAutor_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            FillBooks(dgAutor[0, dgAutor.CurrentRow.Index].Value.ToString());
        }
        private int IncrementSeed(string tableName)
        {
            int seed = 0;
            string sqltext = "select max(id) from " + tableName;
            SQLiteCommand cmd = new SQLiteCommand(sqltext,conn);
            SQLiteDataReader dr = cmd.ExecuteReader();
            
            if (dr.HasRows)
            {
                dr.Read();
                seed = Convert.ToInt32(dr[0]);
            }
            

            return ++seed;

        }



    }
}

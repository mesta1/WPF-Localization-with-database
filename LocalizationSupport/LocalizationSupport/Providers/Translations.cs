using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Globalization;
using System.Collections;
using System.Resources;
using System.Data;
using LocalizationSupport.Properties;


namespace LocalizationSupport.Providers
{

    public class DatabaseResourceManager : ResourceManager
    {
        private string dsn;
        private Hashtable resources;

        public List<CultureInfo> CultureList { get; private set; }

        public DatabaseResourceManager(string connectionString)
        {
            this.dsn = connectionString;
            this.resources = new Hashtable();
            this.CultureList = GetLanguageList(); 
        }               

        protected List<CultureInfo> GetLanguageList()
        {
            List<CultureInfo> cultureList = new List<CultureInfo>();

            DataTable dt = new DataTable();
            OleDbConnection conn = new OleDbConnection();
            conn.ConnectionString = dsn;
            conn.Open();
            OleDbCommand command = new OleDbCommand("Select * From Translations"); // Here there is the reference to table name
            command.Connection = conn;
            OleDbDataAdapter da = new OleDbDataAdapter(command);
            da.Fill(dt);
            conn.Close();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                try
                {
                    if (dt.Columns[i].ColumnName != "Key")
                        cultureList.Add(new CultureInfo(dt.Columns[i].ColumnName));
                }
                catch (Exception exc)
                {
                    throw new CultureNotFoundException("Wrong culture code detected: " + dt.Columns[i].ColumnName.ToString() + "\n" + exc.Message);
                }
            }
            return cultureList;
        }

        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            DatabaseResourceSet rs = null;

            if (this.resources.Contains(culture.Name))
            {
                rs = this.resources[culture.Name] as DatabaseResourceSet;
            }
            else
            {
                rs = new DatabaseResourceSet(dsn, culture);
                this.resources.Add(culture.Name, rs);
            }
            return rs;
        }
    }

    public class DatabaseResourceReader : IResourceReader
    {
        private string dsn;
        private string language;

        public DatabaseResourceReader
           (string dsn, CultureInfo culture)
        {
            this.dsn = dsn;
            this.language = culture.Name;
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            Hashtable dict = new Hashtable();

            OleDbConnection conn = new OleDbConnection();
            conn.ConnectionString = dsn;
            OleDbCommand command = conn.CreateCommand();
            if (language == "")
                language = "Default";

            command.CommandText = "SELECT [key], [" + language + "] " + "FROM Translations"; // Here there is the reference to table name

            try
            {
                conn.Open();

                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetValue(1) != System.DBNull.Value)
                        dict.Add(reader.GetString(0), reader.GetString(1));
                }

                reader.Close();
            }
            catch   // ignore missing columns in the database
            {
            }
            finally
            {
                conn.Close();
            }
            return dict.GetEnumerator();
        }

        public void Close()
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
        }
    }

    public class DatabaseResourceSet : ResourceSet
    {
        internal DatabaseResourceSet(string dsn, CultureInfo culture)
            : base (new DatabaseResourceReader(dsn, culture))
        {
        }

        public override Type GetDefaultReader()
        {
            return typeof(DatabaseResourceReader);
        } 
    }
}

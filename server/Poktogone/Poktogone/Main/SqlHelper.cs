using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Poktogone.Main
{
    [Serializable]
    class UninitializedException : Exception
    {
        public UninitializedException() { }
        public UninitializedException(string message) : base(message) { }
        public UninitializedException(string message, Exception inner) : base(message, inner) { }
        protected UninitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context
        ) : base(info, context) { }
    }

    class Table
    {
        List<String> tables;
        List<String> joins;

        public Table()
        {
            this.tables = new List<string>();
            this.joins = new List<string>();
        }

        public Table(String table, int id) : this()
        {
            this.tables.Add(table);
            this.joins.Add(id.ToString());
        }

        // idName: name of the id in one of the previous tables
        public Table Join(String table, String idName)
        {
            this.tables.Add(table);
            this.joins.Add(idName);
            return this;
        }
    }

    class Where
    {
        public Where() { }

        public Where And() { return this; }
    }

    class SqlHelper // docs. https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient?view=netframework-4.8
    {
        private SqlConnection conn;
        private String connFileName;

        public SqlHelper()
        {
            this.conn = null;
            this.connFileName = "";
        }

        public SqlHelper(String c) : this()
        {
            this.Connect(c);
        }

        public SqlHelper Connect(String mdfFileName)
        {
            String c = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={mdfFileName};Integrated Security=True";
            this.connFileName = mdfFileName;

            this.conn = new SqlConnection(c);
            return this;
        }

        private void AssessConn()
        {
            if (this.conn == null)
                throw new UninitializedException("Connection not established.");
        }

        public SqlDataReader Select(String da)
        {
            Console.WriteLine(da);
            SqlCommand command = new SqlCommand(da, this.conn);

            try
            {
                this.conn.Open();
                SqlDataReader reader = command.ExecuteReader();
                return reader;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /**
         * Hint:
         * <code>using (SqlDataReader r = Select(*-*)) while (r.Read()) { *-* }
         * Do remember to `.Close` any reader you've `.Read`.
         * 
         * TODO: proper verification / usage of command.Parameters.AddWithValue
         */
        public SqlDataReader Select(String table, String column = "*", String where = "", string order = "")
        {
            this.AssessConn();

            String queryString = $"SELECT {column} FROM [dbo].[{table}]";

            if (where != "") queryString += " WHERE " + where;
            if (order != "") queryString += " ORDER BY " + order;

            return this.Select(queryString);
        }

        public SqlDataReader Select(String[] table, String column = "*", String where = "", string order = "")
        {
            this.AssessConn();
            
            return this.Select(String.Join("] JOIN [dbo].[", table), column, where, order);
        }

        public SqlDataReader Select(Table table, Where where = null)
        {
            this.AssessConn();

            return null; // this.Select();
        }
    }
}

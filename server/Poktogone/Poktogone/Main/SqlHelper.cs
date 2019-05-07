using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient; // [MS docs](https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient?view=netframework-4.8)

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
        List<String> idColumns;

        public Table()
        {
            this.tables = new List<String>();
            this.joins = new List<String>();
            this.idColumns = new List<String>();
        }

        public Table(String table, int id) : this()
        {
            this.Join(table, "id", id.ToString());
        }

        public Table Join(String table, String idColumn, String idName)
        {
            this.tables.Add(table);
            this.joins.Add(idName);
            this.idColumns.Add(idColumn);
            return this;
        }

        // idName: name of the id in one of the previous tables (by default, join on '[table].[id]')
        public Table Join(String table, String idName)
        {
            return this.Join(table, "id", idName);
        }

        // id: raw value of the id (by default, join on '[table].[id]')
        public Table Join(String table, int id)
        {
            return this.Join(table, "id", id.ToString());
        }

        public override string ToString()
        {
            String table = "";
            String where = "";

            String tableSep = "";
            String whereSep = "";

            for (int k = 0; k < this.tables.Count; k++)
            {
                String tableName = "";
                String tableAlias = "";
                
                if (this.tables[k].Contains(" AS ")) // "table AS t" --> "[table] AS [t]"
                {
                    String[] tmp = this.tables[k].Replace(" AS ", "@").Split('@');
                    tableName = $"[{tmp[0]}] AS [{tmp[1]}]";
                    tableAlias = $"[{tmp[1]}]";
                }
                else // "table" --> "[table]"
                {
                    tableName = $"[{this.tables[k]}]";
                    tableAlias = tableName;
                }

                table += $"{tableSep}{tableName}";
                where += $"{whereSep}{tableAlias}";
                
                if (this.joins[k].Contains('.')) // "other.bla" --> "[table].[id] = [other].[bla]"
                {
                    String[] tmp = this.joins[k].Split(".".ToCharArray(), 2);
                    where += $".[{this.idColumns[k]}] = [{tmp[0]}].[{tmp[1]}]";
                }
                else // "42" --> "[table].[id] = 42"
                {
                    where += $".[{this.idColumns[k]}] = '{this.joins[k]}'";
                }

                tableSep = ", ";
                whereSep = " AND ";
            }

            return $"{table} WHERE {where}";
        }
    }

    class Where
    {
        List<String> keys;
        List<String> values;

        public Where()
        {
            this.keys = new List<string>();
            this.values = new List<string>();
        }

        public Where(String key, String value) : this()
        {
            this.And(key, value);
        }

        public Where And(String key, String value)
        {
            this.keys.Add(key);
            this.values.Add(value);
            return this;
        }

        public override string ToString()
        {
            String r = "";
            String sep = "";

            for (int k = 0; k < this.keys.Count; k++)
            {
                r += $"{sep}{this.keys[k]}= {this.values[k]}";
                sep = " AND ";
            }

            return r;
        }
    }

    class SqlHelper
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

        public List<Dictionary<String, String>> Select(Table table, Where where = null, params String[] columns)
        {
            this.AssessConn();

            String names = "";
            String sep = "";
            foreach (String c in columns)
            {
                names += $"{sep}[{c.Replace(".", "].[")}]";
                sep = ", ";
            }

            List<Dictionary<String, String>> r = new List<Dictionary<String, String>>();

            using (SqlDataReader reader = this.Select($"SELECT {names} FROM {table}{(where == null ? "" : $" AND {where}")}"))
            {
                while (reader.Read())
                {
                    Dictionary<String, String> d = new Dictionary<String, String>();
                    for (int j = 0; j < reader.FieldCount; j++)
                        d[columns[j]] = reader[j].ToString();
                    r.Add(d);
                }
                reader.Close();
            }
            this.conn.Close();

            return r;
        }

        // e.g.: `var r = Program.dbo.Select(new Table("pokemons", k), "nom", "type1", "type2", "atk", "def")[0];`
        public List<Dictionary<String, String>> Select(Table table, params String[] columns)
        {
            return this.Select(table, null, columns);
        }
    }
}

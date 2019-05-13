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

    /// <summary>
    /// Class to use when doing a SELECT JOINing multiple tables.
    /// </summary>
    class Table
    {
        protected List<String> tables;
        List<String> joins;
        List<String> idColumns;

        protected Table()
        {
            this.tables = new List<String>();
            this.joins = new List<String>();
            this.idColumns = new List<String>();
        }

        /// <summary>
        /// Create a new <code>Table</code> object usable in a SELECT query.
        /// </summary>
        /// <param name="table">Name of the table.</param>
        /// <param name="id">Value of the ID to select onto.</param>
        /// <remarks>Use <code>Table.Join</code> to add an other table to the selection.</remarks>
        public Table(String table, int id) : this()
        {
            this.Join(table, "id", id.ToString());
        }

        /// <summary>
        /// Join an other table to the previous ones.
        /// The condition is "table.idColumn = %idName%" where "%idName%" is an ID referencing another table in the JOIN.
        /// </summary>
        /// <param name="table">Name of the table.</param>
        /// <param name="idColumn">Name of the column to JOIN onto.</param>
        /// <param name="idName">Name of an ID in an other table; should be "otherTable.columnName".</param>
        /// <returns><code>this</code>, for chaining.</returns>
        public Table Join(String table, String idColumn, String idName)
        {
            this.tables.Add(table);
            this.joins.Add(idName);
            this.idColumns.Add(idColumn);
            return this;
        }

        /// <summary>
        /// Join an other table to the previous ones.
        /// By default, the condition is "table.id = %idName%" where "%idName%" is an ID referencing another table in the JOIN.
        /// To precise a column name to JOIN onto, use <see cref="Table.Join(String, String, String)"/>.
        /// </summary>
        /// <param name="table">Name of the table.</param>
        /// <param name="idName">Name of an ID in an other table; should be "otherTable.columnName".</param>
        /// <returns><code>this</code>, for chaining.</returns>
        public Table Join(String table, String idName)
        {
            return this.Join(table, "id", idName);
        }

        /// <summary>
        /// Join an other table to the previous ones.
        /// The condition is "table.idColumn = id".
        /// </summary>
        /// <param name="table">Name of the table.</param>
        /// <param name="idColumn">Name of the column to JOIN onto.</param>
        /// <param name="id">Value of the ID to select onto.</param>
        /// <returns><code>this</code>, for chaining.</returns>
        public Table Join(String table, String idColumn, int id)
        {
            this.tables.Add(table);
            this.joins.Add(id.ToString());
            this.idColumns.Add(idColumn);
            return this;
        }


        /// <summary>
        /// Join an other table to the previous ones.
        /// By default, the condition is "table.id = id".
        /// To precise a column name to JOIN onto, use <see cref="Table.Join(String, String, int)"/>.
        /// </summary>
        /// <param name="table">Name of the table.</param>
        /// <param name="id">Value of the ID to select onto.</param>
        /// <returns><code>this</code>, for chaining.</returns>
        public Table Join(String table, int id)
        {
            return this.Join(table, "id", id);
        }

        /// <summary>
        /// Create the valid JOIN part of the SQL request on the given tables.
        /// </summary>
        /// <returns>The part of the request after "SELECT ".</returns>
        /// <remarks>The WHERE clause is already in as even the first table must give a column to select onto.</remarks>
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

    class SimpleTable : Table
    {
        public SimpleTable(String name) : base()
        {
            this.tables.Add(name);
        }

        public override string ToString()
        {
            return $"[{this.tables[0].Replace(".", "].[")}]";
        }
    }

    /// <summary>
    /// Class to use with a <seealso cref="Table"/> object to complement the select condition.
    /// Actualy unused for Poktogone, hence untested...
    /// </summary>
    class Where
    {
        List<String> keys;
        List<String> values;

        /// <summary>
        /// Create an empty WHERE clause.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>

        public Where()
        {
            this.keys = new List<string>();
            this.values = new List<string>();
        }

        /// <summary>
        /// Create a key / value paire for the WHERE clause.
        /// </summary>
        /// <param name="key">Key, should be table.column.</param>
        /// <param name="value">Value, will be escaped with "'" and any "'" found prior to will be replaced with "''".</param>
        public Where(String key, String value) : this()
        {
            this.And(key, value);
        }

        /// <summary>
        /// Create a key / value paire for the WHERE clause.
        /// </summary>
        /// <param name="key">Key, should be table.column.</param>
        /// <param name="value">Value.</param>
        public Where(String key, int value) : this()
        {
            this.And(key, value);
        }

        /// <summary>
        /// Add a key / value paire in the WHERE clause.
        /// </summary>
        /// <param name="key">Key, should be table.column.</param>
        /// <param name="value">Value, will be escaped with "'" and any "'" found prior to will be replaced with "''".</param>
        /// <returns><code>this</code>, for chaining.</returns>
        public Where And(String key, String value)
        {
            this.keys.Add(key);
            this.values.Add($"'{value.Replace("'", "''")}'");
            return this;
        }

        /// <summary>
        /// Add a key / value paire in the WHERE clause.
        /// </summary>
        /// <param name="key">Key, should be table.column.</param>
        /// <param name="value">Value.</param>
        /// <returns><code>this</code>, for chaining.</returns>
        public Where And(String key, int value)
        {
            return this.And(key, value.ToString());
        }

        /// <summary>
        /// Create the WHERE part of the SQL request.
        /// </summary>
        /// <returns>The part of the request after "WHERE ".</returns>
        /// <remarks>The "WHERE" keyworld is not acctualy in the request as it is supposed to be appended after a <seealso cref="Table"/> object's String representation.</remarks>
        public override string ToString()
        {
            String r = "";
            String sep = "";

            for (int k = 0; k < this.keys.Count; k++)
            {
                String tmp = $"[{this.keys[k].Replace(".", "].[")}]";
                r += $"{sep}{tmp}= {this.values[k]}";
                sep = " AND ";
            }

            return r;
        }
    }

    /// <summary>
    /// A <see cref="SqlHelper"/> is meant to be connected to a sqllocaldb server with an attached file through <see cref="SqlHelper.Connect"/>.
    /// See also: <seealso cref="Table"/>, <seealso cref="Where"/>.
    /// </summary>
    class SqlHelper
    {
        private SqlConnection conn;
        private String connFileName;

        /// <summary>
        /// Create an empty, unusable (because not connected) <see cref="SqlHelper"/>.
        /// You must use <see cref="SqlHelper.Connect(String)"/> before doing any query.
        /// </summary>
        public SqlHelper()
        {
            this.conn = null;
            this.connFileName = "";
        }

        /// <summary>
        /// Create a <see cref="SqlHelper"/>, and connect to the specified file.
        /// </summary>
        /// <param name="mdfFileName">Attach DB filename (e.g.: .mdf).</param>
        public SqlHelper(String mdfFileName) : this()
        {
            this.Connect(mdfFileName);
        }

        /// <summary>
        /// Connect to the specified file.
        /// </summary>
        /// <param name="mdfFileName">Attach DB filename (e.g.: .mdf).</param>
        /// <remarks>
        /// Connection string details:
        /// <code>Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={mdfFileName};Integrated Security=True</code>
        /// </remarks>
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

        /// <summary>
        /// Raw SQL query from a String. Avoid direct usage.
        /// See also: <seealso cref="SqlHelper.Select(Table, string[])"/>, <seealso cref="SqlHelper.Select(Table, Where, string[])"/>.
        /// </summary>
        /// <param name="da">Raw SQL query.</param>
        /// <returns>A reader to the result of the request.</returns>
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

        /// <summary>
        /// Build and execute a SELECT query onto the <see cref="Table"/> object, respecting the <see cref="Where"/> object
        /// and return the rows of the result as a list of dictionaries associated the column name to the value.
        /// </summary>
        /// <param name="table">A <seealso cref="Table"/> object to select onto.</param>
        /// <param name="where">Any additional WHERE contition to append after the (potential) JOINs' conditions.</param>
        /// <param name="columns">The names of the columns to include in the returned dictionaries.</param>
        /// <returns>A list containing each row of the result as a dictionnary associating the name you gave in <code>columns</code> to its value.</returns>
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

        /// <summary>
        /// The same as calling <see cref="SqlHelper.Select(Table, Where, string[])"/> with <code>where = null</code> (i.e.: no aditional where conditions).
        /// </summary>
        /// <param name="table">A <seealso cref="Table"/> object to select onto.</param>
        /// <param name="columns">The names of the columns to include in the returned dictionaries.</param>
        /// <returns>A list containing each row of the result as a dictionnary associating the name you gave in <code>columns</code> to its value.</returns>
        public List<Dictionary<String, String>> Select(Table table, params String[] columns)
        {
            return this.Select(table, null, columns);
        }

        public List<Dictionary<String, String>> Select(String table, Where where, params String[] columns)
        {
            return this.Select(new SimpleTable(table), where, columns);
        }

        public List<Dictionary<String, String>> Select(String table, params String[] columns)
        {
            return this.Select(new SimpleTable(table), null, columns);
        }
    }
}

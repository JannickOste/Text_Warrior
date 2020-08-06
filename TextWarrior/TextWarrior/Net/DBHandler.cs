using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Data.Common;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TextWarrior.Net
{
    class DBHandler
    {
        public string _db_path;
        private string _connection_string;

        public DBHandler(string db_path)
        {
            this._db_path = db_path;
            this._connection_string = build_connection_string(db_path);
        }

        private string build_connection_string(string db_path = null)
        {
            string database_path = db_path != null ? db_path : this._db_path;

            SQLiteConnectionStringBuilder string_builder = new SQLiteConnectionStringBuilder();
            string_builder.DataSource = database_path;
            string_builder.Version = 3;

            return string_builder.ConnectionString;
        }

        private SQLiteConnection getSession()
        {

            return (new SQLiteConnection(this._connection_string));
        }

        public void create_db(string db_path = null)
        {
            // Als argument db_path is ingesteld deze gebruike anders classe path.
            db_path = db_path != null ? db_path : this._db_path;

            SQLiteConnection.CreateFile(db_path);
        }

        public JObject query(string query, Dictionary<String, Object> where = null)
        {
            JObject results = new JObject();
            string query_string = query;
            using (SQLiteConnection conn = this.getSession())
            {
                conn.Open();
                using SQLiteCommand sql_query = new SQLiteCommand(conn);

                // Indien er where resultaten zijn opgegeven paramatizeren.
                if(where != null)
                {
                    query_string += " WHERE (";
                    foreach(string key in where.Keys.ToArray())
                    {
                        if(key != "format_none")
                        {
                            string suffix = key != where.Keys.ToArray().Where(k => k != "format_none").ToArray().Last() ? ", " : ");";
                            query_string += $"{key} = @{key}{suffix}";
                            sql_query.Parameters.AddWithValue(key, where[key]);
                        } else foreach(KeyValuePair<String, Object> pair in (Dictionary<String, Object>)where["format_none"])
                                sql_query.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                    if (query_string.EndsWith(" WHERE ("))
                        query_string = query_string.Remove(query_string.Length - 8);
                    Console.WriteLine(query_string);
                }

                // Query string instellen op query object.
                sql_query.CommandText = query_string;
                Console.WriteLine(query_string);
                // Maak een reader voor de results van de query en zet om in json object indien er fetch actie is opgegeven.
                if (query_string.Split(" ")[0] == "SELECT")
                {
                    DbDataReader query_reader = sql_query.ExecuteReader();
                    while (query_reader.Read())
                    {
                        // Maak een tijdelijke dictionary aan voor elke set van resultaten.
                        Dictionary<String, Object> current_results = new Dictionary<String, Object>();
                        for (int i = 0; i < query_reader.FieldCount; i++) // Loop door alle velden en zet key/waarden in dictionary
                            current_results.Add(query_reader.GetName(i), query_reader.GetValue(i));

                        // zet dictionary om in jtoken en voeg resultaten toe aan resultaten object met index van resultaat.
                        results.Add($"{results.Count}", JToken.FromObject(current_results));
                    }

                    // Anders query uitvoeren zonder resultateop te vragen.
                }
                else sql_query.ExecuteNonQuery();
            }
            return results;
        }


        public JObject select(String[] fields, string table, Dictionary<String, Object> where = null)
        {
            string fields_str = fields.Length == 1 ? fields[0] : $"({string.Join(", ", fields)})";

            return this.query($"SELECT {fields_str} FROM {table}", where);
        }

        public void insert(string table, Dictionary<String, Object> fields, Dictionary<String, Object> where = null)
        {
            // Insert query voorbereiden met parameters, Select functie met lamba expresie gebruiken om voor alle keys een @ teken te parsen.
            string sql_query = $"INSERT INTO {table} ({String.Join(", ", fields.Keys.ToArray())}) " +
                                $"VALUES ({String.Join(", ", fields.Keys.Select(k => $"@{k}").ToArray())})";
            
            if (where != null)
                where.Add("format_none", fields);
            else where = new Dictionary<String, Object>() { { "format_none", fields } };

            this.query(sql_query, where);
        }
        public void update(string table, Dictionary<String, Object> values, Dictionary<String, object> where)
        {
            string query_str = $"UPDATE {table} SET {String.Join(", ", values.Keys.ToArray().Select(k => $"{k} = @{k}"))}";

            where.Add("format_none", values);

            this.query(query_str, where);
        }

        public bool create_table(string table_name, Dictionary<String, object> fields)
        {
            string sql_query = $"CREATE TABLE IF NOT EXISTS {table_name} (";
            foreach(KeyValuePair<String, Object> pair in fields)
            {
                string addition = "";

                Type t = pair.Value.GetType();
                if (t.Equals(typeof(string)))
                    addition = (string)pair.Value;
                else Console.WriteLine("Create table auto type parsing to be added...");

                string suffix = pair.Key == fields.Keys.ToArray().Last() ? ");" : ", ";
                sql_query += $"{pair.Key} {addition}{suffix}";

            }

            this.query(sql_query);
            return true;
        }
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextWarrior.Entity
{
    class EntityHandler
    {
        public User player = null;
        private Net.DBHandler _db = Configuration.db;
        private string[] _entity_types = new String[] { "users", "entitys" };

        public Dictionary<String, String> default_stats = new Dictionary<String, String>()
        {
            {"user", "{" +
                 "'hitpoints':10, " +
                "'mana': 10, " +
                "'strength': 10, " +
                "'dexterity': 10," +
                "'agility' : 10, " +
                "'spirit': 10" +
            "}"}
        };

        public String[] list_entity_names(string entity_type)
        {
            string[] names = null;

            if (_entity_types.Contains(entity_type))
            {
                JObject db_results = _db.select(new string[] { "name" }, entity_type);
                names = new string[db_results.Count];

                for (int i = 0; i < db_results.Count; i++)
                    names[i] = (string)(db_results[i.ToString()])["name"];
            }
            else Console.WriteLine($"[DEBUG ERROR]:\nlist_entity_names({entity_type}) -> Invalid argument supplied.\n- Valid entity types: {String.Join(", ", _entity_types)} ");
            
            return names;
        }

        public Object getEntity(object identifier, string entity_group = "entitys")
        {
            Dictionary<String, Object> where = null;
            if (_entity_types.Contains(entity_group))
            {
                if (identifier.GetType().Equals(typeof(string)))
                    where = new Dictionary<String, Object>()
                    {
                        { "name", (string)identifier }
                    };

                else if (identifier.GetType().Equals(typeof(int)))
                    where = new Dictionary<String, Object>()
                    {
                        {"id", (int)identifier}
                    };

                if (where != null)
                {
                    JObject res = _db.select(new string[] { "id" }, entity_group, where);

                    if (res.Count == 1)
                    {
                        return new User((System.Int64)(res["0"])["id"]);
                    }
                }
            }
            return null;
        }

        public void createEntity(string name, string entity_type, string stats_json)
        {
            _db.insert(entity_type, new Dictionary<String, Object>()
            {
                {"name", name},
                {"stats", stats_json}
            });
        }
    }
}
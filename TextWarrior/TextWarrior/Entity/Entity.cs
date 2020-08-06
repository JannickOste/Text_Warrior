using Nancy.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using TextWarrior.Net;

namespace TextWarrior.Entity
{
    class Entity
    {
        protected System.Int64 _id = -1;
        protected string entity_type = "";

        protected static DBHandler _db = Configuration.db;

        public Entity(System.Int64 id, string entity_type = "entitys")
        {
            this._id = id;
            this.entity_type = entity_type;
        }

        protected JToken get_db_values()
        {
            return _db.select(new string[] { "*" }, entity_type,
                new Dictionary<String, Object>() { { "id", this.getId() } })["0"];
        }

        // Getters.
        public System.Int64 getId() { return this._id; }

        public String getName()
        {
            return ((string)this.get_db_values()["name"]);
        }

        public JObject getStats()
        {
            return JObject.Parse((string)this.get_db_values()["stats"]);
        }

    }
}

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
    class User : Entity
    {
        public User(System.Int64 id) : base(id, "users")
        {
            this._id = id;
        }

        public bool setStat(string key, object value)
        {
            // Verkrijg de huidige stats, check of de opgegeven key geldig is en wijzig de stat in dit geval.
            JObject current_stats = this.getStats();
            if(current_stats.Properties().Select(i => i.Name).ToArray().Contains(key))
            {
                current_stats[key] = JToken.FromObject(value);
                _db.update(entity_type, new Dictionary<String, Object>(){ { "stats", current_stats.ToString(Formatting.None) } }, new Dictionary<String, Object>() {
                    { "id", this._id }
                }); ;
            }

            return true;
        }

    }
}

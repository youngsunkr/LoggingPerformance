﻿using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LoggingPerformance.Octopus.Persistance
{
    public class RelationalJsonContractResolver : DefaultContractResolver
    {
        readonly RelationalMappings mappings;

        public RelationalJsonContractResolver(RelationalMappings mappings)
        {
            this.mappings = mappings;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            DocumentMap map;
            mappings.TryGet(member.DeclaringType, out map);

            var property = base.CreateProperty(member, memberSerialization);

            // ID properties are stored as columns
            if (property.PropertyName == "Id" && map != null)
            {
                property.Ignored = true;
            }

            // Indexed properties are stored as columns
            if (map != null && map.IndexedColumns.Any(c => c.Property != null && c.Property.Name == member.Name))
            {
                property.Ignored = true;
            }
            
            if (!property.Writable)
            {
                var property2 = member as PropertyInfo;
                if (property2 != null)
                {
                    var hasPrivateSetter = property2.GetSetMethod(true) != null;
                    property.Writable = hasPrivateSetter;
                }
            }

            return property;
        }
    }
}
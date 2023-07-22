using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;

namespace shortenerTools.Infrastructure
{
    public static class EntityJsonPropertyConverter
    {
        public static void Serialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> results)
        {
            var convertibleProperties = entity.GetType().GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EntityJsonPropertyConverterAttribute), false).Any());


            foreach (System.Reflection.PropertyInfo property in convertibleProperties)
            {
                string input = System.Text.Json.JsonSerializer.Serialize(property.GetValue(entity));
                EntityProperty entityProperty = new EntityProperty(input);
                results.Add(property.Name, entityProperty);
            }
        }

        public static void Deserialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> properties)
        {
            var convertibleProperties = entity.GetType().GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EntityJsonPropertyConverterAttribute), false).Any());

            foreach (System.Reflection.PropertyInfo property in convertibleProperties)
            {
                if (properties.ContainsKey(property.Name))
                {
                    string stringValue = properties[property.Name]?.StringValue;
                    object value = System.Text.Json.JsonSerializer.Deserialize(stringValue, property.PropertyType);
                    property.SetValue(entity, value);
                }
            }
        }
    }
}
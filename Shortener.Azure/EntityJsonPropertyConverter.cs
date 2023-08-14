using Microsoft.Azure.Cosmos.Table;

namespace Shortener.Azure
{
    public static class EntityJsonPropertyConverter
    {
        public static void Serialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> results)
        {
            if (entity is null)
            {
                return;
            }
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
            if(entity is null)
            {
                return;
            }

            var convertibleProperties = entity.GetType().GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(EntityJsonPropertyConverterAttribute), false).Any());

            foreach (System.Reflection.PropertyInfo property in convertibleProperties)
            {
                if (properties.ContainsKey(property.Name))
                {
                    string? stringValue = properties[property.Name]?.StringValue;
                    if(stringValue is null)
                    {
                        continue;
                    }
                    object? value = System.Text.Json.JsonSerializer.Deserialize(stringValue, property.PropertyType);
                    property.SetValue(entity, value);
                }
            }
        }
    }
}
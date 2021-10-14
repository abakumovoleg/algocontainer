using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Algo.Strategies.Execution.Api.Dtos;

namespace Algo.Strategies.Execution.Api
{
    public class StrategyParametersJsonConverter : DerivedTypeJsonConverter<IStrategyParametersBaseDto>
    {
        protected override Type NameToType(string typeName)
        {
            switch (typeName)
            {
                case "soft":
                    return typeof(SoftStrategyParametersDto);
                case "hard":
                    return typeof(HardStrategyParametersDto);
                case "gentle":
                    return typeof(GentleStrategyParametersDto);
            }

            throw new Exception();
        }

        protected override string TypeToName(Type type)
        {
            return type.Name;
        }
    }

    public abstract class DerivedTypeJsonConverter<TBase> : JsonConverter<TBase>
    {
        protected abstract string TypeToName(Type type);

        protected abstract Type NameToType(string typeName);


        private const string TypePropertyName = "$type";


        public override bool CanConvert(Type objectType)
        {
            return typeof(TBase) == objectType;
        }


        public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // get the $type value by parsing the JSON string into a JsonDocument
            JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
            jsonDocument.RootElement.TryGetProperty(TypePropertyName, out JsonElement typeNameElement);
            string typeName = (typeNameElement.ValueKind == JsonValueKind.String) ? typeNameElement.GetString() : null;
            if (string.IsNullOrWhiteSpace(typeName)) throw new InvalidOperationException($"Missing or invalid value for {TypePropertyName} (base type {typeof(TBase).FullName}).");

            // get the JSON text that was read by the JsonDocument
            string json;
            using (var stream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Encoder = options.Encoder }))
            {
                jsonDocument.WriteTo(writer);
                writer.Flush();
                json = Encoding.UTF8.GetString(stream.ToArray());
            }

            // deserialize the JSON to the type specified by $type
            try
            {
                return (TBase)JsonSerializer.Deserialize(json, NameToType(typeName), options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid JSON in request.", ex);
            }
        }


        public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
        {
            // create an ExpandoObject from the value to serialize so we can dynamically add a $type property to it
            ExpandoObject expando = ToExpandoObject(value);
            expando.TryAdd(TypePropertyName, TypeToName(value.GetType()));

            // serialize the expando
            JsonSerializer.Serialize(writer, expando, options);
        }


        private static ExpandoObject ToExpandoObject(object obj)
        {
            var expando = new ExpandoObject();
            if (obj != null)
            {
                // copy all public properties
                foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
                {
                    expando.TryAdd(property.Name, property.GetValue(obj));
                }
            }

            return expando;
        }
    }
}
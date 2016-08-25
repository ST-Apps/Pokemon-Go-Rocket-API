using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PokemonGo_UWP.Utils.Helpers
{
    /// <summary>
    /// Json.NET converter used to serialize Protobuf wrapped objects
    /// </summary>
    public class ProtobufJsonNetConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var b64String = ((IMessage) value).ToByteString().ToBase64();
            JToken.FromObject(b64String).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //var jObject = JObject.Load(reader);

            var result = ((IMessage) Activator.CreateInstance(objectType));
            result.MergeFrom(ByteString.FromBase64((string) reader.Value));

            // Populate the object properties
            //serializer.Populate(reader, result);

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}

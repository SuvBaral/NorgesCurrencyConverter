using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorgesCurrencyConverter.BLL
{
    public partial class CurencyExchanger
    {
        public Meta Meta { get; set; }

        public Data Data { get; set; }
    }

    public class Data
    {

        public DataSet[] DataSets { get; set; }


        public Structure Structure { get; set; }
    }

    public class DataSet
    {

        public DataSetLink[] Links { get; set; }


        public string Action { get; set; }


        public Dictionary<string, SeriesValue> Series { get; set; }
    }

    public class DataSetLink
    {

        public string Rel { get; set; }


        public string Urn { get; set; }


        public Uri Uri { get; set; }
    }

    public class SeriesValue
    {

        public long[] Attributes { get; set; }


        public Dictionary<string, string[]> Observations { get; set; }
    }

    public class Structure
    {

        public DataSetLink[] Links { get; set; }


        public string Name { get; set; }


        public Descriptions Names { get; set; }


        public string Description { get; set; }


        public Descriptions Descriptions { get; set; }


        public Dimensions Dimensions { get; set; }

        public Attributes Attributes { get; set; }
    }

    public class Dimensions
    {

        public object[] Dataset { get; set; }


        public SeriesElement[] Series { get; set; }


        public Observation[] Observation { get; set; }
    }

    public class Attributes
    {

        public object[] Dataset { get; set; }


        public SeriesElement[] Series { get; set; }


        public Observation[] Observation { get; set; }
    }
    public class Observation
    {

        public string Id { get; set; }


        public string Name { get; set; }


        public string Description { get; set; }


        public long KeyPosition { get; set; }


        public string Role { get; set; }


        public ObservationValue[] Values { get; set; }
    }

    public class ObservationValue
    {

        public DateTimeOffset Start { get; set; }


        public DateTimeOffset End { get; set; }


        public DateTimeOffset Id { get; set; }


        public DateTimeOffset Name { get; set; }
    }

    public class SeriesElement
    {

        public string Id { get; set; }


        public string Name { get; set; }


        public string Description { get; set; }


        public Relationship Relationship { get; set; }

        public object Role { get; set; }

        public SeriesValueClass[] Values { get; set; }

        public long? KeyPosition { get; set; }
    }

    public class Relationship
    {
        public Dimension[] Dimensions { get; set; }
    }

    public class SeriesValueClass
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class Descriptions
    {
        public string En { get; set; }
    }

    public class Meta
    {
        public string Id { get; set; }

        public DateTimeOffset Prepared { get; set; }

        public bool Test { get; set; }

        public Receiver Sender { get; set; }

        public Receiver Receiver { get; set; }

        public MetaLink[] Links { get; set; }
    }

    public class MetaLink
    {
        public string Href { get; set; }

        public string Rel { get; set; }

        public Uri Uri { get; set; }
    }

    public class Receiver
    {

        public string Id { get; set; }
    }

    public enum Dimension { BaseCur, QuoteCur, Tenor };

    public partial class CurencyExchanger
    {
        public static CurencyExchanger FromJson(string json) => JsonConvert.DeserializeObject<CurencyExchanger>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this CurencyExchanger self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                DimensionConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class DimensionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Dimension) || t == typeof(Dimension?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "BASE_CUR":
                    return Dimension.BaseCur;
                case "QUOTE_CUR":
                    return Dimension.QuoteCur;
                case "TENOR":
                    return Dimension.Tenor;
            }
            throw new Exception("Cannot unmarshal type Dimension");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Dimension)untypedValue;
            switch (value)
            {
                case Dimension.BaseCur:
                    serializer.Serialize(writer, "BASE_CUR");
                    return;
                case Dimension.QuoteCur:
                    serializer.Serialize(writer, "QUOTE_CUR");
                    return;
                case Dimension.Tenor:
                    serializer.Serialize(writer, "TENOR");
                    return;
            }
            throw new Exception("Cannot marshal type Dimension");
        }

        public static readonly DimensionConverter Singleton = new DimensionConverter();
    }
}

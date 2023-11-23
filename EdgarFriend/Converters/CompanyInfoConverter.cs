using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdgarFriend;

public class CompanyInfo {
    public Dictionary<string, string> Dictionary { get; set; }
    public List<SymbolMapping> Mappings { get; set; }

    public CompanyInfo(Dictionary<string, string> dictionary, List<SymbolMapping> mappings)
    {
        this.Dictionary = dictionary;
        this.Mappings = mappings;
    }
}

public class CompanyInfoConverter : JsonConverter<CompanyInfo>
{
    public override CompanyInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
        

        var valueDict = new Dictionary<string, string>();
        var mappings = new List<SymbolMapping>();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new CompanyInfo(valueDict, mappings);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            reader.Read();  // Move to the start object token of the company info object

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            int cikStr = 0;
            string ticker = null;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string propertyName = reader.GetString();
                reader.Read();  // Move to the value token

                if (propertyName == "cik_str")
                {
                    cikStr = reader.GetInt32();
                    
                }
                else if (propertyName == "ticker")
                {
                    ticker = reader.GetString();
                }
                else
                {
                    reader.Skip();
                }
            }

            valueDict[cikStr.ToString("D10")] = ticker;  // Left pad the cik_str with zeros to 10 digits
            mappings.Add(new SymbolMapping(ticker, cikStr.ToString("D10")));
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, CompanyInfo value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Writing is not supported.");
    }
}
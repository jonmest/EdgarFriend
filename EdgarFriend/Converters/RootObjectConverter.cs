using System.Globalization;
using System.Text.RegularExpressions;

namespace EdgarFriend;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class RootObjectConverter : JsonConverter<RootObject>
{
    public RootObjectConverter(Dictionary<string, string>? symbolMap)
    {
        SymbolMap = symbolMap;
    }

    private Dictionary<string, string>? SymbolMap { get; }

    private static bool IsQuarterlyFrame(string frame)
    {
        var quarterlyPattern = @"^CY\d{4}Q[1-4]$";
        return Regex.IsMatch(frame, quarterlyPattern);
    }

    private static bool IsInstantaneousFrame(string frame)
    {
        return frame.EndsWith("I");
    }

    private static bool IsAnnualFrame(string frame)
    {
        var annualPattern = @"^CY\d{4}$";
        return Regex.IsMatch(frame, annualPattern);
    }
    
    public override RootObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw JsonException(reader, "Expected start object.");

        var rootObject = new RootObject();
        var facts = new Facts { UsGaap = new List<FundamentalEntry>() };
        rootObject.Facts = facts;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();  // Move to the next token, the value.

                switch (propertyName)
                {
                    case "cik":
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            rootObject.Cik = reader.GetString();
                        }
                        else if (reader.TokenType == JsonTokenType.Number)
                            rootObject.Cik = reader.GetInt32().ToString("D10");

                        if (SymbolMap.ContainsKey(rootObject.Cik)) rootObject.Symbol = SymbolMap[rootObject.Cik];
                        else
                        {
                            while (reader.Read())
                            {
                            }
                            return null;
                        }
                        break;
                    case "entityName":
                        rootObject.EntityName = reader.GetString();
                        break;
                    case "facts":
                        ReadFacts(ref reader, facts, rootObject);
                        break;
                    default:
                        throw JsonException(reader, $"Unexpected property: {propertyName}.");
                }
            }
        }
        return rootObject;
    }

    private JsonException JsonException(Utf8JsonReader reader, string message)
    {
        return new JsonException($"{message} (Position: {reader.TokenStartIndex})");
    }
    
    private void ReadFacts(ref Utf8JsonReader reader, Facts facts, RootObject rootObject)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw JsonException(reader, "Expected start object for facts.");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();  // Move to the next token, the value.

                switch (propertyName)
                {
                    case "us-gaap":
                    case "dei":
                        ReadUsGaap(ref reader, facts.UsGaap, rootObject);
                        break;
                    default:
                        SkipProperty(ref reader);
                        break;
                    //default:
                    //    throw JsonException(reader, $"Unexpected property in facts: {propertyName}.");
                }
            }
        }
    }
    
    private void SkipProperty(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var depth = 1;
            while (depth > 0 && reader.Read())
            {
                if (reader.TokenType == JsonTokenType.StartArray) depth++;
                if (reader.TokenType == JsonTokenType.EndArray) depth--;
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            var depth = 1;
            while (depth > 0 && reader.Read())
            {
                if (reader.TokenType == JsonTokenType.StartObject) depth++;
                if (reader.TokenType == JsonTokenType.EndObject) depth--;
            }
        }
        else
        {
            // For simple value types, do nothing, as they are represented by a single token.
        }
    }

    private void ReadUsGaap(ref Utf8JsonReader reader, List<FundamentalEntry> entries, RootObject rootObject)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw JsonException(reader, "Expected start object for us-gaap.");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var entryLabel = reader.GetString();
                reader.Read();  // Move to the next token, the value.
        
                if (!AcceptedLabels.Contains(entryLabel))
                {
                    reader.Skip();  // Skip the current object if label is not accepted.
                    continue;
                }
                
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw JsonException(reader, $"Expected start object for us-gaap entry {entryLabel}.");

                reader.Read();  // Move to the next token, either 'label', 'description', or 'units'.

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();  // Move to the next token, the property value.

                    switch (propertyName)
                    {
                        case "label":
                            // Handle label property if necessary
                            break;
                        case "description":
                            // Handle description property if necessary
                            break;
                        case "units":
                            ReadUnits(ref reader, entries, entryLabel, rootObject);  // Updated line
                            break;
                        default:
                            throw JsonException(reader, $"Unexpected property in us-gaap entry {entryLabel}: {propertyName}");
                    }

                    reader.Read();  // Move to the next token, either the next property name or an end object token.
                }

                if (reader.TokenType != JsonTokenType.EndObject)
                    throw JsonException(reader, $"Expected end object for us-gaap entry {entryLabel}.");
            }
        }
    }
    
    private void ReadUnits(ref Utf8JsonReader reader, List<FundamentalEntry> entries, string label, RootObject rootObject)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw JsonException(reader, $"Expected start object for units in us-gaap entry {label}.");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var unit = reader.GetString();
                reader.Read();  // Move to the next token, the units array.

                if (reader.TokenType != JsonTokenType.StartArray)
                    throw JsonException(reader, $"Expected start array for unit values in us-gaap entry {label}.");

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    var entry = ReadUsGaapEntry(ref reader, label, unit, rootObject);
                    if (entry.Fy != null && entry.Fp != null && entry.Cik != null && entry.PeriodType != null)
                    {
                        entries.Add(entry);
                    }
                    
                }
            }
        }
    }
    
    private FundamentalEntry ReadUsGaapEntry(ref Utf8JsonReader reader, string label, string unit, RootObject rootObject)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw JsonException(reader, $"Expected start object for unit values in us-gaap entry {label}.");

        var entry = new FundamentalEntry { Label = label, Unit = unit, Cik = rootObject.Cik, Symbol = rootObject.Symbol, EntityName = rootObject.EntityName };

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw JsonException(reader, $"Unexpected token in unit values for us-gaap entry {label}.");

            var propertyName = reader.GetString();
            reader.Read();  // Move to the next token, the property value.

            switch (propertyName)
            {
                case "val":
                    entry.Val = reader.GetDouble();
                    break;
                case "accn":
                    entry.Accn = reader.GetString();
                    break;
                case "start":
                    entry.Start = DateOnly.FromDateTime(DateTime.ParseExact(reader.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture));
                    break;
                case "end":
                    entry.End = DateOnly.FromDateTime(DateTime.ParseExact(reader.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture));
                    break;
                case "fy":
                    if (reader.TokenType == JsonTokenType.Number) entry.Fy = reader.GetInt32();
                    break;
                case "fp":
                    entry.Fp = reader.GetString();
                    break;
                case "filed":
                    entry.Filed = reader.GetString();
                    break;
                case "form":
                    entry.Form = reader.GetString();
                    break;
                case "frame":
                    entry.Frame = reader.GetString();
                    if (IsAnnualFrame(entry.Frame)) entry.PeriodType = "A";
                    else if (IsQuarterlyFrame(entry.Frame)) entry.PeriodType = "Q";
                    else if (IsInstantaneousFrame(entry.Frame)) entry.PeriodType = "I";
                    break;
                default:
                    throw JsonException(reader, $"Unexpected property {propertyName} in unit values for us-gaap entry {label}.");
            }
        }
        return entry;
    }

    public override void Write(Utf8JsonWriter writer, RootObject value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(RootObject);
    }
    
}
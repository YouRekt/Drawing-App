using Avalonia.Media;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrawingAppCG.Models
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? colorString = reader.GetString();
            return colorString is null ? throw new JsonException("The color string is null.") : Color.Parse(colorString);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
    public class PointConverter : JsonConverter<(int x, int y)>
    {
        public override (int x, int y) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            return (
                x: root.GetProperty("x").GetInt32(),
                y: root.GetProperty("y").GetInt32()
            );
        }
        public override void Write(Utf8JsonWriter writer, (int x, int y) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.x);
            writer.WriteNumber("y", value.y);
            writer.WriteEndObject();
        }
    }
}

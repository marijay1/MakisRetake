using CounterStrikeSharp.API.Modules.Utils;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MakisRetake.Configs.JsonProviders;

public class VectorProvider : JsonConverter<Vector> {

    public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.String) {
            throw new JsonException("Expected a string value.");
        }

        var stringValue = reader.GetString();
        if (stringValue == null) {
            throw new JsonException("String value is null.");
        }

        var values = stringValue.Split(' ');
        if (values.Length != 3) {
            throw new JsonException("String value is not in the correct format (X Y Z).");
        }

        float x, y, z;
        if (!float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x) ||
            !float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y) ||
            !float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out z)) {
            throw new JsonException("Unable to parse Vector float values.");
        }

        return new Vector(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options) {
        var vectorString = value.ToString();
        writer.WriteStringValue(vectorString);
    }
}
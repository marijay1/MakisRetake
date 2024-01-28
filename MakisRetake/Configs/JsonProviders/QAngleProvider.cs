﻿using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MakisRetake.Configs.JsonProviders;
public class QAngleProvider : JsonConverter<QAngle> {
    public override QAngle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        // Ensure token is a string
        if (reader.TokenType != JsonTokenType.String) {
            throw new JsonException("Expected a string value.");
        }

        // Get and validate string value
        var stringValue = reader.GetString();
        if (stringValue == null) {
            throw new JsonException("String value is null.");
        }

        // Parse components
        var values = stringValue.Split(' ');
        if (values.Length != 3) {
            throw new JsonException("String value is not in the correct format (X Y Z).");
        }

        float x, y, z;
        if (!float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out x) ||
            !float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out y) ||
            !float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out z)) {
            throw new JsonException("Unable to parse QAngle float values.");
        }

        return new QAngle(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, QAngle value, JsonSerializerOptions options) {
        // Assuming QAngle has a ToString() method that returns the desired format
        var qangleString = value.ToString();
        writer.WriteStringValue(qangleString);
    }
}


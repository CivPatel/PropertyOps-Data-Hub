using Microsoft.AspNetCore.Http;
using System.Text;

namespace PropertyOps.Api.Services;

public static class CsvParser
{
    public static async Task<List<string[]>> ReadAllRowsAsync(IFormFile file)
    {
        var rows = new List<string[]>();

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        string? line;

        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            rows.Add(ParseLine(line)
                .Select(value => value.Trim())
                .ToArray());
        }

        return rows;
    }

    public static List<string> ParseLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            if (character == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (character == ',' && !insideQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(character);
            }
        }

        values.Add(currentValue.ToString());

        return values;
    }
}
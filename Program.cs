using System.Text.Json;

try
{
    if (args.Length != 1) throw new ArgumentException("Pass one JSON input file");
    using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
    Console.WriteLine(JsonSerializer.Serialize(Analyze(data.RootElement)));
    return 0;
}
catch (Exception error) when (error is IOException or JsonException or FormatException
                               or ArgumentException or InvalidOperationException
                               or KeyNotFoundException or OverflowException)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}

static object Analyze(JsonElement input)
{
    var mapping = input.GetProperty("mapping").EnumerateArray().Select(value => value.GetInt32()).ToArray();
    int n = mapping.Length;
    if (n is < 1 or > 10000 || mapping.Any(value => value < 0 || value >= n) || mapping.Distinct().Count() != n)
        throw new ArgumentException("Mapping must contain each index from zero to length minus one exactly once");
    var inverse = new int[n];
    for (int i = 0; i < n; i++) inverse[mapping[i]] = i;
    var visited = new bool[n];
    var cycles = new List<List<int>>();
    System.Numerics.BigInteger order = 1;
    for (int i = 0; i < n; i++)
    {
        if (visited[i]) continue;
        var cycle = new List<int>();
        for (int index = i; !visited[index]; index = mapping[index])
        {
            cycle.Add(index);
            visited[index] = true;
        }
        cycles.Add(cycle);
        order = order / System.Numerics.BigInteger.GreatestCommonDivisor(order, cycle.Count) * cycle.Count;
    }
    return new { cycles, inverse, parity = (n - cycles.Count) % 2 == 0 ? "even" : "odd",
                 order = order.ToString(System.Globalization.CultureInfo.InvariantCulture) };
}

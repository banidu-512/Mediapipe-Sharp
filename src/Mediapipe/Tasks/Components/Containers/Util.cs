namespace Mediapipe.Tasks.Components.Containers;

internal static class Util
{
    public static string Format<T>(T value) => value == null ? "null" : $"{value}";

    public static string Format(string? value) => value == null ? "null" : $"\"{value}\"";

    public static string Format<T>(List<T>? list)
    {
        if (list == null)
        {
            return "null";
        }
        var str = string.Join(", ", list.Select(x => x?.ToString() ?? "null"));
        return $"[{str}]";
    }
}

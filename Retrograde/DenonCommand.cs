namespace Dn500BD.Retrograde;

public class DenonCommand(string label, string code)
{
    public string Label { get; init; } = label;
    public string Code { get; init; } = code;
}

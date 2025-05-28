namespace Dn500BD.Retrograde.Core;

public class DenonCommand(string label, string code)
{
    public string Label { get; init; } = label;
    public string Code { get; init; } = code;
}

namespace WindowKeys.Settings;

public class ActivationSettings
{
	public HashSet<ushort> HotKey { get; set; } = [];
	public char[] SelectionKeys { get; set; } = [];
}
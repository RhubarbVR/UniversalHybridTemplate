namespace UniversalSystemCalls;

public readonly struct SystemColor(byte r, byte g, byte b)
{
	public readonly byte R = r;
	public readonly byte G = g;
	public readonly byte B = b;

	public static SystemColor FromRGB(byte r, byte g, byte b) {
		return new SystemColor(r, g, b);
	}

	public static SystemColor White => new(255, 255, 255);
	public static SystemColor Black => new(0, 0, 0);
	public static SystemColor Red => new(255, 0, 0);
	public static SystemColor Green => new(0, 255, 0);
	public static SystemColor Blue => new(0, 0, 255);
	public static SystemColor Yellow => new(255, 255, 0);
	public static SystemColor Cyan => new(0, 255, 255);
	public static SystemColor Magenta => new(255, 0, 255);

	public override string ToString() {
		return $"(R: {R}, G: {G}, B: {B})";
	}

	public static bool operator ==(SystemColor left, SystemColor right) {
		return left.R == right.R && left.G == right.G && left.B == right.B;
	}
	public static bool operator !=(SystemColor left, SystemColor right) {
		return left.R != right.R || left.G != right.G || left.B != right.B;
	}

	public readonly override bool Equals(object obj) {
		return obj is SystemColor color && this == color;
	}

	public readonly override int GetHashCode() {
		return HashCode.Combine(R, G, B);
	}

	public string ToHexString() {
		return $"#{R:x2}{G:x2}{B:x2}";
	}
}

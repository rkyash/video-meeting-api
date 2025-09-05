namespace VideoMeeting.Domain.ValueObjects;

public class RoomCode : IEquatable<RoomCode>
{
    private RoomCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool Equals(RoomCode? other)
    {
        return other is not null && Value == other.Value;
    }

    public static RoomCode Create(string? code = null)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            if (code.Length != 8 || !code.All(char.IsDigit))
                throw new ArgumentException("Room code must be exactly 8 digits.", nameof(code));

            return new RoomCode(code);
        }

        // Generate random 6-digit code
        var random = new Random();
        var generatedCode = random.Next(10000000, 99999999).ToString();
        return new RoomCode(generatedCode);
    }

    public static implicit operator string(RoomCode roomCode)
    {
        return roomCode.Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoomCode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
using System.Collections;
using System.Runtime.CompilerServices;

namespace R3EventsGenerator.Utilities;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[] _array;

    public EquatableArray()
    {
        _array = [];
    }

    public EquatableArray(T[] array)
    {
        _array = array;
    }

    public static implicit operator EquatableArray<T>(T[] array)
    {
        return new(array);
    }

    public ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _array[index];
    }

    public int Length => _array.Length;

    public ReadOnlySpan<T> AsSpan()
    {
        return _array.AsSpan();
    }

    public ReadOnlySpan<T>.Enumerator GetEnumerator()
    {
        return AsSpan().GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return _array.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _array.AsEnumerable().GetEnumerator();
    }

    public bool Equals(EquatableArray<T> other)
    {
        return AsSpan().SequenceEqual(other.AsSpan());
    }
}

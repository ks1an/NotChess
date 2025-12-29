using System;

public class ReactiveProperty<T>
{
    public event Action<T> OnChanged;
    T _value;
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            OnChanged?.Invoke(_value);
        }
    }
}

namespace CommonLibs.Utils;


/*
        * This simple class represent One of the Possible values i.e
        * either an Ok variant or an Err Variant.

        * setting any of the one makes the other default to its default value for that type.
    */
public class OneOf<T, E>
{
    private T _ok;
    private E _err;
    // NOTE: having these constructors also prevent from creating any type with T = E
    // because constructors will be same then.
    public OneOf(T ok)
    {
        _ok = ok;
        _err = default(E);
    }
    public OneOf(E err)
    {
        _err = err;
        _ok = default(T);
    }

    public T Ok
    {
        get
        {
            return _ok;
        }
        set
        {
            _ok = value;
            _err = default(E);
        }
    }
    public E Err
    {
        get
        {
            return _err;
        }
        set
        {
            _err = value;
            _ok = default(T);
        }
    }
}

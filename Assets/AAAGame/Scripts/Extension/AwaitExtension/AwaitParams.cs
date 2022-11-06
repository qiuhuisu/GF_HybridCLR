using System.Threading.Tasks;
using GameFramework;

public class AwaitParams<T> : IReference
{
    public object UserData { get; private set; }

    public TaskCompletionSource<T> Source { get; private set; }

    public static AwaitParams<T> Create(object userData, TaskCompletionSource<T> source)
    {
        AwaitParams<T> awaitDataWrap = ReferencePool.Acquire<AwaitParams<T>>();
        awaitDataWrap.UserData = userData;
        awaitDataWrap.Source = source;
        return awaitDataWrap;
    }

    public void Clear()
    {
        UserData = null;
        Source = null;
    }
}
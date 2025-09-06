public interface IInitializable
{
    /// <summary>
    /// Called after Awake and for objects that are disabled at startup because
    /// Disabled objects dont call Awake().
    /// </summary>
    void Initialize();
}

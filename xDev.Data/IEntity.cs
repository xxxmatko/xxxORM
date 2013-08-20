namespace xDev.Data
{
    /// <summary>
    /// Interface which describes entity.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    public interface IEntity</*out*/ T>
        where T : class, new()
    {
    }
}

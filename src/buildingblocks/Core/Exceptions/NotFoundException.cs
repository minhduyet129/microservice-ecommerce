namespace BuildingBlocks.Core.Exceptions;

public class NotFoundException : Exception
{
    public string EntityName { get; set; }
    public object Key { get; set; }

    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}
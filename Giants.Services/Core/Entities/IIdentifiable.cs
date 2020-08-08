namespace Giants.Services.Core.Entities
{
    public interface IIdentifiable
    {
        string id { get; }

        string DocumentType { get; }
    }
}

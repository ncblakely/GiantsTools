namespace Giants.Services
{
    public interface IIdentifiable
    {
        string id { get; }

        string DocumentType { get; }
    }
}

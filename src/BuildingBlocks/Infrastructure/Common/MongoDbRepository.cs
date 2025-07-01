using MongoDB.Driver;
using Contracts.Domains;
using Contracts.Domains.Interfaces;
using Infrastructure.Extensions;
using Shared.Configurations;

namespace Infrastructure.Common;

public class MongoDbRepository<T> : IMongoDbRepositoryBase<T> where T : MongoEntity
{
    public IMongoDatabase Database { get; }

    public MongoDbRepository(IMongoClient client, MongoDbSettings settings)
    {
        Database = client.GetDatabase(settings.DatabaseName)
            .WithWriteConcern(WriteConcern.Acknowledged);
    }

    public IMongoCollection<T> FindAll(ReadPreference? readPreference = null)
    {
        return Database
            .WithReadPreference(readPreference ?? ReadPreference.Primary)
            .GetCollection<T>(GetCollectionName());
    }

    protected virtual IMongoCollection<T> Collection =>
        Database.GetCollection<T>(GetCollectionName());

    public Task CreateAsync(T entity) => Collection.InsertOneAsync(entity);

    public Task UpdateAsync(T entity)
    {
        var idProperty = typeof(T).GetProperty(nameof(MongoEntity.Id));
        if (idProperty == null)
        {
            throw new InvalidOperationException($"The entity type {typeof(T).Name} does not have an 'Id' property.");
        }

        var idValue = idProperty.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(idValue))
        {
            throw new ArgumentException("The entity's 'Id' property cannot be null or empty.", nameof(entity));
        }

        var filter = Builders<T>.Filter.Eq(e => e.Id, idValue);
        return Collection.ReplaceOneAsync(filter, entity);
    }

    public Task DeleteAsync(string id) => Collection.DeleteOneAsync(x => x.Id.Equals(id));

    private static string? GetCollectionName()
    {
        return (typeof(T).GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault() as
            BsonCollectionAttribute)?.CollectionName;
    }
}
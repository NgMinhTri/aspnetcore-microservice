﻿using Infrastructure.Extensions;
using Inventory.Product.API.Entities.Abstraction;
using Inventory.Product.API.Extensions;
using Inventory.Product.API.Repositories.Abstraction;
using MongoDB.Driver;
using Shared.Configurations;
using System.Linq.Expressions;

namespace Inventory.Product.API.Repositories
{
    public class MongoDbRepository<T> : IMongoDbRepositoryBase<T> where T : MongoEntity
    {
        private IMongoDatabase Database { get; }
        public MongoDbRepository(IMongoClient client, MongoDbSettings settings)
        {
            Database = client.GetDatabase(settings.DatabaseName);
        }

        protected virtual IMongoCollection<T> Collection =>
            Database.GetCollection<T>(GetCollectionName());
        public Task CreateAsync(T entity) => Collection.InsertOneAsync(entity);

        public Task DeleteAsync(string id)
            => Collection.DeleteOneAsync(x => x.Id.Equals(id));

        public IMongoCollection<T> FindAll(ReadPreference? readPreference = null)
        => Database.WithReadPreference(readPreference ?? ReadPreference.Primary)
            .GetCollection<T>(GetCollectionName());

        public Task UpdateAsync(T entity)
        {
            Expression<Func<T, string>> func = f => f.Id;
            var value = (string)entity.GetType().GetProperty(func.Body.ToString()
                .Split(".")[1])?.GetValue(entity, null);
            var filter = Builders<T>.Filter.Eq(func, value);
            return Collection.ReplaceOneAsync(filter, entity);
        }

        private static string? GetCollectionName()
        {
            return (typeof(T).GetCustomAttributes(typeof(BsonCollectionAttribute), inherit: true)
                .FirstOrDefault() as BsonCollectionAttribute)?.CollectionName;
        }
    }
}

using Mongo.Migration.Documents.Attributes;
using Mongo.Migration.Documents.Locators;
using Mongo.Migration.Exceptions;
using Mongo.Migration.Startup;
using MongoDB.Driver;
using System.Linq;

namespace Mongo.Migration.Migrations.Database;

internal class StartUpDatabaseMigrationRunner : IStartUpDatabaseMigrationRunner
{
    private readonly IMongoClient _client;

    private readonly ICollectionLocator _collectionLocator;

    private readonly string _databaseName;

    private readonly IDatabaseMigrationRunner _migrationRunner;

    public StartUpDatabaseMigrationRunner(
        IMongoMigrationSettings settings,
        ICollectionLocator collectionLocator,
        IDatabaseMigrationRunner migrationRunner)
        : this(
            collectionLocator,
            migrationRunner)
    {
        if (settings.Database is null || (settings.ConnectionString is null && settings.ClientSettings is null && _client is null))
        {
            throw new MongoMigrationNoMongoClientException();
        }

        this._databaseName = settings.Database;

        if (settings.ConnectionString is null && settings.ClientSettings is null && _client is not null)
        {
            return;
        }

        if (settings.ClientSettings is not null)
        {
            this._client = new MongoClient(settings.ClientSettings);
        }
        else
        {
            this._client = new MongoClient(settings.ConnectionString);
        }
    }

    public StartUpDatabaseMigrationRunner(
        IMongoClient client,
        IMongoMigrationSettings settings,
        ICollectionLocator collectionLocator,
        IDatabaseMigrationRunner migrationRunner)
        : this(
            settings,
            collectionLocator,
            migrationRunner)
    {
        this._client = client;
    }

    private StartUpDatabaseMigrationRunner(
        ICollectionLocator collectionLocator,
        IDatabaseMigrationRunner migrationRunner)
    {
        this._collectionLocator = collectionLocator;
        this._migrationRunner = migrationRunner;
    }

    public void RunAll()
    {
        var locations = this._collectionLocator.GetLocatesOrEmpty().ToList();
        var information = locations.FirstOrDefault().Value;
        var databaseName = this.GetDatabaseOrDefault(information);

        this._migrationRunner.Run(this._client.GetDatabase(databaseName));
    }

    private string GetDatabaseOrDefault(CollectionLocationInformation information)
    {
        if (string.IsNullOrEmpty(this._databaseName) && string.IsNullOrEmpty(information.Database))
        {
            throw new NoDatabaseNameFoundException();
        }

        return string.IsNullOrEmpty(information.Database) ? this._databaseName : information.Database;
    }
}
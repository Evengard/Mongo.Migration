using Mongo.Migration.Documents.Serializers;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using NUnit.Framework;
using Serilog;

namespace Mongo.Migration.Test.Migrations.Database;

[SetUpFixture]
public class DatabaseMigrationRunnerSetup
{
    private readonly ILogger _logger = Log.ForContext<DatabaseMigrationRunnerSetup>();

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        try
        {
            var documentSerializaer = new DocumentVersionSerializer();
            BsonSerializer.RegisterSerializer(documentSerializaer.ValueType, documentSerializaer);
        }
        catch (BsonSerializationException ex)
        {
            this._logger.Warning(ex, "Serialization registration failure");
        }
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        // Do logout here
    }
}
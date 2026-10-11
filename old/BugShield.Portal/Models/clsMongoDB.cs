using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using BugShield.Portal.Properties;
using MongoDB.Driver.Builders;

namespace BugShield.Portal.Models
{
    public class clsMongoDB
    {
    //    private readonly MongoServer _server;
    //    private readonly MongoDatabaseSettings _settings;
    //    private readonly DatabaseNamespace _namespace;

    //    protected static IMongoClient _client;
    //    protected static IMongoDatabase _database;
    //    public IQueryable<clsMongoDB> GetAllRoles()
    //    {
    //        try
    //        {
    //            var db = this.DatabaseConnector.GetDatabase("Commerce");
    //            var roles = db.GetCollection<Role>("Roles");
    //            return roles.AsQueryable();
    //        }
    //        catch (Exception ex)
    //        {
    //            System.Diagnostics.Debug.WriteLine(ex.Message);
    //            throw;
    //        }
    //    }
    //    public clsMongoDB GetByID  (string id)
    //    {
              
    //    _client = new IMongoClient(mongoClientSettings);


    //        _database = _client.GetDatabase("bugshield");
    //        var query = Query<clsMongoDB>.EQ(e => e.Id, id);
    //        var speaker = _database.GetCollection<Speaker>("speakers").FindOne(query);
    //    }
    }
}
using GymManagmentSystem.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace GymManagmentSystem.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IConfiguration _configuration;

        public MongoDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = configuration.GetValue<string>("MongoDB:ConnectionString");
            var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName");
            
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            
            // Create indexes for unique constraints
            CreateIndexes();
        }

        public IMongoCollection<Enquiry> Enquiries => _database.GetCollection<Enquiry>("Enquiries");
        public IMongoCollection<EnquiryHistory> EnquiryHistories => _database.GetCollection<EnquiryHistory>("EnquiryHistories");
        public IMongoCollection<MembershipPlan> MembershipPlans => _database.GetCollection<MembershipPlan>("MembershipPlans");
        public IMongoCollection<MembersMembership> MembersMemberships => _database.GetCollection<MembersMembership>("MembersMemberships");
        public IMongoCollection<Activity> Activities => _database.GetCollection<Activity>("Activities");
        public IMongoCollection<PaymentReceipt> PaymentReceipts => _database.GetCollection<PaymentReceipt>("PaymentReceipts");

        private void CreateIndexes()
        {
            // Create unique index on Enquiry email and phone
            var enquiryIndexEmail = Builders<Enquiry>.IndexKeys.Ascending(e => e.Email);
            var enquiryIndexPhone = Builders<Enquiry>.IndexKeys.Ascending(e => e.Phone);
            
            Enquiries.Indexes.CreateOne(new CreateIndexModel<Enquiry>(
                enquiryIndexEmail, 
                new CreateIndexOptions { Unique = true, Sparse = true }
            ));
            
            Enquiries.Indexes.CreateOne(new CreateIndexModel<Enquiry>(
                enquiryIndexPhone, 
                new CreateIndexOptions { Unique = true, Sparse = true }
            ));
            
            // Create index on EnquiryId for faster lookups
            var enquiryIdIndex = Builders<Enquiry>.IndexKeys.Ascending(e => e.EnquiryId);
            Enquiries.Indexes.CreateOne(new CreateIndexModel<Enquiry>(enquiryIdIndex));
            
            // Create index on MembershipPlanId for faster lookups
            var planIdIndex = Builders<MembershipPlan>.IndexKeys.Ascending(p => p.MembershipPlanId);
            MembershipPlans.Indexes.CreateOne(new CreateIndexModel<MembershipPlan>(planIdIndex));
            
            // Create index on MembersMembershipId for faster lookups
            var membershipIdIndex = Builders<MembersMembership>.IndexKeys.Ascending(m => m.MembersMembershipId);
            MembersMemberships.Indexes.CreateOne(new CreateIndexModel<MembersMembership>(membershipIdIndex));
            
            // Create index on ActivityId for faster lookups
            var activityIdIndex = Builders<Activity>.IndexKeys.Ascending(a => a.ActivityId);
            Activities.Indexes.CreateOne(new CreateIndexModel<Activity>(activityIdIndex));
            
            // Create index on PaymentReceiptId for faster lookups
            var receiptIdIndex = Builders<PaymentReceipt>.IndexKeys.Ascending(p => p.PaymentReceiptId);
            PaymentReceipts.Indexes.CreateOne(new CreateIndexModel<PaymentReceipt>(receiptIdIndex));
        }
        
        // Helper method to get next sequential ID
        public int GetNextSequenceValue(string collectionName)
        {
            var collection = _database.GetCollection<Counter>("Counters");
            var filter = Builders<Counter>.Filter.Eq(c => c.Id, collectionName);
            var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
            var options = new FindOneAndUpdateOptions<Counter>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            
            var counter = collection.FindOneAndUpdate(filter, update, options);
            return counter.SequenceValue;
        }
    }
    
    // Counter class for auto-incrementing IDs
    public class Counter
    {
        public string Id { get; set; }
        public int SequenceValue { get; set; }
    }
}


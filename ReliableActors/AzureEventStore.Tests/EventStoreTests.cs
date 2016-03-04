namespace AzureEventStore.Tests
{
    using System;
    using System.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class EventStoreTests
    {
        private EventStore<IEvent> event_store;
        private readonly string document_db_connection_address = ConfigurationManager.AppSettings["document_db_connection_address"];
        private readonly string document_db_connection_key = ConfigurationManager.AppSettings["document_db_connection_key"];
        private readonly string database_id = Guid.NewGuid().ToString("D");
        private readonly string collection_id = Guid.NewGuid().ToString("D");

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            var client = new DocumentClient(new Uri(document_db_connection_address), document_db_connection_key);
            await client.DeleteDatabaseAsync($"dbs/{database_id}");
        }

        [SetUp]
        public void Setup()
        {
            event_store = new EventStore<IEvent>(
                new DocumentDbConfiguration(
                    new Uri(document_db_connection_address),
                    document_db_connection_key,
                    database_id,
                    collection_id));

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Binder = new TypeNameSerialisationBinder("AzureEventStore.Tests.{0}, AzureEventStore.Tests"),
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        [Test]
        public async Task Create_New_Stream()
        {
            var id = Guid.NewGuid();

            await event_store.AppendToStream(id, new List<IEvent> { new Event(Guid.NewGuid(), "Test") });
        }

        [Test]
        public async Task Update_Existing_Stream()
        {
            var id = Guid.NewGuid();

            await event_store.AppendToStream(id, new List<IEvent> { new Event(Guid.NewGuid(), "Test") });

            await event_store.AppendToStream(id, new List<IEvent> { new SomeOtherEvent(Guid.NewGuid(), DateTime.Now, 5473) });
        }

        [Test]
        public async Task Read_Stream()
        {
            var id = Guid.NewGuid();

            await event_store.AppendToStream(
                id,
                new List<IEvent>
                {
                    new Event(Guid.NewGuid(), "Test"),
                    new SomeOtherEvent(Guid.NewGuid(), DateTime.Now, new Random().Next())
                });

            var stream = await event_store.ReadStream(id);

            Assert.That(stream.Count(), Is.EqualTo(2));
            Assert.That(stream.Id == id);
        }
    }
}
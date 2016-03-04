﻿namespace AzureEventStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public class EventStore<T> : IEventStore<T>
    {
        private readonly DocumentDbConfiguration document_db_configuration;
        private bool database_initialised;

        public EventStore(DocumentDbConfiguration documentDbConfiguration)
        {
            if (documentDbConfiguration == null)
                throw new ArgumentNullException(nameof(documentDbConfiguration));

            document_db_configuration = documentDbConfiguration;
        }

        public async Task AppendToStream(Guid streamId, IEnumerable<T> events)
        {
            var client = new DocumentClient(document_db_configuration.EndpointAddress, document_db_configuration.AuthorisationKey);
            await InitialiseStore(client);

            var stream = ReadStream(streamId, client);

            var updatedStream = new EventStream<T>(stream.Id, stream.Concat(events));

            await client.UpsertDocumentAsync($"dbs/{document_db_configuration.DatabaseId}/colls/{document_db_configuration.CollectionId}", updatedStream);
        }

        public async Task<EventStream<T>> ReadStream(Guid streamId)
        {
            var client = new DocumentClient(document_db_configuration.EndpointAddress, document_db_configuration.AuthorisationKey);
            await InitialiseStore(client);

            return ReadStream(streamId, client);
        }

        private EventStream<T> ReadStream(Guid streamId, DocumentClient client)
        {

            var r = client
                .CreateDocumentQuery<EventStream<T>>(
                    $"dbs/{document_db_configuration.DatabaseId}/colls/{document_db_configuration.CollectionId}")
                .Where(x => x.Id == streamId)
                .AsEnumerable()
                .FirstOrDefault();

            return r ?? new EventStream<T>(streamId, new List<T>());
        }

        private async Task InitialiseStore(DocumentClient client)
        {
            if (database_initialised)
                return;

            await InitialiseDatabase(client);
            await InitialiseCollection(client);

            database_initialised = true;
        }

        private async Task InitialiseDatabase(DocumentClient client)
        {
            var database = client.CreateDatabaseQuery().Where(db => db.Id == document_db_configuration.DatabaseId).AsEnumerable().FirstOrDefault();
            if (database == null)
                await client.CreateDatabaseAsync(new Database { Id = document_db_configuration.DatabaseId });
        }

        private async Task InitialiseCollection(DocumentClient client)
        {
            var collection = client.CreateDocumentCollectionQuery($"dbs/{document_db_configuration.DatabaseId}").Where(c => c.Id == document_db_configuration.CollectionId).AsEnumerable().FirstOrDefault();
            if (collection == null)
                await client.CreateDocumentCollectionAsync($"dbs/{document_db_configuration.DatabaseId}", new DocumentCollection { Id = document_db_configuration.CollectionId });
        }
    }
}

﻿using Fstab.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace Raml.Tools.Tests
{
    [TestFixture]
    public class ComplexJsonSchemaTests
    {
        [Test]
        public async Task ShouldDeserializeContent()
        {
            var content = new StringContent("{ storage: { remotePath : 'path', server : 'server' } }",
                Encoding.UTF8,
                "application/json");

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = content;

            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("http://localhost");

            var proxy = new Fstab.FstabApi(client);
            proxy.SchemaValidation.RaiseExceptions = false;

            var entries = await proxy.Entries.Get();

            var formatters = new List<MediaTypeFormatter>();
            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SerializerSettings.Converters.Add(new JsonDeviceConverter());
            formatters.Add(jsonFormatter);
            
            entries.Formatters = formatters;

            var device = entries.Content.Storage;

            Assert.IsNotNull(device);
        }

        [Test]
        [Ignore]
        public async Task ShouldValidateSchema()
        {
            var content = new StringContent("{ storage: { remotePath : 'path', server : 'server' } }",
                Encoding.UTF8,
                "application/json");

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = content;

            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("http://localhost");

            var proxy = new Fstab.FstabApi(client);
            proxy.SchemaValidation.RaiseExceptions = false;

            var entries = await proxy.Entries.Get();
            
            Assert.IsTrue(entries.IsValid.Value);
        }

        class FakeHttpMessageHandler : HttpMessageHandler
        {
            HttpResponseMessage response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                this.response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                System.Threading.CancellationToken cancellationToken)
            {
                var tcs = new TaskCompletionSource<HttpResponseMessage>();

                tcs.SetResult(response);

                return tcs.Task;
            }
        }

        public abstract class JsonCreationConverter<T> : JsonConverter
        {
            protected abstract T Create(Type objectType, JObject jsonObject);

            public override bool CanConvert(Type objectType)
            {
                return typeof(T).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType,
              object existingValue, JsonSerializer serializer)
            {
                var jsonObject = JObject.Load(reader);
                var target = Create(objectType, jsonObject);
                serializer.Populate(jsonObject.CreateReader(), target);
                return target;
            }

            public override void WriteJson(JsonWriter writer, object value,
                JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public class JsonDeviceConverter : JsonCreationConverter<Storage>
        {
            protected override Storage Create(Type objectType, JObject jsonObject)
            {
                return new Storagenfs();
            }
        }



    }
}

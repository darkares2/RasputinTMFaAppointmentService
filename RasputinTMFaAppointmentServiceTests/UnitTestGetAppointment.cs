using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rasputin.TM;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace RasputinTMFaAppointmentServiceTests {
    public class UnitGetAppointment
    {
        [Fact]
        public async Task TestGetByUserIDNotFound()
        {
             var query = new Dictionary<String, StringValues>();
            query.TryAdd("UserID", Guid.NewGuid().ToString());
            var reqMock = new Mock<HttpRequest>();
            reqMock.Setup(req => req.Query).Returns(new QueryCollection(query));
            var logger = Mock.Of<ILogger>();
            OkObjectResult result = (OkObjectResult)await GetAppointment.Run(reqMock.Object, null, logger);
            Assert.Equal("null", result.Value);
        }

        [Fact]
        public async Task TestGetByUserID()
        {
            Guid userID = Guid.NewGuid();
            var query = new Dictionary<String, StringValues>();
            query.TryAdd("UserID", userID.ToString());
            var reqMock = new Mock<HttpRequest>();
            reqMock.Setup(req => req.Query).Returns(new QueryCollection(query));
            var logger = Mock.Of<ILogger>();
            var cloudTable = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            var mockResult = new Mock<TableQuerySegment<DynamicTableEntity>>();
            Appointment entry = new Appointment();
            entry.RowKey = Guid.NewGuid().ToString();
            entry.UserID = userID;

            List<Appointment> entries = new List<Appointment>();
            entries.Add(entry);
            var ctor = typeof(TableQuerySegment<Appointment>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            var mockQuerySegment = ctor.Invoke(new object[] { new List<Appointment>(entries) }) as TableQuerySegment<Appointment>;


            //MethodInfo setTokenMethod = typeof(TableQuerySegment<Appointment>).GetMethod("set_ContinuationToken", BindingFlags.NonPublic | BindingFlags.Instance);

            //var continuationToken = new TableContinuationToken();
            //setTokenMethod.Invoke(mockQuerySegment, new object[] { continuationToken });

            cloudTable.Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<Appointment>>(), It.IsAny<TableContinuationToken>()))
                                .Returns(Task.FromResult(mockQuerySegment));

            OkObjectResult result = (OkObjectResult)await GetAppointment.Run(reqMock.Object, cloudTable.Object, logger);
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject[]>((string)result.Value, converter);
            Assert.Equal(userID.ToString(), obj[0].UserID.ToString());
        }
    }
}
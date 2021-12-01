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
    public class GetAppointmentTests {
        [Fact]
        public async Task TestGetByUserIDNotFound()
        {
             var query = new Dictionary<String, StringValues>();
            query.TryAdd("UserID", Guid.NewGuid().ToString());
            var reqMock = new Mock<HttpRequest>();
            reqMock.Setup(req => req.Query).Returns(new QueryCollection(query));
            var logger = Mock.Of<ILogger>();
            OkObjectResult result = (OkObjectResult)await GetAppointment.Run(reqMock.Object, null, logger);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("null", result.Value);
        }

        [Fact]
        public async Task TestGetByUserID()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            Guid userID = Guid.NewGuid();
            var qs = new Dictionary<string, StringValues>
            {
                { "UserID", userID.ToString() }
            };
            request.Query = new QueryCollection(qs);

            var iLoggerMock = new Mock<ILogger>();
            var tblAppointmentMock = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            Appointment appointment1 = new Appointment() { RowKey = Guid.NewGuid().ToString(), UserID = userID,  ServiceID = Guid.NewGuid(), SlotUserID = Guid.NewGuid(), Timeslot = DateTime.Now, Open = true };
            Appointment appointment2 = new Appointment() { RowKey = Guid.NewGuid().ToString(), UserID = userID, ServiceID = Guid.NewGuid(), SlotUserID = Guid.NewGuid(), Timeslot = DateTime.Now.AddDays(10), Open = true };
            List<Appointment> appointments = new List<Appointment>() { appointment1, appointment2 };
            var resultMock = new Mock<TableQuerySegment<Appointment>>(appointments);
            tblAppointmentMock.Setup(_ => _.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<Appointment>>(), It.IsAny<TableContinuationToken>())).ReturnsAsync(resultMock.Object);

            // Act
            OkObjectResult result = (OkObjectResult)await GetAppointment.Run(request, tblAppointmentMock.Object, iLoggerMock.Object);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Appointment[] appointmentResult = (Appointment[])JsonConvert.DeserializeObject((string)result.Value, typeof(Appointment[]));
            Assert.Equal(2, appointmentResult.Length);
            Assert.Equal(appointment1.UserID, appointmentResult[0].UserID);
            Assert.Equal(appointment2.UserID, appointmentResult[1].UserID);
        }

        [Fact]
        public async Task TestGetBySlotUserID()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            Guid slotUserID = Guid.NewGuid();
            var qs = new Dictionary<string, StringValues>
            {
                { "SlotUserID", slotUserID.ToString() }
            };
            request.Query = new QueryCollection(qs);

            var iLoggerMock = new Mock<ILogger>();
            var tblAppointmentMock = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            Appointment appointment1 = new Appointment() { RowKey = Guid.NewGuid().ToString(), UserID = Guid.NewGuid(), ServiceID = Guid.NewGuid(), SlotUserID = slotUserID, Timeslot = DateTime.Now, Open = true };
            Appointment appointment2 = new Appointment() { RowKey = Guid.NewGuid().ToString(), UserID = Guid.NewGuid(), ServiceID = Guid.NewGuid(), SlotUserID = slotUserID, Timeslot = DateTime.Now.AddDays(10), Open = true };
            List<Appointment> appointments = new List<Appointment>() { appointment1, appointment2 };
            var resultMock = new Mock<TableQuerySegment<Appointment>>(appointments);
            tblAppointmentMock.Setup(_ => _.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<Appointment>>(), It.IsAny<TableContinuationToken>())).ReturnsAsync(resultMock.Object);

            // Act
            OkObjectResult result = (OkObjectResult)await GetAppointment.Run(request, tblAppointmentMock.Object, iLoggerMock.Object);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Appointment[] appointmentResult = (Appointment[])JsonConvert.DeserializeObject((string)result.Value, typeof(Appointment[]));
            Assert.Equal(2, appointmentResult.Length);
            Assert.Equal(appointment1.SlotUserID, appointmentResult[0].SlotUserID);
            Assert.Equal(appointment2.SlotUserID, appointmentResult[1].SlotUserID);
        }

        [Fact]
        public async Task TestGetByAppointmentID()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;
            Guid appointmentID = Guid.NewGuid();
            var qs = new Dictionary<string, StringValues>
            {
                { "AppointmentID", appointmentID.ToString() }
            };
            request.Query = new QueryCollection(qs);

            var iLoggerMock = new Mock<ILogger>();
            var tblAppointmentMock = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            Appointment appointment2 = new Appointment() { RowKey = appointmentID.ToString(), UserID = Guid.NewGuid(), ServiceID = Guid.NewGuid(), SlotUserID = Guid.NewGuid(), Timeslot = DateTime.Now.AddDays(10), Open = true };
            List<Appointment> appointments = new List<Appointment>() { appointment2 };
            TableResult tableResult = new TableResult();
            tableResult.Result = appointment2;
            tableResult.HttpStatusCode = 200;
            tblAppointmentMock.Setup(_ => _.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(tableResult);

            // Act
            OkObjectResult result = (OkObjectResult)await GetAppointment.Run(request, tblAppointmentMock.Object, iLoggerMock.Object);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Appointment appointmentResult = (Appointment)JsonConvert.DeserializeObject((string)result.Value, typeof(Appointment));
            Assert.Equal(appointment2.AppointmentID, appointmentResult.AppointmentID);
        }

    }
}
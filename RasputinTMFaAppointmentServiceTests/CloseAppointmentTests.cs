using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rasputin.TM;
using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace RasputinTMFaAppointmentServiceTests
{
    public class CloseAppointmentTests {
        private Stream Serialize(object value)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            return new MemoryStream(Encoding.Default.GetBytes(jsonString));
        }

        [Fact]
        public async Task CloseAppointmentTest()
        {
            // Arrange
            AppointmentCloseRequest appointmentCloseRequest = new AppointmentCloseRequest() { AppointmentID = Guid.NewGuid() };
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Body = Serialize(appointmentCloseRequest);
            var logger = Mock.Of<ILogger>();
            var tblAppointmentMock = new Mock<CloudTable>(new Uri("http://localhost"), new StorageCredentials(accountName: "blah", keyValue: "blah"), (TableClientConfiguration)null);
            Appointment appointment2 = new Appointment() { RowKey = appointmentCloseRequest.AppointmentID.ToString(), UserID = Guid.NewGuid(), ServiceID = Guid.NewGuid(), SlotUserID = Guid.NewGuid(), Timeslot = DateTime.Now.AddDays(10), Open = true, ETag = "*" };
            List<Appointment> appointments = new List<Appointment>() { appointment2 };
            TableResult tableResult = new TableResult();
            tableResult.Result = appointment2;
            tableResult.HttpStatusCode = 200;
            tblAppointmentMock.Setup(_ => _.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(tableResult);

//            TableOperation operation = null;
  //          tblAppointmentMock.Setup(_ => _.ExecuteAsync(It.IsAny<TableOperation>()))
    //                .Callback<TableOperation>((obj) => operation = obj);

            // Act
            OkObjectResult result = (OkObjectResult)await CloseAppointment.Run(request, tblAppointmentMock.Object, logger);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Appointment appointmentResult = (Appointment)JsonConvert.DeserializeObject((string)result.Value, typeof(Appointment));

            Assert.Equal(appointmentCloseRequest.AppointmentID, appointmentResult.AppointmentID);
            Assert.False(appointmentResult.Open);
        }


    }
}
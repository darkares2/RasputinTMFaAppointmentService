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
using System.IO;
using System.Text;

namespace RasputinTMFaAppointmentServiceTests {
    public class CreateAppointmentTests {
        private Stream Serialize(object value)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            return new MemoryStream(Encoding.Default.GetBytes(jsonString));
        }

        [Fact]
        public async Task CreateAppointmentSimple()
        {
            // Arrange
            AppointmentCreateRequest appointmentCreateRequest = new AppointmentCreateRequest() { ServiceID = Guid.NewGuid(), SlotID = Guid.NewGuid(), UserID = Guid.NewGuid() };
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Body = Serialize(appointmentCreateRequest);
            var logger = Mock.Of<ILogger>();

            string queueAdd = "";
            var queueMock = new Mock<IAsyncCollector<string>>();
            queueMock.Setup(_ => _.AddAsync(It.IsAny<string>(), default)).Callback<string, System.Threading.CancellationToken>( (x, c) => queueAdd = x);

            // Act
            OkObjectResult result = (OkObjectResult)await CreateAppointment.Run(request, queueMock.Object, logger);

            // Assert
            Assert.Equal(200, result.StatusCode);
            AppointmentCreateRequest appointmentResult = (AppointmentCreateRequest)JsonConvert.DeserializeObject((string)result.Value, typeof(AppointmentCreateRequest));
            AppointmentCreateRequest appointmentQueued = (AppointmentCreateRequest)JsonConvert.DeserializeObject(queueAdd, typeof(AppointmentCreateRequest));
            Assert.Equal(appointmentCreateRequest.ServiceID, appointmentQueued.ServiceID);
            Assert.Equal(appointmentCreateRequest.SlotID, appointmentQueued.SlotID);
            Assert.Equal(appointmentCreateRequest.UserID, appointmentQueued.UserID);
            Assert.Equal(appointmentCreateRequest.ServiceID, appointmentResult.ServiceID);
            Assert.Equal(appointmentCreateRequest.SlotID, appointmentResult.SlotID);
            Assert.Equal(appointmentCreateRequest.UserID, appointmentResult.UserID);
        }


    }
}
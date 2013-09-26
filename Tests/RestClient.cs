using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{

    using NUnit.Framework;

    [TestFixture]
    public class RestClientTest
    {

        [Test]
        public void Put()
        {
            TaskClient.Clients.HttpRest restService = new TaskClient.Clients.HttpRest();
            TaskClient.Clients.ApiResult result = restService.ApiEnqueue(new Dictionary<string, object> { { "MType", "testChannel" } });

            Assert.IsTrue(result == TaskClient.Clients.ApiResult.MessageRejected);// rejected 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{

    using NUnit.Framework;

    //[TestFixture]
    //public class RestClientTest
    //{

    //    [Test]
    //    public void Put()
    //    {
    //        TaskClient.Clients.HttpRest restService = new TaskClient.Clients.HttpRest();
    //        TaskClient.Clients.ApiResult result = restService.ApiEnqueue(new Dictionary<string, object> { { "MType", "testChannel" } });

    //        Assert.IsTrue(result == TaskClient.Clients.ApiResult.MessageRejected);// rejected 
    //    }
    //    [Test]
    //    public void GetValidation()
    //    {
    //        TaskClient.Clients.HttpRest restService = new TaskClient.Clients.HttpRest();
    //        TaskQueue.RepresentedModel result = restService.GetValidationInfo("EMail");// assume this default plugin initiated
    ////result.schema.val2[0].Inherited
    //        Assert.IsTrue(result.schema.val1.Contains("To"));
    //        Assert.IsTrue(result.schema.val2[result.schema.val1.IndexOf("To")].Required);
    //    }
    //}
}

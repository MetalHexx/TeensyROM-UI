//namespace TeensyRom.Api.Tests.Integration.Common
//{
//    public class RadTestException : Exception
//    {
//        public HttpResponseMessage? HttpResponse { get; set; }

//        public RadTestException(string message) : base(message) { }

//        public RadTestException(string stringResponse, HttpResponseMessage httpResponse, Exception innerException)
//            : base($"\r\nProblem deserializing endpoint response.\r\nStatus Code: {httpResponse.StatusCode}\r\nReason Phrase: {httpResponse.ReasonPhrase}\r\nResponse Body: {stringResponse}", innerException)
//        {
//            HttpResponse = httpResponse;
//        }
//    }
//}

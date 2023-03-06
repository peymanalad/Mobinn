// See https://aka.ms/new-console-template for more information

using RestSharp;

var client = new RestClient("http://188.0.240.110/api/select");
var request = new RestRequest();
request.AddHeader("cache-control", "no-cache");
request.AddHeader("Content-Type", "application/json");
request.AddParameter("undefined", "{\"op\" : \"pattern\"" +
    ",\"user\" : \"09121997874\"" +
    ",\"pass\":  \"faraz0013922122\"" +
    ",\"fromNum\" : \"+98EVENT\"" +
    ",\"toNum\": \"09122800039\"" +
    ",\"patternCode\": \"3kjs7hwa0i79dj7\"" +
    ",\"inputData\" : [{\"code\": \"123456\"}]}"
    , ParameterType.RequestBody);
var response = client.Post(request);
Console.WriteLine(response);
namespace Cirrus.Base.Primitives;

public class HttpClientConsumer
{
    public required string HttpClientName { get; init; }
    public required Action<HttpClient> ClientConfiguration { get; init; }
    
    public HttpClient CreateClient(IHttpClientFactory factory) => factory.CreateClient(HttpClientName);
}
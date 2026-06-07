using System.Text.Json;
using Cirrus.Models.Network.Response;
using Cirrus.Models.Network.Response.Advanced;
using Cirrus.Network.EncryptionHandlers;
using Cirrus.Network.Primitives;
using BatchApiParameter = (string ContainerRoute, string ApiRoute, object? Parameter)[];
using NetworkSerializationContext = Cirrus.Network.Serialization.NetworkSerializationContext;

namespace Cirrus.Network.Api;

sealed partial class Advanced
{
    /// <summary>
    /// Sends multiple requests at once.
    /// API Route: /api/advanced/batch.
    /// </summary>
    // [MusicApi("batch")]
    // We do not allow a batch API to include another batch API. Thus, this API is not exposed to Reflection.
    internal MusicApi<Dictionary<string, string>, BatchApiResponse> BatchApi => field ??= new(
        $"{RouteBase}/batch",
        HttpMethod.Post,
        new()
        {
            EncryptionHandler = EapiHandler.Current,
            Url = "/api/batch"
        },
        static _ => $"{Constants.InterfaceRequestBase}/eapi/batch",
        static p =>
        {
            Dictionary<string, object> body = new() { ["e_r"] = true };
            foreach (var (url, requestBody) in p)
            {
                body[url] = requestBody;
            }
            return body;
        }
    );

    /// <summary>
    /// Sends multiple requests at once.
    /// </summary>
    /// <param name="requests">Requests to send.</param>
    /// <returns>Responses of the requests.</returns>
    public async Task<MusicApiResponse?[]> SendMultipleRequestsAsync(BatchApiParameter requests)
    {
        Dictionary<string, string> requestData = [];
        List<(string Url, Type ResponseType)?> requestUrls = [];
        foreach (var (containerRoute, apiRoute, parameter) in requests)
        {
            var api = Client.GetApiFromPath(containerRoute, apiRoute);
            switch (api)
            {
                case IFixedMusicApi fma:
                {
                    var requestUrl = fma.Url;
                    var requestBody = fma.Body;
                    if (!requestUrl.StartsWith($"{Constants.RequestBase}/api/")) continue;
                    requestUrl = requestUrl.Replace(Constants.RequestBase, string.Empty) + new string('/', requestUrls.Count);
                    requestData[requestUrl] = JsonSerializer.Serialize<Dictionary<string, object>>(requestBody,
                        NetworkSerializationContext.Default.DictionaryStringObject);
                    requestUrls.Add((requestUrl, fma.ResponseType));
                    break;
                }
                case IDynamicMusicApi dma:
                {
                    if (parameter is null || !dma.ParameterType.IsEquivalentTo(parameter.GetType())) continue;
                    var requestUrl = dma.UrlParser(parameter);
                    var requestBody = dma.BodyParser?.Invoke(parameter) ?? new();
                    requestUrl = requestUrl.Replace(Constants.RequestBase, string.Empty) + new string('/', requestUrls.Count);
                    requestData[requestUrl] = JsonSerializer.Serialize<Dictionary<string, object>>(requestBody,
                        NetworkSerializationContext.Default.DictionaryStringObject);
                    requestUrls.Add((requestUrl, dma.ResponseType));
                    break;
                }
                default:
                {
                    requestUrls.Add(null);
                    break;
                }
            }
        }
        var response = await BatchApi.RequestAsync(requestData);
        return requestUrls.Select(r => r is { } rr ? response.GetResponse(rr.Url, rr.ResponseType) : null).ToArray();
    }
}
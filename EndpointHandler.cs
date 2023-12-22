using System.Text.Json;

namespace DotnetRedfish;

public class EndpointHandler
{
    private readonly string _rootPath;
    private readonly ILogger<EndpointHandler> _logger;

    public EndpointHandler(string? rootPath, ILogger<EndpointHandler> logger)
    {
        _logger = logger;
        _rootPath = Directory.GetCurrentDirectory() + '/' + rootPath;
    }

    public async Task Handle(HttpContext context)
    {
        if (context.Request.Path.Value is null)
        {
            _logger.LogError("Get null request");
            context.Response.StatusCode = 404;
            return;
        }

        string relativePath = context.Request.Path.Value.Replace("/redfish/v1", string.Empty);
        if (!relativePath.EndsWith('/'))
        {
            relativePath += '/';
        }

        string fullPath = _rootPath + relativePath;

        string? content = await GetContentByPath(fullPath);
        if (content is null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        Dictionary<string, string>? headers = await TryGetHeaders(fullPath);
        if (headers is not null)
        {
            foreach (KeyValuePair<string, string> item in headers)
            {
                if (!item.Key.Equals("content-length", StringComparison.CurrentCultureIgnoreCase) ||
                    !item.Key.Equals("transfer-encoding", StringComparison.CurrentCultureIgnoreCase))
                {
                    context.Response.Headers.TryAdd(item.Key, item.Value);
                }
            }
        }
        else
        {
            context.Response.ContentType = "application/json";
        }

        await context.Response.WriteAsync(content);
    }

    private Task<string?> GetContentByPath(string fullPath)
    {
        string contentPath = fullPath + "index.json";
        
        if (!File.Exists(contentPath))
        {
            _logger.LogError("Not found content on {Path}", contentPath);
            return Task.FromResult<string?>(null);
        }

        return File.ReadAllTextAsync(contentPath);
    }

    public async Task<Dictionary<string, string>?> TryGetHeaders(string fullPath)
    {
        string headersPath = fullPath + "headers.json";
        if (File.Exists(headersPath))
        {
            string rawHeaders = await File.ReadAllTextAsync(headersPath);

            try
            {
                Headers? headersObject = JsonSerializer.Deserialize(rawHeaders, AppSerializerContext.Default.Headers);
                if (headersObject is not null)
                {
                    return headersObject.Get;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ex while parse headers on {Path}", headersPath);
            }
        }

        return null;
    }
}

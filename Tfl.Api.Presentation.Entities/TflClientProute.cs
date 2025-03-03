using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tfl.Api.Presentation.Entities;

public static class TflConfiguration
{
    public static string AppId { get; set; }
    public static string AppKey { get; set; }
}

public partial class LineClient
{
    partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
    {
        urlBuilder.Append("&");
        urlBuilder.Append("app_id");
        urlBuilder.Append("=");
        urlBuilder.Append(TflConfiguration.AppId);
        urlBuilder.Append("&");
        urlBuilder.Append("app_key");
        urlBuilder.Append("=");
        urlBuilder.Append(TflConfiguration.AppKey);
    }
}

public partial class StopPointClient
{
    partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
    {
        urlBuilder.Append("?");
        urlBuilder.Append("app_id");
        urlBuilder.Append("=");
        urlBuilder.Append(TflConfiguration.AppId);
        urlBuilder.Append("&");
        urlBuilder.Append("app_key");
        urlBuilder.Append("=");
        urlBuilder.Append(TflConfiguration.AppKey);
    }
}

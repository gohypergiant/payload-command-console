using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE.Rest
{
    public sealed class HttpResponse
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string Content { get; private set; }
        public HttpHeaders Headers { get; } = new HttpHeaders();

        // onl;y used internally when a file response is built
        private FileInfo FileInfo { get; set; }

        public HttpResponse(HttpStatusCode statusCode, string content = null)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public string ContentType
        {
            get => Headers.ContentType;
            set => Headers.ContentType = value;
        }

        public void Send(HttpContext context)
        {
            if (this.FileInfo != null && !this.FileInfo.Exists)
            {
                this.Content = "File not found";
                this.StatusCode = HttpStatusCode.NotFound;
                this.FileInfo = null;
            }

            SendHeaderBlock(context);
            SendContent(context);
        }

        private void SendHeaderBlock(HttpContext context)
        {
            // status header
            var message = $"HTTP/1.1 {(int)StatusCode}\r\n";

            // general header
            Headers["Date"] = DateTime.Now.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss zzzz");

            // content header
            long length;

            if (this.FileInfo != null)
            {
                length = this.FileInfo.Length;
            }
            else
            {
                length = Content.Length;
            }
            Headers["Content-Length"] = length.ToString();

            foreach (var h in Headers)
            {
                message += $"{h.Key}: {h.Value}\r\n";
            }

            // content break
            message += "\r\n";

            context.Client.Client.Send(Encoding.ASCII.GetBytes(message));
        }

        private void SendContent(HttpContext context)
        {
            if (this.FileInfo != null)
            {
                context.Client.Client.SendFile(this.FileInfo.FullName);
            }
            else
            {
                context.Client.Client.Send(Encoding.ASCII.GetBytes(Content));
            }
        }

        public static HttpResponse OK(string content = "OK")
        {
            return new HttpResponse(HttpStatusCode.OK, content);
        }

        public static HttpResponse File(string filePath)
        {
            return File(new FileInfo(filePath));
        }

        public static HttpResponse File(string filePath, string contentType)
        {
            return File(new FileInfo(filePath), contentType);
        }

        public static HttpResponse File(FileInfo fileInfo, string contentType = "application/octet-stream")
        {
            return new HttpResponse(HttpStatusCode.OK)
            {
                FileInfo = fileInfo,
                ContentType = contentType
            };
        }

        public static HttpResponse Json<T>(T content, JsonSerializerOptions options = null)
        {
            if (options == null)
            {
                options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new JsonStringEnumConverter());
            }

            var json = JsonSerializer.Serialize(content, options);

            return new HttpResponse(HttpStatusCode.OK, json)
            {
                ContentType = "application/json"
            };
        }

        public static HttpResponse Created(string content = "Created")
        {
            return new HttpResponse(HttpStatusCode.Created, content);
        }

        public static HttpResponse NotFound(string content = "Not Found")
        {
            return new HttpResponse(HttpStatusCode.NotFound, content);
        }

        public static HttpResponse BadRequest(string content = "Bad Request")
        {
            return new HttpResponse(HttpStatusCode.BadRequest, content);
        }

        public static HttpResponse ServerError(string content = "Server Error")
        {
            return new HttpResponse(HttpStatusCode.ServerError, content);
        }
    }
}

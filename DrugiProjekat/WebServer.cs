using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using DrugiProjekat;

namespace DrugiProjekat
{
    internal class WebServer
    {
        private HttpListener listener;
        private string root;
        private string listenUrl;
        Cache ImageCache;

        public WebServer(string[] arg)
        {
            Console.WriteLine("Server thread started.");

            this.listenUrl = arg[0];
            this.root = arg[1];

            ImageCache = new Cache();
        }

        public async Task Start()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(listenUrl);
                listener.Start();
                Console.WriteLine("Server is listening.\n");

                while (listener.IsListening)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HandleRequest(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private async Task HandleRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                string url = request.Url.ToString();
                string ignore = $"{listenUrl}favicon.ico";
                if (url == ignore)
                {
                    return;
                }
                Console.WriteLine($"Received request at {url}.");

                HttpListenerResponse response = context.Response;

                if (request.HttpMethod != "GET")
                {
                    await HandleError(response, "method");
                }
                else
                {
                    string fileExt = Path.GetExtension(url);
                    string imageName = Path.GetFileName(url);
                    if (imageName == "")
                    {
                        await HandleError(response, "");
                    }
                    else
                    {
                        if (fileExt == ".jpg" || fileExt == ".jpeg" || fileExt == ".png")
                        {
                            await returnImage(response, imageName);
                            Console.WriteLine("Request succesfully processed.\n");
                        }
                        else
                        {
                            await HandleError(response, "type");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task HandleError(HttpListenerResponse res, string error)
        {
            string ret = "";
            if (error == "method")
            {
                //vraca se informacija o gresci
                ret = "<h2>Error - only GET request is valid.</h2>";
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                res.StatusDescription = "Bad request";
                Console.WriteLine("Error - only GET request is valid.\n");
            }
            else
            {
                if (error == "type")
                {
                    ret = "<h2>Error - invalid type of file requested.</h2>";
                    res.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    res.StatusDescription = "Not acceptable.";
                    Console.WriteLine("Error - invalid type of file requested.\n");
                }
                else
                {
                    if (error == "notfound")
                    {
                        ret = "<h2>Error - requested file is not available.</h2>";
                        res.StatusCode = (int)HttpStatusCode.NotFound;
                        res.StatusDescription = "Not found.";
                        Console.WriteLine("Error - requested file is not available.\n");

                    }
                    else
                    {
                        ret = "<h2>Error - bad request.</h2>";
                        res.StatusCode = (int)HttpStatusCode.BadRequest;
                        res.StatusDescription = "Bad request.";
                        Console.WriteLine("Error - bad request.\n");

                    }
                }
            }

            res.Headers.Set("Content-Type", "text/html");
            byte[] buf = Encoding.UTF8.GetBytes(ret);
            using Stream output = res.OutputStream;
            res.ContentLength64 = buf.Length;
            await output.WriteAsync(buf, 0, buf.Length);
            output.Close();
        }
        private async Task returnImage(HttpListenerResponse res, string imageName)
        {

            string path = Directory.GetFiles(root, imageName, SearchOption.AllDirectories).FirstOrDefault();

            if (path != null)
            {
                byte[] buf;

                //provera da li je u kesu

                if (ImageCache.GetImageFromCache(imageName, out buf))
                {
                    Console.WriteLine("Requested image is in cache.");
                }
                else
                {
                    buf = File.ReadAllBytes(path);
                    ImageCache.AddImageToCache(imageName, buf);
                    Console.WriteLine("Requested image is not in cache. Adding it to cache.");
                }

                res.StatusCode = (int)HttpStatusCode.OK;
                res.StatusDescription = "Status OK";
                res.Headers.Set("Content-Type", "image/jpg");
                using Stream output = res.OutputStream;
                res.ContentLength64 = buf.Length;
                await output.WriteAsync(buf, 0, buf.Length);
                output.Close();
            }
            else
            {
                await HandleError(res, "notfound");
            }

        }
    }
}

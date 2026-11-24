using System.Net;
using System.Text;

static class Program
{
    private static long _visitCount;
    private const string CountFile = "/dati/count.txt"; // 1. Percorso del file nel volume

    private static async Task Main()
    {
        Directory.CreateDirectory("/dati"); // 2. Crea la cartella se non esiste
        if (File.Exists(CountFile)) long.TryParse(File.ReadAllText(CountFile), out _visitCount); // 3. Legge il vecchio numero all'avvio

        using var listener = new HttpListener();
        listener.Prefixes.Add("http://*:8080/");
        listener.Start();

        Console.WriteLine("Listening on http://127.0.0.1:8080/");

        while (true)
        {
            var context = await listener.GetContextAsync();
            await RespondAsync(context);
        }
    }

    private static async Task RespondAsync(HttpListenerContext context)
    {
        _visitCount++;
        await File.WriteAllTextAsync(CountFile, _visitCount.ToString()); // 4. Salva il nuovo numero nel file

        var request = context.Request;
        var requestDetails = new StringBuilder()
            .Append(request.HttpMethod)
            .Append(' ')
            .Append(request.RawUrl)
            .Append(" HTTP/")
            .AppendLine(request.ProtocolVersion.ToString());

        foreach (var header in request.Headers.AllKeys)
        {
            requestDetails.Append(header).Append(": ").AppendLine(request.Headers[header]);
        }

        var message = $"Questa pagina ha ricevuto {_visitCount} richieste HTTP";
        var responseBody = Encoding.UTF8.GetBytes(
            $"{message}{Environment.NewLine}{Environment.NewLine}Richiesta HTTP:{Environment.NewLine}{requestDetails}");
        var response = context.Response;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = responseBody.Length;

        try
        {
            await response.OutputStream.WriteAsync(responseBody);
        }
        finally
        {
            response.Close();
        }
    }
}

// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using CTrue.DcsConnect;
using RurouniJones.Dcs.Grpc.V0.Mission;
using Spectre.Console;
using Spectre.Console.Cli;

Console.WriteLine("C-True DCS Console");

if (args.Length == 0)
{
    Console.WriteLine("Usage: DcsConsole <server_name>");
    return 0;
}

var app = new CommandApp<ConnectCommand>();
return app.Run(args);


public class ConnectCommand : Command<ConnectCommand.ConnectSettings>
{
    public class ConnectSettings : CommandSettings
    {
        [CommandOption("-s|--server")]
        public string? Server { get; set; }

        [CommandOption("-p|--port")]
        [DefaultValue(50051)]
        public ushort Port { get; set; }
    }

    public override int Execute(CommandContext context, ConnectSettings settings)
    {
        try
        {
            DcsConnector dcsConnect = new DcsConnector(new MyLogProvider());
            dcsConnect.ConnectionStatusChanged += (sender, eventArgs) => AnsiConsole.MarkupLine("[green]Connected.[/]");

            Action<DcsUnitUpdate> myUnitHandler = update =>
            {
                AnsiConsole.MarkupLine($"Received update: [green]{update.Unit.Name}[/]");
            };

            Action<StreamEventsResponse> eventHandler = response => { Console.WriteLine($"Event: {response.EventCase}"); };

            dcsConnect.SubscribeUnitUpdates(myUnitHandler);
            dcsConnect.SubscribeEventUpdates(eventHandler);

            string serverAddr = settings.Server ?? "";

            AnsiConsole.MarkupLine($"Connecting to [green]{settings.Server}:{settings.Port}[/]");

            dcsConnect.Connect(serverAddr, settings.Port, "DcsConsole");

            Console.WriteLine("Press any key to list current units");
            Console.ReadKey();
            dcsConnect.Units.ForEach(x => Console.WriteLine(x.Callsign));

            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]ERROR: {e.Message}[/]");
        }

        // Omitted
        return 0;
    }
}

public class MyLogProvider : IDcsConnectLogProvider
{
    #region Implementation of IDcsConnectLogProvider

    public IDcsConnectLog GetLogger(string name)
    {
        return new MyLogger();
    }

    #endregion
}

public class MyLogger : IDcsConnectLog
{
    #region Implementation of IDcsConnectLog

    public bool IsDebugEnabled { get; } = true;
    public bool IsInfoEnabled { get; } = true;
    public bool IsWarnEnabled { get; } = true;
    public bool IsErrorEnabled { get; } = true;

    public void Debug(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);
    }

    public void Debug(string message, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(message);
    }

    public void Info(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);
    }

    public void Info(string message, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(message);

    }

    public void Warn(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);

    }

    public void Warn(string message, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(message);
    }

    public void Error(string message)
    {
        throw new NotImplementedException();
    }

    public void Error(string message, Exception ex)
    {
        throw new NotImplementedException();
    }

    #endregion
}
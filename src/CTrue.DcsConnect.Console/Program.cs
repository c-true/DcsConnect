// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography.X509Certificates;
using CTrue.DcsConnect;
using RurouniJones.Dcs.Grpc.V0.Mission;

Console.WriteLine("C-True DCS Console");

if (args.Length == 0)
{
    Console.WriteLine("Usage: DcsConsole <server_name>");
    return;
}

DcsConnector dcsConnect = new DcsConnector(new MyLogProvider());
dcsConnect.ConnectionStatusChanged += (sender, eventArgs) => Console.WriteLine("Connected");

Action<DcsUnitUpdate> myUnitHandler = update =>
{
    Console.WriteLine("Received update: " +  update.Unit.Name);
};

Action<StreamEventsResponse> eventHandler = response => { Console.WriteLine("Event"); };

dcsConnect.SubscribeUnitUpdates(myUnitHandler);
dcsConnect.SubscribeEventUpdates(eventHandler);

string serverAddr = args[0];

dcsConnect.Connect(serverAddr, 50051, "DcsConsole");

Console.ReadKey();
dcsConnect.Units.ForEach(x => Console.WriteLine(x.Callsign));
Console.ReadKey();


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
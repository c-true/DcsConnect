using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RurouniJones.Dcs.Grpc.V0.Atmosphere;
using RurouniJones.Dcs.Grpc.V0.Coalition;
using RurouniJones.Dcs.Grpc.V0.Common;
using RurouniJones.Dcs.Grpc.V0.Controller;
using RurouniJones.Dcs.Grpc.V0.Custom;
using RurouniJones.Dcs.Grpc.V0.Group;
using RurouniJones.Dcs.Grpc.V0.Hook;
using RurouniJones.Dcs.Grpc.V0.Mission;
using RurouniJones.Dcs.Grpc.V0.Net;
using RurouniJones.Dcs.Grpc.V0.Timer;
using RurouniJones.Dcs.Grpc.V0.Trigger;
using RurouniJones.Dcs.Grpc.V0.Tts;
using RurouniJones.Dcs.Grpc.V0.Unit;
using RurouniJones.Dcs.Grpc.V0.World;

namespace CTrue.DcsConnect
{
    /// <summary>
    /// Communicates with DCS using the gRPC interface provided by the Rust-DCS project.
    /// </summary>
    public interface IDcsConnector : IDisposable
    {
        /// <summary>
        /// Triggered when the status of the proxy connection has changed. Use <see cref="Connected"/> to get current status.
        /// </summary>
        event EventHandler ConnectionStatusChanged;

        /// <summary>
        /// The <see cref="PlayerChatMessageReceived"/> event is raised when a player has sent a chat message.
        /// </summary>
        event EventHandler<PlayerChatMessage> PlayerChatMessageReceived;

        /// <summary>
        /// The <see cref="PlayersListChanged"/> event is raised when a player has been added or removed from the players list.
        /// </summary>
        event EventHandler PlayersListChanged;

        /// <summary>
        /// The <see cref="GroupCommandExecuted"/> event is raised when a command has been recieved from a unit's group menu.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="AddGroupCommand"/> to add a command to the player's menu, and receive notification via the <see cref="GroupCommandExecuted"/> event for executed commands.
        /// </remarks>
        event EventHandler<GroupCommandExecutedEventArgs> GroupCommandExecuted;

        /// <summary>
        /// The <see cref="PlayerInUnitChanged"/> event is raised when a player enters or leaves a slot in the mission.
        /// </summary>
        event EventHandler<PlayerInUnitChangedEventArgs> PlayerInUnitChanged;

        /// <summary>
        /// Is the proxy connected or not
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Textual description of status of proxy
        /// </summary>
        string ConnectionStatus { get; }

        /// <summary>
        /// Gets the current server-host
        /// </summary>
        string ServerHost { get; }

        /// <summary>
        /// Gets information about the server, such as current mission and mission time.
        /// </summary>
        DcsServerInfo ServerInfo { get; }
        
        /// <summary>
        /// Gets a list of players connected to the DCS server.
        /// </summary>
        List<PlayerInfo> Players { get; }

        /// <summary>
        /// Gets a list of known units.
        /// </summary>
        List<UnitInfo> Units { get; }

        /// <summary>
        /// Gets a list of known groups.
        /// </summary>
        List<GroupInfo> Groups { get; }

        /// <summary>
        /// Connects proxy
        /// </summary>
        void Connect(string serverHost, ushort port, string clientId);

        /// <summary>
        /// Disconnects from the DCS Server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Gets information about a specific unit.
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns>Returns a <see cref="UnitInfo"/> instance or null if the unit does not exist.</returns>
        UnitInfo GetUnit(uint unitId);

        PlayerInfo GetPlayer(string playerName);

        /// <summary>
        /// Displays a text on the screen for all players on the DCS Server.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="displayTime">The duration to display the text, in seconds.</param>
        void ShowText(string text, int displayTime);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        void ShowSmoke(SmokeRequest.Types.SmokeColor color, double lat, double lon);

        /// <summary>
        /// Creates a mark with text in the F10 map at the specified position.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="message"></param>
        /// <param name="text"></param>
        /// <param name="readOnly"></param>
        uint MarkInMap(double lat, double lon, string message, string text, bool readOnly);

        /// <summary>
        /// Removes a placed mark in the F10 map.
        /// </summary>
        /// <param name="markId">The id returned by the <see cref="MarkInMap"/> call.</param>
        void RemoveMark(uint markId);

        /// <summary>
        /// Creates an illumination bomb at the specified altitude.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="power"></param>
        /// <param name="altitude"></param>
        void ShowIllumination(double lat, double lon, uint power, int altitude);

        /// <summary>
        /// Creates an explosion at the specified position.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="power"></param>
        void ShowExplosion(double lat, double lon, uint power);

        /// <summary>
        ///  Reloads the current mission.
        /// </summary>
        void ReloadCurrentMission();
        
        /// <summary>
        ///  Pauses or unpauses the DCS mission.
        /// </summary>
        /// <param name="paused"></param>
        /// <remarks>
        /// The time in DCS will stop and all updates be paused.
        /// </remarks>
        void SetPaused(bool paused);

        /// <summary>
        /// Gets a boolean value indicating whether the DCS server is paused.
        /// </summary>
        /// <returns>Returns true if the DCS server is paused.</returns>
        bool IsPaused();

        void SendChatTo(uint playerId, string message);

        void SendChat(Coalition coalition, string message);

        void OutTextForCoalition(uint coalition, string text);
        
        void OutTextForUnit(uint unitId, string text);

        string AddCoalitionCommand(uint coalition, string name, string menuId);
        void RemoveCoalitionCommand(Coalition coalition);

        string AddGroupCommand(string groupName, string name, string menuId);
        string AddGroupCommandSubMenu(string groupName, string name);
        void RemoveGroupCommand(string groupName);

        void SubscribeUnitUpdates(Action<DcsUnitUpdate> unitUpdateHandler);
        void SubscribeEventUpdates(Action<StreamEventsResponse> eventsUpdateHandler);
    }

    public class DcsConnector : IDcsConnector
    {
        private readonly IDcsConnectLog _log;

        private AtmosphereService.AtmosphereServiceClient _atmosphereServiceClient;
        private CoalitionService.CoalitionServiceClient _coalitionServiceClient;
        private ControllerService.ControllerServiceClient _controllerServiceClient;
        private CustomService.CustomServiceClient _customServiceClient;
        private GroupService.GroupServiceClient _groupServiceClient;
        private HookService.HookServiceClient _hookServiceClient;
        private MissionService.MissionServiceClient _missionServiceClient;
        private NetService.NetServiceClient _netServiceClient;
        private TimerService.TimerServiceClient _timerServiceClient;
        private TriggerService.TriggerServiceClient _triggerServiceClient;
        private TtsService.TtsServiceClient _ttsServiceClient;
        private UnitService.UnitServiceClient _unitServiceClient;
        private WorldService.WorldServiceClient _worldServiceClient;

        private readonly Timer _connectTimer;
        private readonly TimeSpan _reconnectInterval = TimeSpan.FromSeconds(1);

        private Channel _grpcChannel;
        private AsyncServerStreamingCall<StreamUnitsResponse> _unitsSubscriptionStream;
        private AsyncServerStreamingCall<StreamEventsResponse> _eventsSubscriptionStream;
        private Action<DcsUnitUpdate> _unitsProcessingDelegate;
        private Action<StreamEventsResponse> _eventsProcessingDelegate;

        private CancellationTokenSource _subscriptionTokenSource;
        private Task _unitsReceiveTask;
        private Task _eventsReceiveTask;
        private bool _cleanupInProgress;

        private readonly BlockingCollection<StreamUnitsResponse> _incomingUnitsQueue = new BlockingCollection<StreamUnitsResponse>();
        private readonly BlockingCollection<StreamEventsResponse> _incomingEventsQueue = new BlockingCollection<StreamEventsResponse>();
        private bool _messageProcessorRunning;

        private readonly DcsServerInfo _serverInfo = new DcsServerInfo();
        private readonly ConcurrentDictionary<uint, UnitInfo> _units = new ConcurrentDictionary<uint, UnitInfo>();
        private readonly ConcurrentDictionary<uint, PlayerInfo> _players = new ConcurrentDictionary<uint, PlayerInfo>();
        private readonly ConcurrentDictionary<uint, GroupInfo> _groups = new ConcurrentDictionary<uint, GroupInfo>();

        private bool _isDisposed { get; set; }
        private object _cleanupLock = new object();

        public DcsConnector(IDcsConnectLogProvider logProvider)
        {
            _log = logProvider.GetLogger(GetType().Name);

            _connectTimer = new Timer(OnConnectionTimer);

            // Ensure resolve of DNS works
            Environment.SetEnvironmentVariable("GRPC_DNS_RESOLVER", "native");
        }

        #region IDcsConnector

        public event EventHandler ConnectionStatusChanged;
        public event EventHandler<PlayerChatMessage> PlayerChatMessageReceived;

        public event EventHandler PlayersListChanged;
        public event EventHandler<GroupCommandExecutedEventArgs> GroupCommandExecuted;
        public event EventHandler<PlayerInUnitChangedEventArgs> PlayerInUnitChanged;

        public bool Connected { get; private set; }

        public string ConnectionStatus { get; private set; }

        public string ServerHost { get; private set; }

        /// <summary>
        /// Which port to use
        /// </summary>
        public ushort GrpcPort { get; private set; }

        public uint UnitPollRate { get; set; } = 1;

        public List<UnitInfo> Units => _units.Values.ToList();

        public List<PlayerInfo> Players => _players.Values.ToList();

        public List<GroupInfo> Groups => _groups.Values.ToList();

        public DcsServerInfo ServerInfo => _serverInfo;

        public AtmosphereService.AtmosphereServiceClient AtmosphereServiceClient => _atmosphereServiceClient ?? throw ThrowNotConnectedException();
        public CoalitionService.CoalitionServiceClient CoalitionServiceClient => _coalitionServiceClient ?? throw ThrowNotConnectedException();
        public ControllerService.ControllerServiceClient ControllerServiceClient => _controllerServiceClient ?? throw ThrowNotConnectedException();
        public CustomService.CustomServiceClient CustomServiceClient => _customServiceClient ?? throw ThrowNotConnectedException();
        public GroupService.GroupServiceClient GroupServiceClient => _groupServiceClient ?? throw ThrowNotConnectedException();
        public HookService.HookServiceClient HookServiceClient => _hookServiceClient ?? throw ThrowNotConnectedException();
        public MissionService.MissionServiceClient MissionServiceClient => _missionServiceClient ?? throw ThrowNotConnectedException();
        public NetService.NetServiceClient NetServiceClient => _netServiceClient ?? throw ThrowNotConnectedException();
        public TimerService.TimerServiceClient TimerServiceClient => _timerServiceClient?? throw ThrowNotConnectedException();
        public TriggerService.TriggerServiceClient TriggerServiceClient => _triggerServiceClient ?? throw ThrowNotConnectedException();
        public TtsService.TtsServiceClient TtsServiceClient => _ttsServiceClient ?? throw ThrowNotConnectedException();
        public UnitService.UnitServiceClient UnitServiceClient => _unitServiceClient ?? throw ThrowNotConnectedException();
        public WorldService.WorldServiceClient WorldServiceClient => _worldServiceClient ?? throw ThrowNotConnectedException();

        public void Connect(string address, ushort port, string clientId)
        {
            if (string.Equals(ServerHost, address))
            {
                // Already using this host
                return;
            }

            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(Connect)}] Connecting to {address}");

            StartMessageProcessing();

            if (Connected)
            {
                Cleanup($"Changing host, disconnecting from {ServerHost}");
            }

            ServerHost = address;
            GrpcPort = port;

            _connectTimer.Change(TimeSpan.FromMilliseconds(1), TimeSpan.Zero);
        }


        public void Disconnect()
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(Disconnect)}]");

            ServerHost = string.Empty;

            if (!Connected || _missionServiceClient == null)
                return;

            Cleanup("Disconnecting");
        }


        public void SubscribeUnitUpdates(Action<DcsUnitUpdate> unitUpdateHandler)
        {
            _unitsProcessingDelegate = unitUpdateHandler;
        }

        public void SubscribeEventUpdates(Action<StreamEventsResponse> eventsUpdateHandler)
        {
            _eventsProcessingDelegate = eventsUpdateHandler;
        }

        public UnitInfo GetUnit(uint unitId)
        {
            if(_units.TryGetValue(unitId, out UnitInfo ui))
                return ui;

            return null;
        }

        public PlayerInfo GetPlayer(string playerName)
        {
            return _players.Values.FirstOrDefault(p => p.PlayerName.Equals(playerName));
        }

        #region Mission

        public string AddCoalitionCommand(uint coalition, string name, string menuId)
        {
            Struct details = new Struct
            {
                Fields =
                {
                    ["menuId"] = Value.ForString(menuId),
                }
            };

            var resp = MissionServiceClient.AddCoalitionCommand(new AddCoalitionCommandRequest()
            {
                Coalition = (Coalition)coalition,
                Name = name,
                Details = details
            });

            return resp.Path.ToString();
        }

        public void RemoveCoalitionCommand(Coalition coalition)
        {
            MissionServiceClient.RemoveCoalitionCommandItem(new RemoveCoalitionCommandItemRequest()
            {
                Coalition = coalition
            });
        }

        public string AddGroupCommand(string groupName, string name, string menuId)
        {
            Struct details = new Struct
            {
                Fields =
                {
                    ["menuId"] = Value.ForString(menuId),
                }
            };

            var resp = MissionServiceClient.AddGroupCommand(new AddGroupCommandRequest()
            {
                GroupName = groupName,
                Name = name,
                Details = details
            });

            return resp.Path.ToString();
        }

        public string AddGroupCommandSubMenu(string groupName, string name)
        {
            var resp = MissionServiceClient.AddGroupCommandSubMenu(new AddGroupCommandSubMenuRequest()
            {
                GroupName = groupName,
                Name = name
            });

            return resp.Path.ToString();
        }

        public void RemoveGroupCommand(string groupName)
        {
            MissionServiceClient.RemoveGroupCommandItem(new RemoveGroupCommandItemRequest()
            {
                GroupName = groupName
            });
        }

        #endregion

        #region Net

        public void SendChatTo(uint playerId, string message)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(SendChatTo)}]");

            NetServiceClient.SendChatTo(new SendChatToRequest()
            {
                TargetPlayerId = playerId,
                Message = message
            });
        }

        public void SendChat(Coalition coalition, string message)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(SendChat)}]");

            NetServiceClient.SendChat(new SendChatRequest()
            {
                Coalition = coalition,
                Message = message
            });
        }

        #endregion

        #region Hook

        public void ReloadCurrentMission()
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(ReloadCurrentMission)}]");

            HookServiceClient.ReloadCurrentMission(new ReloadCurrentMissionRequest());
        }

        public void SetPaused(bool paused)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(SetPaused)}]");

            HookServiceClient.SetPaused(new SetPausedRequest()
            {
                Paused = paused
            });
        }

        public bool IsPaused()
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(IsPaused)}]");

            return HookServiceClient.GetPaused(new GetPausedRequest()).Paused;
        }

        #endregion

        #region Trigger

        public void ShowText(string text, int displayTime)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(ShowText)}]");

            if (!Connected) throw ThrowNotConnectedException();

            TriggerServiceClient.OutText(new OutTextRequest()
            {
                ClearView = false,
                DisplayTime = displayTime,
                Text = text
            });
        }

        public void ShowSmoke(SmokeRequest.Types.SmokeColor color, double lat, double lon)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(ShowSmoke)}]");

            if (!Connected) return;

            TriggerServiceClient.Smoke(new SmokeRequest()
            {
                Color = color,
                Position = new InputPosition()
                {
                    Lat = lat,
                    Lon = lon
                }
            });
        }

        public uint MarkInMap(double lat, double lon, string message, string text, bool readOnly)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(MarkInMap)}]");

            if (!Connected) return 0;

            var res = TriggerServiceClient.MarkToAll(new MarkToAllRequest()
            {
                Message = message,
                Position = new InputPosition()
                {
                    Lat = lat,
                    Lon = lon
                },
                ReadOnly = readOnly,
                Text = text
            });

            return res.Id;
        }

        public void RemoveMark(uint markId)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(RemoveMark)}]");

            if (!Connected) return;

            TriggerServiceClient.RemoveMark(new RemoveMarkRequest()
            {
                Id = markId
            });
        }

        public void ShowIllumination(double lat, double lon, uint power, int altitude)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(ShowIllumination)}]");

            if (!Connected) return;

            TriggerServiceClient.IlluminationBomb(new IlluminationBombRequest()
            {
                Position = new InputPosition()
                {
                    Lat = lat,
                    Lon = lon,
                    Alt = altitude
                },
                Power = power
            });
        }

        public void ShowExplosion(double lat, double lon, uint power)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(ShowExplosion)}]");

            if (!Connected) return;

            TriggerServiceClient.Explosion(new ExplosionRequest()
            {
                Position = new InputPosition()
                {
                    Lat = lat,
                    Lon = lon,
                },
                Power = power
            });
        }

        public void OutTextForCoalition(uint coalition, string text)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(OutTextForCoalition)}]");

            if (!Connected) return;

            var resp = TriggerServiceClient.OutTextForCoalition(new OutTextForCoalitionRequest()
            {
                Coalition = (Coalition)coalition,
                Text = text,
                DisplayTime = 1000,
                ClearView = false
            });
        }

        public void OutTextForGroup(uint groupId, string text)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(OutTextForGroup)}]");

            if (!Connected) return;

            var resp = TriggerServiceClient.OutTextForGroup(new OutTextForGroupRequest()
            {
                GroupId = groupId,
                Text = text,
                DisplayTime = 1000,
                ClearView = false
            });
        }

        public void OutTextForUnit(uint unitId, string text)
        {
            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(OutTextForUnit)}]");

            if (!Connected) return;

            var resp = TriggerServiceClient.OutTextForUnit(new OutTextForUnitRequest()
            {
                UnitId = unitId,
                Text = text,
                DisplayTime = 60,
                ClearView = false
            });
        }

        #endregion

        #endregion IDcsConnector

        #region Event Handlers

        private void ProcessEvents(StreamEventsResponse evt)
        {
            _serverInfo.MissionTime = evt.Time;

            switch (evt.EventCase)
            {
                case StreamEventsResponse.EventOneofCase.Birth:
                    HandleEventBirth(evt.Time, evt.Birth);
                    break;
                case StreamEventsResponse.EventOneofCase.MissionStart:
                    HandleEventMissionStart(evt.Time, evt.MissionStart);
                    break;
                case StreamEventsResponse.EventOneofCase.MissionEnd:
                    HandleEventMissionEnd(evt.Time, evt.MissionEnd);
                    break;
                case StreamEventsResponse.EventOneofCase.Connect:
                    HandleEventConnect (evt.Time, evt.Connect);
                    break;
                case StreamEventsResponse.EventOneofCase.Disconnect:
                    HandleEventDisconnect(evt.Time, evt.Disconnect);
                    break;
                case StreamEventsResponse.EventOneofCase.PlayerEnterUnit:
                    HandleEventPlayerEnterUnit(evt.Time, evt.PlayerEnterUnit);
                    break;
                case StreamEventsResponse.EventOneofCase.PlayerChangeSlot:
                    HandleEventPlayerChangeSlot(evt.Time, evt.PlayerChangeSlot);
                    break;
                case StreamEventsResponse.EventOneofCase.PlayerLeaveUnit:
                    HandleEventPlayerLeaveUnit(evt.Time, evt.PlayerLeaveUnit);
                    break;
                case StreamEventsResponse.EventOneofCase.PlayerSendChat:
                    HandleEventPlayerSendChat(evt.Time, evt.PlayerSendChat);
                    break;
                case StreamEventsResponse.EventOneofCase.PilotDead:
                    HandleEventPilotDead(evt.Time, evt.PilotDead);
                    break;
                case StreamEventsResponse.EventOneofCase.CoalitionCommand:
                    HandleEventCoalitionCommand(evt.Time, evt.CoalitionCommand);
                    break;
                case StreamEventsResponse.EventOneofCase.GroupCommand:
                    HandleEventGroupCommand(evt.Time, evt.GroupCommand);
                    break;
                case StreamEventsResponse.EventOneofCase.MissionCommand:
                    HandleEventMissionCommand(evt.Time, evt.MissionCommand);
                    break;
                case StreamEventsResponse.EventOneofCase.MarkAdd:
                    HandleEventMarkAdd(evt.Time, evt.MarkAdd);
                    break;
                case StreamEventsResponse.EventOneofCase.MarkChange:
                    HandleEventMarkChange(evt.Time, evt.MarkChange);
                    break;
                case StreamEventsResponse.EventOneofCase.MarkRemove:
                    HandleEventMarkRemove(evt.Time, evt.MarkRemove);
                    break;
            }
        }

        private void HandleEventBirth(double time, StreamEventsResponse.Types.BirthEvent birth)
        {
            
        }

        private void HandleEventMissionStart(double time, StreamEventsResponse.Types.MissionStartEvent evt)
        {
        }

        private void HandleEventMissionEnd(double time, StreamEventsResponse.Types.MissionEndEvent evt)
        {

        }

        private void HandleEventConnect(double time, StreamEventsResponse.Types.ConnectEvent evt)
        {
            if (!_players.TryGetValue(evt.Id, out PlayerInfo pi))
            {
                pi = new PlayerInfo(evt.Id, evt.Name, evt.Ucid);
                pi.NetworkAddress = evt.Addr;

                _players.TryAdd(evt.Id, pi);
            }

            pi.Connected = true;

            PlayersListChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleEventDisconnect(double time, StreamEventsResponse.Types.DisconnectEvent evt)
        {
            if (!_players.TryGetValue(evt.Id, out PlayerInfo pi)) return;

            pi.Connected = false;
            pi.Status = $"Disconnect reason: {evt.Reason}";

            PlayersListChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The event, which only works in single player, is raised when the player enters a vehicle.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="evt"></param>
        private void HandleEventPlayerEnterUnit(double time, StreamEventsResponse.Types.PlayerEnterUnitEvent evt)
        {

            // See discussion about why this does not work in multiplayer:
            // https://forum.dcs.world/topic/312428-worldevents_event_player_enter_unit-not-fired-when-a-client-enters-aircraft-on-multiplayer-server/

            if (evt.Initiator.InitiatorCase != Initiator.InitiatorOneofCase.Unit) return;

            PlayerInfo pi = GetPlayer(evt.Initiator.Unit.PlayerName);

            if (pi == null)
            {
                // When player is directly connected to DCS and not through multiplayer server there is no player record.
                AddLocalPLayer(evt.Initiator.Unit.PlayerName);
            }

            NotifyPlayerInUnitChanged(evt.Initiator.Unit.Id, evt.Initiator.Unit.PlayerName,
                PlayerInUnitChangeType.PlayerEnteredUnit);

            PlayersListChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The event, which only works in single player, is raised when the player leaves a vehicle.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="evt"></param>
        private void HandleEventPlayerLeaveUnit(double time, StreamEventsResponse.Types.PlayerLeaveUnitEvent evt)
        {
            if (evt.Initiator.InitiatorCase != Initiator.InitiatorOneofCase.Unit) return;

            PlayerInfo pi = GetPlayer(evt.Initiator.Unit.PlayerName);

            if (pi == null) return; // PLayer is not known and it's not my job to figure it out

            NotifyPlayerInUnitChanged(evt.Initiator.Unit.Id, evt.Initiator.Unit.PlayerName,
                PlayerInUnitChangeType.PlayerLeavedUnit);

            PlayersListChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleEventPlayerChangeSlot(double time, StreamEventsResponse.Types.PlayerChangeSlotEvent evt)
        {
            if (!_players.TryGetValue(evt.PlayerId, out PlayerInfo pi)) return;
            
            pi.Coalition = evt.Coalition;
            pi.SlotId = evt.SlotId;

            PlayersListChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleEventPlayerSendChat(double time, StreamEventsResponse.Types.PlayerSendChatEvent evt)
        {
            if (!_players.TryGetValue(evt.PlayerId, out PlayerInfo pi)) return;

            PlayerChatMessageReceived?.Invoke(this, new PlayerChatMessage(pi.PlayerId, evt.Message));
        }

        private void HandleEventPilotDead(double evtTime, StreamEventsResponse.Types.PilotDeadEvent evt)
        {
        }

        private void HandleEventCoalitionCommand(double evtTime, StreamEventsResponse.Types.CoalitionCommandEvent evt)
        {

        }

        private void HandleEventGroupCommand(double evtTime, StreamEventsResponse.Types.GroupCommandEvent evt)
        {
            GroupCommandExecuted?.Invoke(this, new GroupCommandExecutedEventArgs(evt.Group.Id, evt.Details.Fields.FirstOrDefault().Value.StringValue));
        }

        private void HandleEventMissionCommand(double evtTime, StreamEventsResponse.Types.MissionCommandEvent evt)
        {
        }

        private void HandleEventMarkAdd(double evtTime, StreamEventsResponse.Types.MarkAddEvent evtMarkAdd)
        {

        }

        private void HandleEventMarkChange(double evtTime, StreamEventsResponse.Types.MarkChangeEvent evtMarkChange)
        {

        }

        private void HandleEventMarkRemove(double evtTime, StreamEventsResponse.Types.MarkRemoveEvent evtMarkRemove)
        {

        }

        #endregion

        #region Connection / Disconnection handling

        private void UpdateConnectionStatus(bool connected, string message)
        {
            if (connected == Connected && string.Equals(ConnectionStatus, message))
                return;

            if (_log.IsInfoEnabled)
                _log.Info($"[{nameof(UpdateConnectionStatus)}] Connected={connected}. Message={message}");

            Connected = connected;
            ConnectionStatus = message;
            ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionTimer(object state)
        {
            try
            {
                if (_cleanupInProgress) return; // Wait with attempting connect until cleanup has been done and connection state set correctly

                _connectTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

                DoConnectRpc();
            }
            catch (Exception ex)
            {
                _log.Error($"[{nameof(OnConnectionTimer)}]", ex);
            }
            finally
            {
                if (!_isDisposed && !Connected)
                    RestartConnectionTimer();
            }
        }

        private void DoConnectRpc()
        {
            if (_log.IsDebugEnabled)
                _log.Debug($"[{nameof(DoConnectRpc)}] address: {ServerHost} port: {GrpcPort}");

            if (Connected)
            {
                return;
            }

            try
            {
                _grpcChannel = new Channel($"{ServerHost}:{GrpcPort}", ChannelCredentials.Insecure);

                _atmosphereServiceClient = new AtmosphereService.AtmosphereServiceClient(_grpcChannel);
                _coalitionServiceClient = new CoalitionService.CoalitionServiceClient(_grpcChannel);
                _controllerServiceClient = new ControllerService.ControllerServiceClient(_grpcChannel);
                _customServiceClient = new CustomService.CustomServiceClient(_grpcChannel);
                _groupServiceClient = new GroupService.GroupServiceClient(_grpcChannel);
                _hookServiceClient = new HookService.HookServiceClient(_grpcChannel);
                _missionServiceClient = new MissionService.MissionServiceClient(_grpcChannel);
                _netServiceClient = new NetService.NetServiceClient(_grpcChannel);
                _timerServiceClient = new TimerService.TimerServiceClient(_grpcChannel);
                _triggerServiceClient = new TriggerService.TriggerServiceClient(_grpcChannel);
                _ttsServiceClient = new TtsService.TtsServiceClient(_grpcChannel);
                _unitServiceClient = new UnitService.UnitServiceClient(_grpcChannel);
                _worldServiceClient = new WorldService.WorldServiceClient(_grpcChannel);

                VerifyConnectionToService();
                UpdateConnectionStatus(true, $"Connected to {ServerHost}");

                _subscriptionTokenSource = new CancellationTokenSource();
                var cancellationToken = _subscriptionTokenSource.Token;

                if (UnitPollRate > 0)
                {
                    _unitsSubscriptionStream = SubscribeToUnitsStream(UnitPollRate);
                    var unitResponseStream = _unitsSubscriptionStream.ResponseStream;
                    _unitsReceiveTask = Task.Run(() => ReceiveUnitsTask(unitResponseStream, cancellationToken), cancellationToken);
                }

                _eventsSubscriptionStream = SubscribeToEventsStream();
                var eventsResponseStream = _eventsSubscriptionStream.ResponseStream;
                _eventsReceiveTask = Task.Run(() => ReceiveEventsTask(eventsResponseStream, cancellationToken), cancellationToken);
            }
            catch (RpcException rpcEx)
            {
                // Do not log as error when service is not available
                if (rpcEx.StatusCode != StatusCode.Unavailable)
                {
                    _log.Error($"[{nameof(DoConnectRpc)}] {rpcEx}");
                }
                else if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[{nameof(DoConnectRpc)}] {rpcEx}");
                }

                Cleanup("Failed to connect to server");
            }
            catch (Exception ex)
            {
                _log.Error($"[{nameof(DoConnectRpc)}] {ex}");
                Cleanup("Failed to connect to server");
            }
        }

        private void VerifyConnectionToService()
        {
            MissionServiceClient.GetScenarioCurrentTime(new GetScenarioCurrentTimeRequest());

            var theatreResponse = WorldServiceClient.GetTheatre(new GetTheatreRequest());
            _serverInfo.Theatre = theatreResponse.Theatre;

            var getMissionNameResponse = HookServiceClient.GetMissionName(new GetMissionNameRequest());
            _serverInfo.MissionName = getMissionNameResponse.Name;

            var getMissionDescResp = HookServiceClient.GetMissionDescription(new GetMissionDescriptionRequest());
            _serverInfo.MissionDescription = getMissionDescResp.Description;

            _serverInfo.IsServer = HookServiceClient.IsServer(new IsServerRequest()).Server;
            _serverInfo.IsMultiplayer = HookServiceClient.IsMultiplayer(new IsMultiplayerRequest()).Multiplayer;

            RefreshGroupsList();
            RefreshPayersList();
        }

        private void RefreshGroupsList()
        {
            var groups = CoalitionServiceClient.GetGroups(new GetGroupsRequest()
            {
                Coalition = Coalition.All,
                Category = GroupCategory.Unspecified
            });

            foreach (var group in groups.Groups)
            {
                if (!_groups.TryGetValue(group.Id, out GroupInfo gi))
                {
                    gi = new GroupInfo(group.Id);

                    gi.Name = group.Name;
                    gi.Coalition = (uint)group.Coalition;
                    gi.Category = (uint)group.Category;
                
                    if(_log.IsInfoEnabled) _log.Info($"Group added: {gi.Name} is {group.Category} in coalition {group.Coalition}");
                    _groups.TryAdd(gi.GroupId, gi);
                }
                else
                {
                    gi.Name = group.Name;
                    gi.Coalition = (uint)group.Coalition;
                    gi.Category = (uint)group.Category;
                }
            }
        }

        private void RefreshPayersList()
        {
            var players = NetServiceClient.GetPlayers(new GetPlayersRequest()).Players;

            foreach (var evt in players)
            {
                if (!_players.TryGetValue(evt.Id, out PlayerInfo pi))
                {
                    pi = new PlayerInfo(evt.Id, evt.Name, evt.Ucid);
                    pi.Connected = true;
                    pi.Coalition = evt.Coalition;
                    pi.SlotId = evt.Slot;

                    pi.NetworkAddress = evt.RemoteAddress;

                    if (_log.IsInfoEnabled) _log.Info($"Player added: {pi.PlayerName} from {pi.NetworkAddress}");
                    _players.TryAdd(evt.Id, pi);
                }
                else
                {
                    pi.Connected = true;
                    pi.Coalition = evt.Coalition;
                    pi.SlotId = evt.Slot;

                    pi.NetworkAddress = evt.RemoteAddress;
                }
            }

            PlayersListChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StartMessageProcessing()
        {
            if (_messageProcessorRunning)
                return;
            Task.Factory.StartNew(RunUnitsProcessingQueue);
            Task.Factory.StartNew(RunEventsProcessingQueue);
            _messageProcessorRunning = true;
        }

        private void Cleanup(string message)
        {
            if (_log.IsInfoEnabled)
                _log.Debug($"[{nameof(Cleanup)}] Connected: {Connected}, Clean up in progress:{_cleanupInProgress}");

            if (_cleanupInProgress)
                return;

            lock (_cleanupLock)
            {
                try
                {
                    _cleanupInProgress = true;

                    _atmosphereServiceClient = null;
                    _coalitionServiceClient = null;
                    _controllerServiceClient = null;
                    _customServiceClient = null;
                    _groupServiceClient = null;
                    _hookServiceClient = null;
                    _missionServiceClient = null;
                    _netServiceClient = null;
                    _timerServiceClient = null;
                    _triggerServiceClient = null;
                    _ttsServiceClient = null;
                    _unitServiceClient = null;
                    _worldServiceClient = null;

                    _grpcChannel = null;

                    _subscriptionTokenSource?.Cancel();
                    _subscriptionTokenSource?.Dispose();
                    _subscriptionTokenSource = null;

                    _unitsSubscriptionStream?.Dispose();
                    _unitsSubscriptionStream = null;

                    _unitsSubscriptionStream?.Dispose();
                    _unitsSubscriptionStream = null;

                    _eventsSubscriptionStream?.Dispose();
                    _eventsSubscriptionStream = null;
                
                    _unitsReceiveTask?.Wait(100);
                    _unitsReceiveTask?.Dispose();
                    _unitsReceiveTask = null;

                    _eventsReceiveTask?.Wait(100);
                    _eventsReceiveTask?.Dispose();
                    _eventsReceiveTask = null;

                    _units.Clear();
                    _players.Clear();
                }
                catch (Exception ex)
                {
                    _log.Error($"[{nameof(Cleanup)}] {ex}");
                }
                finally
                {
                    _cleanupInProgress = false;
                }
            }

            UpdateConnectionStatus(false, message);
        }

        private Exception ThrowNotConnectedException()
        {
            return new Exception("Not connected");
        }

        private void RestartConnectionTimer()
        {
            if (_log.IsDebugEnabled)
                _log.Debug($"[{nameof(RestartConnectionTimer)}] Connection timer restarted");
            _connectTimer.Change(_reconnectInterval, Timeout.InfiniteTimeSpan);
        }

        #endregion

        #region Unit stream handling

        /// <summary>
        /// Subscribe to the stream from the server, this proxy only supports one stream
        /// </summary>
        /// <returns></returns>
        private AsyncServerStreamingCall<StreamUnitsResponse> SubscribeToUnitsStream(uint pollRate, uint maxBackoff = 30)
        {
            StreamUnitsRequest subscribeUnitsRequest = new StreamUnitsRequest()
            {
                PollRate = pollRate,
                MaxBackoff = maxBackoff
            };

            return MissionServiceClient.StreamUnits(subscribeUnitsRequest);
        }

        private void ReceiveUnitsTask(IAsyncStreamReader<StreamUnitsResponse> responseStream, CancellationToken cancellationToken)
        {
            bool serverShutDown = false;

            try
            {
                while (responseStream.MoveNext(cancellationToken).Result)
                {
                    var responseMsg = responseStream.Current;

                    if (_log.IsDebugEnabled)
                        _log.Debug($"{nameof(ReceiveUnitsTask)}: {responseMsg}");

                    _incomingUnitsQueue.Add(responseMsg);
                }

                serverShutDown = true;
            }
            catch (Exception ex)
            {
                _log.Error($"[{nameof(ReceiveUnitsTask)}] {ex}");
                // Clear receive-task, prevent cleanup from waiting for this task
                _eventsReceiveTask = null;
                _unitsReceiveTask = null;
                Cleanup("Server-side shutdown");
            }
            finally
            {
                if (!_isDisposed)
                {
                    if(serverShutDown)
                    {
                        // Cleanup and wait for reconnect
                        Cleanup("Server-side shutdown");
                    }

                    RestartConnectionTimer();
                }
            }
        }

        private void RunUnitsProcessingQueue()
        {
            try
            {
                foreach (var evt in _incomingUnitsQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        _serverInfo.MissionTime = evt.Time;

                        DcsUnitUpdate uu = null;
                        if (evt.UpdateCase == StreamUnitsResponse.UpdateOneofCase.Gone)
                        {
                            RemoveUnit(evt.Gone.Id);
                            uu = DcsUnitUpdate.DeleteUnit(evt.Time, evt.Gone.Id, evt.Gone.Name);
                        }
                        else if (evt.UpdateCase == StreamUnitsResponse.UpdateOneofCase.Unit)
                        {
                            var ui = AddOrUpdateUnit(evt.Unit);
                            uu = DcsUnitUpdate.UpdateUnit(evt.Time, ui);
                        }

                        if(uu != null)
                            _unitsProcessingDelegate?.Invoke(uu);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{nameof(RunUnitsProcessingQueue)}", ex);
                    }
                }
            }
            catch
            {
                // Done
            }
        }

        #endregion

        #region Event stream handling

        protected AsyncServerStreamingCall<StreamEventsResponse> SubscribeToEventsStream()
        {
            StreamEventsRequest subscribeRequest = new StreamEventsRequest();
            return MissionServiceClient.StreamEvents(subscribeRequest);
        }

        private void ReceiveEventsTask(IAsyncStreamReader<StreamEventsResponse> responseStream, CancellationToken cancellationToken)
        {
            bool serverShutDown = false;

            try
            {
                while (responseStream.MoveNext(cancellationToken).Result)
                {
                    var responseMsg = responseStream.Current;

                    if (_log.IsDebugEnabled)
                        _log.Debug($"{nameof(ReceiveEventsTask)}: {responseMsg}");

                    _incomingEventsQueue.Add(responseMsg);
                }

                serverShutDown = true;
            }
            catch (Exception ex)
            {
                _log.Error($"[{nameof(ReceiveEventsTask)}] {ex}");
                // Clear receive-task, prevent cleanup from waiting for this task
                _eventsReceiveTask = null;
                _unitsReceiveTask = null;
                Cleanup("Server-side shutdown");
            }
            finally
            {
                if (!_isDisposed)
                {
                    if (serverShutDown)
                    {
                        // Cleanup and wait for reconnect
                        Cleanup("Server-side shutdown");
                    }

                    RestartConnectionTimer();
                }
            }
        }

        private void RunEventsProcessingQueue()
        {
            try
            {
                foreach (var evt in _incomingEventsQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        ProcessEvents(evt);
                        _eventsProcessingDelegate.Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"{nameof(RunEventsProcessingQueue)}", ex);
                    }
                }
            }
            catch
            {
                // Done
            }
        }

        #endregion

        #region Unit Management

        private UnitInfo AddOrUpdateUnit(Unit unit)
        {
            UnitInfo ui = null;

            string playerNameForPlayerInUnitHasChanged = null;
            PlayerInUnitChangeType playerInUnitChangeType = PlayerInUnitChangeType.PlayerEnteredUnit;

            if (_units.ContainsKey(unit.Id))
            {
                ui = _units[unit.Id];

                string orgPlayerName = ui.PlayerName;

                ui.Update(unit);

                if (string.IsNullOrEmpty(orgPlayerName))
                {
                    // No player in unit

                    if (string.IsNullOrEmpty(ui.PlayerName))
                    {
                        // no change
                    }
                    else
                    {
                        playerNameForPlayerInUnitHasChanged = ui.PlayerName;
                        playerInUnitChangeType = PlayerInUnitChangeType.PlayerEnteredUnit;
                    }
                }
                else
                {
                    // Player in unit
                    if (string.IsNullOrEmpty(ui.PlayerName))
                    {
                        // but not anymore
                        playerNameForPlayerInUnitHasChanged = orgPlayerName;
                        playerInUnitChangeType = PlayerInUnitChangeType.PlayerLeavedUnit;
                    }
                    else
                    {
                        // and still is, so no change
                    }
                }
            }
            else
            {
                ui = new UnitInfo(unit);
                if (!string.IsNullOrEmpty(ui.PlayerName))
                {
                    playerNameForPlayerInUnitHasChanged = ui.PlayerName;
                    playerInUnitChangeType = PlayerInUnitChangeType.PlayerEnteredUnit;
                }
            }

            _units[ui.Id] = ui;

            if(!string.IsNullOrEmpty(playerNameForPlayerInUnitHasChanged))
                NotifyPlayerInUnitChanged(ui.Id, playerNameForPlayerInUnitHasChanged, playerInUnitChangeType);

            return ui;
        }

        private void RemoveUnit(uint unitId)
        {
            _units.TryRemove(unitId, out UnitInfo removedUi);

            if (removedUi != null && !string.IsNullOrEmpty(removedUi.PlayerName))
            {
                NotifyPlayerInUnitChanged(removedUi.Id, removedUi.PlayerName, PlayerInUnitChangeType.PlayerLeavedUnit);
            }
        }

        #endregion

        #region Player Management

        private void NotifyPlayerInUnitChanged(uint unitId, string playerNameForPlayerInUnitHasChanged,
            PlayerInUnitChangeType playerInUnitChangeType)
        {
            var pi = GetPlayer(playerNameForPlayerInUnitHasChanged);

            if (pi == null)
            {
                AddLocalPLayer(playerNameForPlayerInUnitHasChanged);
            }

            PlayerInUnitChanged?.Invoke(this,
                new PlayerInUnitChangedEventArgs(unitId, playerInUnitChangeType, pi));
        }

        private void AddLocalPLayer(string playerName)
        {
            // Most likely a client that has connected to a local session. TODO: Check with server type.
            PlayerInfo client = new PlayerInfo(playerName);
            _players.TryAdd(client.PlayerId, client);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    _incomingUnitsQueue.CompleteAdding();
                    _incomingUnitsQueue.Dispose();
                    _incomingEventsQueue.CompleteAdding();
                    _incomingEventsQueue.Dispose();
                    _connectTimer.Dispose();
                    Cleanup("Disposing");
                }
                catch (Exception ex)
                {
                    if (_log.IsDebugEnabled)
                        _log.Debug("[Dispose] Failed to clean up resources", ex);
                }
            }

            _isDisposed = true;
        }

        #endregion IDisposable
    }
}
import * as alt from 'alt';
import * as game from 'natives';
import * as Utils from 'client/Utils/utils';

export class VoiceChat
{
    public static view: alt.WebView;

    public static serverUniqueIdentifier: string;
    public static requiredBranch: string;
    public static minimumVersion: string;
    public static soundPack: string;
    public static ingameChannel: number;
    public static ingameChannelPassword: string;

    public static deadplayers: string[];
    public static radioChannel: string;
    public static isConnected: boolean;
    public static nextUpdate: number;

    public static isTalking: boolean;
    public static isMicrophoneMuted: boolean;
    public static isSoundMuted: boolean;

    private static _isIngame: boolean;

    constructor()
    {
        alt.onServer('Voice_Initialize', (serverUniqueIdentifier: string, requiredBranch: string, minimumVersion: string, soundPack: string, ingameChannel: number, ingameChannelPassword: string) =>
        {
            VoiceChat.serverUniqueIdentifier = serverUniqueIdentifier;
            VoiceChat.requiredBranch = requiredBranch;
            VoiceChat.minimumVersion = minimumVersion;
            VoiceChat.soundPack = soundPack;
            VoiceChat.ingameChannel = ingameChannel;
            VoiceChat.ingameChannelPassword = ingameChannelPassword;

            VoiceChat.nextUpdate = Date.now() + 300;
            VoiceChat.deadplayers = [];

            if (VoiceChat.view == null) {
                VoiceChat.view = new alt.WebView("http://resource/client/cef/voice/index.html");
            }

            VoiceChat.view.on('SaltyChat_OnConnected', () => {
                VoiceChat.isConnected = true;
                this.InitiatePlugin();
            });

            VoiceChat.view.on('SaltyChat_OnError', (data: string) => {
                //alt.log(data);
            });

            VoiceChat.view.on('SaltyChat_OnMessage', (arg) =>
            {
                let pluginCommand: PluginCommand = JSON.parse(arg);
                if (pluginCommand.Command == Command.Ping && Date.now() > VoiceChat.nextUpdate)
                {
                    VoiceChat.ExecuteCommand(new PluginCommand(Command.Pong, VoiceChat.serverUniqueIdentifier, undefined));
                    VoiceChat.nextUpdate = Date.now() + 300;
                    return;
                }
                
                if (pluginCommand.Command == Command.StateUpdate) {
                    let pluginState = pluginCommand.Parameter as PluginState;

                    if (pluginState == null)
                        return;

                    if (pluginState.IsReady != VoiceChat._isIngame)
                        VoiceChat._isIngame = pluginState.IsReady;

                    if (pluginState.IsTalking != VoiceChat.isTalking) {
                        VoiceChat.isTalking = pluginState.IsTalking;
                    }

                    if (pluginState.IsMicrophoneMuted != VoiceChat.isMicrophoneMuted) {
                        VoiceChat.isMicrophoneMuted = pluginState.IsMicrophoneMuted;
                    }

                    if (pluginState.IsSoundMuted != VoiceChat.isSoundMuted) {
                        VoiceChat.isSoundMuted = pluginState.IsSoundMuted;
                    }

                    if (pluginState.IsTalking)
                        game.playFacialAnim(alt.Player.local.scriptID, "mic_chatter", "mp_facial");
                    else
                        game.playFacialAnim(alt.Player.local.scriptID, "mood_normal_1", "facials@gen_male@variations@normal");
                }     
            });

            alt.setInterval(() => {
                this.PlayerStateUpdate();
            }, 300);
        });

        alt.onServer('Player_Disconnected', ((playerName: string) => {
            if (VoiceChat.deadplayers.find(p => p == playerName))
                Utils.ArrayRemove(VoiceChat.deadplayers, playerName);

            VoiceChat.ExecuteCommand(new PluginCommand(Command.RemovePlayer, VoiceChat.serverUniqueIdentifier, new PlayerState(playerName)));
        }));

        alt.onServer('Voice_IsTalking', (playerName: string, isTalking: boolean) => {
            /*
            alt.Player.all.forEach((player: alt.Player) => {
                if (player.getSyncedMeta("Voice_TeamSpeakName") == playerName) {
                    if (isTalking)
                        game.playFacialAnim(player.scriptID, "mic_chatter", "mp_facial");
                    else
                        game.playFacialAnim(player.scriptID, "mood_normal_1", "facials@gen_male@variations@normal");

                    return;
                }
            });*/
        });

        alt.onServer('Voice_EstablishedCall', (playerName: string) => {
            alt.Player.all.forEach((player: alt.Player) => {
                if (player.getSyncedMeta("Voice_TeamSpeakName") == playerName) {
                    VoiceChat.ExecuteCommand(new PluginCommand(Command.PhoneCommunicationUpdate, VoiceChat.serverUniqueIdentifier, new PhoneCommunication(playerName, 0, 0, true)))

                    return;
                }
            });
        });

        alt.onServer('Voice_EndCall', (playerName: string) => {
            VoiceChat.ExecuteCommand(new PluginCommand(Command.StopPhoneCommunication, VoiceChat.serverUniqueIdentifier, new PhoneCommunication(playerName)));
        });

        alt.onServer('Voice_SetRadioChannel', (radioChannel: string) => {
            if (radioChannel == "") {
                VoiceChat.radioChannel = "";
                this.PlaySound("leaveRadioChannel", false, "radio");
            }
            else {
                VoiceChat.radioChannel = radioChannel;
                this.PlaySound("enterRadioChannel", false, "radio");
            }
        });

        alt.onServer('Voice_TalkingOnRadio', (playerName: string, isOnRadio: boolean) => {
            if (alt.Player.local.getSyncedMeta("Voice_TeamSpeakName") == playerName) {
                this.PlaySound("selfMicClick", false, "MicClick");
            }
            else {
                if (isOnRadio) {
                    VoiceChat.ExecuteCommand(
                        new PluginCommand(Command.RadioCommunicationUpdate, VoiceChat.serverUniqueIdentifier, new RadioCommunication(playerName, RadioType.LongRange, RadioType.LongRange, true, 0, true, null)))

                    game.stopSound(-1);
                    game.stopSound(1);
                    game.playSoundFrontend(-1, "Start_Squelch", "CB_RADIO_SFX", true);
                    game.playSoundFrontend(1, "Background_Loop", "CB_RADIO_SFX", true);
                }
                else {
                    VoiceChat.ExecuteCommand(
                        new PluginCommand(Command.StopRadioCommunication, VoiceChat.serverUniqueIdentifier, new RadioCommunication(playerName, RadioType.LongRange, RadioType.LongRange, true, 0, true, null)))

                    game.stopSound(-1);
                    game.stopSound(1);
                    game.playSoundFrontend(1, "End_Squelch", "CB_RADIO_SFX", true);
                }
            }
        });
    
        alt.onServer('Player_Died', (playerName: string) => {
            if (!VoiceChat.deadplayers.find(p => p == playerName))
                VoiceChat.deadplayers.push(playerName);
        });

        alt.onServer('Player_Revived', (playerName: string) => {
            if (VoiceChat.deadplayers.find(p => p == playerName))
                Utils.ArrayRemove(VoiceChat.deadplayers, playerName);
        });
    }

    private InitiatePlugin()
    {
        let tsname = alt.Player.local.getSyncedMeta("Voice_TeamSpeakName");
        if (tsname != null) {
            VoiceChat.ExecuteCommand(new PluginCommand(Command.Initiate, VoiceChat.serverUniqueIdentifier, new GameInstance(VoiceChat.serverUniqueIdentifier, tsname, VoiceChat.ingameChannel, VoiceChat.ingameChannelPassword, VoiceChat.soundPack)));
        } 
    }

    private PlaySound(fileName: string, loop: boolean = false, handle: string)
    {
        if (fileName == "")
            handle = fileName;

        VoiceChat.ExecuteCommand(new PluginCommand(Command.PlaySound, VoiceChat.serverUniqueIdentifier, new Sound(fileName, loop, handle)))
    }

    private StopSound(handle: string) {
        VoiceChat.ExecuteCommand(new PluginCommand(Command.PlaySound, VoiceChat.serverUniqueIdentifier, new Sound(handle)))
    }

    private PlayerStateUpdate()
    {
        let playerPos = alt.Player.local.pos;

        alt.Player.all.forEach((nPlayer: alt.Player) =>
        {
            let nPlayerName = nPlayer.getSyncedMeta("Voice_TeamSpeakName");
            if (nPlayer != alt.Player.local && nPlayerName != null) {
                let voiceRange = 12;

                if (nPlayer.getSyncedMeta("Voice_VoiceRange") !== undefined) {
                    let nPlayerVoiceRange = nPlayer.getSyncedMeta("Voice_VoiceRange");

                    switch (nPlayerVoiceRange)
                    {
                        case "Parler":
                            voiceRange = 12;
                            break;

                        case "Crier":
                            voiceRange = 26;
                            break;

                        case "Chuchoter":
                            voiceRange = 4;
                            break;
                    }

                    VoiceChat.ExecuteCommand(new PluginCommand(Command.PlayerStateUpdate, VoiceChat.serverUniqueIdentifier, new PlayerState(nPlayerName, TSVector.Convert(nPlayer.pos), voiceRange, null, true, null)));
                }
            }
        });

        VoiceChat.ExecuteCommand(new PluginCommand(Command.SelfStateUpdate, VoiceChat.serverUniqueIdentifier, new PlayerState(null, TSVector.Convert(playerPos), null, game.getGameplayCamRot(0).z, false, null)));
    }

    /*
     *         internal static void OnEstablishCall(string playerName)
        {
#warning There seems to be an issue where the "client"-object is not correctly referenced on the client, remove workaround if the issue is resolved

            foreach (RAGE.Elements.Player player in RAGE.Elements.Entities.Players.All)
            {
                if (!player.TryGetSharedData(SaltyShared.SharedData.Voice_TeamSpeakName, out string tsName) || tsName != playerName)
                    continue;

                RAGE.Vector3 ownPosition = RAGE.Elements.Player.LocalPlayer.Position;
                RAGE.Vector3 playerPosition = player.Position;

                Voice.ExecuteCommand(
                    new PluginCommand(
                        Command.PhoneCommunicationUpdate,
                        Voice._serverUniqueIdentifier,
                        new PhoneCommunication(
                            playerName,
                            RAGE.Game.Zone.GetZoneScumminess(RAGE.Game.Zone.GetZoneAtCoords(ownPosition.X, ownPosition.Y, ownPosition.Z)) +
                            RAGE.Game.Zone.GetZoneScumminess(RAGE.Game.Zone.GetZoneAtCoords(playerPosition.X, playerPosition.Y, playerPosition.Z))
                        )
                    )
                );

                break;
            }
        }
     */

    public static OnEstablishCall(playerName: string)
    {
        VoiceChat.ExecuteCommand(new PluginCommand(Command.PhoneCommunicationUpdate, VoiceChat.serverUniqueIdentifier, new PhoneCommunication(playerName, 0, 5)));

        /*       parameters 0, 0 replace this, is for quality audio fucking useless
                 RAGE.Game.Zone.GetZoneScumminess(RAGE.Game.Zone.GetZoneAtCoords(ownPosition.X, ownPosition.Y, ownPosition.Z)) +
                 RAGE.Game.Zone.GetZoneScumminess(RAGE.Game.Zone.GetZoneAtCoords(playerPosition.X, playerPosition.Y, playerPosition.Z))
        */
    }

    public static OnEndCall(playerName: string)
    {
        VoiceChat.ExecuteCommand(new PluginCommand(Command.StopPhoneCommunication, VoiceChat.serverUniqueIdentifier, new PhoneCommunication(playerName)));
    }

    private static ExecuteCommand(pluginCommand: PluginCommand)
    {
        this.view.emit('runCommand', JSON.stringify(pluginCommand));
    }
}

class Sound
{
    public FileName: string;
    public IsLoop: boolean;
    public Handle: string;

    constructor(fileName: string, loop: boolean = false, handle: string = "")
    {
        this.FileName = fileName;
        this.IsLoop = loop;
        this.Handle = handle;
    }
}

class PluginCommand
{
    public Command : Command;
    public ServerUniqueIdentifier: string;
    public Parameter: object;

    constructor(command: Command, serverUniqueIdentifier: string, parameter: object)
    {
        this.Command = command;
        this.ServerUniqueIdentifier = serverUniqueIdentifier;
        this.Parameter = parameter;
    } 
}

class PlayerState
{
    public Name: string = null;
    public Position: TSVector = TSVector.Zero;
    public Rotation: number = null;
    public VoiceRange: number = null;
    public IsAlive: boolean = false;
    public VolumeOverride: number = null;

    constructor(name: string = null, position: TSVector = null, voiceRange: number = null, rotation: number = null, isAlive: boolean = null, volumeOverride: number = null)
    {
        if (name != null)
            this.Name = name;

        if (position != null)
            this.Position = position;

        if (voiceRange != null)
            this.VoiceRange = voiceRange;

        if (rotation != null)
            this.Rotation = rotation;

        if (isAlive != null)
            this.IsAlive = isAlive;

        if (volumeOverride != null)
            this.VolumeOverride = volumeOverride;
    }
}

class GameInstance
{
    public ServerUniqueIdentifier: string;
    public Name: string;
    public ChannelId: number;
    public ChannelPassword: string;
    public SoundPack : string;

    constructor(serverUniqueIdentifier: string, name: string, channelId: number, channelPassword: string, soundPack: string) {
        this.ServerUniqueIdentifier = serverUniqueIdentifier;
        this.Name = name;
        this.ChannelId = channelId;
        this.ChannelPassword = channelPassword;
        this.SoundPack = soundPack;
    }
}

class RadioCommunication
{
    public Name: string;
    public SenderRadioType: RadioType;
    public OwnRadioType: RadioType;
    public PlayMicClick: boolean;
    public Volume: number;
    public Direct: boolean;
    public RelayedBy: string[];

    constructor(name: string, senderRadioType: RadioType, ownRadioType: RadioType, playMicClick: boolean, volume: number, direct: boolean, relayedBy: string[])
    {
        this.Name = name;
        this.SenderRadioType = senderRadioType;
        this.OwnRadioType = ownRadioType;
        this.PlayMicClick = playMicClick;
        this.Volume = volume;
        this.Direct = direct;
        this.RelayedBy = relayedBy;
    }
}

class PhoneCommunication
{
    public Name: string;
    public SignalStrength: number = undefined;
    public Volume: number;
    public Direct: boolean;
    public RelayedBy: string[];

    constructor(name: string, signalStrength: number = undefined, volume: number = undefined, direct: boolean = true, relayedBy: string[] = undefined)
    {
        this.Name = name;

        if (signalStrength !== undefined)
            this.SignalStrength = signalStrength;

        if (volume !== undefined)
            this.Volume = volume;

        if (relayedBy !== undefined)
            this.RelayedBy = relayedBy;

        this.Direct = direct;
    }
}

class TSVector {

    public X: Number;
    public Y: Number;
    public Z: Number;

    public static readonly Zero: TSVector = new TSVector(0, 0, 0);

    constructor(x: Number, y: Number, z: Number) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public static Convert(position: alt.Vector3) {
        return new TSVector(position.x, position.y, position.z);
    }
}

class PluginState {
    public UpdateBranch: string
    public Version: string
    public IsConnectedToServer: boolean
    public IsReady: boolean
    public IsTalking : boolean
    public IsMicrophoneMuted: boolean
    public IsSoundMuted: boolean
}

enum Command {
    /// <summary>
    /// Use <see cref="GameInstance"/> as parameter
    /// </summary>
    Initiate = 0,

    /// <summary>
    /// Will be sent by the WebSocket and should be answered with a <see cref="Command.Pong"/>
    /// </summary>
    Ping = 1,

    /// <summary>
    /// Answer to a <see cref="Command.Ping"/> request
    /// </summary>
    Pong = 2,

    /// <summary>
    /// Will be sent by the WebSocket on state changes (e.g. mic muted/unmuted) and received by <see cref="Voice.OnPluginMessage(object[])"/> - uses <see cref="PluginState"/> as parameter
    /// </summary>
    StateUpdate = 3,

    /// <summary>
    /// Use <see cref="PlayerState"/> as parameter
    /// </summary>
    SelfStateUpdate = 4,

    /// <summary>
    /// Use <see cref="PlayerState"/> as parameter
    /// </summary>
    PlayerStateUpdate = 5,

    /// <summary>
    /// Use <see cref="PlayerState"/> as parameter
    /// </summary>
    RemovePlayer = 6,

    /// <summary>
    /// Use <see cref="PhoneCommunication"/> as parameter
    /// </summary>
    PhoneCommunicationUpdate = 7,

    /// <summary>
    /// Use <see cref="PhoneCommunication"/> as parameter
    /// </summary>
    StopPhoneCommunication = 8,

    /// <summary>
    /// Use <see cref="RadioTower"/> as parameter
    /// </summary>
    RadioTowerUpdate = 9,

    /// <summary>
    /// Use <see cref="RadioCommunication"/> as parameter
    /// </summary>
    RadioCommunicationUpdate = 10,

    /// <summary>
    /// Use <see cref="RadioCommunication"/> as parameter
    /// </summary>
    StopRadioCommunication = 11,

    /// <summary>
    /// Use <see cref="Sound"/> as parameter
    /// </summary>
    PlaySound = 12,

    /// <summary>
    /// Use <see cref="Sound"/> as parameter
    /// </summary>
    StopSound = 13
}

enum RadioType {
    /// <summary>
    /// No radio communication
    /// </summary>
    None = 1,

    /// <summary>
    /// Short range radio communication - appx. 3 kilometers
    /// </summary>
    ShortRange = 2,

    /// <summary>
    /// Long range radio communication - appx. 8 kilometers
    /// </summary>
    LongRange = 4,

    /// <summary>
    /// Distributed radio communication, depending on <see cref="RadioTower"/> - appx. 3 (short range) or 8 (long range) kilometers
    /// </summary>
    Distributed = 8,
}

enum Error {
    OK = 0,
    InvalidJson = 1,
    NotConnectedToServer = 2,
    AlreadyInGame = 3,
    ChannelNotAvailable = 4,
    NameNotAvailable = 5,
    InvalidValue = 6
}

enum UpdateBranch {
    Stable = 0,
    Testing = 1,
    PreBuild = 2
}
// Decompiled with JetBrains decompiler
// Type: StardewValley.Multiplayer
// Assembly: Stardew Valley, Version=1.3.6703.20634, Culture=neutral, PublicKeyToken=null
// MVID: DC31F653-4A63-4BED-95C5-27501CF0099B
// Assembly location: F:\Blue_Games\Steam\steamapps\common\Stardew Valley\Stardew Valley.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StardewValley
{
  public class Multiplayer
  {
    public int defaultInterpolationTicks = 15;
    public int farmerDeltaBroadcastPeriod = 3;
    public int locationDeltaBroadcastPeriod = 3;
    public int worldStateDeltaBroadcastPeriod = 3;
    public int playerLimit = 4;
    public string protocolVersion = "1.3-31";
    private List<long> disconnectingFarmers = new List<long>();
    public long latestID = long.MinValue + (long) Game1.random.Next(1000);
    public const byte farmerDelta = 0;
    public const byte serverIntroduction = 1;
    public const byte playerIntroduction = 2;
    public const byte locationIntroduction = 3;
    public const byte forceEvent = 4;
    public const byte warpFarmer = 5;
    public const byte locationDelta = 6;
    public const byte locationSprites = 7;
    public const byte characterWarp = 8;
    public const byte availableFarmhands = 9;
    public const byte chatMessage = 10;
    public const byte connectionMessage = 11;
    public const byte worldDelta = 12;
    public const byte teamDelta = 13;
    public const byte newDaySync = 14;
    public const byte chatInfoMessage = 15;
    public const byte userNameUpdate = 16;
    public const byte farmerGainExperience = 17;
    public const byte serverToClientsMessage = 18;
    public const byte disconnecting = 19;
    public long recentMultiplayerEntityID;
    public const string MSG_START_FESTIVAL_EVENT = "festivalEvent";
    public const string MSG_END_FESTIVAL = "endFest";
    public const string MSG_PLACEHOLDER = "[replace me]";

    public virtual long getNewID()
    {
      return this.latestID++;
    }

    public virtual int MaxPlayers
    {
      get
      {
        if (Game1.server == null)
          return 1;
        return this.playerLimit;
      }
    }

    public virtual bool isDisconnecting(Farmer farmer)
    {
      return this.isDisconnecting(farmer.UniqueMultiplayerID);
    }

    public virtual bool isDisconnecting(long uid)
    {
      return this.disconnectingFarmers.Contains(uid);
    }

    public virtual bool isClientBroadcastType(byte messageType)
    {
      switch (messageType)
      {
        case 0:
        case 2:
        case 4:
        case 6:
        case 7:
        case 10:
        case 13:
        case 14:
        case 15:
        case 19:
          return true;
        default:
          return false;
      }
    }

    public virtual bool allowSyncDelay()
    {
      return Game1.newDaySync == null;
    }

    public virtual int interpolationTicks()
    {
      if (!this.allowSyncDelay())
        return 0;
      return this.defaultInterpolationTicks;
    }

    public virtual IEnumerable<NetFarmerRoot> farmerRoots()
    {
      if ((NetFieldBase<Farmer, NetRef<Farmer>>) Game1.serverHost != (NetRef<Farmer>) null)
        yield return Game1.serverHost;
      foreach (NetRoot<Farmer> netRoot in Game1.otherFarmers.Roots.Values)
      {
        if ((NetFieldBase<Farmer, NetRef<Farmer>>) Game1.serverHost == (NetRef<Farmer>) null || (NetFieldBase<Farmer, NetRef<Farmer>>) netRoot != (NetRef<Farmer>) Game1.serverHost)
          yield return netRoot as NetFarmerRoot;
      }
    }

    public virtual NetFarmerRoot farmerRoot(long id)
    {
      if (id == Game1.serverHost.Value.UniqueMultiplayerID)
        return Game1.serverHost;
      if (Game1.otherFarmers.ContainsKey(id))
        return Game1.otherFarmers.Roots[id] as NetFarmerRoot;
      return (NetFarmerRoot) null;
    }

    public virtual void tickFarmerRoots()
    {
      foreach (NetFarmerRoot farmerRoot in this.farmerRoots())
      {
        farmerRoot.Clock.InterpolationTicks = this.interpolationTicks();
        farmerRoot.Tick();
      }
      Game1.player.teamRoot.Clock.InterpolationTicks = this.interpolationTicks();
      Game1.player.teamRoot.Tick();
    }

    public virtual void broadcastFarmerDeltas()
    {
      foreach (NetFarmerRoot farmerRoot in this.farmerRoots())
      {
        if (farmerRoot.Dirty && Game1.player.UniqueMultiplayerID == farmerRoot.Value.UniqueMultiplayerID)
          this.broadcastFarmerDelta(farmerRoot.Value, this.writeObjectDeltaBytes<Farmer>((NetRoot<Farmer>) farmerRoot));
      }
      if (!Game1.player.teamRoot.Dirty)
        return;
      this.broadcastTeamDelta(this.writeObjectDeltaBytes<FarmerTeam>(Game1.player.teamRoot));
    }

    protected virtual void broadcastTeamDelta(byte[] delta)
    {
      if (Game1.IsServer)
      {
        foreach (Farmer farmer in (IEnumerable<Farmer>) Game1.otherFarmers.Values)
        {
          if (farmer != Game1.player)
            Game1.server.sendMessage(farmer.UniqueMultiplayerID, (byte) 13, Game1.player, (object) delta);
        }
      }
      else
      {
        if (!Game1.IsClient)
          return;
        Game1.client.sendMessage((byte) 13, (object) delta);
      }
    }

    protected virtual void broadcastFarmerDelta(Farmer farmer, byte[] delta)
    {
      foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
      {
        if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
          otherFarmer.Value.queueMessage((byte) 0, farmer, (object) farmer.UniqueMultiplayerID, (object) delta);
      }
    }

    public virtual void tickLocationRoots()
    {
      if (Game1.IsClient)
      {
        foreach (GameLocation activeLocation in this.activeLocations())
        {
          if ((NetFieldBase<GameLocation, NetRef<GameLocation>>) activeLocation.Root != (NetRef<GameLocation>) null && activeLocation.Root.Value == activeLocation)
          {
            activeLocation.Root.Clock.InterpolationTicks = this.interpolationTicks();
            activeLocation.Root.Tick();
          }
        }
      }
      else
      {
        foreach (GameLocation location in (IEnumerable<GameLocation>) Game1.locations)
        {
          if ((NetFieldBase<GameLocation, NetRef<GameLocation>>) location.Root != (NetRef<GameLocation>) null)
          {
            location.Root.Clock.InterpolationTicks = this.interpolationTicks();
            location.Root.Tick();
          }
        }
        MineShaft.ForEach((Action<MineShaft>) (mine =>
        {
          if (!((NetFieldBase<GameLocation, NetRef<GameLocation>>) mine.Root != (NetRef<GameLocation>) null))
            return;
          mine.Root.Clock.InterpolationTicks = this.interpolationTicks();
          mine.Root.Tick();
        }));
      }
    }

    public virtual void broadcastLocationDeltas()
    {
      if (Game1.IsClient)
      {
        if (Game1.currentLocation == null || !((NetFieldBase<GameLocation, NetRef<GameLocation>>) Game1.currentLocation.Root != (NetRef<GameLocation>) null) || !Game1.currentLocation.Root.Dirty)
          return;
        this.broadcastLocationDelta(Game1.currentLocation);
      }
      else
      {
        foreach (GameLocation location in (IEnumerable<GameLocation>) Game1.locations)
        {
          if ((NetFieldBase<GameLocation, NetRef<GameLocation>>) location.Root != (NetRef<GameLocation>) null && location.Root.Dirty)
            this.broadcastLocationDelta(location);
        }
        MineShaft.ForEach((Action<MineShaft>) (mine =>
        {
          if (!((NetFieldBase<GameLocation, NetRef<GameLocation>>) mine.Root != (NetRef<GameLocation>) null) || !mine.Root.Dirty)
            return;
          this.broadcastLocationDelta((GameLocation) mine);
        }));
      }
    }

    public virtual void broadcastLocationDelta(GameLocation loc)
    {
      if ((NetFieldBase<GameLocation, NetRef<GameLocation>>) loc.Root == (NetRef<GameLocation>) null || !loc.Root.Dirty)
        return;
      byte[] bytes = this.writeObjectDeltaBytes<GameLocation>(loc.Root);
      this.broadcastLocationBytes(loc, (byte) 6, bytes);
    }

    protected virtual void broadcastLocationBytes(GameLocation loc, byte messageType, byte[] bytes)
    {
      OutgoingMessage message = new OutgoingMessage(messageType, Game1.player, new object[3]
      {
        (object) loc.isStructure.Value,
        (bool) ((NetFieldBase<bool, NetBool>) loc.isStructure) ? (object) loc.uniqueName.Value : (object) loc.name.Value,
        (object) bytes
      });
      this.broadcastLocationMessage(loc, message);
    }

    protected virtual void broadcastLocationMessage(GameLocation loc, OutgoingMessage message)
    {
      if (Game1.IsClient)
      {
        Game1.client.sendMessage(message);
      }
      else
      {
        Action<Farmer> action = (Action<Farmer>) (f =>
        {
          if (f == Game1.player)
            return;
          Game1.server.sendMessage(f.UniqueMultiplayerID, message);
        });
        if (this.isAlwaysActiveLocation(loc))
        {
          foreach (Farmer farmer in (IEnumerable<Farmer>) Game1.otherFarmers.Values)
            action(farmer);
        }
        else
        {
          foreach (Farmer farmer in loc.farmers)
            action(farmer);
          if (!(loc is BuildableGameLocation))
            return;
          foreach (Building building in (loc as BuildableGameLocation).buildings)
          {
            if (building.indoors.Value != null)
            {
              foreach (Farmer farmer in building.indoors.Value.farmers)
                action(farmer);
            }
          }
        }
      }
    }

    public virtual void broadcastSprites(GameLocation location, List<TemporaryAnimatedSprite> sprites)
    {
      this.broadcastSprites(location, sprites.ToArray());
    }

    public virtual void broadcastSprites(GameLocation location, params TemporaryAnimatedSprite[] sprites)
    {
      location.temporarySprites.AddRange((IEnumerable<TemporaryAnimatedSprite>) sprites);
      if (sprites.Length == 0 || !Game1.IsMultiplayer)
        return;
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter((Stream) memoryStream))
        {
          writer.Write(sprites.Length);
          foreach (TemporaryAnimatedSprite sprite in sprites)
            sprite.Write(writer, location);
        }
        this.broadcastLocationBytes(location, (byte) 7, memoryStream.ToArray());
      }
    }

    public virtual void broadcastWorldStateDeltas()
    {
      if (!Game1.netWorldState.Dirty)
        return;
      Game1.netWorldState.Tick();
      byte[] numArray = this.writeObjectDeltaBytes<IWorldState>(Game1.netWorldState);
      foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
      {
        if (otherFarmer.Value != Game1.player)
          otherFarmer.Value.queueMessage((byte) 12, Game1.player, (object) numArray);
      }
    }

    public virtual void receiveWorldState(BinaryReader msg)
    {
      this.readObjectDelta<IWorldState>(msg, Game1.netWorldState);
      if (Game1.IsServer)
        return;
      int timeOfDay1 = Game1.timeOfDay;
      Game1.netWorldState.Value.WriteToGame1();
      int timeOfDay2 = Game1.timeOfDay;
      if (timeOfDay1 == timeOfDay2 || Game1.currentLocation == null || Game1.newDaySync != null)
        return;
      Game1.performTenMinuteClockUpdate();
    }

    public virtual void requestCharacterWarp(NPC character, GameLocation targetLocation, Vector2 position)
    {
      if (!Game1.IsClient)
        return;
      GameLocation currentLocation = character.currentLocation;
      if (currentLocation == null)
        throw new ArgumentException("In warpCharacter, the character's currentLocation must not be null");
      Guid guid = currentLocation.characters.GuidOf(character);
      if (guid == Guid.Empty)
        throw new ArgumentException("In warpCharacter, the character must be in its currentLocation");
      OutgoingMessage message = new OutgoingMessage((byte) 8, Game1.player, new object[6]
      {
        (object) currentLocation.isStructure.Value,
        (bool) ((NetFieldBase<bool, NetBool>) currentLocation.isStructure) ? (object) currentLocation.uniqueName.Value : (object) currentLocation.name.Value,
        (object) guid,
        (object) targetLocation.isStructure.Value,
        (bool) ((NetFieldBase<bool, NetBool>) targetLocation.isStructure) ? (object) targetLocation.uniqueName.Value : (object) targetLocation.name.Value,
        (object) position
      });
      Game1.serverHost.Value.queueMessage(message);
    }

    public virtual NetRoot<GameLocation> locationRoot(GameLocation location)
    {
      if ((NetFieldBase<GameLocation, NetRef<GameLocation>>) location.Root == (NetRef<GameLocation>) null && Game1.IsMasterGame)
      {
        new NetRoot<GameLocation>().Set(location);
        location.Root.Clock.InterpolationTicks = this.interpolationTicks();
        location.Root.MarkClean();
      }
      return location.Root;
    }

    public virtual void broadcastEvent(Event evt, GameLocation location, Vector2 positionBeforeEvent)
    {
      if (evt.id == -1)
        return;
      object[] objArray = new object[5]
      {
        (object) evt.id,
        (object) (int) positionBeforeEvent.X,
        (object) (int) positionBeforeEvent.Y,
        (object) (byte) ((bool) ((NetFieldBase<bool, NetBool>) location.isStructure) ? 1 : 0),
        (bool) ((NetFieldBase<bool, NetBool>) location.isStructure) ? (object) location.uniqueName.Value : (object) location.Name
      };
      if (Game1.IsServer)
      {
        foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
        {
          if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
            Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, (byte) 4, Game1.player, objArray);
        }
      }
      else
      {
        if (!Game1.IsClient)
          return;
        Game1.client.sendMessage((byte) 4, objArray);
      }
    }

    public virtual NetRoot<T> readObjectFull<T>(BinaryReader reader) where T : INetObject<INetSerializable>
    {
      NetRoot<T> netRoot = NetRoot<T>.Connect(reader);
      netRoot.Clock.InterpolationTicks = this.defaultInterpolationTicks;
      return netRoot;
    }

    public virtual void writeObjectFull<T>(BinaryWriter writer, NetRoot<T> root, long? peer) where T : INetObject<INetSerializable>
    {
      root.CreateConnectionPacket(writer, peer);
    }

    public virtual byte[] writeObjectFullBytes<T>(NetRoot<T> root, long? peer) where T : INetObject<INetSerializable>
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter((Stream) memoryStream))
        {
          root.CreateConnectionPacket(writer, peer);
          return memoryStream.ToArray();
        }
      }
    }

    public virtual void readObjectDelta<T>(BinaryReader reader, NetRoot<T> root) where T : INetObject<INetSerializable>
    {
      root.Read(reader);
    }

    public virtual void writeObjectDelta<T>(BinaryWriter writer, NetRoot<T> root) where T : INetObject<INetSerializable>
    {
      root.Write(writer);
    }

    public virtual byte[] writeObjectDeltaBytes<T>(NetRoot<T> root) where T : INetObject<INetSerializable>
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (BinaryWriter writer = new BinaryWriter((Stream) memoryStream))
        {
          root.Write(writer);
          return memoryStream.ToArray();
        }
      }
    }

    public virtual NetFarmerRoot readFarmer(BinaryReader reader)
    {
      NetFarmerRoot netFarmerRoot = new NetFarmerRoot();
      netFarmerRoot.ReadConnectionPacket(reader);
      netFarmerRoot.Clock.InterpolationTicks = this.defaultInterpolationTicks;
      return netFarmerRoot;
    }

    public virtual void addPlayer(NetFarmerRoot f)
    {
      long uniqueMultiplayerId = f.Value.UniqueMultiplayerID;
      f.Value.teamRoot = Game1.player.teamRoot;
      Game1.otherFarmers.Roots[uniqueMultiplayerId] = (NetRoot<Farmer>) f;
      this.disconnectingFarmers.Remove(uniqueMultiplayerId);
      if (Game1.chatBox == null)
        return;
      string str = ChatBox.formattedUserNameLong(f.Value);
      Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerJoined", (object) str));
    }

    public virtual void receivePlayerIntroduction(BinaryReader reader)
    {
      this.addPlayer(this.readFarmer(reader));
    }

    public virtual void broadcastPlayerIntroduction(NetFarmerRoot farmerRoot)
    {
      if (Game1.server == null)
        return;
      foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
      {
        if (farmerRoot.Value.UniqueMultiplayerID != otherFarmer.Value.UniqueMultiplayerID)
          Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, (byte) 2, farmerRoot.Value, (object) Game1.server.getUserName(farmerRoot.Value.UniqueMultiplayerID), (object) this.writeObjectFullBytes<Farmer>((NetRoot<Farmer>) farmerRoot, new long?(otherFarmer.Value.UniqueMultiplayerID)));
      }
    }

    public virtual void broadcastUserName(long farmerId, string userName)
    {
      if (Game1.server != null)
        return;
      foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
      {
        Farmer farmer = otherFarmer.Value;
        if (farmer.UniqueMultiplayerID != farmerId)
          Game1.server.sendMessage(farmer.UniqueMultiplayerID, (byte) 16, Game1.serverHost.Value, (object) farmerId, (object) userName);
      }
    }

    public virtual string getUserName(long id)
    {
      if (id == Game1.player.UniqueMultiplayerID)
        return Game1.content.LoadString("Strings\\UI:Chat_SelfPlayerID");
      if (Game1.server != null)
        return Game1.server.getUserName(id);
      if (Game1.client != null)
        return Game1.client.getUserName(id);
      return "?";
    }

    public virtual void playerDisconnected(long id)
    {
      if (!Game1.otherFarmers.ContainsKey(id) || this.disconnectingFarmers.Contains(id))
        return;
      if (Game1.IsMasterGame)
        this.saveFarmhand(Game1.otherFarmers.Roots[id] as NetFarmerRoot);
      if (Game1.chatBox != null)
        Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerLeft", (object) ChatBox.formattedUserNameLong(Game1.otherFarmers[id])));
      this.disconnectingFarmers.Add(id);
    }

    protected virtual void removeDisconnectedFarmers()
    {
      foreach (long disconnectingFarmer in this.disconnectingFarmers)
        Game1.otherFarmers.Remove(disconnectingFarmer);
      this.disconnectingFarmers.Clear();
    }

    public virtual void sendFarmhand()
    {
      (Game1.player.NetFields.Root as NetFarmerRoot).MarkReassigned();
    }

    protected virtual void saveFarmhand(NetFarmerRoot farmhand)
    {
      FarmHouse homeOfFarmer = Utility.getHomeOfFarmer((Farmer) ((NetFieldBase<Farmer, NetRef<Farmer>>) farmhand));
      if (!(homeOfFarmer is Cabin))
        return;
      (homeOfFarmer as Cabin).saveFarmhand(farmhand);
    }

    public virtual void saveFarmhands()
    {
      if (!Game1.IsMasterGame)
        return;
      foreach (NetRoot<Farmer> netRoot in Game1.otherFarmers.Roots.Values)
        this.saveFarmhand(netRoot as NetFarmerRoot);
    }

    public virtual void clientRemotelyDisconnected()
    {
      Game1.ExitToTitle((Action) (() =>
      {
        ConfirmationDialog confirmationDialog = new ConfirmationDialog(Game1.content.LoadString("Strings\\UI:Client_RemotelyDisconnected"), (ConfirmationDialog.behavior) null, (ConfirmationDialog.behavior) null);
        confirmationDialog.okButton.visible = false;
        (Game1.activeClickableMenu as TitleMenu).skipToTitleButtons();
        TitleMenu.subMenu = (IClickableMenu) confirmationDialog;
      }));
    }

    public virtual void sendServerToClientsMessage(string message)
    {
      if (!Game1.IsServer)
        return;
      foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
        otherFarmer.Value.queueMessage((byte) 18, Game1.player, (object) message);
    }

    public virtual void sendChatMessage(LocalizedContentManager.LanguageCode language, string message)
    {
      if (Game1.IsClient)
      {
        Game1.client.sendMessage((byte) 10, (object) language, (object) message);
      }
      else
      {
        if (!Game1.IsServer)
          return;
        foreach (long key in (IEnumerable<long>) Game1.otherFarmers.Keys)
          Game1.server.sendMessage(key, (byte) 10, Game1.player, (object) language, (object) message);
      }
    }

    public virtual void receiveChatMessage(Farmer sourceFarmer, LocalizedContentManager.LanguageCode language, string message)
    {
      if (Game1.chatBox == null)
        return;
      Game1.chatBox.receiveChatMessage(sourceFarmer.UniqueMultiplayerID, 0, language, message);
    }

    public virtual void globalChatInfoMessage(string messageKey, params string[] args)
    {
      if (!Game1.IsMultiplayer)
        return;
      this.receiveChatInfoMessage(Game1.player, messageKey, args);
      this.sendChatInfoMessage(messageKey, args);
    }

    protected virtual void sendChatInfoMessage(string messageKey, params string[] args)
    {
      if (Game1.IsClient)
      {
        Game1.client.sendMessage((byte) 15, (object) messageKey, (object) args);
      }
      else
      {
        if (!Game1.IsServer)
          return;
        foreach (long key in (IEnumerable<long>) Game1.otherFarmers.Keys)
          Game1.server.sendMessage(key, (byte) 15, Game1.player, (object) messageKey, (object) args);
      }
    }

    protected virtual void receiveChatInfoMessage(Farmer sourceFarmer, string messageKey, string[] args)
    {
      if (Game1.chatBox == null)
        return;
      try
      {
        string[] array = ((IEnumerable<string>) args).Select<string, string>((Func<string, string>) (arg =>
        {
          if (!arg.StartsWith("achievement:"))
            return arg;
          int int32 = Convert.ToInt32(arg.Substring("achievement:".Length));
          return Game1.content.Load<Dictionary<int, string>>("Data\\Achievements")[int32].Split('^')[0];
        })).ToArray<string>();
        Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_" + messageKey, (object[]) array));
      }
      catch (ContentLoadException ex)
      {
      }
      catch (FormatException ex)
      {
      }
      catch (OverflowException ex)
      {
      }
      catch (KeyNotFoundException ex)
      {
      }
    }

    public virtual void parseServerToClientsMessage(string message)
    {
      if (!Game1.IsClient)
        return;
      if (!(message == "festivalEvent"))
      {
        if (!(message == "endFest") || Game1.CurrentEvent == null)
          return;
        Game1.CurrentEvent.forceEndFestival(Game1.player);
      }
      else
      {
        if (Game1.currentLocation.currentEvent == null)
          return;
        Game1.currentLocation.currentEvent.forceFestivalContinue();
      }
    }

    public virtual IEnumerable<GameLocation> activeLocations()
    {
      if (Game1.currentLocation != null)
        yield return Game1.currentLocation;
      Farm farm = Game1.getFarm();
      if (farm != null && farm != Game1.currentLocation)
        yield return (GameLocation) farm;
      GameLocation locationFromName = Game1.getLocationFromName("FarmHouse");
      if (locationFromName != null && locationFromName != Game1.currentLocation)
        yield return locationFromName;
      foreach (Building building in farm.buildings)
      {
        if (building.indoors.Value != null && building.indoors.Value != Game1.currentLocation)
          yield return building.indoors.Value;
      }
    }

    public virtual bool isAlwaysActiveLocation(GameLocation location)
    {
      if (location.Name == "Farm" || location.Name == "FarmHouse")
        return true;
      if ((NetFieldBase<GameLocation, NetRef<GameLocation>>) location.Root != (NetRef<GameLocation>) null)
        return location.Root.Value.Equals((GameLocation) Game1.getFarm());
      return false;
    }

    protected virtual void readActiveLocation(IncomingMessage msg, bool forceCurrentLocation = false)
    {
      NetRoot<GameLocation> netRoot = this.readObjectFull<GameLocation>(msg.Reader);
      if (this.isAlwaysActiveLocation(netRoot.Value))
      {
        for (int index = 0; index < Game1.locations.Count; ++index)
        {
          if (Game1.locations[index].Equals(netRoot.Value))
          {
            Game1.locations[index] = netRoot.Value;
            break;
          }
        }
      }
      if (!(Game1.locationRequest != null | forceCurrentLocation))
        return;
      if (Game1.locationRequest != null)
      {
        Game1.currentLocation = Game1.findStructure(netRoot.Value, Game1.locationRequest.Name);
        if (Game1.currentLocation == null)
          Game1.currentLocation = netRoot.Value;
      }
      else if (forceCurrentLocation)
        Game1.currentLocation = netRoot.Value;
      if (Game1.locationRequest != null)
        Game1.locationRequest.Loaded(netRoot.Value);
      Game1.currentLocation.resetForPlayerEntry();
      Game1.player.currentLocation = Game1.currentLocation;
      if (Game1.locationRequest != null)
        Game1.locationRequest.Warped(netRoot.Value);
      Game1.currentLocation.updateSeasonalTileSheets();
      if (Game1.isDebrisWeather)
        Game1.populateDebrisWeatherArray();
      Game1.locationRequest = (LocationRequest) null;
    }

    public virtual bool isActiveLocation(GameLocation location)
    {
      return Game1.IsMasterGame || Game1.currentLocation != null && (NetFieldBase<GameLocation, NetRef<GameLocation>>) Game1.currentLocation.Root != (NetRef<GameLocation>) null && Game1.currentLocation.Root.Value == location.Root.Value || this.isAlwaysActiveLocation(location);
    }

    protected virtual GameLocation readLocation(BinaryReader reader)
    {
      bool isStructure = reader.ReadByte() > (byte) 0;
      GameLocation locationFromName = Game1.getLocationFromName(reader.ReadString(), isStructure);
      if (locationFromName == null || (NetFieldBase<GameLocation, NetRef<GameLocation>>) this.locationRoot(locationFromName) == (NetRef<GameLocation>) null)
        return (GameLocation) null;
      if (!this.isActiveLocation(locationFromName))
        return (GameLocation) null;
      return locationFromName;
    }

    protected virtual LocationRequest readLocationRequest(BinaryReader reader)
    {
      bool isStructure = reader.ReadByte() > (byte) 0;
      return Game1.getLocationRequest(reader.ReadString(), isStructure);
    }

    protected virtual void readWarp(BinaryReader reader, int tileX, int tileY, Action afterWarp)
    {
      LocationRequest locationRequest = this.readLocationRequest(reader);
      if (afterWarp != null)
        locationRequest.OnWarp += new LocationRequest.Callback(afterWarp.Invoke);
      Game1.warpFarmer(locationRequest, tileX, tileY, Game1.player.FacingDirection);
    }

    protected virtual NPC readNPC(BinaryReader reader)
    {
      GameLocation gameLocation = this.readLocation(reader);
      Guid guid = reader.ReadGuid();
      if (!gameLocation.characters.ContainsGuid(guid))
        return (NPC) null;
      return gameLocation.characters[guid];
    }

    public virtual TemporaryAnimatedSprite[] readSprites(BinaryReader reader, GameLocation location)
    {
      int length = reader.ReadInt32();
      TemporaryAnimatedSprite[] temporaryAnimatedSpriteArray = new TemporaryAnimatedSprite[length];
      for (int index = 0; index < length; ++index)
      {
        TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite();
        temporaryAnimatedSprite.Read(reader, location);
        temporaryAnimatedSprite.ticksBeforeAnimationStart += this.interpolationTicks();
        temporaryAnimatedSpriteArray[index] = temporaryAnimatedSprite;
      }
      return temporaryAnimatedSpriteArray;
    }

    protected virtual void receiveTeamDelta(BinaryReader msg)
    {
      this.readObjectDelta<FarmerTeam>(msg, Game1.player.teamRoot);
    }

    protected virtual void receiveNewDaySync(IncomingMessage msg)
    {
      if (Game1.newDaySync == null && msg.SourceFarmer == Game1.serverHost.Value)
        Game1.NewDay(0.0f);
      if (Game1.newDaySync == null)
        return;
      Game1.newDaySync.receiveMessage(msg);
    }

    protected virtual void receiveFarmerGainExperience(IncomingMessage msg)
    {
      if (msg.SourceFarmer != Game1.serverHost.Value)
        return;
      Game1.player.gainExperience(msg.Reader.ReadInt32(), msg.Reader.ReadInt32());
    }

    public virtual void processIncomingMessage(IncomingMessage msg)
    {
      switch (msg.MessageType)
      {
        case 0:
          NetFarmerRoot netFarmerRoot = this.farmerRoot(msg.Reader.ReadInt64());
          this.readObjectDelta<Farmer>(msg.Reader, (NetRoot<Farmer>) netFarmerRoot);
          break;
        case 2:
          this.receivePlayerIntroduction(msg.Reader);
          break;
        case 3:
          this.readActiveLocation(msg, false);
          break;
        case 4:
          int eventId = msg.Reader.ReadInt32();
          int tileX = msg.Reader.ReadInt32();
          int tileY = msg.Reader.ReadInt32();
          if (Game1.CurrentEvent != null)
            break;
          this.readWarp(msg.Reader, tileX, tileY, (Action) (() =>
          {
            Farmer farmerActor = (msg.SourceFarmer.NetFields.Root as NetRoot<Farmer>).Clone().Value;
            farmerActor.currentLocation = Game1.currentLocation;
            farmerActor.completelyStopAnimatingOrDoingAction();
            farmerActor.hidden.Value = false;
            Event eventById = Game1.currentLocation.findEventById(eventId, farmerActor);
            Game1.currentLocation.startEvent(eventById);
            farmerActor.Position = Game1.player.Position;
          }));
          break;
        case 6:
          GameLocation gameLocation = this.readLocation(msg.Reader);
          if (gameLocation == null)
            break;
          this.readObjectDelta<GameLocation>(msg.Reader, gameLocation.Root);
          break;
        case 7:
          GameLocation location = this.readLocation(msg.Reader);
          if (location == null)
            break;
          location.temporarySprites.AddRange((IEnumerable<TemporaryAnimatedSprite>) this.readSprites(msg.Reader, location));
          break;
        case 8:
          NPC character = this.readNPC(msg.Reader);
          GameLocation targetLocation = this.readLocation(msg.Reader);
          if (character == null || targetLocation == null)
            break;
          Game1.warpCharacter(character, targetLocation, msg.Reader.ReadVector2());
          break;
        case 10:
          this.receiveChatMessage(msg.SourceFarmer, msg.Reader.ReadEnum<LocalizedContentManager.LanguageCode>(), msg.Reader.ReadString());
          break;
        case 12:
          this.receiveWorldState(msg.Reader);
          break;
        case 13:
          this.receiveTeamDelta(msg.Reader);
          break;
        case 14:
          this.receiveNewDaySync(msg);
          break;
        case 15:
          string messageKey = msg.Reader.ReadString();
          string[] args = new string[(int) msg.Reader.ReadByte()];
          for (int index = 0; index < args.Length; ++index)
            args[index] = msg.Reader.ReadString();
          this.receiveChatInfoMessage(msg.SourceFarmer, messageKey, args);
          break;
        case 17:
          this.receiveFarmerGainExperience(msg);
          break;
        case 18:
          this.parseServerToClientsMessage(msg.Reader.ReadString());
          break;
        case 19:
          this.playerDisconnected(msg.SourceFarmer.UniqueMultiplayerID);
          break;
      }
    }

    public virtual void StartServer()
    {
      Game1.server = (IGameServer) new GameServer();
      Game1.server.startServer();
    }

    public virtual void Disconnect()
    {
      if (Game1.server != null)
      {
        Game1.server.stopServer();
        Game1.server = (IGameServer) null;
        this.saveFarmhands();
      }
      if (Game1.client != null)
      {
        this.sendFarmhand();
        this.UpdateLate(true);
        Game1.client.disconnect(true);
        Game1.client = (Client) null;
      }
      Game1.otherFarmers.Clear();
    }

    protected virtual void updatePendingConnections()
    {
      switch (Game1.multiplayerMode)
      {
        case 1:
          if (Game1.client == null || Game1.client.readyToPlay)
            break;
          Game1.client.receiveMessages();
          break;
        case 2:
          if (Game1.server != null || !Game1.options.enableServer)
            break;
          this.StartServer();
          break;
      }
    }

    public virtual void UpdateEarly()
    {
      if (Game1.CurrentEvent == null)
        this.removeDisconnectedFarmers();
      this.updatePendingConnections();
      if (Game1.server != null)
        Game1.server.receiveMessages();
      else if (Game1.client != null)
        Game1.client.receiveMessages();
      this.tickFarmerRoots();
      this.tickLocationRoots();
    }

    public virtual void UpdateLate(bool forceSync = false)
    {
      if (Game1.multiplayerMode != (byte) 0)
      {
        if (!this.allowSyncDelay() | forceSync || Game1.ticks % this.farmerDeltaBroadcastPeriod == 0)
          this.broadcastFarmerDeltas();
        if (!this.allowSyncDelay() | forceSync || Game1.ticks % this.locationDeltaBroadcastPeriod == 0)
          this.broadcastLocationDeltas();
        if (!this.allowSyncDelay() | forceSync || Game1.ticks % this.worldStateDeltaBroadcastPeriod == 0)
          this.broadcastWorldStateDeltas();
      }
      if (Game1.server != null)
        Game1.server.sendMessages();
      if (Game1.client == null)
        return;
      Game1.client.sendMessages();
    }

    public virtual void inviteAccepted()
    {
      if (!(Game1.activeClickableMenu is TitleMenu))
        return;
      TitleMenu activeClickableMenu = Game1.activeClickableMenu as TitleMenu;
      if (TitleMenu.subMenu == null)
      {
        activeClickableMenu.performButtonAction("Invite");
      }
      else
      {
        if (!(TitleMenu.subMenu is FarmhandMenu) && !(TitleMenu.subMenu is CoopMenu))
          return;
        TitleMenu.subMenu = (IClickableMenu) new FarmhandMenu();
      }
    }

    public virtual Client InitClient(Client client)
    {
      return client;
    }

    public virtual Server InitServer(Server server)
    {
      return server;
    }
  }
}

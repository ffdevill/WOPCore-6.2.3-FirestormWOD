/*
 * Copyright (C) 2008-2012 TrinityCore <http://www.trinitycore.org/>
 * Copyright (C) 2005-2009 MaNGOS <http://getmangos.com/>
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation; either version 2 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program. If not, see <http://www.gnu.org/licenses/>.
 */

#ifndef SC_SCRIPTMGR_H
#define SC_SCRIPTMGR_H

#include "Common.h"
#include <ace/Singleton.h>
#include <ace/Atomic_Op.h>

#include "DBCStores.h"
#include "Player.h"
#include "SharedDefines.h"
#include "World.h"
#include "Weather.h"

class AuctionHouseObject;
class AuraScript;
class Battleground;
class BattlegroundMap;
class Channel;
class ChatCommand;
class Creature;
class CreatureAI;
class DynamicObject;
class GameObject;
class GameObjectAI;
class Guild;
class GridMap;
class Group;
class InstanceMap;
class InstanceScript;
class Item;
class Map;
class OutdoorPvP;
class Player;
class Quest;
class ScriptMgr;
class Spell;
class SpellScript;
class SpellCastTargets;
class Transport;
class Unit;
class Vehicle;
class WorldPacket;
class WorldSocket;
class WorldObject;

struct AchievementCriteriaData;
struct AuctionEntry;
struct ConditionSourceInfo;
struct Condition;
struct ItemTemplate;
struct OutdoorPvPData;

#define VISIBLE_RANGE       166.0f                          //MAX visible range (size of grid)

// Generic scripting text function.
void DoScriptText(int32 textEntry, WorldObject* pSource, Unit* target = NULL);

/*
    TODO: Add more script type classes.

    MailScript
    SessionScript
    CollisionScript
    ArenaTeamScript

*/

/*
    Standard procedure when adding new script type classes:

    First of all, define the actual class, and have it inherit from ScriptObject, like so:

    class MyScriptType : public ScriptObject
    {
        uint32 _someId;

        private:

            void RegisterSelf();

        protected:

            MyScriptType(const char* name, uint32 someId)
                : ScriptObject(name), _someId(someId)
            {
                ScriptRegistry<MyScriptType>::AddScript(this);
            }

        public:

            // If a virtual function in your script type class is not necessarily
            // required to be overridden, just declare it virtual with an empty
            // body. If, on the other hand, it's logical only to override it (i.e.
            // if it's the only method in the class), make it pure virtual, by adding
            // = 0 to it.
            virtual void OnSomeEvent(uint32 someArg1, std::string& someArg2) { UNUSED(p_Player); }

            // This is a pure virtual function:
            virtual void OnAnotherEvent(uint32 someArg) = 0;
    }

    Next, you need to add a specialization for ScriptRegistry. Put this in the bottom of
    ScriptMgr.cpp:

    template class ScriptRegistry<MyScriptType>;

    Now, add a cleanup routine in ScriptMgr::~ScriptMgr:

    SCR_CLEAR(MyScriptType);

    Now your script type is good to go with the script system. What you need to do now
    is add functions to ScriptMgr that can be called from the core to actually trigger
    certain events. For example, in ScriptMgr.h:

    void OnSomeEvent(uint32 someArg1, std::string& someArg2);
    void OnAnotherEvent(uint32 someArg);

    In ScriptMgr.cpp:

    void ScriptMgr::OnSomeEvent(uint32 someArg1, std::string& someArg2)
    {
        FOREACH_SCRIPT(MyScriptType)->OnSomeEvent(someArg1, someArg2);
    }

    void ScriptMgr::OnAnotherEvent(uint32 someArg)
    {
        FOREACH_SCRIPT(MyScriptType)->OnAnotherEvent(someArg1, someArg2);
    }

    Now you simply call these two functions from anywhere in the core to trigger the
    event on all registered scripts of that type.
*/

class ScriptObject
{
    friend class ScriptMgr;

    public:

        // Do not override this in scripts; it should be overridden by the various script type classes. It indicates
        // whether or not this script type must be assigned in the database.
        virtual bool IsDatabaseBound() const { return false; }

        const std::string& GetName() const { return _name; }

    protected:

        ScriptObject(const char* name)
            : _name(name)
        {
        }

        virtual ~ScriptObject()
        {
        }

    private:

        const std::string _name;
};

template<class TObject> class UpdatableScript
{
    protected:
        UpdatableScript()
        {
        }

    public:
        virtual void OnUpdate(TObject* p_Object, uint32 p_Diff) 
        { 
            UNUSED(p_Object);
            UNUSED(p_Diff);
        }

};

class SpellScriptLoader : public ScriptObject
{
    protected:

        SpellScriptLoader(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Should return a fully valid SpellScript pointer.
        virtual SpellScript* GetSpellScript() const { return NULL; }

        // Should return a fully valid AuraScript pointer.
        virtual AuraScript* GetAuraScript() const { return NULL; }
};

class ServerScript : public ScriptObject
{
    protected:

        ServerScript(const char* name);

    public:

        // Called when reactive socket I/O is started (WorldSocketMgr).
        virtual void OnNetworkStart() { }

        // Called when reactive I/O is stopped.
        virtual void OnNetworkStop() { }

        // Called when a remote socket establishes a connection to the server. Do not store the socket object.
        virtual void OnSocketOpen(WorldSocket* /*socket*/) { }

        // Called when a socket is closed. Do not store the socket object, and do not rely on the connection
        // being open; it is not.
        virtual void OnSocketClose(WorldSocket* /*socket*/, bool /*wasNew*/) { }

        // Called when a packet is sent to a client. The packet object is a copy of the original packet, so reading
        // and modifying it is safe.
        virtual void OnPacketSend(WorldSocket* /*socket*/, WorldPacket& /*packet*/) { }

        // Called when a (valid) packet is received by a client. The packet object is a copy of the original packet, so
        // reading and modifying it is safe.
        virtual void OnPacketReceive(WorldSocket* /*socket*/, WorldPacket& /*packet*/) { }

        // Called when an invalid (unknown opcode) packet is received by a client. The packet is a reference to the orignal
        // packet; not a copy. This allows you to actually handle unknown packets (for whatever purpose).
        virtual void OnUnknownPacketReceive(WorldSocket* /*socket*/, WorldPacket& /*packet*/) {  }
};

class WorldScript : public ScriptObject
{
    protected:

        WorldScript(const char* name);

    public:

        // Called when the open/closed state of the world changes.
        virtual void OnOpenStateChange(bool /*open*/) {  }

        // Called after the world configuration is (re)loaded.
        virtual void OnConfigLoad(bool /*reload*/) {  }

        // Called before the message of the day is changed.
        virtual void OnMotdChange(std::string& /*newMotd*/) {  }

        // Called when a world shutdown is initiated.
        virtual void OnShutdownInitiate(ShutdownExitCode /*code*/, ShutdownMask /*mask*/) {  }

        // Called when a world shutdown is cancelled.
        virtual void OnShutdownCancel() {  }

        // Called on every world tick (don't execute too heavy code here).
        virtual void OnUpdate(uint32 /*diff*/) {  }

        // Called when the world is started.
        virtual void OnStartup() {  }

        // Called when the world is actually shut down.
        virtual void OnShutdown() {  }
};

class FormulaScript : public ScriptObject
{
    protected:

        FormulaScript(const char* name);

    public:

        // Called after calculating honor.
        virtual void OnHonorCalculation(float& /*honor*/, uint8 /*level*/, float /*multiplier*/) { }

        // Called after gray level calculation.
        virtual void OnGrayLevelCalculation(uint8& /*grayLevel*/, uint8 /*playerLevel*/) { }

        // Called after calculating experience color.
        virtual void OnColorCodeCalculation(XPColorChar& /*color*/, uint8 /*playerLevel*/, uint8 /*mobLevel*/) {  }

        // Called after calculating zero difference.
        virtual void OnZeroDifferenceCalculation(uint8& /*diff*/, uint8 /*playerLevel*/) { }

        // Called after calculating base experience gain.
        virtual void OnBaseGainCalculation(uint32& /*gain*/, uint8 /*playerLevel*/, uint8 /*mobLevel*/, ContentLevels /*content*/) { }

        // Called after calculating experience gain.
        virtual void OnGainCalculation(uint32& /*gain*/, Player* p_Player, Unit* /*unit*/) { UNUSED(p_Player); }

        // Called when calculating the experience rate for group experience.
        virtual void OnGroupRateCalculation(float& /*rate*/, uint32 /*count*/, bool /*isRaid*/) { }
};

template<class TMap> class MapScript : public UpdatableScript<TMap>
{
    MapEntry const* _mapEntry;

    protected:
        MapScript(uint32 mapId)
            : _mapEntry(sMapStore.LookupEntry(mapId))
        {
            if (!_mapEntry)
                sLog->outError(LOG_FILTER_TSCR, "Invalid MapScript for %u; no such map ID.", mapId);
        }

    public:

        // Gets the MapEntry structure associated with this script. Can return NULL.
        MapEntry const* GetEntry() { return _mapEntry; }

        // Called when the map is created.
        virtual void OnCreate(TMap* p_Map) { UNUSED(p_Map); }

        // Called just before the map is destroyed.
        virtual void OnDestroy(TMap* p_Map) { UNUSED(p_Map); }

        // Called when a grid map is loaded.
        virtual void OnLoadGridMap(TMap* p_Map, GridMap* /*gmap*/, uint32 /*gx*/, uint32 /*gy*/) { UNUSED(p_Map); }

        // Called when a grid map is unloaded.
        virtual void OnUnloadGridMap(TMap* p_Map, GridMap* /*gmap*/, uint32 /*gx*/, uint32 /*gy*/)  { UNUSED(p_Map); }

        // Called when a player enters the map.
        virtual void OnPlayerEnter(TMap* p_Map, Player* p_Player) { UNUSED(p_Map); }

        // Called when a player leaves the map.
        virtual void OnPlayerLeave(TMap* p_Map, Player* p_Player) { UNUSED(p_Map); }

        // Called on every map update tick.
        virtual void OnUpdate(TMap* p_Map, uint32 /*diff*/) { UNUSED(p_Map); }
};

class WorldMapScript : public ScriptObject, public MapScript<Map>
{
    protected:

        WorldMapScript(const char* name, uint32 mapId);
};

class InstanceMapScript : public ScriptObject, public MapScript<InstanceMap>
{
    protected:

        InstanceMapScript(const char* name, uint32 mapId);

    public:

        bool IsDatabaseBound() const { return true; }

        // Gets an InstanceScript object for this instance.
        virtual InstanceScript* GetInstanceScript(InstanceMap* /*map*/) const { return NULL; }
};

class BattlegroundMapScript : public ScriptObject, public MapScript<BattlegroundMap>
{
    protected:

        BattlegroundMapScript(const char* name, uint32 mapId);
};

class ItemScript : public ScriptObject
{
    protected:

        ItemScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Called when a dummy spell effect is triggered on the item.
        virtual bool OnDummyEffect(Unit* /*caster*/, uint32 /*spellId*/, SpellEffIndex /*effIndex*/, Item* /*target*/) { return false; }

        // Called when a player accepts a quest from the item.
        virtual bool OnQuestAccept(Player* p_Player, Item* /*item*/, Quest const* /*quest*/) { return false; }

        // Called when a player uses the item.
        virtual bool OnUse(Player* p_Player, Item* /*item*/, SpellCastTargets const& /*targets*/) { return false; }

        // Called when the item expires (is destroyed).
        virtual bool OnExpire(Player* p_Player, ItemTemplate const* /*proto*/) { return false; }
};

class CreatureScript : public ScriptObject, public UpdatableScript<Creature>
{
    protected:

        CreatureScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Called when a dummy spell effect is triggered on the creature.
        virtual bool OnDummyEffect(Unit* /*caster*/, uint32 /*spellId*/, SpellEffIndex /*effIndex*/, Creature* /*target*/) { return false; }

        // Called when a player opens a gossip dialog with the creature.
        virtual bool OnGossipHello(Player* p_Player, Creature* /*creature*/) { return false; }

        // Called when a player selects a gossip item in the creature's gossip menu.
        virtual bool OnGossipSelect(Player* p_Player, Creature* /*creature*/, uint32 /*sender*/, uint32 /*action*/) { return false; }

        // Called when a player selects a gossip with a code in the creature's gossip menu.
        virtual bool OnGossipSelectCode(Player* p_Player, Creature* /*creature*/, uint32 /*sender*/, uint32 /*action*/, const char* /*code*/) { return false; }

        // Called when a player accepts a quest from the creature.
        virtual bool OnQuestAccept(Player* p_Player, Creature* /*creature*/, Quest const* /*quest*/) { return false; }

        // Called when a player selects a quest in the creature's quest menu.
        virtual bool OnQuestSelect(Player* p_Player, Creature* /*creature*/, Quest const* /*quest*/) { return false; }

        // Called when a player completes a quest with the creature.
        virtual bool OnQuestComplete(Player* p_Player, Creature* /*creature*/, Quest const* /*quest*/) { return false; }

        // Called when a player selects a quest reward.
        virtual bool OnQuestReward(Player* p_Player, Creature* /*creature*/, Quest const* /*quest*/, uint32 /*opt*/) { return false; }

        // Called when the dialog status between a player and the creature is requested.
        virtual uint32 GetDialogStatus(Player* p_Player, Creature* /*creature*/) { return 100; }

        // Called when a CreatureAI object is needed for the creature.
        virtual CreatureAI* GetAI(Creature* /*creature*/) const { return NULL; }
};

class GameObjectScript : public ScriptObject, public UpdatableScript<GameObject>
{
    protected:

        GameObjectScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Called when a dummy spell effect is triggered on the gameobject.
        virtual bool OnDummyEffect(Unit* /*caster*/, uint32 /*spellId*/, SpellEffIndex /*effIndex*/, GameObject* /*target*/) { return false; }

        // Called when a player opens a gossip dialog with the gameobject.
        virtual bool OnGossipHello(Player* p_Player, GameObject* /*go*/) { return false; }

        // Called when a player selects a gossip item in the gameobject's gossip menu.
        virtual bool OnGossipSelect(Player* p_Player, GameObject* /*go*/, uint32 /*sender*/, uint32 /*action*/) { return false; }

        // Called when a player selects a gossip with a code in the gameobject's gossip menu.
        virtual bool OnGossipSelectCode(Player* p_Player, GameObject* /*go*/, uint32 /*sender*/, uint32 /*action*/, const char* /*code*/) { return false; }

        // Called when a player accepts a quest from the gameobject.
        virtual bool OnQuestAccept(Player* p_Player, GameObject* /*go*/, Quest const* /*quest*/) { return false; }

        // Called when a player selects a quest reward.
        virtual bool OnQuestReward(Player* p_Player, GameObject* /*go*/, Quest const* /*quest*/, uint32 /*opt*/) { return false; }

        // Called when the dialog status between a player and the gameobject is requested.
        virtual uint32 GetDialogStatus(Player* p_Player, GameObject* /*go*/) { return 100; }

        // Called when the game object is destroyed (destructible buildings only).
        virtual void OnDestroyed(GameObject* p_GameObject, Player* p_Player)
        {
            UNUSED(p_GameObject);
        }
        // Called when the game object is damaged (destructible buildings only).
        virtual void OnDamaged(GameObject* p_GameObject, Player* p_Player)
        {
            UNUSED(p_GameObject);
        }
        // Called when the game object loot state is changed.
        virtual void OnLootStateChanged(GameObject* p_GameObject, uint32 /*state*/, Unit* /*unit*/)
        { 
            UNUSED(p_GameObject);
        }
        // Called when the game object state is changed.
        virtual void OnGameObjectStateChanged(const GameObject * p_GameObject, uint32 /*state*/)
        {
            UNUSED(p_GameObject);
        }

        // Called when server want to send elevator update, by default all gameobject type transport are elevator
        virtual bool OnGameObjectElevatorCheck(GameObject const* /*go*/) const { return true; }

        // Called when a GameObjectAI object is needed for the gameobject.
        virtual GameObjectAI* GetAI(GameObject* /*go*/) const { return NULL; }
};

class AreaTriggerScript : public ScriptObject
{
    protected:
        AreaTriggerScript(const char* name);

    public:
        bool IsDatabaseBound() const { return true; }

        // Called when the area trigger is activated by a player.
        virtual bool OnTrigger(Player* player, AreaTriggerEntry const* trigger) 
        {
            UNUSED(player);
            UNUSED(trigger);

            return false;
        }

        // Called on each update of AreaTriggers.
        virtual void OnUpdate(AreaTrigger* p_AreaTrigger)
        {
            UNUSED(p_AreaTrigger);
        }
};

class BattlegroundScript : public ScriptObject
{
    protected:

        BattlegroundScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Should return a fully valid Battleground object for the type ID.
        virtual Battleground* GetBattleground() const = 0;
};

class OutdoorPvPScript : public ScriptObject
{
    protected:

        OutdoorPvPScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Should return a fully valid OutdoorPvP object for the type ID.
        virtual OutdoorPvP* GetOutdoorPvP() const = 0;
};

class CommandScript : public ScriptObject
{
    protected:

        CommandScript(const char* name);

    public:

        // Should return a pointer to a valid command table (ChatCommand array) to be used by ChatHandler.
        virtual ChatCommand* GetCommands() const = 0;
};

class WeatherScript : public ScriptObject, public UpdatableScript<Weather>
{
    protected:

        WeatherScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Called when the weather changes in the zone this script is associated with.
        virtual void OnChange(Weather* p_Weather, WeatherState /*state*/, float /*grade*/) 
        {
            UNUSED(p_Weather);
        }
};

class AuctionHouseScript : public ScriptObject
{
    protected:

        AuctionHouseScript(const char* name);

    public:

        // Called when an auction is added to an auction house.
        virtual void OnAuctionAdd(AuctionHouseObject* p_AuctionHouseObject, AuctionEntry* /*entry*/)
        {
            UNUSED(p_AuctionHouseObject);
        }

        // Called when an auction is removed from an auction house.
        virtual void OnAuctionRemove(AuctionHouseObject* p_AuctionHouseObject, AuctionEntry* /*entry*/)
        {
            UNUSED(p_AuctionHouseObject);
        }
        // Called when an auction was succesfully completed.
        virtual void OnAuctionSuccessful(AuctionHouseObject* p_AuctionHouseObject, AuctionEntry* /*entry*/)
        {
            UNUSED(p_AuctionHouseObject);
        }
        // Called when an auction expires.
        virtual void OnAuctionExpire(AuctionHouseObject* p_AuctionHouseObject, AuctionEntry* /*entry*/)
        {
            UNUSED(p_AuctionHouseObject);
        }
};

class ConditionScript : public ScriptObject
{
    protected:

        ConditionScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Called when a single condition is checked for a player.
        virtual bool OnConditionCheck(Condition* /*condition*/, ConditionSourceInfo& /*sourceInfo*/) { return true; }
};

class VehicleScript : public ScriptObject
{
    protected:

        VehicleScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Called after a vehicle is installed.
        virtual void OnInstall(Vehicle* p_Vehicle)
        { 
            UNUSED(p_Vehicle); 
        }
        // Called after a vehicle is uninstalled.
        virtual void OnUninstall(Vehicle* p_Vehicle)
        {
            UNUSED(p_Vehicle);
        }
        // Called when a vehicle resets.
        virtual void OnReset(Vehicle* p_Vehicle)
        {
            UNUSED(p_Vehicle);
        }
        // Called after an accessory is installed in a vehicle.
        virtual void OnInstallAccessory(Vehicle* p_Vehicle, Creature* /*accessory*/)
        {
            UNUSED(p_Vehicle);
        }
        // Called after a passenger is added to a vehicle.
        virtual void OnAddPassenger(Vehicle* p_Vehicle, Unit* /*passenger*/, int8 /*seatId*/)
        {
            UNUSED(p_Vehicle);
        }
        // Called after a passenger is removed from a vehicle.
        virtual void OnRemovePassenger(Vehicle* p_Vehicle, Unit* /*passenger*/)
        {
            UNUSED(p_Vehicle);
        }

        // Called when a CreatureAI object is needed for the creature.
        virtual CreatureAI* GetAI(Creature* /*creature*/) const { return NULL; }
};

class DynamicObjectScript : public ScriptObject, public UpdatableScript<DynamicObject>
{
    protected:

        DynamicObjectScript(const char* name);
};

class TransportScript : public ScriptObject, public UpdatableScript<Transport>
{
    protected:
        TransportScript(const char* name);

    public:
        bool IsDatabaseBound() const 
        { 
            return true; 
        }

        /// Called when a player boards the transport.
        virtual void OnAddPassenger(Transport* p_Transport, Player* p_Player)
        { 
            UNUSED(p_Transport);
            UNUSED(p_Player);
        }

        /// Called when a creature boards the transport.
        virtual void OnAddCreaturePassenger(Transport* p_Transport, Creature* p_Creature)
        {
            UNUSED(p_Transport);
            UNUSED(p_Creature);
        }

        // Called when a player exits the transport.
        virtual void OnRemovePassenger(Transport* p_Transport, Player* p_Player)
        {
            UNUSED(p_Transport);
            UNUSED(p_Player);
        }

        // Called when a transport moves.
        virtual void OnRelocate(Transport* p_Transport, uint32 p_WaypointID, uint32 p_MapID, float p_X, float p_Y, float p_Z)
        {
            UNUSED(p_Transport);
            UNUSED(p_WaypointID);
            UNUSED(p_MapID);
            UNUSED(p_X);
            UNUSED(p_Y);
            UNUSED(p_Z);
        }
};

class AchievementCriteriaScript : public ScriptObject
{
    protected:

        AchievementCriteriaScript(const char* name);

    public:

        bool IsDatabaseBound() const { return true; }

        // Called when an additional criteria is checked.
        virtual bool OnCheck(Player* source, Unit* target) = 0;
};

class PlayerScript : public ScriptObject
{
    protected:

        PlayerScript(const char* name);

    public:

        // Called when a player kills another player
        virtual void OnPVPKill(Player* p_Killer, Player* /*killed*/) 
        { 
            UNUSED(p_Killer); 
        }
        // Called when a player kills another player
        virtual void OnModifyPower(Player* p_Killer, Powers /*power*/, int32 value)
        {
            UNUSED(p_Killer);
        }
        // Called when a player kills a creature
        virtual void OnCreatureKill(Player* p_Killer, Creature* /*killed*/)
        {
            UNUSED(p_Killer);
        }
        // Called when a player is killed by a creature
        virtual void OnPlayerKilledByCreature(Creature* p_Killer, Player* /*killed*/)
        {
            UNUSED(p_Killer);
        }
        // Called when a player's level changes (right before the level is applied)
        virtual void OnLevelChanged(Player* p_Player, uint8 /*newLevel*/) { UNUSED(p_Player); }

        // Called when a player's free talent points change (right before the change is applied)
        virtual void OnFreeTalentPointsChanged(Player* p_Player, uint32 /*points*/) { UNUSED(p_Player); }

        // Called when a player's talent points are reset (right before the reset is done)
        virtual void OnTalentsReset(Player* p_Player, bool /*noCost*/) { UNUSED(p_Player); }

        // Called when a player's money is modified (before the modification is done)
        virtual void OnMoneyChanged(Player* p_Player, int64& /*amount*/) { UNUSED(p_Player); }

        // Called when a player gains XP (before anything is given)
        virtual void OnGiveXP(Player* p_Player, uint32& /*amount*/, Unit* /*victim*/) { UNUSED(p_Player); }

        // Called when a player's reputation changes (before it is actually changed)
        virtual void OnReputationChange(Player* p_Player, uint32 /*factionId*/, int32& /*standing*/, bool /*incremental*/) { UNUSED(p_Player); }

        // Called when a duel is requested
        virtual void OnDuelRequest(Player* p_Target, Player* /*challenger*/)
        {
            UNUSED(p_Target);
        }
        // Called when a duel starts (after 3s countdown)
        virtual void OnDuelStart(Player* p_Player1, Player* /*player2*/)
        {
            UNUSED(p_Player1);
        }

        // Called when a duel ends
        virtual void OnDuelEnd(Player* p_Winner, Player* /*loser*/, DuelCompleteType /*type*/) { UNUSED(p_Winner); }

        // The following methods are called when a player sends a chat message.
        virtual void OnChat(Player* p_Player, uint32 /*type*/, uint32 /*lang*/, std::string& /*msg*/) { UNUSED(p_Player); }

        virtual void OnChat(Player* p_Player, uint32 /*type*/, uint32 /*lang*/, std::string& /*msg*/, Player* /*receiver*/) { UNUSED(p_Player); }

        virtual void OnChat(Player* p_Player, uint32 /*type*/, uint32 /*lang*/, std::string& /*msg*/, Group* /*group*/) { UNUSED(p_Player); }

        virtual void OnChat(Player* p_Player, uint32 /*type*/, uint32 /*lang*/, std::string& /*msg*/, Guild* /*guild*/) { UNUSED(p_Player); }

        virtual void OnChat(Player* p_Player, uint32 /*type*/, uint32 /*lang*/, std::string& /*msg*/, Channel* /*channel*/) { UNUSED(p_Player); }

        // Both of the below are called on emote opcodes.
        virtual void OnEmote(Player* p_Player, uint32 /*emote*/) { UNUSED(p_Player); }

        virtual void OnTextEmote(Player* p_Player, uint32 /*textEmote*/, uint32 /*soundIndex*/, uint64 guid) { UNUSED(p_Player); }

        // Called in Spell::Cast.
        virtual void OnSpellCast(Player* p_Player, Spell* /*spell*/, bool /*skipCheck*/) { UNUSED(p_Player); }

        virtual void OnSpellLearned(Player* /*p_Player*/, uint32 /*p_SpellId*/) {}

        // Called when a player logs in.
        virtual void OnLogin(Player* p_Player) { UNUSED(p_Player); }

        // Called when a player logs out.
        virtual void OnLogout(Player* p_Player) { UNUSED(p_Player); }

        // Called when a player is created.
        virtual void OnCreate(Player* p_Player) { UNUSED(p_Player); }

        // Called when a player is deleted.
        virtual void OnDelete(uint64 p_GUID) { UNUSED(p_GUID); }

        // Called when a player is bound to an instance
        virtual void OnBindToInstance(Player* p_Player, Difficulty /*difficulty*/, uint32 /*mapId*/, bool /*permanent*/) { UNUSED(p_Player); }

        // Called when a player switches to a new zone
        virtual void OnUpdateZone(Player* p_Player, uint32 /*newZone*/, uint32 /*p_OldZoneID*/, uint32 /*newArea*/) { UNUSED(p_Player); }

        // Called when a player updates his movement
        virtual void OnPlayerUpdateMovement(Player* p_Player) { UNUSED(p_Player); }

        // Called when player rewards some quest
        virtual void OnQuestReward(Player* p_Player, const Quest* /*quest*/) { UNUSED(p_Player); }

        // Called when a player validates some quest objective
        virtual void OnObjectiveValidate(Player* p_Player, uint32 /*questid*/, uint32 /*ObjectiveId*/) { UNUSED(p_Player); }

        // Called when a player shapeshift
        virtual void OnChangeShapeshift(Player* p_Player, ShapeshiftForm /*p_Form*/) { UNUSED(p_Player); }
};

class GuildScript : public ScriptObject
{
    protected:

        GuildScript(const char* name);

    public:
        bool IsDatabaseBound() const { return false; }

        // Called when a member is added to the guild.
        virtual void OnAddMember(Guild* p_Guild, Player* p_Player, uint8& /*plRank*/)
        {
            UNUSED(p_Guild);
            UNUSED(p_Player); 
        }
        // Called when a member is removed from the guild.
        virtual void OnRemoveMember(Guild* p_Guild, Player* p_Player, bool /*isDisbanding*/, bool /*isKicked*/)
        {
            UNUSED(p_Guild);
            UNUSED(p_Player); 
        }
        // Called when the guild MOTD (message of the day) changes.
        virtual void OnMOTDChanged(Guild* p_Guild, const std::string& /*newMotd*/)
        { 
            UNUSED(p_Guild);
        }
        // Called when the guild info is altered.
        virtual void OnInfoChanged(Guild* p_Guild, const std::string& /*newInfo*/)
        { 
            UNUSED(p_Guild); 
        }
        // Called when a guild is created.
        virtual void OnCreate(Guild* p_Guild, Player* /*leader*/, const std::string& /*name*/)
        {
            UNUSED(p_Guild);
        }
        // Called when a guild is disbanded.
        virtual void OnDisband(Guild* p_Guild)
        { 
            UNUSED(p_Guild); 
        }
        // Called when a guild member withdraws money from a guild bank.
        virtual void OnMemberWitdrawMoney(Guild* p_Guild, Player* p_Player, uint64& /*amount*/, bool /*isRepair*/)
        { 
            UNUSED(p_Guild);
        }
        // Called when a guild member deposits money in a guild bank.
        virtual void OnMemberDepositMoney(Guild* p_Guild, Player* p_Player, uint64& /*amount*/)
        { 
            UNUSED(p_Guild);
        }
        // Called when a guild member moves an item in a guild bank.
        virtual void OnItemMove(Guild* p_Guild, Player* p_Player, Item* /*pItem*/, bool /*isSrcBank*/, uint8 /*srcContainer*/, uint8 /*srcSlotId*/, bool /*isDestBank*/, uint8 /*destContainer*/, uint8 /*destSlotId*/)
        {
            UNUSED(p_Guild);
            UNUSED(p_Player); 
        }
        virtual void OnEvent(Guild* p_Guild, uint8 /*eventType*/, uint32 /*playerGuid1*/, uint32 /*playerGuid2*/, uint8 /*newRank*/)
        {
            UNUSED(p_Guild);
        }
        virtual void OnBankEvent(Guild* p_Guild, uint8 /*eventType*/, uint8 /*tabId*/, uint32 /*playerGuid*/, uint32 /*itemOrMoney*/, uint16 /*itemStackCount*/, uint8 /*destTabId*/)
        {
            UNUSED(p_Guild);
        }
};

class GroupScript : public ScriptObject
{
    protected:

        GroupScript(const char* name);

    public:

        bool IsDatabaseBound() const { return false; }

        // Called when a member is added to a group.
        virtual void OnAddMember(Group* p_Group, uint64 guid)
        { 
            UNUSED(p_Group);
        }
        // Called when a member is invited to join a group.
        virtual void OnInviteMember(Group* p_Group, uint64 guid)
        {
            UNUSED(p_Group);
        }
        // Called when a member is removed from a group.
        virtual void OnRemoveMember(Group* p_Group, uint64 guid, RemoveMethod /*method*/, uint64 /*kicker*/, const char* /*reason*/)
        {
            UNUSED(p_Group);
        }
        // Called when the leader of a group is changed.
        virtual void OnChangeLeader(Group* p_Group, uint64 /*newLeaderGuid*/, uint64 /*oldLeaderGuid*/)
        {
            UNUSED(p_Group);
        }
        // Called when a group is disbanded.
        virtual void OnDisband(Group* p_Group  )
        {
            UNUSED(p_Group);
        }
};

// Placed here due to ScriptRegistry::AddScript dependency.
#define sScriptMgr ACE_Singleton<ScriptMgr, ACE_Null_Mutex>::instance()

// Manages registration, loading, and execution of scripts.
class ScriptMgr
{
    friend class ACE_Singleton<ScriptMgr, ACE_Null_Mutex>;
    friend class ScriptObject;

    private:

        ScriptMgr();
        virtual ~ScriptMgr();

    public: /* Initialization */

        void Initialize();
        void LoadDatabase();
        void FillSpellSummary();

        const char* ScriptsVersion() const { return "Integrated Trinity Scripts"; }

        void IncrementScriptCount() { ++_scriptCount; }
        uint32 GetScriptCount() const { return _scriptCount; }

    public: /* Unloading */

        void Unload();

    public: /* SpellScriptLoader */

        void CreateSpellScripts(uint32 spellId, std::list<SpellScript*>& scriptVector);
        void CreateAuraScripts(uint32 spellId, std::list<AuraScript*>& scriptVector);
        void CreateSpellScriptLoaders(uint32 spellId, std::vector<std::pair<SpellScriptLoader*, std::multimap<uint32, uint32>::iterator> >& scriptVector);

    public: /* ServerScript */

        void OnNetworkStart();
        void OnNetworkStop();
        void OnSocketOpen(WorldSocket* socket);
        void OnSocketClose(WorldSocket* socket, bool wasNew);
        void OnPacketReceive(WorldSocket* socket, WorldPacket packet);
        void OnPacketSend(WorldSocket* socket, WorldPacket packet);
        void OnUnknownPacketReceive(WorldSocket* socket, WorldPacket packet);

    public: /* WorldScript */

        void OnOpenStateChange(bool open);
        void OnConfigLoad(bool reload);
        void OnMotdChange(std::string& newMotd);
        void OnShutdownInitiate(ShutdownExitCode code, ShutdownMask mask);
        void OnShutdownCancel();
        void OnWorldUpdate(uint32 diff);
        void OnStartup();
        void OnShutdown();

    public: /* FormulaScript */

        void OnHonorCalculation(float& honor, uint8 level, float multiplier);
        void OnGrayLevelCalculation(uint8& grayLevel, uint8 playerLevel);
        void OnColorCodeCalculation(XPColorChar& color, uint8 playerLevel, uint8 mobLevel);
        void OnZeroDifferenceCalculation(uint8& diff, uint8 playerLevel);
        void OnBaseGainCalculation(uint32& gain, uint8 playerLevel, uint8 mobLevel, ContentLevels content);
        void OnGainCalculation(uint32& gain, Player* player, Unit* unit);
        void OnGroupRateCalculation(float& rate, uint32 count, bool isRaid);

    public: /* MapScript */

        void OnCreateMap(Map* map);
        void OnDestroyMap(Map* map);
        void OnLoadGridMap(Map* map, GridMap* gmap, uint32 gx, uint32 gy);
        void OnUnloadGridMap(Map* map, GridMap* gmap, uint32 gx, uint32 gy);
        void OnPlayerEnterMap(Map* map, Player* player);
        void OnPlayerLeaveMap(Map* map, Player* player);
        void OnMapUpdate(Map* map, uint32 diff);

    public: /* InstanceMapScript */

        InstanceScript* CreateInstanceData(InstanceMap* map);

    public: /* ItemScript */

        bool OnDummyEffect(Unit* caster, uint32 spellId, SpellEffIndex effIndex, Item* target);
        bool OnQuestAccept(Player* player, Item* item, Quest const* quest);
        bool OnItemUse(Player* player, Item* item, SpellCastTargets const& targets);
        bool OnItemExpire(Player* player, ItemTemplate const* proto);

    public: /* CreatureScript */

        bool OnDummyEffect(Unit* caster, uint32 spellId, SpellEffIndex effIndex, Creature* target);
        bool OnGossipHello(Player* player, Creature* creature);
        bool OnGossipSelect(Player* player, Creature* creature, uint32 sender, uint32 action);
        bool OnGossipSelectCode(Player* player, Creature* creature, uint32 sender, uint32 action, const char* code);
        bool OnQuestAccept(Player* player, Creature* creature, Quest const* quest);
        bool OnQuestSelect(Player* player, Creature* creature, Quest const* quest);
        bool OnQuestComplete(Player* player, Creature* creature, Quest const* quest);
        bool OnQuestReward(Player* player, Creature* creature, Quest const* quest, uint32 opt);
        uint32 GetDialogStatus(Player* player, Creature* creature);
        CreatureAI* GetCreatureAI(Creature* creature);
        void OnCreatureUpdate(Creature* creature, uint32 diff);

    public: /* GameObjectScript */

        bool OnDummyEffect(Unit* caster, uint32 spellId, SpellEffIndex effIndex, GameObject* target);
        bool OnGossipHello(Player* player, GameObject* go);
        bool OnGossipSelect(Player* player, GameObject* go, uint32 sender, uint32 action);
        bool OnGossipSelectCode(Player* player, GameObject* go, uint32 sender, uint32 action, const char* code);
        bool OnQuestAccept(Player* player, GameObject* go, Quest const* quest);
        bool OnQuestReward(Player* player, GameObject* go, Quest const* quest, uint32 opt);
        uint32 GetDialogStatus(Player* player, GameObject* go);
        void OnGameObjectDestroyed(GameObject* go, Player* player);
        void OnGameObjectDamaged(GameObject* go, Player* player);
        void OnGameObjectLootStateChanged(GameObject* go, uint32 state, Unit* unit);
        void OnGameObjectStateChanged(GameObject* go, uint32 state);
        void OnGameObjectUpdate(GameObject* go, uint32 diff);

        /// - Transport GO
        bool OnGameObjectElevatorCheck(GameObject const* p_GameObject) const;

        GameObjectAI* GetGameObjectAI(GameObject* go);

    public:
        /* AreaTriggerEntityScript */
        void OnCreateAreaTriggerEntity(AreaTrigger* p_AreaTrigger);
        void OnUpdateAreaTriggerEntity(AreaTrigger* p_AreaTrigger, uint32 p_Time);
        void OnRemoveAreaTriggerEntity(AreaTrigger* p_AreaTrigger, uint32 p_Time);

    public: /* AreaTriggerScript */

        bool OnAreaTrigger(Player* player, AreaTriggerEntry const* trigger);

    public: /* BattlegroundScript */

        Battleground* CreateBattleground(BattlegroundTypeId typeId);

    public: /* OutdoorPvPScript */

        OutdoorPvP* CreateOutdoorPvP(OutdoorPvPData const* data);

    public: /* CommandScript */

        std::vector<ChatCommand*> GetChatCommands();

    public: /* WeatherScript */

        void OnWeatherChange(Weather* weather, WeatherState state, float grade);
        void OnWeatherUpdate(Weather* weather, uint32 diff);

    public: /* AuctionHouseScript */

        void OnAuctionAdd(AuctionHouseObject* ah, AuctionEntry* entry);
        void OnAuctionRemove(AuctionHouseObject* ah, AuctionEntry* entry);
        void OnAuctionSuccessful(AuctionHouseObject* ah, AuctionEntry* entry);
        void OnAuctionExpire(AuctionHouseObject* ah, AuctionEntry* entry);

    public: /* ConditionScript */

        bool OnConditionCheck(Condition* condition, ConditionSourceInfo& sourceInfo);

    public: /* VehicleScript */

        void OnInstall(Vehicle* veh);
        void OnUninstall(Vehicle* veh);
        void OnReset(Vehicle* veh);
        void OnInstallAccessory(Vehicle* veh, Creature* accessory);
        void OnAddPassenger(Vehicle* veh, Unit* passenger, int8 seatId);
        void OnRemovePassenger(Vehicle* veh, Unit* passenger);

    public: /* DynamicObjectScript */

        void OnDynamicObjectUpdate(DynamicObject* dynobj, uint32 diff);

    public: /* TransportScript */

        void OnAddPassenger(Transport* transport, Player* player);
        void OnAddCreaturePassenger(Transport* transport, Creature* creature);
        void OnRemovePassenger(Transport* transport, Player* player);
        void OnTransportUpdate(Transport* transport, uint32 diff);
        void OnRelocate(Transport* transport, uint32 waypointId, uint32 mapId, float x, float y, float z);

    public: /* AchievementCriteriaScript */

        bool OnCriteriaCheck(uint32 scriptId, Player* source, Unit* target);

    public: /* PlayerScript */

        void OnPVPKill(Player* killer, Player* killed);
        void OnModifyPower(Player* player, Powers power, int32 value);
        void OnCreatureKill(Player* killer, Creature* killed);
        void OnPlayerKilledByCreature(Creature* killer, Player* killed);
        void OnPlayerLevelChanged(Player* player, uint8 oldLevel);
        void OnPlayerFreeTalentPointsChanged(Player* player, uint32 newPoints);
        void OnPlayerTalentsReset(Player* player, bool noCost);
        void OnPlayerMoneyChanged(Player* player, int64& amount);
        void OnGivePlayerXP(Player* player, uint32& amount, Unit* victim);
        void OnPlayerReputationChange(Player* player, uint32 factionID, int32& standing, bool incremental);
        void OnPlayerDuelRequest(Player* target, Player* challenger);
        void OnPlayerDuelStart(Player* player1, Player* player2);
        void OnPlayerDuelEnd(Player* winner, Player* loser, DuelCompleteType type);
        void OnPlayerChat(Player* player, uint32 type, uint32 lang, std::string& msg);
        void OnPlayerChat(Player* player, uint32 type, uint32 lang, std::string& msg, Player* receiver);
        void OnPlayerChat(Player* player, uint32 type, uint32 lang, std::string& msg, Group* group);
        void OnPlayerChat(Player* player, uint32 type, uint32 lang, std::string& msg, Guild* guild);
        void OnPlayerChat(Player* player, uint32 type, uint32 lang, std::string& msg, Channel* channel);
        void OnPlayerEmote(Player* player, uint32 emote);
        void OnPlayerTextEmote(Player* player, uint32 textEmote, uint32 emoteNum, uint64 guid);
        void OnPlayerSpellCast(Player* player, Spell* spell, bool skipCheck);
        void OnPlayerSpellLearned(Player* p_Player, uint32 p_SpellId);
        void OnPlayerLogin(Player* player);
        void OnPlayerLogout(Player* player);
        void OnPlayerCreate(Player* player);
        void OnPlayerDelete(uint64 guid);
        void OnPlayerBindToInstance(Player* player, Difficulty difficulty, uint32 mapid, bool permanent);
        void OnPlayerUpdateZone(Player* player, uint32 newZone, uint32 p_OldZoneID, uint32 newArea);
        void OnPlayerUpdateMovement(Player* p_Player);
        void OnQuestReward(Player* player, const Quest* quest);
        void OnObjectiveValidate(Player* player, uint32 questId, uint32 ObjectiveId);
        void OnPlayerChangeShapeshift(Player* p_Player, ShapeshiftForm p_Form);

    public: /* GuildScript */

        void OnGuildAddMember(Guild* guild, Player* player, uint8& plRank);
        void OnGuildRemoveMember(Guild* guild, Player* player, bool isDisbanding, bool isKicked);
        void OnGuildMOTDChanged(Guild* guild, const std::string& newMotd);
        void OnGuildInfoChanged(Guild* guild, const std::string& newInfo);
        void OnGuildCreate(Guild* guild, Player* leader, const std::string& name);
        void OnGuildDisband(Guild* guild);
        void OnGuildMemberWitdrawMoney(Guild* guild, Player* player, uint64 &amount, bool isRepair);
        void OnGuildMemberDepositMoney(Guild* guild, Player* player, uint64 &amount);
        void OnGuildItemMove(Guild* guild, Player* player, Item* pItem, bool isSrcBank, uint8 srcContainer, uint8 srcSlotId,
            bool isDestBank, uint8 destContainer, uint8 destSlotId);
        void OnGuildEvent(Guild* guild, uint8 eventType, uint32 playerGuid1, uint32 playerGuid2, uint8 newRank);
        void OnGuildBankEvent(Guild* guild, uint8 eventType, uint8 tabId, uint32 playerGuid, uint32 itemOrMoney, uint16 itemStackCount, uint8 destTabId);

    public: /* GroupScript */

        void OnGroupAddMember(Group* group, uint64 guid);
        void OnGroupInviteMember(Group* group, uint64 guid);
        void OnGroupRemoveMember(Group* group, uint64 guid, RemoveMethod method, uint64 kicker, const char* reason);
        void OnGroupChangeLeader(Group* group, uint64 newLeaderGuid, uint64 oldLeaderGuid);
        void OnGroupDisband(Group* group);

    public: /* Scheduled scripts */

        uint32 IncreaseScheduledScriptsCount() { return ++_scheduledScripts; }
        uint32 DecreaseScheduledScriptCount() { return --_scheduledScripts; }
        uint32 DecreaseScheduledScriptCount(size_t count) { return _scheduledScripts -= count; }
        bool IsScriptScheduled() const { return _scheduledScripts > 0; }

    private:

        uint32 _scriptCount;

        //atomic op counter for active scripts amount
        ACE_Atomic_Op<ACE_Thread_Mutex, long> _scheduledScripts;
};

#endif

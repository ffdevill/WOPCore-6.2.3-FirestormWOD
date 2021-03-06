using System;
using System.Collections.Generic;
using System.Globalization;
using WowPacketParser.Enums;
using WowPacketParser.Enums.Version;
using WowPacketParser.Misc;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;
using Guid = WowPacketParser.Misc.Guid;

namespace WowPacketParser.Parsing.Parsers
{
    public static class SpellHandler
    {
        [Parser(Opcode.SMSG_LOSS_OF_CONTROL_AURA_UPDATE)]
        public static void HandleLossOfControlAuraUpdate(Packet packet)
        {
            var counter = packet.ReadBits("counter", 22);

            var unkLen1 = new uint[counter];
            var unkLen2 = new uint[counter];
            for (int i = 0; i < counter; ++i)
            {
                unkLen1[i] = packet.ReadBits("unkLen1", 8, i);
                unkLen2[i] = packet.ReadBits("unkLen2", 8, i);
            }

            for (int i = 0; i < counter; ++i)
            {
                packet.ReadByte("Unk Byte", i);
                packet.ReadByte("Unk Byte", i);
            }
        }

        [Parser(Opcode.SMSG_SPELLINTERRUPTLOG)] // 4.3.4
        public static void HandleSpellInterruptLog(Packet packet)
        {
            var guid1 = new byte[8];
            var guid2 = new byte[8];

            guid2[4] = packet.ReadBit();
            guid1[5] = packet.ReadBit();
            guid1[6] = packet.ReadBit();
            guid1[1] = packet.ReadBit();
            guid1[3] = packet.ReadBit();
            guid1[0] = packet.ReadBit();
            guid2[3] = packet.ReadBit();
            guid2[5] = packet.ReadBit();
            guid2[1] = packet.ReadBit();
            guid1[4] = packet.ReadBit();
            guid1[7] = packet.ReadBit();
            guid2[7] = packet.ReadBit();
            guid2[6] = packet.ReadBit();
            guid1[2] = packet.ReadBit();
            guid2[2] = packet.ReadBit();
            guid2[0] = packet.ReadBit();

            packet.ReadXORByte(guid1, 7);
            packet.ReadXORByte(guid1, 6);
            packet.ReadXORByte(guid1, 3);
            packet.ReadXORByte(guid1, 2);
            packet.ReadXORByte(guid2, 3);
            packet.ReadXORByte(guid2, 6);
            packet.ReadXORByte(guid2, 2);
            packet.ReadXORByte(guid2, 4);
            packet.ReadXORByte(guid2, 7);
            packet.ReadXORByte(guid2, 0);
            packet.ReadXORByte(guid1, 4);
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Interrupt Spell ID");
            packet.ReadXORByte(guid2, 1);
            packet.ReadXORByte(guid1, 0);
            packet.ReadXORByte(guid1, 5);
            packet.ReadXORByte(guid1, 1);
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Interrupted Spell ID");
            packet.ReadXORByte(guid2, 5);

            packet.WriteGuid("GUID 1", guid1);
            packet.WriteGuid("GUID 2", guid1);
        }

        [Parser(Opcode.SMSG_PLAYERBOUND)]
        public static void HandlePlayerBound(Packet packet)
        {
            packet.ReadGuid("GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Area, "Area ID");
        }

        [Parser(Opcode.CMSG_CANCEL_TEMP_ENCHANTMENT)]
        public static void HandleCancelTempEnchantment(Packet packet)
        {
            packet.ReadUInt32("Slot");
        }

        [Parser(Opcode.SMSG_DONT_AUTO_PUSH_SPELLS_TO_ACTION_BAR)]
        public static void HandleDontAutoPushSpellsToActionBar(Packet packet)
        {
            var spellCounter = packet.ReadBits("SpellCounter", 22);
            var nextSpellIdCounter = packet.ReadBits("nextSpellIdCounter", 22);

            for (int i = 0; i < spellCounter; i++)
                packet.ReadUInt32("Spell ID");

            for (int i = 0; i < nextSpellIdCounter; i++)
                packet.ReadUInt32("nextSpell ID");
        }

        [Parser(Opcode.SMSG_SUPERCEDED_SPELL)]
        public static void HandleSupercededSpell(Packet packet)
        {
            var spellCounter = packet.ReadBits("SpellCounter", 22);
            var nextSpellIdCounter = packet.ReadBits("nextSpellIdCounter", 22);

            for (int i = 0; i < spellCounter; i++)
                packet.ReadUInt32("Spell ID");

            for (int i = 0; i < nextSpellIdCounter; i++)
                packet.ReadUInt32("nextSpell ID");
        }

        [Parser(Opcode.SMSG_RESYNC_RUNES)]
        public static void HandleResyncRunes(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            for (var i = 0; i < count; ++i)
            {
                packet.ReadByte("Rune Type");
                packet.ReadByte("Cooldown Time");
            }
        }

        [Parser(Opcode.SMSG_CONVERT_RUNE)]
        public static void HandleConvertRune(Packet packet)
        {
            packet.ReadByte("Index");
            packet.ReadByte("New Rune Type");
        }

        [Parser(Opcode.SMSG_ADD_RUNE_POWER)]
        public static void HandleAddRunePower(Packet packet)
        {
            packet.ReadUInt32("Mask?"); // TC: 1 << index
        }

        [Parser(Opcode.SMSG_COOLDOWN_CHEAT)]
        public static void HandleCooldownCheat(Packet packet)
        {
            packet.ReadGuid("GUID");
        }

        [Parser(Opcode.SMSG_COOLDOWN_EVENT)]
        public static void HandleCooldownEvent(Packet packet)
        {
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");
            packet.ReadGuid("GUID");
        }

        [Parser(Opcode.SMSG_NOTIFY_DEST_LOC_SPELL_CAST)]
        public static void HandleNotifyDestLocSpellCast(Packet packet)
        {
            // TODO: Verify and/or finish this
            // Everything is guessed
            packet.ReadGuid("Caster GUID");
            packet.ReadGuid("Target GUID");
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");
            packet.ReadVector3("Position");
            packet.ReadVector3("Target Position");
            packet.ReadSingle("Elevation");
            packet.ReadSingle("Speed");
            packet.ReadUInt32("Duration");
            packet.ReadInt32("Unk");

            if (packet.Length == 64) // packet always has length 64 length except for some rare exceptions with length 60 (hardcoded in the client)
                packet.ReadSingle("Unk");
        }

        [Parser(Opcode.SMSG_WEEKLY_SPELL_USAGE)]
        public static void HandleWeeklySpellUsage(Packet packet)
        {
            var count = packet.ReadBits("Count", 23);

            for (int i = 0; i < count; ++i)
            {
                packet.ReadInt32("Unk Int32");
                packet.ReadByte("Unk Int8");
            }
        }

        [Parser(Opcode.SMSG_SEND_UNLEARN_SPELLS)]
        public static void HandleSendUnlearnSpells(Packet packet)
        {
            var count = packet.ReadInt32("Count");
            for (var i = 0; i < count; i++)
                packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID", i);
        }

        [Parser(Opcode.SMSG_RESUME_CAST_BAR)]
        public static void HandleResumeCastBar(Packet packet)
        {
            packet.ReadPackedGuid("Caster GUID");

            packet.ReadPackedGuid("Target GUID");

            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");

            packet.ReadInt32("Cast Time");

            packet.ReadInt32("Unk Int32");
        }

        [Parser(Opcode.SMSG_INITIAL_SPELLS, ClientVersionBuild.Zero, ClientVersionBuild.V5_1_0_16309)]
        public static void HandleInitialSpells(Packet packet)
        {
            packet.ReadByte("Talent Spec");

            var count = packet.ReadUInt16("Spell Count");
            var spells = new List<uint>(count);
            for (var i = 0; i < count; i++)
            {
                uint spellId;
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    spellId = (uint) packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID", i);
                else
                    spellId = (uint) packet.ReadEntryWithName<UInt16>(StoreNameType.Spell, "Spell ID", i);

                packet.ReadInt16("Unk Int16", i);

                spells.Add(spellId);
            }

            var startSpell = new StartSpell {Spells = spells};

            WoWObject character;
            if (Storage.Objects.TryGetValue(SessionHandler.LoginGuid, out character))
            {
                var player = character as Player;
                if (player != null && player.FirstLogin)
                    Storage.StartSpells.Add(new Tuple<Race, Class>(player.Race, player.Class), startSpell, packet.TimeSpan);
            }

            var cooldownCount = packet.ReadUInt16("Cooldown Count");
            for (var i = 0; i < cooldownCount; i++)
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Cooldown Spell ID", i);
                else
                    packet.ReadEntryWithName<UInt16>(StoreNameType.Spell, "Cooldown Spell ID", i);

                if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
                    packet.ReadInt32("Cooldown Cast Item ID", i);
                else
                    packet.ReadUInt16("Cooldown Cast Item ID", i);

                packet.ReadUInt16("Cooldown Spell Category", i);
                packet.ReadInt32("Cooldown Time", i);
                var catCd = packet.ReadUInt32();
                packet.WriteLine("[{0}] Cooldown Category Time: {1}", i, ((catCd >> 31) != 0 ? "Infinite" : (catCd & 0x7FFFFFFF).ToString(CultureInfo.InvariantCulture)));
            }
        }

        [Parser(Opcode.SMSG_INITIAL_SPELLS, ClientVersionBuild.V5_1_0_16309)]
        public static void HandleInitialSpells510(Packet packet)
        {
            var count = packet.ReadBits("Spell Count", 24);
            packet.ReadBit("Unk Bit");
            packet.ResetBitReader();

            var spells = new List<uint>((int)count);
            for (var i = 0; i < count; i++)
            {
                var spellId = packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID", i);
                spells.Add((uint)spellId);
            }

            var startSpell = new StartSpell { Spells = spells };

            WoWObject character;
            if (Storage.Objects.TryGetValue(SessionHandler.LoginGuid, out character))
            {
                var player = character as Player;
                if (player != null && player.FirstLogin)
                    Storage.StartSpells.Add(new Tuple<Race, Class>(player.Race, player.Class), startSpell, packet.TimeSpan);
            }
        }

        private static Aura ReadAuraUpdateBlock(ref Packet packet, int i)
        {
            var aura = new Aura();

            aura.Slot = packet.ReadByte("Slot", i);

            var id = packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID", i);
            if (id <= 0)
                return null;
            aura.SpellId = (uint)id;

            var type = ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_0_14333) ? TypeCode.Int16 : TypeCode.Byte;
            aura.AuraFlags = packet.ReadEnum<AuraFlag>("Flags", type, i);

            aura.Level = packet.ReadByte("Level", i);

            aura.Charges = packet.ReadByte("Charges", i);

            aura.CasterGuid = !aura.AuraFlags.HasAnyFlag(AuraFlag.NotCaster) ? packet.ReadPackedGuid("Caster GUID", i) : new Guid();

            if (aura.AuraFlags.HasAnyFlag(AuraFlag.Duration))
            {
                aura.MaxDuration = packet.ReadInt32("Max Duration", i);
                aura.Duration = packet.ReadInt32("Duration", i);
            }
            else
            {
                aura.MaxDuration = 0;
                aura.Duration = 0;
            }

            if (aura.AuraFlags.HasAnyFlag(AuraFlag.Scalable))
            {
                // This aura is scalable with level/talents
                // Here we show each effect value after scaling
                if (aura.AuraFlags.HasAnyFlag(AuraFlag.EffectIndex0))
                    packet.ReadInt32("Effect 0 Value", i);
                if (aura.AuraFlags.HasAnyFlag(AuraFlag.EffectIndex1))
                    packet.ReadInt32("Effect 1 Value", i);
                if (aura.AuraFlags.HasAnyFlag(AuraFlag.EffectIndex2))
                    packet.ReadInt32("Effect 2 Value", i);
            }

            packet.AddSniffData(StoreNameType.Spell, (int)aura.SpellId, "AURA_UPDATE");

            return aura;
        }

        private static Aura ReadAuraUpdateBlock505(ref Packet packet, int i)
        {
            var aura = new Aura();

            aura.Slot = packet.ReadByte("Slot", i);

            var id = packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID", i);
            if (id <= 0)
                return null;

            aura.SpellId = (uint)id;

            aura.AuraFlags = packet.ReadEnum<AuraFlagMoP>("Flags", TypeCode.Byte, i);

            var mask = packet.ReadUInt32("Effect Mask", i);

            aura.Level = (uint)packet.ReadInt16("Level", i);

            aura.Charges = packet.ReadByte("Charges", i);

            aura.CasterGuid = !aura.AuraFlags.HasAnyFlag(AuraFlagMoP.NoCaster) ? packet.ReadPackedGuid("Caster GUID", i) : new Guid();

            if (aura.AuraFlags.HasAnyFlag(AuraFlagMoP.Duration))
            {
                aura.MaxDuration = packet.ReadInt32("Max Duration", i);
                aura.Duration = packet.ReadInt32("Duration", i);
            }
            else
            {
                aura.MaxDuration = 0;
                aura.Duration = 0;
            }

            if (aura.AuraFlags.HasAnyFlag(AuraFlagMoP.Scalable))
            {
                var b1 = packet.ReadByte("Effect Count", i);
                for (var j = 0; j < b1; ++j)
                    if (((1 << j) & mask) != 0)
                        packet.ReadSingle("Effect Value", i, j);
            }

            packet.AddSniffData(StoreNameType.Spell, (int)aura.SpellId, "AURA_UPDATE");

            return aura;
        }

        [HasSniffData] // in ReadAuraUpdateBlock and ReadAuraUpdateBlock505
        [Parser(Opcode.SMSG_AURA_UPDATE_ALL)]
        [Parser(Opcode.SMSG_AURA_UPDATE)]
        public static void HandleAuraUpdate(Packet packet)
        {
            var guid = packet.ReadPackedGuid("GUID");
            var i = 0;
            var auras = new List<Aura>();
            while (packet.CanRead())
            {
                Aura aura = null;
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_0_5_16048))
                    aura = ReadAuraUpdateBlock505(ref packet, i++);
                else
                    aura = ReadAuraUpdateBlock(ref packet, i++);

                if (aura != null)
                    auras.Add(aura);
            }

            // This only works if the parser saw UPDATE_OBJECT before this packet
            if (Storage.Objects.ContainsKey(guid))
            {
                var unit = Storage.Objects[guid].Item1 as Unit;
                if (unit != null)
                {
                    // If this is the first packet that sends auras
                    // (hopefully at spawn time) add it to the "Auras" field,
                    // if not create another row of auras in AddedAuras
                    // (similar to ChangedUpdateFields)

                    if (unit.Auras == null)
                        unit.Auras = auras;
                    else if (unit.AddedAuras == null)
                        unit.AddedAuras = new List<List<Aura>> { auras };
                    else
                        unit.AddedAuras.Add(auras);
                }
            }
        }

        [Parser(Opcode.CMSG_CAST_SPELL)]
        public static void HandleCastSpell(Packet packet)
        {
            packet.ReadByte("Cast Count");
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");

            if (ClientVersion.AddedInVersion(ClientType.Cataclysm))
                packet.ReadInt32("Glyph Index");

            var castFlags = packet.ReadEnum<CastFlag>("Cast Flags", TypeCode.Byte);
            if (castFlags.HasAnyFlag(CastFlag.HasTrajectory))
            {
                ReadSpellCastTargets(ref packet);
                HandleSpellMissileAndMove(ref packet);
            }
            else
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_1_0_16309))
                    if (castFlags.HasAnyFlag(CastFlag.Unknown4))
                        HandleSpellMove(ref packet);

                ReadSpellCastTargets(ref packet);
            }
        }

        public static TargetFlag ReadSpellCastTargets(ref Packet packet)
        {
            var targetFlags = packet.ReadEnum<TargetFlag>("Target Flags", TypeCode.Int32);

            if (targetFlags.HasAnyFlag(TargetFlag.Unit | TargetFlag.CorpseEnemy | TargetFlag.GameObject |
                TargetFlag.CorpseAlly | TargetFlag.UnitMinipet))
                packet.ReadPackedGuid("Target GUID");

            if (targetFlags.HasAnyFlag(TargetFlag.Item | TargetFlag.TradeItem))
                packet.ReadPackedGuid("Item Target GUID");

            if (targetFlags.HasAnyFlag(TargetFlag.SourceLocation))
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                    packet.ReadPackedGuid("Source Transport GUID");

                packet.ReadVector3("Source Position");
            }

            if (targetFlags.HasAnyFlag(TargetFlag.DestinationLocation))
            {
                if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
                    packet.ReadPackedGuid("Destination Transport GUID");

                packet.ReadVector3("Destination Position");
            }

            if (targetFlags.HasAnyFlag(TargetFlag.NameString))
                packet.ReadCString("Target String");

            return targetFlags;
        }

        public static void HandleSpellMissileAndMove(ref Packet packet) // 4.3.4
        {
            packet.ReadSingle("Elevation");
            packet.ReadSingle("Missile speed");
            if (ClientVersion.RemovedInVersion(ClientType.Cataclysm))
            {
                var opcode = packet.ReadInt32();
                // None length is recieved, so we have to calculate the remaining bytes.
                var remainingLength = packet.Length - packet.Position;
                var bytes = packet.ReadBytes((int)remainingLength);

                using (var newpacket = new Packet(bytes, opcode, packet.Time, packet.Direction, packet.Number, packet.Writer, packet.FileName))
                    Handler.Parse(newpacket, true);
                return;
            }
            else
                HandleSpellMove(ref packet);
        }

        public static void HandleSpellMove510(ref Packet packet)
        {
            var hasMovement = packet.ReadBoolean("Has Movement Data");
            if (hasMovement)
            {
                var guid = new byte[8];
                var transportGuid = new byte[8];
                var hasTransTime2 = false;
                var hasTransTime3 = false;
                var hasFallDirection = false;
                var pos = new Vector4();

                pos.Z = packet.ReadSingle();
                pos.X = packet.ReadSingle();
                pos.Y = packet.ReadSingle();

                guid[7] = packet.ReadBit();
                var hasTrans = packet.ReadBit("Has Transport");
                var hasFallData = packet.ReadBit("Has Fall Data");
                var hasField152 = !packet.ReadBit("Lacks field152");
                var hasMovementFlags = !packet.ReadBit();
                packet.ReadBit();
                guid[0] = packet.ReadBit();
                var hasMovementFlags2 = !packet.ReadBit();
                var hasO = !packet.ReadBit("Lacks Orientation");
                guid[2] = packet.ReadBit();
                var hasTime = !packet.ReadBit("Lacks Timestamp");
                guid[1] = packet.ReadBit();
                packet.ReadBit("Has Spline");
                guid[3] = packet.ReadBit();
                var unkLoopCounter = packet.ReadBits(24);
                guid[5] = packet.ReadBit();
                guid[6] = packet.ReadBit();
                var hasPitch = !packet.ReadBit("Lacks Pitch");
                guid[4] = packet.ReadBit();
                var hasSplineElev = !packet.ReadBit("Lacks Spline Elevation");
                packet.ReadBit();
                if (hasTrans)
                {
                    hasTransTime2 = packet.ReadBit();
                    transportGuid[2] = packet.ReadBit();
                    transportGuid[3] = packet.ReadBit();
                    transportGuid[4] = packet.ReadBit();
                    transportGuid[0] = packet.ReadBit();
                    transportGuid[5] = packet.ReadBit();
                    hasTransTime3 = packet.ReadBit();
                    transportGuid[6] = packet.ReadBit();
                    transportGuid[7] = packet.ReadBit();
                    transportGuid[1] = packet.ReadBit();
                }

                if (hasMovementFlags2)
                    packet.ReadEnum<MovementFlagExtra>("Extra Movement Flags", 13);

                if (hasFallData)
                    hasFallDirection = packet.ReadBit("Has Fall Direction");

                if (hasMovementFlags)
                    packet.ReadEnum<MovementFlag>("Movement Flags", 30);

                packet.ReadXORByte(guid, 1);
                packet.ReadXORByte(guid, 2);
                packet.ReadXORByte(guid, 5);
                for (var i = 0; i < unkLoopCounter; i++)
                    packet.ReadUInt32("Unk UInt32", i);

                packet.ReadXORByte(guid, 6);
                packet.ReadXORByte(guid, 7);
                packet.ReadXORByte(guid, 4);
                packet.ReadXORByte(guid, 0);
                packet.ReadXORByte(guid, 3);

                if (hasTrans)
                {
                    var tpos = new Vector4();
                    packet.ReadUInt32("Transport Time");
                    packet.ReadXORByte(transportGuid, 5);
                    packet.ReadSByte("Transport Seat");
                    tpos.O = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 7);
                    packet.ReadXORByte(transportGuid, 1);
                    tpos.Z = packet.ReadSingle();
                    tpos.X = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 6);
                    tpos.Y = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 4);
                    packet.ReadXORByte(transportGuid, 2);

                    if (hasTransTime3)
                        packet.ReadUInt32("Transport Time 3");

                    if (hasTransTime2)
                        packet.ReadUInt32("Transport Time 2");

                    packet.ReadXORByte(transportGuid, 0);
                    packet.ReadXORByte(transportGuid, 3);

                    packet.WriteGuid("Transport Guid", transportGuid);
                    packet.WriteLine("Transport Position: {0}", tpos);
                }

                if (hasTime)
                    packet.ReadUInt32("Timestamp");

                if (hasO)
                    pos.O = packet.ReadSingle();

                if (hasFallData)
                {
                    packet.ReadUInt32("Fall Time");
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Fall Sin");
                        packet.ReadSingle("Fall Cos");
                        packet.ReadSingle("Horizontal Speed");
                    }
                    packet.ReadSingle("Vertical Speed");
                }

                if (hasField152)
                    packet.ReadUInt32("Unk field152");

                if (hasPitch)
                    packet.ReadSingle("Pitch");

                if (hasSplineElev)
                    packet.ReadSingle("Spline Elevation");

                packet.WriteGuid("Guid", guid);
                packet.WriteLine("Position: {0}", pos);
            }
        }

        public static void HandleSpellMove(ref Packet packet)
        {
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_1_0_16309))
            {
                HandleSpellMove510(ref packet);
                return;
            }

            var hasMovement = packet.ReadBoolean("Has Movement Data");
            if (hasMovement)
            {
                var guid = new byte[8];
                var transportGuid = new byte[8];
                var hasTransTime2 = false;
                var hasTransTime3 = false;
                var hasFallDirection = false;
                var pos = new Vector4();

                pos.Z = packet.ReadSingle();
                pos.Y = packet.ReadSingle();
                pos.X = packet.ReadSingle();

                var bytes = new byte[8];
                var hasFallData = packet.ReadBit("Has fall data");
                var hasTime = !packet.ReadBit("Has timestamp");
                var hasO = !packet.ReadBit("Has O");
                packet.ReadBit("Has Spline");
                packet.ReadBit();
                guid[6] = bytes[4] = packet.ReadBit(); //6
                guid[4] = bytes[5] = packet.ReadBit(); //4
                var hasMovementFlags2 = !packet.ReadBit();
                var bytes2 = new byte[8];
                guid[3] = bytes2[0] = packet.ReadBit(); //3
                guid[5] = bytes2[1] = packet.ReadBit(); //5
                var hasSplineElev = !packet.ReadBit("Has Spline Elevation");
                var hasPitch = !packet.ReadBit("Has Pitch");
                guid[7] = bytes2[6] = packet.ReadBit(); //7
                var hasTrans = packet.ReadBit("Has transport");
                guid[2] = bytes2[5] = packet.ReadBit(); //2
                var hasMovementFlags = !packet.ReadBit();
                guid[1] = packet.ReadBit();
                guid[0] = packet.ReadBit();
                if (hasTrans)
                {
                    transportGuid[6] = packet.ReadBit();//54
                    transportGuid[2] = packet.ReadBit();//50
                    transportGuid[5] = packet.ReadBit();//53
                    hasTransTime2 = packet.ReadBit();
                    transportGuid[7] = packet.ReadBit();//55
                    transportGuid[4] = packet.ReadBit();//52
                    hasTransTime3 = packet.ReadBit();
                    transportGuid[0] = packet.ReadBit();//48
                    transportGuid[1] = packet.ReadBit();//49
                    transportGuid[3] = packet.ReadBit();//51
                }

                if (hasMovementFlags2)
                    packet.ReadEnum<MovementFlagExtra>("Movement flags extra", 12);

                if (hasMovementFlags)
                    packet.ReadEnum<MovementFlag>("Movement flags", 30);

                if (hasFallData)
                    hasFallDirection = packet.ReadBit("hasFallDirection");

                packet.ParseBitStream(guid, 1, 4, 7, 3, 0, 2, 5, 6);

                if (hasTrans)
                {
                    var tpos = new Vector4();
                    packet.ReadSByte("Transport Seat");
                    tpos.O = packet.ReadSingle();
                    packet.ReadUInt32("Transport Time");

                    packet.ReadXORByte(transportGuid, 6);
                    packet.ReadXORByte(transportGuid, 5);

                    if (hasTransTime2)
                        packet.ReadUInt32("Transport Time 2");

                    tpos.X = packet.ReadSingle();

                    packet.ReadXORByte(transportGuid, 4);

                    tpos.Z = packet.ReadSingle();

                    packet.ReadXORByte(transportGuid, 2);
                    packet.ReadXORByte(transportGuid, 0);

                    if (hasTransTime3)
                        packet.ReadUInt32("Transport Time 3");

                    packet.ReadXORByte(transportGuid, 1);
                    packet.ReadXORByte(transportGuid, 3);

                    tpos.Y = packet.ReadSingle();

                    packet.ReadXORByte(transportGuid, 7);

                    packet.WriteGuid("Transport Guid", transportGuid);
                    packet.WriteLine("Transport Position: {0}", tpos);
                }

                if (hasO)
                    pos.O = packet.ReadSingle();

                if (hasSplineElev)
                    packet.ReadSingle("Spline elevation");

                if (hasFallData)
                {
                    packet.ReadUInt32("Fall time");
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Fall Cos");
                        packet.ReadSingle("Fall Sin");
                        packet.ReadSingle("Horizontal Speed");
                    }
                    packet.ReadSingle("Vertical Speed");
                }

                if (hasTime)
                    packet.ReadUInt32("Timestamp");

                if (hasPitch)
                    packet.ReadSingle("Pitch");

                packet.WriteGuid("Guid", guid);
                packet.WriteLine("Position: {0}", pos);
            }
        }

        [HasSniffData]
        [Parser(Opcode.SMSG_SPELL_START)]
        [Parser(Opcode.SMSG_SPELL_GO)]
        public static void HandleSpellStart(Packet packet)
        {
            bool isSpellGo = packet.Opcode == Opcodes.GetOpcode(Opcode.SMSG_SPELL_GO);

            packet.ReadPackedGuid("Caster GUID");
            packet.ReadPackedGuid("Caster Unit GUID");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadByte("Cast Count");

            var spellId = packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");

            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) && !isSpellGo)
                packet.ReadByte("Cast Count");

            var flagsTypeCode = ClientVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? TypeCode.Int32 : TypeCode.UInt16;
            var flags = packet.ReadEnum<CastFlag>("Cast Flags", flagsTypeCode);

            packet.ReadUInt32("Time");
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_3_0_15005))
                packet.ReadUInt32("Time2");

            if (isSpellGo)
            {
                var hitCount = packet.ReadByte("Hit Count");
                for (var i = 0; i < hitCount; i++)
                    packet.ReadGuid("Hit GUID", i);

                var missCount = packet.ReadByte("Miss Count");
                for (var i = 0; i < missCount; i++)
                {
                    packet.ReadGuid("Miss GUID", i);

                    var missType = packet.ReadEnum<SpellMissType>("Miss Type", TypeCode.Byte, i);
                    if (missType == SpellMissType.Reflect)
                        packet.ReadEnum<SpellMissType>("Miss Reflect", TypeCode.Byte, i);
                }
            }

            var targetFlags = ReadSpellCastTargets(ref packet);

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                if (flags.HasAnyFlag(CastFlag.PredictedPower))
                {
                    if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_1_0_16309))
                    {
                        var count = packet.ReadUInt32("Modified Power Count");
                        for (var i = 0; i < count; i++)
                        {
                            packet.ReadEnum<PowerType>("Power Type", TypeCode.UInt32, i);
                            packet.ReadInt32("Power Value", i);
                        }
                    }
                    else
                        packet.ReadInt32("Rune Cooldown");
                }

                if (flags.HasAnyFlag(CastFlag.RuneInfo))
                {
                    var spellRuneState = packet.ReadByte("Spell Rune State");
                    var playerRuneState = packet.ReadByte("Player Rune State");

                    for (var i = 0; i < 6; i++)
                    {
                        if (ClientVersion.RemovedInVersion(ClientVersionBuild.V4_2_2_14545))
                        {
                            var mask = 1 << i;
                            if ((mask & spellRuneState) == 0)
                                continue;

                            if ((mask & playerRuneState) != 0)
                                continue;
                        }

                        packet.ReadByte("Rune Cooldown Passed", i);
                    }
                }

                if (isSpellGo)
                {
                    if (flags.HasAnyFlag(CastFlag.AdjustMissile))
                    {
                        packet.ReadSingle("Elevation");
                        packet.ReadInt32("Delay time");
                    }
                }
            }

            if (flags.HasAnyFlag(CastFlag.Projectile))
            {
                packet.ReadInt32("Ammo Display ID");
                packet.ReadEnum<InventoryType>("Ammo Inventory Type", TypeCode.Int32);
            }

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            {
                if (isSpellGo)
                {
                    if (flags.HasAnyFlag(CastFlag.VisualChain))
                    {
                        packet.ReadInt32("Unk Int32 2");
                        packet.ReadInt32("Unk Int32 3");
                    }

                    if (targetFlags.HasAnyFlag(TargetFlag.DestinationLocation))
                        packet.ReadSByte("Unk Byte 2"); // Some count

                    if (targetFlags.HasAnyFlag(TargetFlag.ExtraTargets))
                    {
                        var targetCount = packet.ReadInt32("Extra Targets Count");
                        for (var i = 0; i < targetCount; i++)
                        {
                            packet.ReadVector3("Extra Target Position", i);
                            packet.ReadGuid("Extra Target GUID", i);
                        }
                    }
                }
                else
                {
                    if (flags.HasAnyFlag(CastFlag.Immunity))
                    {
                        packet.ReadInt32("CastSchoolImmunities");
                        packet.ReadInt32("CastImmunities");
                    }

                    if (flags.HasAnyFlag(CastFlag.HealPrediction))
                    {
                        packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Predicted Spell ID");

                        if (packet.ReadByte("Unk Byte") == 2)
                            packet.ReadPackedGuid("Unk Guid");
                    }
                }
            }

            if (isSpellGo)
                packet.AddSniffData(StoreNameType.Spell, spellId, "SPELL_GO");
        }

        [Parser(Opcode.SMSG_LEARNED_SPELL)]
        public static void HandleLearnedSpell(Packet packet)
        {
            var autoPush = packet.ReadBit() != 0;
            Console.WriteLine("autoPush: " + autoPush);

            var count = packet.ReadBits("Count", 22);

            for (int i = 0; i < count; i++)
                packet.ReadUInt32("Spell ID");
        }

        [Parser(Opcode.CMSG_UPDATE_PROJECTILE_POSITION)]
        public static void HandleUpdateProjectilePosition(Packet packet)
        {
            packet.ReadGuid("GUID");
            packet.ReadEntryWithName<Int32>(StoreNameType.Spell, "Spell ID");
            packet.ReadByte("Cast ID");
            packet.ReadVector3("Position");
        }

        [Parser(Opcode.SMSG_SET_PROJECTILE_POSITION)]
        public static void HandleSetProjectilePosition(Packet packet)
        {
            packet.ReadGuid("GUID");
            packet.ReadByte("Cast ID");
            packet.ReadVector3("Position");
        }

        [Parser(Opcode.SMSG_REMOVED_SPELL, ClientVersionBuild.Zero, ClientVersionBuild.V3_1_0_9767)]
        public static void HandleRemovedSpell(Packet packet)
        {
            packet.ReadEntryWithName<UInt16>(StoreNameType.Spell, "Spell ID");
        }

        [Parser(Opcode.CMSG_CANCEL_AURA)]
        [Parser(Opcode.SMSG_REMOVED_SPELL, ClientVersionBuild.V3_1_0_9767)]
        [Parser(Opcode.CMSG_CANCEL_CHANNELLING)]
        public static void HandleRemovedSpell2(Packet packet)
        {
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
        }

        [Parser(Opcode.SMSG_PLAY_SPELL_IMPACT)]
        public static void HandleCastVisual(Packet packet)
        {
            packet.ReadGuid("Caster GUID");
            packet.ReadUInt32("SpellVisualKit ID");
        }

        [Parser(Opcode.SMSG_PLAY_SPELL_VISUAL)]
        public static void HandleCastVisual434(Packet packet)
        {
            var casterGuid = new byte[8];
            var guid2 = new byte[8];

            casterGuid[7] = packet.ReadBit();
            guid2[6] = packet.ReadBit();
            guid2[5] = packet.ReadBit();
            casterGuid[3] = packet.ReadBit();
            casterGuid[0] = packet.ReadBit();
            casterGuid[6] = packet.ReadBit();
            guid2[2] = packet.ReadBit();
            packet.ReadBit("Unk Bit"); // usually 1
            casterGuid[5] = packet.ReadBit();
            guid2[1] = packet.ReadBit();
            guid2[0] = packet.ReadBit();
            casterGuid[1] = packet.ReadBit();
            casterGuid[4] = packet.ReadBit();
            guid2[4] = packet.ReadBit();
            guid2[7] = packet.ReadBit();
            guid2[3] = packet.ReadBit();
            casterGuid[2] = packet.ReadBit();

            packet.ReadXORByte(casterGuid, 4);
            packet.ReadXORByte(guid2, 0);
            packet.ReadSingle("Y");
            packet.ReadXORByte(guid2, 6);
            packet.ReadSingle("Z");
            packet.ReadSingle("O");
            packet.ReadXORByte(casterGuid, 0);
            packet.ReadInt32("SpellVisualKit ID"); // not confirmed
            packet.ReadXORByte(casterGuid, 3);
            packet.ReadXORByte(guid2, 3);
            packet.ReadInt16("Unk Int16 1"); // usually 0
            packet.ReadXORByte(casterGuid, 7);
            packet.ReadInt16("Unk Int16 2"); // usually 0
            packet.ReadXORByte(guid2, 5);
            packet.ReadXORByte(guid2, 2);
            packet.ReadSingle("X");
            packet.ReadXORByte(guid2, 4);
            packet.ReadXORByte(guid2, 1);
            packet.ReadXORByte(casterGuid, 2);
            packet.ReadXORByte(guid2, 7);
            packet.ReadXORByte(casterGuid, 5);
            packet.ReadXORByte(casterGuid, 6);
            packet.ReadXORByte(casterGuid, 1);

            packet.WriteGuid("Caster GUID", casterGuid);
            packet.WriteGuid("Unk GUID", guid2);
        }

        [Parser(Opcode.SMSG_PLAY_SPELL_VISUAL_KIT)]
        public static void HandleCastVisualKit(Packet packet)
        {
            var thisGuid = new byte[8];

            thisGuid = packet.StartBitStream(3, 0, 6, 7, 4, 1, 5, 2);

            packet.ReadUInt32("Unk UInt32");
            packet.ReadXORByte(thisGuid, 4);
            packet.ReadXORByte(thisGuid, 7);
            packet.ReadXORByte(thisGuid, 0);
            packet.ReadUInt32("Unk UInt32");
            packet.ReadXORByte(thisGuid, 2);
            packet.ReadXORByte(thisGuid, 1);
            packet.ReadXORByte(thisGuid, 5);
            packet.ReadXORByte(thisGuid, 6);
            packet.ReadXORByte(thisGuid, 3);
            packet.ReadUInt32("Unk UInt32");

            packet.WriteGuid("Unit GUID", thisGuid);
        }

        [Parser(Opcode.SMSG_PET_CAST_FAILED)]
        [Parser(Opcode.SMSG_CAST_FAILED)]
        public static void HandleCastFailed(Packet packet)
        {
            if (ClientVersion.AddedInVersion(ClientType.WrathOfTheLichKing))
                packet.ReadByte("Cast count");

            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");

            var result = packet.ReadEnum<SpellCastFailureReason>("Reason", TypeCode.Byte);

            if (ClientVersion.RemovedInVersion(ClientType.WrathOfTheLichKing))
                packet.ReadByte("Cast count");

            switch (result)
            {
                case SpellCastFailureReason.NeedExoticAmmo:
                    if (packet.CanRead())
                        packet.ReadUInt32("Item SubclassMask");
                    break;
                case SpellCastFailureReason.TooManyOfItem:
                    if (packet.CanRead())
                        packet.ReadUInt32("Limit");
                    break;
                case SpellCastFailureReason.Totems:
                case SpellCastFailureReason.TotemCategory:
                    if (packet.CanRead())
                        packet.ReadUInt32("Totem 1");
                    if (packet.CanRead())
                        packet.ReadUInt32("Totem 2");
                    break;
                case SpellCastFailureReason.Reagents:
                    if (packet.CanRead())
                        packet.ReadUInt32("Reagent ID");
                    break;
                case SpellCastFailureReason.RequiresSpellFocus:
                    if (packet.CanRead())
                        packet.ReadUInt32("Spell Focus");
                    break;
                case SpellCastFailureReason.RequiresArea:
                    if (packet.CanRead())
                        packet.ReadEntryWithName<UInt32>(StoreNameType.Area, "Area ID");
                    break;
                case SpellCastFailureReason.CustomError:
                    if (packet.CanRead())
                        packet.ReadUInt32("Error ID");
                    break;
                case SpellCastFailureReason.PreventedByMechanic:
                    if (packet.CanRead())
                        packet.ReadEnum<SpellMechanic>("Mechanic", TypeCode.UInt32);
                    break;
                case SpellCastFailureReason.EquippedItemClass:
                case SpellCastFailureReason.EquippedItemClassMainhand:
                case SpellCastFailureReason.EquippedItemClassOffhand:
                    if (packet.CanRead())
                        packet.ReadEnum<ItemClass>("Class", TypeCode.UInt32);
                    if (packet.CanRead())
                        packet.ReadUInt32("SubclassMask");
                    break;
                case SpellCastFailureReason.MinSkill:
                    if (packet.CanRead())
                        packet.ReadUInt32("Skill Type"); // SkillLine.dbc
                    if (packet.CanRead())
                        packet.ReadUInt32("Required Amount");
                    break;
                case SpellCastFailureReason.FishingTooLow:
                    if (packet.CanRead())
                        packet.ReadUInt32("Required fishing skill");
                    break;
                // Following is post 3.3.5a
                case SpellCastFailureReason.NotReady:
                    if (packet.CanRead())
                        packet.ReadInt32("Extra Cast Number");
                    break;
                case SpellCastFailureReason.Silenced:
                case SpellCastFailureReason.NotStanding:
                    if (packet.CanRead())
                        packet.ReadInt32("Unk");
                    break;
                default:
                    if (packet.CanRead())
                        packet.ReadUInt32("Unknown1");
                    if (packet.CanRead())
                        packet.ReadUInt32("Unknown2");
                    break;
            }
        }

        [Parser(Opcode.SMSG_SPELL_FAILURE)]
        public static void HandleSpellFailure(Packet packet)
        {
            packet.ReadByte("CastCount");
            packet.ReadUInt32("Unk UInt32");
            packet.ReadUInt32("Spell ID");

            packet.ReadBit("bit4");
            packet.ReadBit("bit5");
        }

        [Parser(Opcode.SMSG_SPELL_FAILED_OTHER)]
        public static void HandleSpellFailedOther(Packet packet)
        {
            var guid = new byte[8];

            guid = packet.StartBitStream(5, 6, 7, 0, 4, 3, 1, 2);

            packet.ReadXORByte(guid, 4);
            packet.ReadXORByte(guid, 4);
            packet.ReadByte("Unk Byte");
            packet.ReadByte("Unk Byte");
            packet.ReadXORByte(guid, 0);
            packet.ReadXORByte(guid, 7);
            packet.ReadXORByte(guid, 2);
            packet.ReadXORByte(guid, 1);
            packet.ReadXORByte(guid, 3);
            packet.ReadUInt32("Unk UInt32");
            packet.ReadXORByte(guid, 5);

            packet.WriteGuid("Caster GUID", guid);
        }

        [Parser(Opcode.SMSG_SPELLINSTAKILLLOG)]
        public static void HandleSpellInstakillLog(Packet packet)
        {
            packet.ReadGuid("Target GUID");
            packet.ReadGuid("Caster GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
        }

        [Parser(Opcode.SMSG_SPELLORDAMAGE_IMMUNE)]
        [Parser(Opcode.SMSG_PROCRESIST)]
        public static void HandleSpellProcResist(Packet packet)
        {
            packet.ReadGuid("Caster GUID");
            packet.ReadGuid("Target GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
            packet.ReadBoolean("Unk bool");

            // not found in 3.3.5a sniff nor 3.3.3a
            //if (ClientVersion.Build == ClientVersionBuild.V3_3_5a_12340) // blizzard retardness, client doesn't even read this
            //{
            //    packet.ReadSingle("Unk");
            //    packet.ReadSingle("Unk");
            //}
        }

        [Parser(Opcode.MSG_CHANNEL_UPDATE)]
        public static void HandleSpellChannelUpdate(Packet packet)
        {
            packet.ReadPackedGuid("GUID");
            packet.ReadUInt32("Timestamp");
        }

        [Parser(Opcode.MSG_CHANNEL_START)]
        public static void HandleSpellChannelStart(Packet packet)
        {
            packet.ReadPackedGuid("GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
            packet.ReadInt32("Duration");

            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V4_0_6a_13623))
                return;

            if (packet.ReadBoolean("Has castflag Immunity"))
            {
                packet.ReadUInt32("CastSchoolImmunities");
                packet.ReadUInt32("CastImmunities");
            }

            if (packet.ReadBoolean("Has castflag HealPrediction"))
            {
                packet.ReadPackedGuid("Target GUID");
                packet.ReadInt32("Heal Amount");

                var type = packet.ReadByte("Type");
                if (type == 2)
                    packet.ReadPackedGuid("Unk GUID");
            }
        }

        [Parser(Opcode.SMSG_BREAK_TARGET)]
        [Parser(Opcode.SMSG_DISMOUNT)]
        public static void HandleDismount(Packet packet)
        {
            packet.ReadPackedGuid("GUID");
        }

        [Parser(Opcode.SMSG_MOUNTRESULT)]
        public static void HandleMountResult(Packet packet)
        {
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_0_6a_13623) && ClientVersion.RemovedInVersion(ClientVersionBuild.V4_3_4_15595))
                packet.ReadPackedGuid("GUID");
            packet.ReadEnum<MountResult>("Result", TypeCode.Int32);
        }

        [Parser(Opcode.SMSG_DISMOUNTRESULT)]
        public static void HandleDismountResult(Packet packet)
        {
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_3_4_15595)) // Don't know for previous.
                packet.ReadEnum<DismountResult>("Result", TypeCode.Int32);
        }

        [Parser(Opcode.CMSG_GET_MIRRORIMAGE_DATA)]
        public static void HandleGetMirrorImageData(Packet packet)
        {
            packet.ReadGuid("GUID");
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_3_0_15005))
                packet.ReadUInt32("Display Id");
        }

        [Parser(Opcode.SMSG_MIRRORIMAGE_DATA)]
        public static void HandleMirrorImageData(Packet packet)
        {
            packet.ReadGuid("GUID");
            packet.ReadUInt32("Display ID");
            var race = packet.ReadEnum<Race>("Race", TypeCode.Byte);

            if (race == Race.None)
                return;

            packet.ReadEnum<Gender>("Gender", TypeCode.Byte);
            packet.ReadEnum<Class>("Class", TypeCode.Byte);

            if (!packet.CanRead())
                return;

            packet.ReadByte("Skin");
            packet.ReadByte("Face");
            packet.ReadByte("Hair Style");
            packet.ReadByte("Hair Color");
            packet.ReadByte("Facial Hair");
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
                packet.ReadGuid("Guild Guid");
            else
                packet.ReadUInt32("Guild Id");

            EquipmentSlotType[] slots = {
                EquipmentSlotType.Head, EquipmentSlotType.Shoulders, EquipmentSlotType.Shirt,
                EquipmentSlotType.Chest, EquipmentSlotType.Waist, EquipmentSlotType.Legs,
                EquipmentSlotType.Feet, EquipmentSlotType.Wrists, EquipmentSlotType.Hands,
                EquipmentSlotType.Back, EquipmentSlotType.Tabard };

            for (var i = 0; i < 11; ++i)
                packet.ReadEntryWithName<UInt32>(StoreNameType.Item, "[" + slots[i] + "] Item Entry");
        }

        [Parser(Opcode.SMSG_CLEAR_COOLDOWN)]
        public static void HandleClearCooldown(Packet packet)
        {
            var playerGuid = new byte[8];

            packet.StartBitStream(playerGuid, 6, 5, 2, 4, 0, 7, 3);
            var unkBit = packet.ReadBit("unkBit");
            playerGuid[1] = packet.ReadBit();

            packet.ParseBitStream(playerGuid, 5, 2, 6, 3, 1, 0, 4, 7);
            packet.ReadUInt32("Spell ID");

            packet.WriteGuid("Player GUID", playerGuid);
        }

        [Parser(Opcode.SMSG_SET_PCT_SPELL_MODIFIER, ClientVersionBuild.V4_0_6_13596)]
        [Parser(Opcode.SMSG_SET_FLAT_SPELL_MODIFIER, ClientVersionBuild.V4_0_6_13596)]
        public static void HandleSetSpellModifierFlat406(Packet packet)
        {
            if (packet.Opcode == 7174)
            {
                // SMSG_SET_PCT_SPELL_MODIFIER - 7174 in 5.4.0
                var modCount = packet.ReadBits("Modifier type count", 22);

                var modTypeCount = new uint[modCount];
                for (var j = 0; j < modCount; ++j)
                    modTypeCount[j] = (uint)packet.ReadBits("Count", 21, j);

                for (var j = 0; j < modCount; ++j)
                {
                    packet.ReadEnum<SpellModOp>("Spell Mod", TypeCode.Byte, j);

                    for (var i = 0; i < modTypeCount[j]; ++i)
                    {
                        packet.ReadSingle("Amount", j, i);
                        packet.ReadByte("Spell Mask bitpos", j, i);
                    }
                }
            }
            else if (packet.Opcode == 5424)
            {
                // SMSG_SET_FLAT_SPELL_MODIFIER - 5424 in 5.4.0
                var modCount = packet.ReadBits("Modifier type count", 22);

                var modTypeCount = new uint[modCount];
                for (var j = 0; j < modCount; ++j)
                    modTypeCount[j] = (uint)packet.ReadBits("Count", 21, j);

                for (var j = 0; j < modCount; ++j)
                {
                    for (var i = 0; i < modTypeCount[j]; ++i)
                    {
                        packet.ReadSingle("Amount", j, i);
                        packet.ReadByte("Spell Mask bitpos", j, i);
                    }

                    packet.ReadEnum<SpellModOp>("Spell Mod", TypeCode.Byte, j);
                }
            }
        }

        [Parser(Opcode.SMSG_DISPEL_FAILED)]
        public static void HandleDispelFailed(Packet packet)
        {
            packet.ReadGuid("Caster GUID");
            packet.ReadGuid("Target GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Dispelling Spell ID");

            for (var i = 0; packet.CanRead(); i++)
                packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Dispelled Spell ID", i);
        }

        [Parser(Opcode.CMSG_TOTEM_DESTROYED)]
        public static void HandleTotemDestroyed(Packet packet)
        {
            packet.ReadByte("Slot");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_3_0_15005)) // guessing, present on 4.3.4
                packet.ReadGuid("Unk");
        }

        [Parser(Opcode.SMSG_TOTEM_CREATED)]
        public static void HandleTotemCreated(Packet packet)
        {
            var guid = new byte[8];

            packet.StartBitStream(guid, 2, 6, 4, 0, 7, 3, 5, 1);

            packet.ReadUInt32("Duration");
            packet.ReadXORByte(guid, 1);
            packet.ReadXORByte(guid, 3);
            packet.ReadUInt32("Spell ID");
            packet.ReadXORByte(guid, 7);
            packet.ReadXORByte(guid, 4);
            packet.ReadXORByte(guid, 0);
            packet.ReadByte("Slot");
            packet.ReadXORByte(guid, 6);
            packet.ReadXORByte(guid, 2);
            packet.ReadXORByte(guid, 5);

            packet.WriteGuid("Totem GUID", guid);
        }

        [Parser(Opcode.CMSG_CANCEL_CAST)]
        public static void HandlePlayerCancelCast(Packet packet)
        {
            packet.ReadByte("Count");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell Id");
        }

        [Parser(Opcode.SMSG_UNIT_SPELLCAST_START)]
        public static void HandleUnitSpellcastStart(Packet packet)
        {
            packet.ReadPackedGuid("Caster GUID");
            packet.ReadPackedGuid("Target GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell Id");
            packet.ReadInt32("Time Casted");
            packet.ReadInt32("Cast time");

            if (packet.ReadBoolean("Unknown bool"))
            {
                packet.ReadUInt32("Unk");
                packet.ReadUInt32("Unk");
            }
        }

        [Parser(Opcode.SMSG_SPELL_DELAYED)]
        public static void HandleSpellDelayed(Packet packet)
        {
            packet.ReadPackedGuid("Caster GUID");
            packet.ReadInt32("Delay Time");
        }

        [Parser(Opcode.SMSG_SPELL_UPDATE_CHAIN_TARGETS)]
        public static void HandleUpdateChainTargets(Packet packet)
        {
            packet.ReadGuid("Caster GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
            var count = packet.ReadInt32("Count");
            for (var i = 0; i < count; i++)
                packet.ReadGuid("Chain target");
        }

        [Parser(Opcode.SMSG_AURACASTLOG)]
        public static void HandleAuraCastLog(Packet packet)
        {
            packet.ReadGuid("Caster GUID");
            packet.ReadGuid("Target GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
            packet.ReadSingle("Unk 1");
            packet.ReadSingle("Unk 2");
        }


        [Parser(Opcode.SMSG_SPELL_CHANCE_PROC_LOG)]
        public static void HandleChanceProcLog(Packet packet)
        {
            packet.ReadGuid("Caster GUID");
            packet.ReadGuid("Target GUID");
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
            packet.ReadSingle("unk1");
            packet.ReadSingle("unk2");
            packet.ReadEnum<UnknownFlags>("ProcFlags", TypeCode.UInt32);
            packet.ReadEnum<UnknownFlags>("Flags2", TypeCode.UInt32);
        }

        [Parser(Opcode.SMSG_SPELL_CATEGORY_COOLDOWN)]
        public static void HandleSpellCategoryCooldown(Packet packet)
        {
            var count = packet.ReadBits("Count", 23);

            for (int i = 0; i < count; ++i)
            {
                packet.ReadInt32("Category Cooldown");
                packet.ReadInt32("Cooldown");
            }
        }

        [Parser(Opcode.SMSG_AURA_POINTS_DEPLETED)]
        public static void HandleAuraPointsDepleted(Packet packet)
        {
            var guid = packet.StartBitStream(2, 4, 1, 7, 5, 0, 3, 6);

            packet.ReadXORByte(guid, 5);
            packet.ReadXORByte(guid, 0);
            packet.ReadByte("Points?");
            packet.ReadXORByte(guid, 3);
            packet.ReadXORByte(guid, 7);
            packet.ReadXORByte(guid, 4);
            packet.ReadXORByte(guid, 2);
            packet.ReadByte("Slot ID?");
            packet.ReadXORByte(guid, 6);
            packet.ReadXORByte(guid, 1);
            packet.WriteGuid("Guid", guid);
        }

        [Parser(Opcode.SMSG_DISENCHANT_CREDIT)] // 4.3.4
        public static void HandleDisenchantCredit(Packet packet)
        {
            var guid = packet.StartBitStream(0, 6, 3, 1, 7, 5, 2, 4);

            packet.ReadEntryWithName<UInt32>(StoreNameType.Item, "Item Entry");

            packet.ReadXORByte(guid, 4);
            packet.ReadXORByte(guid, 1);

            packet.ReadInt32("Unk Int32");
            packet.ReadInt32("Unk Int32");

            packet.ReadXORByte(guid, 6);
            packet.ReadXORByte(guid, 5);
            packet.ReadXORByte(guid, 2);
            packet.ReadXORByte(guid, 7);
            packet.ReadXORByte(guid, 3);
            packet.ReadXORByte(guid, 0);

            packet.WriteGuid("Guid", guid);
        }

        [Parser(Opcode.SMSG_MISSILE_CANCEL)] // 4.3.4
        public static void HandleMissileCancel(Packet packet)
        {
            var guid = new byte[8];
            guid[7] = packet.ReadBit();
            guid[2] = packet.ReadBit();
            guid[4] = packet.ReadBit();
            guid[6] = packet.ReadBit();
            packet.ReadBit("Cancel");
            guid[1] = packet.ReadBit();
            guid[0] = packet.ReadBit();
            guid[3] = packet.ReadBit();
            guid[5] = packet.ReadBit();

            packet.ReadXORByte(guid, 6);
            packet.ReadXORByte(guid, 1);
            packet.ReadXORByte(guid, 4);
            packet.ReadXORByte(guid, 2);
            packet.ReadXORByte(guid, 5);
            packet.ReadXORByte(guid, 7);
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
            packet.ReadXORByte(guid, 0);
            packet.ReadXORByte(guid, 3);

            packet.WriteGuid("Guid", guid);
        }

        [Parser(Opcode.SMSG_SPIRIT_HEALER_CONFIRM)]
        [Parser(Opcode.SMSG_CLEAR_TARGET)]
        public static void HandleMiscGuid(Packet packet)
        {
            packet.ReadGuid("GUID");
        }

        [Parser(Opcode.SMSG_MODIFY_COOLDOWN)]
        public static void HandleModifyCooldown(Packet packet)
        {
            packet.ReadEntryWithName<UInt32>(StoreNameType.Spell, "Spell ID");
            packet.ReadGuid("GUID");
            packet.ReadInt32("Cooldown Mod");
        }

        [Parser(Opcode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA)]
        [Parser(Opcode.CMSG_CANCEL_AUTO_REPEAT_SPELL)]
        [Parser(Opcode.CMSG_CANCEL_GROWTH_AURA)]
        [Parser(Opcode.CMSG_CANCEL_MOUNT_AURA)]
        [Parser(Opcode.CMSG_REQUEST_CATEGORY_COOLDOWNS)]
        public static void HandleSpellNull(Packet packet)
        {
        }
    }
}

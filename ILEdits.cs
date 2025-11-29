using Terraria.ModLoader;
using System.IO;
using Terraria;
using ScuffedAnticheatMod.Network;
using ReLogic.Content;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System;
using ScuffedAnticheatMod.UI;
using System.Reflection;
using Terraria.ModLoader.Core;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Localization;

namespace ScuffedAnticheatMod
{
    public class ILEdits
    {
        private static void GetData_ILEdit(ILContext il)
        {
            ILCursor c = new ILCursor(il);


            ILLabel[] targets = null;
            while (c.TryGotoNext(i => i.MatchSwitch(out targets)))
            {
                // Compiler wants the starting case to be 0, so it will subtract away the lowest case and shift everything down
                // ldc.i4.1
                // sub
                // switch
                int offset = 0;
                if (c.Prev.MatchSub() && c.Prev.Previous.MatchLdcI4(out offset))
                {
                    ;
                }

                // Get the label for case 3 if it exists
                int case3Index = 3 - offset;
                if (case3Index < 0 || case3Index >= targets.Length || targets[case3Index] is not ILLabel target3)
                {
                    continue;
                }

                // Move cursor to case 3: PlayerInfo
                c.GotoLabel(target3);
                HookCase3(c);

                // Get the label for case 5 if it exists
                int case5Index = 5 - offset;
                if (case5Index < 0 || case5Index >= targets.Length || targets[case5Index] is not ILLabel target5)
                {
                    continue;
                }
                // Move the cursor to case 5:
                c.GotoLabel(target5);
                HookCase5(il, c);

                return;
            }

            throw new Exception("Hook location not found, switch(*) { case 5: ...");
        }

        private static void HookCase3(ILCursor c)
        {
            int playerNumberIndex = -1;
            c.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<BinaryReader>("reader"),
                i => i.MatchCallvirt<BinaryReader>("ReadByte"),
                i => i.MatchStloc(out playerNumberIndex)
            );

            c.GotoNext(
                i => i.MatchLdcI4(6),
                i => i.MatchLdcI4(-1),
                i => i.MatchLdcI4(-1),
                i => i.MatchLdnull(),
                i => i.MatchLdcI4(0),
                i => i.MatchLdcR4(0f),
                i => i.MatchLdcR4(0f),
                i => i.MatchLdcR4(0f),
                i => i.MatchLdcI4(0),
                i => i.MatchLdcI4(0),
                i => i.MatchLdcI4(0),
                i => i.MatchCall<bool>("TrySendData")
            );

            c.Emit(OpCodes.Ldloc_S, playerNumberIndex);

            // Hold the player hostage until they do SAM handshake
            c.EmitDelegate(async (int playerNumber) =>
            {
                SAMNetwork.InitiateHandshake(playerNumber);
                int timeout = 10000;
                int interval = 100;
                int elapsed = 0;

                while (!SAMNetwork.allowedPlayers[playerNumber] && elapsed < timeout)
                {
                    await Task.Delay(interval);
                    elapsed += interval;
                }

                if (elapsed < timeout)
                {
                    FinishCase3(c);
                    return;
                }
                else
                    NetMessage.SendData(MessageID.Kick, playerNumber, -1, NetworkText.FromLiteral("SAM: Client failed to authenticate"));
            });
            c.Emit(OpCodes.Ret);
        }

        private static void FinishCase3_Runtime()
        {
            Netplay.Connection.
        }

        private static void FinishCase3(ILCursor c)
        {
            c.Emit(OpCodes.Ldc_I4_6);
            c.Emit(OpCodes.Ldc_I4_M1);
            c.Emit(OpCodes.Ldc_I4_M1);
            c.Emit(OpCodes.Ldnull);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Ldc_R4, 0f);
            c.Emit(OpCodes.Ldc_R4, 0f);
            c.Emit(OpCodes.Ldc_R4, 0f);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit<bool>(OpCodes.Call, "TrySendData");
            c.Emit(OpCodes.Pop);
            c.Emit<RemoteServer>(OpCodes.Ldsfld, "Connection");
            c.Emit<int>(OpCodes.Ldfld, "State");
            c.Emit(OpCodes.Ldc_I4_2);
            c.Emit(OpCodes.Bne_Un, OpCodes.Ret);
            c.Emit<RemoteServer>(OpCodes.Ldsfld, "Connection");
            c.Emit(OpCodes.Ldc_I4_3);
            c.Emit<int>(OpCodes.Stfld, "State");
            c.Emit(OpCodes.Ret);
        }

        private static void HookCase5(ILContext il, ILCursor c)
        {       
                // load argument 0 (this) onto stack and use it for hook
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<MessageBuffer, (int, int)>>((self) =>
                {
                    BinaryReader r = self.reader;
                    long start = r.BaseStream.Position;

                    // 1) bufferID (overriden), 2) slotType, 3) type (doesn't work idk), 4) prefix, 5) stack
                    _ = r.ReadByte();
                    int slotType = r.ReadInt16();
                    // ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{slotType}"), Color.Green);

                    r.BaseStream.Position = start;

                    return (self.whoAmI, slotType);
                });
                TypeReference tupleType = il.Method.Module.ImportReference(typeof((int, int)));
                VariableDefinition localTuple = new VariableDefinition(tupleType);
                il.Body.Variables.Add(localTuple);
                c.Emit(OpCodes.Stloc, localTuple);

                c.GotoNext(i => i.MatchEndfinally());
                // c.GotoNext(i => i.MatchLeave(out _));
                // c.GotoNext(
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchLdcR4(out _),
                //     i => i.MatchLdcR4(out _),
                //     i => i.MatchLdcR4(out _),
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchCall(typeof(bool), "TrySendData")
                //     );
                c.Emit(OpCodes.Ldloc, localTuple);
                c.EmitDelegate<Action<(int, int)>>(x =>
                {
                    // ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{x.Item1}, {x.Item2}"), Color.Beige);
                    UpdateInventory.OnInventoryChange(x.Item1, x.Item2);
                });

                // foreach (var instr in c.Instrs)
                // {
                //     instance.Logger.Info($"{instr.Offset:X4}: {instr.OpCode} {instr.Operand}");
                // }
        }
    }
}
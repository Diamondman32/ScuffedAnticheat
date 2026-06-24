using System.IO;
using Terraria;
using ScuffedAnticheatMod.Network;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Threading.Tasks;

namespace ScuffedAnticheatMod
{
    public class ILEdits
    {
        public static void GetData_ILEdit(ILContext il)
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

                // Get the label for case 5 if it exists
                int case5Index = 5 - offset;
                if (case5Index < 0 || case5Index >= targets.Length || targets[case5Index] is not ILLabel target5)
                {
                    continue;
                }
                // Move the cursor to case 5:
                c.GotoLabel(target5);
                HookCase5(il, c);

                // Get the label for case 6 if it exists
                int case6Index = 6 - offset;
                if (case6Index < 0 || case6Index >= targets.Length || targets[case6Index] is not ILLabel target6)
                {
                    continue;
                }

                // Move cursor to case 6:
                c.GotoLabel(target6);
                HookCase6(c);

                return;
            }

            throw new Exception("Hook location not found, switch(*) { case 5: ...");
        }

        // Send each item update to SAM
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

                    r.BaseStream.Position = start;

                    return (self.whoAmI, slotType);
                });
                TypeReference tupleType = il.Method.Module.ImportReference(typeof((int, int)));
                VariableDefinition localTuple = new VariableDefinition(tupleType);
                il.Body.Variables.Add(localTuple);
                c.Emit(OpCodes.Stloc, localTuple);

                c.GotoNext(i => i.MatchEndfinally());
                c.Emit(OpCodes.Ldloc, localTuple);
                c.EmitDelegate<Action<(int, int)>>(x =>
                {
                    if (Netplay.Clients[x.Item1].State > 6)
                        UpdateInventory.OnInventoryChange(x.Item1, x.Item2);
                });
        }

        // Hijack the join step until client does mod check
        private static void HookCase6(ILCursor c)
        {
            c.GotoNext(MoveType.After, i => i.MatchLdfld<MessageBuffer>("whoAmI") );

            c.Emit(OpCodes.Dup);

            // Hold the player hostage until they do SAM handshake. They are given 10 seconds lol
            c.EmitDelegate((int playerNumber) =>
            {
                SAMNetwork.InitiateHandshake(playerNumber);
                
                // Create timer on background thread to give the client time without blocking server
                _ = Task.Run(async () =>
                {
                    int timeout = 10000;
                    int interval = 100;
                    int elapsed = 0;

                    while (!SAMNetwork.allowedPlayers[playerNumber] && elapsed < timeout)
                    {
                        await Task.Delay(interval);
                        elapsed += interval;
                    }

                    // Terraria is non-thread safe so any actions must be done on main thread
                    if (elapsed < timeout)
                        SAMModSystem.EnqueueJoiningPlayer(playerNumber);
                    else
                        SAMModSystem.EnqueueJoiningPlayer(-playerNumber - 1);
                });
            });
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ret);
        }

        public static void FinishCase6_Runtime(int playerNumber)
        {
            if (Netplay.Clients[playerNumber].State == 1)
            {
                Netplay.Clients[playerNumber].State = 2;
            }
            NetMessage.TrySendData(7, playerNumber);
            Main.SyncAnInvasion(playerNumber);
        }
    }
}
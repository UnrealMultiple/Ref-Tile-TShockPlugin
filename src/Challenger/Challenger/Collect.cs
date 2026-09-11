using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;
using static Terraria.UI.ItemSlot;

namespace Challenger;

internal static class Collect
{
    public static CProjectile[] cprojs = new CProjectile[1000];

    public static CNPC[] cnpcs = new CNPC[200];

    public static CPlayer[] cplayers = new CPlayer[255];

    public static int worldevent = 0;

    public static HashSet<int> noneedlifeNPC = new HashSet<int> { 115, 116, 488 };
}
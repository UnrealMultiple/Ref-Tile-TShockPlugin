using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.UI;
using Terraria.Utilities;
using TerrariaApi.Server;
using TShockAPI;

namespace WeaponPlus;

public partial class WeaponPlus : TerrariaPlugin
{

    #region 更换背包中的武器
    public static void ReplaceWeaponsInBackpack(Player? player, WItem? item, int model = 0)
    {
        if (player == null || !player.active || item == null)
        {
            return;
        }
        var whoAmI = player.whoAmI;
        for (var i = 0; i < NetItem.InventoryIndex.Item2; i++)
        {
            if (player.inventory[i].type == item.id)
            {
                var stack = player.inventory[i].stack;
                var prefix = player.inventory[i].prefix;
                player.inventory[i].TurnToAir();
                TShock.Players[whoAmI].SendData((PacketTypes) 5, "", whoAmI, i);
                switch (model)
                {
                    case 0:
                    {
                        var num2 = Item.NewItem(null!, player.Center, item.id, stack);
                        Main.item[num2].playerIndexTheItemIsReservedFor = whoAmI;
                        Main.item[num2].inner.prefix = prefix;
                        var num3 = (int) (item.orig_damage * 0.05f * item.damage_level);
                        num3 = (num3 < item.damage_level) ? item.damage_level : num3;
                        var obj = Main.item[num2];
                        obj.inner.damage += num3;
                        var obj2 = Main.item[num2];
                        obj2.inner.scale += item.orig_scale * 0.05f * item.scale_level;
                        var obj3 = Main.item[num2];
                        obj3.inner.knockBack += item.orig_knockBack * 0.05f * item.knockBack_level;
                        Main.item[num2].inner.useAnimation = item.orig_useAnimation - item.useSpeed_level;
                        Main.item[num2].inner.useTime = (int) (item.orig_useTime * 1f / item.orig_useAnimation * Main.item[num2].inner.useAnimation);
                        var obj4 = Main.item[num2];
                        obj4.inner.shootSpeed += item.orig_shootSpeed * 0.05f * item.shootSpeed_level;
                        TShock.Players[whoAmI].SendData((PacketTypes) 21, null, num2);
                        TShock.Players[whoAmI].SendData((PacketTypes) 22, null, num2);
                        TShock.Players[whoAmI].SendData((PacketTypes) 88, null, num2, 255f, 63f);
                        break;
                    }
                    case 1:
                    {
                        var num = Item.NewItem(null!, player.Center, item.id, stack);
                        Main.item[num].playerIndexTheItemIsReservedFor = whoAmI;
                        Main.item[num].inner.prefix = prefix;
                        TShock.Players[whoAmI].SendData((PacketTypes) 21, null, num);
                        TShock.Players[whoAmI].SendData((PacketTypes) 22, null, num);
                        break;
                    }
                }
            }
        }
    }
    #endregion

    #region 扣除
    public bool Deduction(WItem WItem, TSPlayer whoAMI, PlusType plusType, int gap)
    {
        if (!WItem.plusPrice(plusType, out var price, gap))
        {
            whoAMI.SendMessage(LangTipsGet("当前该类型升级已达到上限，无法升级"), Color.Red);
            return false;
        }
        if (this.DeductCoin(whoAMI, price))
        {
            WItem.allCost += price;
            var num1 = Terraria.Utils.CoinsCount(out var flag, whoAMI.TPlayer.inventory, new int[] { 58, 57, 56, 55, 54 });
            var num2 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank.item, new int[0]);
            var num3 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank2.item, new int[0]);
            var num4 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank3.item, new int[0]);
            var num5 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank4.item, new int[0]);
            whoAMI.SendMessage(LangTipsGet("扣除钱币：") + cointostring(price, out var items) + "   " + LangTipsGet("当前剩余：") + cointostring(num1 + num2 + num3 + num4 + num5, out items), Color.Pink);
            return true;
        }
        whoAMI.SendInfoMessage(LangTipsGet("钱币不够！"));
        return false;
    }
    #endregion

    #region 提示语的随机颜色方法
    public Color getRandColor()
    {
        return new Color(Main.rand.Next(60, 255), Main.rand.Next(60, 255), Main.rand.Next(60, 255));
    }
    #endregion

    #region 扣钱方法
    public bool DeductCoin(TSPlayer one, long coin)
    {
        if (coin <= 0)
        {
            return true;
        }
        var num1 = Terraria.Utils.CoinsCount(out var flag, one.TPlayer.inventory, new int[] { 58, 57, 56, 55, 54 });
        var num2 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank.item, new int[0]);
        var num3 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank2.item, new int[0]);
        var num4 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank3.item, new int[0]);
        var num5 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank4.item, new int[0]);
        if (num1 + num2 + num3 + num4 + num5 < coin)
        {
            return false;
        }
        var num6 = 0L;
        for (var i = 0; i < NetItem.MaxInventory; i++)
        {
            if (i < NetItem.InventoryIndex.Item2)
            {
                if (one.TPlayer.inventory[i].IsACoin && i != 54 && i != 55 && i != 56 && i != 57 && i != 58)
                {
                    num6 += one.TPlayer.inventory[i].value / 5L * one.TPlayer.inventory[i].stack;
                    one.TPlayer.inventory[i].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, i);
                    if (num6 >= coin)
                    {
                        break;
                    }
                }
            }
            else if (i >= NetItem.MiscDyeIndex.Item2 && i < NetItem.PiggyIndex.Item2)
            {
                var num7 = i - NetItem.PiggyIndex.Item1;
                if (one.TPlayer.bank.item[num7].IsACoin)
                {
                    num6 += one.TPlayer.bank.item[num7].value / 5L * one.TPlayer.bank.item[num7].stack;
                    one.TPlayer.bank.item[num7].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, i);
                    if (num6 >= coin)
                    {
                        break;
                    }
                }
            }
            else if (i >= NetItem.PiggyIndex.Item2 && i < NetItem.SafeIndex.Item2)
            {
                var num8 = i - NetItem.SafeIndex.Item1;
                if (one.TPlayer.bank2.item[num8].IsACoin)
                {
                    num6 += one.TPlayer.bank2.item[num8].value / 5L * one.TPlayer.bank2.item[num8].stack;
                    one.TPlayer.bank2.item[num8].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, i);
                    if (num6 >= coin)
                    {
                        break;
                    }
                }
            }
            else if (i >= NetItem.TrashIndex.Item2 && i < NetItem.ForgeIndex.Item2)
            {
                var num9 = i - NetItem.ForgeIndex.Item1;
                if (one.TPlayer.bank3.item[num9].IsACoin)
                {
                    num6 += one.TPlayer.bank3.item[num9].value / 5L * one.TPlayer.bank3.item[num9].stack;
                    one.TPlayer.bank3.item[num9].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, i);
                    if (num6 >= coin)
                    {
                        break;
                    }
                }
            }
            else
            {
                if (i < NetItem.ForgeIndex.Item2 || i >= NetItem.VoidIndex.Item2)
                {
                    continue;
                }
                var num10 = i - NetItem.VoidIndex.Item1;
                if (one.TPlayer.bank4.item[num10].IsACoin)
                {
                    num6 += one.TPlayer.bank4.item[num10].value / 5L * one.TPlayer.bank4.item[num10].stack;
                    one.TPlayer.bank4.item[num10].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, i);
                    if (num6 >= coin)
                    {
                        break;
                    }
                }
            }
        }
        num6 -= coin;
        List<Item> items;
        if (num2 > 0)
        {
            for (var j = NetItem.MiscDyeIndex.Item2; j < NetItem.PiggyIndex.Item2; j++)
            {
                var num11 = j - NetItem.PiggyIndex.Item1;
                if (one.TPlayer.bank.item[num11].IsACoin)
                {
                    num6 += one.TPlayer.bank.item[num11].value / 5L * one.TPlayer.bank.item[num11].stack;
                    one.TPlayer.bank.item[num11].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, j);
                }
            }
            cointostring(num6, out items);
            for (var k = NetItem.MiscDyeIndex.Item2; k < NetItem.PiggyIndex.Item2; k++)
            {
                var num12 = k - NetItem.PiggyIndex.Item1;
                if (one.TPlayer.bank.item[num12].IsAir && items.Count > 0)
                {
                    one.TPlayer.bank.item[num12] = items.First();
                    items.RemoveAt(0);
                    one.SendData((PacketTypes) 5, "", one.Index, k);
                }
            }
        }
        else if (num3 > 0)
        {
            for (var l = NetItem.PiggyIndex.Item2; l < NetItem.SafeIndex.Item2; l++)
            {
                var num13 = l - NetItem.SafeIndex.Item1;
                if (one.TPlayer.bank2.item[num13].IsACoin)
                {
                    num6 += one.TPlayer.bank2.item[num13].value / 5L * one.TPlayer.bank2.item[num13].stack;
                    one.TPlayer.bank2.item[num13].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, l);
                }
            }
            cointostring(num6, out items);
            for (var m = NetItem.PiggyIndex.Item2; m < NetItem.SafeIndex.Item2; m++)
            {
                var num14 = m - NetItem.SafeIndex.Item1;
                if (one.TPlayer.bank2.item[num14].IsAir && items.Count > 0)
                {
                    one.TPlayer.bank2.item[num14] = items.First();
                    items.RemoveAt(0);
                    one.SendData((PacketTypes) 5, "", one.Index, m);
                }
            }
        }
        else if (num4 > 0)
        {
            for (var n = NetItem.TrashIndex.Item2; n < NetItem.ForgeIndex.Item2; n++)
            {
                var num15 = n - NetItem.ForgeIndex.Item1;
                if (one.TPlayer.bank3.item[num15].IsACoin)
                {
                    num6 += one.TPlayer.bank3.item[num15].value / 5L * one.TPlayer.bank3.item[num15].stack;
                    one.TPlayer.bank3.item[num15].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, n);
                }
            }
            cointostring(num6, out items);
            for (var num16 = NetItem.TrashIndex.Item2; num16 < NetItem.ForgeIndex.Item2; num16++)
            {
                var num17 = num16 - NetItem.ForgeIndex.Item1;
                if (one.TPlayer.bank3.item[num17].IsAir && items.Count > 0)
                {
                    one.TPlayer.bank3.item[num17] = items.First();
                    items.RemoveAt(0);
                    one.SendData((PacketTypes) 5, "", one.Index, num16);
                }
            }
        }
        else if (num5 > 0)
        {
            for (var num18 = NetItem.ForgeIndex.Item2; num18 < NetItem.VoidIndex.Item2; num18++)
            {
                var num19 = num18 - NetItem.VoidIndex.Item1;
                if (one.TPlayer.bank4.item[num19].IsACoin)
                {
                    num6 += one.TPlayer.bank4.item[num19].value / 5L * one.TPlayer.bank4.item[num19].stack;
                    one.TPlayer.bank4.item[num19].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, num18);
                }
            }
            cointostring(num6, out items);
            for (var num20 = NetItem.ForgeIndex.Item2; num20 < NetItem.VoidIndex.Item2; num20++)
            {
                var num21 = num20 - NetItem.VoidIndex.Item1;
                if (one.TPlayer.bank4.item[num21].IsAir && items.Count > 0)
                {
                    one.TPlayer.bank4.item[num21] = items.First();
                    items.RemoveAt(0);
                    one.SendData((PacketTypes) 5, "", one.Index, num20);
                }
            }
        }
        else
        {
            for (var num22 = 0; num22 < NetItem.InventoryIndex.Item2; num22++)
            {
                if (one.TPlayer.inventory[num22].IsACoin && num22 != 54 && num22 != 55 && num22 != 56 && num22 != 57 && num22 != 58)
                {
                    num6 += one.TPlayer.inventory[num22].value / 5L * one.TPlayer.inventory[num22].stack;
                    one.TPlayer.inventory[num22].TurnToAir();
                    one.SendData((PacketTypes) 5, "", one.Index, num22);
                }
            }
            cointostring(num6, out items);
            for (var num23 = 0; num23 < NetItem.InventoryIndex.Item2; num23++)
            {
                if (one.TPlayer.inventory[num23].IsAir && items.Count > 0 && num23 != 54 && num23 != 55 && num23 != 56 && num23 != 57 && num23 != 58)
                {
                    one.TPlayer.inventory[num23] = items.First();
                    items.RemoveAt(0);
                    one.SendData((PacketTypes) 5, "", one.Index, num23);
                }
            }
        }
        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                var num24 = Item.NewItem(new EntitySource_DebugCommand(), one.TPlayer.Center, item.type, item.stack);
                Main.item[num24].playerIndexTheItemIsReservedFor = one.Index;
                one.SendData((PacketTypes) 21, "", num24, 1f);
                one.SendData((PacketTypes) 22, null, num24);
            }
        }
        return true;
    }
    #endregion

    #region 共串
    public static string cointostring(long coin, out List<Item> items)
    {
        items = new List<Item>();
        var num = coin % 100;
        coin /= 100;
        var num2 = coin % 100;
        coin /= 100;
        var num3 = coin % 100;
        coin /= 100;
        var num4 = coin;
        var itemById = TShock.Utils.GetItemById(74);
        itemById.stack = (int) num4;
        items.Add(itemById);
        var itemById2 = TShock.Utils.GetItemById(73);
        itemById2.stack = (int) num3;
        items.Add(itemById2);
        var itemById3 = TShock.Utils.GetItemById(72);
        itemById3.stack = (int) num2;
        items.Add(itemById3);
        var itemById4 = TShock.Utils.GetItemById(71);
        itemById4.stack = (int) num;
        items.Add(itemById4);
        return $"{num4}[i:74],  {num3}[i:73],  {num2}[i:72],  {num}[i:71]";
    }
    #endregion
}
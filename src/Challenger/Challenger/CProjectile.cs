using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace Challenger;

public class CProjectile
{
    public Projectile proj;

    private readonly int type;

    private readonly int index;

    private readonly int owner;

    public float[] ai;

    public int lable;

    private bool _isActive;

    public bool isActive
    {
        get => this.proj != null && this.proj.type == this.type && this.proj.whoAmI == this.index && this.proj.owner == this.owner && Main.player[this.owner].active && this.proj.active && this._isActive;
        set => this._isActive = value;
    }

    protected CProjectile()
    {
        this.proj = null!;
        this.type = 0;
        this.index = 0;
        this.owner = 0;
        this.ai = new float[8];
        this.lable = 0;
        this.isActive = false;
    }

    protected CProjectile(Projectile? proj)
    {
        if (proj == null)
        {
            this.proj = null!;
            this.type = 0;
            this.index = 0;
            this.owner = 0;
            this.ai = new float[8];
            this.lable = 0;
            this.isActive = false;
        }
        else
        {
            this.proj = proj;
            this.type = proj.type;
            this.index = proj.whoAmI;
            this.owner = proj.owner;
            this.ai = new float[8];
            this.lable = 0;
            this.isActive = proj.active;
        }
    }

    protected CProjectile(Projectile? proj, float[] ai, int lable)
    {
        if (proj == null)
        {
            this.proj = null!;
            this.type = 0;
            this.index = 0;
            this.owner = 0;
            this.ai = new float[8];
            this.lable = 0;
            this.isActive = false;
        }
        else
        {
            this.proj = proj;
            this.type = proj.type;
            this.index = proj.whoAmI;
            this.owner = proj.owner;
            this.ai = ai;
            this.lable = lable;
            this.isActive = proj.active;
        }
    }

    public static void CKill(int index)
    {
        if (Main.projectile[index] != null && Main.projectile[index].active)
        {
            
            TSPlayer.All.SendData((PacketTypes) 29, "", Main.projectile[index].key, 0f, 0f, 0);
            Collect.cprojs[index]?.isActive = false;
        }
    }

    public void CKill()
    {
        if (this.proj != null && this.proj.active)
        {
            this.proj.active = false;
            TSPlayer.All.SendData((PacketTypes) 29, "", this.proj.key, 0f, 0f, 0);
            Collect.cprojs[this.proj.whoAmI]?.isActive = false;
        }
    }

    public static void Update(int index)
    {
        TSPlayer.All.SendData((PacketTypes) 27, null, index, 0f, 0f, 0f, 0);
    }

    public void Update()
    {
        TSPlayer.All.SendData((PacketTypes) 27, null, this.index, 0f, 0f, 0f, 0);
    }

    public virtual void ProjectileAI()
    {
    }

    public virtual void PreProjectileKilled()
    {
    }

    public virtual void MyEffect()
    {
    }
}
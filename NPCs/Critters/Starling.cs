using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LivingWorldMod.Utils;
using System.Collections.Generic;

namespace LivingWorldMod.NPCs.Critters
{
	public class Starling : ModNPC
	{
		internal class Star
		{
			Vector2 position;
			Color color;
			public float scale;
			public float rotation;
			public Star(Vector2 pos, Color col, float scal, float rot)
			{
				color = col;
				position = pos;
				scale = scal;
				rotation = rot;
			}
			public void Draw(SpriteBatch spriteBatch, BasicEffect effect)
			{
				StarDraw.DrawStarBasic(effect, position, rotation, scale * 0.8f, Color.White);
				StarDraw.DrawStarBasic(effect, position, rotation, scale * 1.5f, color * 0.5f);
			}
			public void AI()
			{
				scale -= 0.3f;
				rotation += 0.01f;
			}
		}
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starling");
			Main.npcFrameCount[npc.type] = 4;
		}

		public override void SetDefaults()
		{
			npc.width = 18;
			npc.height = 16;
			npc.damage = 0;
			npc.defense = 0;
			npc.lifeMax = 5;
			npc.dontCountMe = true;
			npc.HitSound = SoundID.NPCHit4;
			npc.DeathSound = SoundID.NPCDeath4;
			Main.npcCatchable[npc.type] = true;
			npc.catchItem = (short)ModContent.ItemType<StarlingItem>();
			npc.knockBackResist = .45f;
			npc.aiStyle = 64;
			npc.npcSlots = 0;
			npc.noGravity = true;
			aiType = NPCID.Firefly;
			Main.npcFrameCount[npc.type] = 4;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.playerSafe) {
				return 0f;
			}
			return spawnInfo.sky && !Main.dayTime ? 0.1f : 0;
		}

		public override void FindFrame(int frameHeight)
		{
			npc.frameCounter += 0.15f;
			npc.frameCounter %= Main.npcFrameCount[npc.type];
			int frame = (int)npc.frameCounter;
			npc.frame.Y = frame * frameHeight;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			foreach(Star star in stars)
			{
				star.Draw(spriteBatch, effect);
			}
			 float sineAdd = (float)Math.Sin(sinCounter * 1.33f);
			Vector2 center = new Vector2((float)(Main.npcTexture[npc.type].Width / 2), (float)(Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2));
            #region shader
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            Vector4 colorMod = Color.Gold.ToVector4();
            LivingWorldMod.CircleNoise.Parameters["distance"].SetValue(2.9f - (sineAdd / 10));
            LivingWorldMod.CircleNoise.Parameters["colorMod"].SetValue(colorMod);
            LivingWorldMod.CircleNoise.Parameters["noise"].SetValue(mod.GetTexture("Textures/noise"));
           	LivingWorldMod.CircleNoise.Parameters["rotation"].SetValue(sinCounter / 5);
            LivingWorldMod.CircleNoise.Parameters["opacity2"].SetValue(0.5f + (sineAdd / 10));
            LivingWorldMod.CircleNoise.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(mod.GetTexture("Effects/Masks/Extra_49"), (npc.Center - Main.screenPosition) + new Vector2(0, 2), null, Color.White, 0f, new Vector2(50, 50), 0.55f + (sineAdd / 18), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            #endregion
			spriteBatch.Draw(Main.npcTexture[npc.type], npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY), npc.frame,
							 drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale * (new Vector2((float)Math.Sin(sinCounter * 2), (float)Math.Sin((sinCounter * 2) + 3.14f)) / 4) + new Vector2(1,1), SpriteEffects.None, 0);
			return false;
		}
		float sinCounter = 0;
		private static BasicEffect effect = new BasicEffect(Main.instance.GraphicsDevice);
		internal List<Star> stars = new List<Star>();
        public override void AI()
        {
			foreach(Star star in stars.ToArray())
			{
				star.AI();
				if (star.scale < 5)
				{
					stars.Remove(star);
				}
			}
			effect.VertexColorEnabled = true;

            sinCounter += 0.03f;
			Lighting.AddLight((int)((npc.position.X + (float)(npc.width / 2)) / 16f), (int)((npc.position.Y + (float)(npc.height / 2)) / 16f), .8f, .8f, .4f);
			if (sinCounter % 1 < 0.031f)
			{
				int red = Main.rand.Next(60,255);
				int green = Main.rand.Next(60,255);
				int blue = Main.rand.Next(60,255);
				Color color = new Color(red,green,blue);
				stars.Add(new Star(npc.Center + new Vector2(Main.rand.Next(-20,20), Main.rand.Next(-20,20)), 
				color,
				15,
				Main.rand.NextFloat(6.28f)
				));
			}
		}
	}
	public class StarlingItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starling");
		}

		public override void SetDefaults()
		{
			item.width = item.height = 32;
			item.rare = ItemRarityID.Green;
			item.maxStack = 99;
			item.noUseGraphic = true;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.sellPrice(0, 0, 7, 0);
			item.useTime = item.useAnimation = 20;

			item.noMelee = true;
			item.consumable = true;
			item.autoReuse = true;

		}
		public override bool UseItem(Player player)
		{
			NPC.NewNPC((int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<Starling>());
			return true;
		}

	}
}

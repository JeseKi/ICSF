using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using icsf.Systems;

namespace icsf.Items;

public class HitboxControllerItem : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.MechanicalLens}";

	public override void SetDefaults()
	{
		Item.width = 20;
		Item.height = 20;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.UseSound = SoundID.MenuOpen;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.buyPrice(silver: 50);
		Item.consumable = false;
	}

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI == Main.myPlayer) {
			HitboxOverlayUiSystem.ToggleUi();
		}

		return true;
	}

	public override bool CanRightClick() => true;

	public override void RightClick(Player player)
	{
		if (player.whoAmI == Main.myPlayer) {
			HitboxOverlayUiSystem.ToggleUi();
		}
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Wood, 10)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}

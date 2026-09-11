using Terraria;

namespace WorldEdit.Expressions;

public class OrExpression : Expression
{
	public OrExpression(Expression left, Expression right)
	{
		Left = left;
		Right = right;
	}

	public override bool Evaluate(ref TileData tile)
	{
		return Left.Evaluate(ref tile) || Right.Evaluate(ref tile);
	}
}

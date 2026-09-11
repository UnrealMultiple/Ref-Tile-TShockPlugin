using Terraria;

namespace WorldEdit.Expressions;

public class AndExpression : Expression
{
	public AndExpression(Expression left, Expression right)
	{
		Left = left;
		Right = right;
	}

	public override bool Evaluate(ref TileData tile)
	{
		return Left.Evaluate(ref tile) && Right.Evaluate(ref tile);
	}
}

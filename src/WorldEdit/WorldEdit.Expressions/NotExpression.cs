using Terraria;

namespace WorldEdit.Expressions;

public class NotExpression : Expression
{
	public NotExpression(Expression expression)
	{
		Left = expression;
	}

	public override bool Evaluate(ref TileData tile)
	{
		return !Left.Evaluate(ref tile);
	}
}

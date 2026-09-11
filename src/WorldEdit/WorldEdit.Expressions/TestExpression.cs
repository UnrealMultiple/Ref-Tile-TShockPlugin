using Terraria;

namespace WorldEdit.Expressions;

public sealed class TestExpression : Expression
{
	public Test Test;

	public TestExpression(Test test)
	{
		Test = test;
	}

	public override bool Evaluate(ref TileData tile)
	{
		return Test(ref tile);
	}
}

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<ConditionalSelectBenchmark>();

[DisassemblyDiagnoser]
public class ConditionalSelectBenchmark
{
	private static readonly byte[] Mask = [255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0];

	[Benchmark]
	public byte ConditionalSelectField()
	{
		var result = Vector128.ConditionalSelect(Vector128.Create(Mask), Vector128<byte>.One, Vector128<byte>.Zero);

		return result[0];
	}

	[Benchmark]
	public byte BlendVariable()
	{
		var result = Sse41.BlendVariable(Vector128<byte>.One, Vector128<byte>.Zero, Vector128.Create(Mask));

		return result[0];
	}
}

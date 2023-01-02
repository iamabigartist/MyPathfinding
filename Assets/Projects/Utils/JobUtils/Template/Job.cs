using Unity.Burst;
using Unity.Jobs;
namespace Utils.JobUtils.Template
{
	[BurstCompile(
		DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance,
		CompileSynchronously = true)]
	public struct Job<TJobRunner> : IJob
		where TJobRunner : struct, IJobRunner
	{
		public TJobRunner runner;
		public void Execute()
		{
			runner.Execute();
		}
		public static void Plan(TJobRunner Runner, ref JobHandle deps)
		{
			var job = new Job<TJobRunner>() { runner = Runner };
			deps = job.Schedule(deps);
		}
	}
	public interface IJobRunner
	{
		void Execute();
	}
}
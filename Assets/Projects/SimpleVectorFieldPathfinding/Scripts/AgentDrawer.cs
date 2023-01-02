using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
namespace SimpleVectorFieldPathfinding
{
	public class AgentDrawer
	{
		List<float2> agents;
		public AgentDrawer(List<float2> Agents)
		{
			agents = Agents;
		}

		NativeHashSet<int2> set;

		public delegate void DrawAgent(int index, float2 agent_location);
		public void Draw(DrawAgent DrawAgentFunction)
		{
			for (int i = 0; i < agents.Count; i++)
			{
				DrawAgentFunction(i, agents[i]);
			}
		}
	}
}